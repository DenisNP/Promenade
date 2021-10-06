using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Promenade.Geo;
using Promenade.Geo.Models;
using Promenade.Models;
using Promenade.Services.Abstract;

namespace Promenade.Services
{
    public class GeoService
    {
        private const int SavedToRawRatio = 20;
        // private const int CategoryDuplicateRatio = 1;
        private const int TagDuplicateRatio = 5;
        private readonly double[] _rangeDistances = {0.67, 1, 2, 4};
        private const double RadiusToBoundRatio = 1.2;
        private const double FurtherToCloserRatio = 2.0;
        private const double NearDistance = 0.06;
        private const double CompareTolerance = 0.00001;
        
        private readonly IDbService _dbService;
        private readonly ContentService _contentService;
        private readonly IMemoryCache _memoryCache;

        public GeoService(IDbService dbService, ContentService contentService, IMemoryCache memoryCache)
        {
            _dbService = dbService;
            _contentService = contentService;
            _memoryCache = memoryCache;
        }
        
        private User LoadUser(string userId)
        {
            var user = _dbService.ById<User>(userId);
            if (user == null)
            {
                user = new User
                {
                    Id = userId,
                    Categories = _contentService.GenerateInitial()
                };
                _dbService.UpdateAsync(user);
            }

            return user;
        }

        public State GetState(string userId)
        {
            if (!_memoryCache.TryGetValue(userId, out State state))
            {
                var user = LoadUser(userId);
                state = new State
                {
                    User = user,
                    Coordinates = new GeoPoint(),
                    IsNearPoi = false,
                    Poi = null,
                    Route = null,
                    Visited = user.GetVisitedAsPoi(),
                    Achievements = CalculateAchievements(user.PoiSaved.Values.ToList())
                };
                
                SaveState(state);
            }

            return state;
        }

        public State Find(string userId, double lat, double lng, int rangeId)
        {
            var state = GetState(userId);
            if (state.Poi != null) Stop(userId, state, true);
            
            // set coordinates
            state.Coordinates = new GeoPoint(lat, lng);
            
            // get pois from cache or from Overpass
            List<Poi> pois;
            if (state.CachedResult != null && state.CachedResult.IsEqual(state.Coordinates, rangeId))
            {
                pois = state.CachedResult.Pois;
            }
            else
            {
                pois = QueryPois(rangeId, state);
                state.CachedResult = new CachedResult
                {
                    Coordinates = state.Coordinates,
                    RangeId = rangeId,
                    Pois = pois
                };
            }
            
            // choose poi
            var distance = _rangeDistances[rangeId];
            var poi = ChoosePoi(state.Coordinates, distance, pois, state.User, state.LastCategoriesFound, state.LastTagsFound);

            if (poi == null)
            {
                state.Poi = null;
                state.Route = null;
            }
            else
            {
                var mapbox = new Mapbox();
                var route = mapbox.Walk(state.Coordinates, poi.Coordinates);
                
                state.Poi = poi;
                state.Route = route;
            }

            SetIsNear(state);
            SaveState(state);
            return state;
        }

        public void WriteAllPois(GeoPoint topLeft, GeoPoint bottomRight)
        {
            // create overpass object and add clauses
            Overpass overpass = new Overpass().SetTimeout(300);
            KeyValuePair<string, string>[] tags = _contentService.GetTagsForCategories(_contentService.GetAllIds());
            foreach (KeyValuePair<string, string> tag in tags)
            {
                overpass = overpass.AddClause(Overpass.AllTypes, tag);
            }
            
            // get points
            const double distanceStepInKm = 5.0;
            double horizontalDistance = GeoUtils.Distance(topLeft, new GeoPoint(topLeft.Lat, bottomRight.Lng));
            double verticalDistance = GeoUtils.Distance(topLeft, new GeoPoint(bottomRight.Lat, topLeft.Lng));
            var horizontalSteps = (int)Math.Ceiling(horizontalDistance / distanceStepInKm);
            var verticalSteps = (int)Math.Ceiling(verticalDistance / distanceStepInKm);
            
            Console.WriteLine("Horizontal distance: {0}; steps: {1}", horizontalDistance, horizontalSteps);
            Console.WriteLine("Vertical distance: {0}; steps: {1}", verticalDistance, verticalSteps);
            Console.WriteLine();
            
            // go
            const string directory = @"D:\places\";
            if (!Directory.Exists(directory)) 
                Directory.CreateDirectory(directory);
            
            for (var h = 0; h < horizontalSteps; h++)
            {
                var v = 0;
                var errors = 0;
                while (v < verticalSteps)
                {
                    if (File.Exists(directory + $"places_{h}_{v}.json"))
                    {
                        v++;
                        continue;
                    }
                    
                    GeoPoint currentLeft = GeoUtils.FindPointAtDistanceFrom(topLeft, Math.PI / 2, distanceStepInKm * h);
                    GeoPoint currentTop = GeoUtils.FindPointAtDistanceFrom(topLeft, Math.PI, distanceStepInKm * v);
                    var currentTopLeft = new GeoPoint(currentTop.Lat, currentLeft.Lng);
                    Console.WriteLine("Current TL: {0}, {1}", currentTopLeft.Lat, currentTopLeft.Lng);

                    GeoPoint currentBottomRight = GeoUtils.FindPointAtDistanceFrom(currentTopLeft, 3 * Math.PI / 4,
                        distanceStepInKm * Math.Sqrt(2));
                    
                    List<Poi> currentPois = overpass.Execute(currentTopLeft, currentBottomRight);
                    if (currentPois == null)
                    {
                        errors++;
                        int w = errors * 10000;
                        Console.WriteLine("{0}_{1} error, waiting for {2}ms\n", h, v, w);
                        Thread.Sleep(w);
                        continue;
                    }

                    if (errors > 0) errors--;
                    Console.WriteLine("points loaded: {0}", currentPois.Count);
                    
                    var fileName = $"places_{h}_{v}.json";
                    List<PlaceData> places = currentPois.Select(p =>
                    {
                        _contentService.FillEmptyData(p);
                        return new PlaceData
                        {
                            Id = p.Id,
                            Name = p.Description,
                            Category = _contentService.GetCategory(p.CategoryId).Name,
                            Images = Array.Empty<string>(),
                            Location = p.Coordinates,
                            Tags = p.Tags
                        };
                    }).ToList();
                    
                    File.WriteAllText(directory + fileName, JsonConvert.SerializeObject(places, Formatting.Indented));
                    Console.WriteLine(fileName + " written\n");

                    v++;

                    if (errors > 0)
                    {
                        int w = 10000 * errors;
                        Console.WriteLine("waiting for {0}ms", w);
                        Thread.Sleep(w);
                    }
                }
            }
            
            Console.WriteLine("All done");
        }
        
        private class PlaceData
        {
            public string Id { get; set; } = "";
            public string Name { get; set; } = "";
            public string Category { get; set; } = "";
            public string[] Images { get; set; } = Array.Empty<string>();
            public KeyValuePair<string, string>[] Tags { get; set; }
            public GeoPoint Location { get; set; }
        }

        private List<Poi> QueryPois(int rangeId, State state)
        {
            // create overpass object and add clauses
            var overpass = new Overpass();
            var tags = _contentService.GetTagsForCategories(
                state.User.Categories.Where(c => c.Enabled)
                    .Select(c => c.Id)
                    .ToArray()
            );
            foreach (var tag in tags)
            {
                overpass = overpass.AddClause(Overpass.AllTypes, tag);
            }
            
            // generate bounding box
            var center = state.Coordinates;
            if (rangeId < 0 || rangeId > _rangeDistances.Length - 1)
                throw new ArgumentOutOfRangeException(nameof(rangeId), $"RangeId should be from 0 to {_rangeDistances.Length - 1}");

            var distance = _rangeDistances[rangeId];
            var topLeft = GeoUtils.FindPointAtDistanceFrom(center, -Math.PI / 4, distance * RadiusToBoundRatio);
            var bottomRight = GeoUtils.FindPointAtDistanceFrom(center, 3 * Math.PI / 4, distance * RadiusToBoundRatio);
            
            // get points
            var pois = overpass.Execute(topLeft, bottomRight);
            pois.ForEach(p => _contentService.FillEmptyData(p));

            return pois;
        }

        private Poi ChoosePoi(
            GeoPoint center,
            double distance,
            List<Poi> pois,
            User user,
            List<IdFoundRecord> lastCategories,
            List<IdFoundRecord> lastTags
        )
        {
            if (pois.Count == 0) return null;
            
            // filter poi by visited
            pois = pois.Where(p => !user.PoiSaved.ContainsKey(p.Id) || !user.PoiSaved[p.Id].Visited).ToList();
            
            // store categories and tagsweights
            // var catWeights = ConvertToWeights(lastCategories);
            var tagWeights = ConvertToWeights(lastTags);
            
            double GetThreshold(Poi p)
            {
                var actualDist = GeoUtils.Distance(p.Coordinates, center);
                var diff = actualDist - distance;
                var coeff = diff > 0 ? FurtherToCloserRatio : 1.0;
                var rawValue = coeff * Math.Sqrt(Math.Abs(diff));
                var savedValue = user.SavedPoiScore(p.Id);
                // var categoryDuplicateValue = catWeights.ContainsKey(p.CategoryId) ? catWeights[p.CategoryId] : 0;
                var tagDuplicateValue = tagWeights.ContainsKey(p.FullTagId) ? tagWeights[p.FullTagId] : 0;

                return rawValue
                       + SavedToRawRatio * savedValue
                       // + CategoryDuplicateRatio * categoryDuplicateValue
                       + TagDuplicateRatio * tagDuplicateValue;
            }

            return pois.MinBy(GetThreshold);
        }

        private Dictionary<int, int> ConvertToWeights(List<IdFoundRecord> list)
        {
            return list.ToDictionary(x => x.Id, y => y.Order);
        }
        
        public State Move(string userId, double lat, double lng)
        {
            var state = GetState(userId);
            var changed = state.Coordinates == null
                          || Math.Abs(state.Coordinates.Lat - lat) > CompareTolerance
                          || Math.Abs(state.Coordinates.Lng - lng) > CompareTolerance;
            
            state.Coordinates = new GeoPoint(lat, lng);

            if (state.Poi != null && changed)
            {
                var mapbox = new Mapbox();
                var route = mapbox.Walk(state.Coordinates, state.Poi.Coordinates);

                state.Route = route;
            }
            
            SetIsNear(state);
            SaveState(state);
            return state;
        }
        
        public State Stop(string userId, State state = null, bool disableSave = false)
        {
            state ??= GetState(userId);
            if (state.Poi != null)
            {
                // update user data
                state.User.VisitPoi(state.Poi, state.IsNearPoi);
                if (state.IsNearPoi)
                {
                    state.Visited = state.User.GetVisitedAsPoi();
                    state.Achievements = CalculateAchievements(state.User.PoiSaved.Values.ToList());
                }

                // update db
                _dbService.Update(state.User);
                
                // reorder list by time
                state.LastCategoriesFound = WriteRecord(state.LastCategoriesFound, state.Poi.CategoryId);
                state.LastTagsFound = WriteRecord(state.LastTagsFound, state.Poi.FullTagId);

                // clear point
                state.Poi = null;
                state.Route = null;
                state.IsNearPoi = false;
                
                if (!disableSave) SaveState(state);
            }
                
            return state;
        }

        private List<IdFoundRecord> WriteRecord(List<IdFoundRecord> records, int newId)
        {
            var record = records.FirstOrDefault(c => c.Id == newId);
            if (record == null)
            {
                record = new IdFoundRecord() {Id = newId};
                records.Add(record);
            }
            record.Time = DateTime.UtcNow;

            var newList = records.OrderBy(r => r.Time).ToList();
            for (var i = 0; i < newList.Count; i++)
            {
                newList[i].Order = i + 1;
            }

            return newList;
        }

        public State SetSettings(string userId, SettingsDto settings)
        {
            var state = GetState(userId);
            
            // set categories
            foreach (var categorySetting in settings.Categories)
            {
                var category = state.User.Categories.SingleOrDefault(c => c.Id == categorySetting.Id);
                if (category == null) continue;

                category.Enabled = categorySetting.Enabled;
            }
            
            // check if there is any category enabled
            if (!state.User.Categories.Any(c => c.Enabled)) 
                state.User.Categories[0].Enabled = true; // enable first
            
            // clear cache
            state.CachedResult = null;

            // save state and user data
            _dbService.UpdateAsync(state.User);
            SaveState(state);

            return state;
        }

        private void SetIsNear(State state)
        {
            if (state.Poi == null)
            {
                state.IsNearPoi = false;
                return;
            }
            
            var dist = GeoUtils.Distance(state.Poi.Coordinates, state.Coordinates);
            state.IsNearPoi = dist <= NearDistance;
        }

        private void SaveState(State state)
        {
            _memoryCache.Set(state.User.Id, state, new MemoryCacheEntryOptions { SlidingExpiration = TimeSpan.FromHours(2) });
        }

        private AchievementProgress[] CalculateAchievements(List<SavedPoi> poiSaved)
        {
            var achievements = _contentService.GetAllAchievements();
            var progress = new List<AchievementProgress>();
            
            foreach (var a in achievements)
            {
                var p = new AchievementProgress(a);
                var done = poiSaved.Count(pSaved => pSaved.Visited && pSaved.CategoryId == a.CategoryId);
                p.Done = done;
                
                progress.Add(p);
            }

            return progress.ToArray();
        }
    }
}