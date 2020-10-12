using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using Promenade.Geo;
using Promenade.Geo.Models;
using Promenade.Models;
using Promenade.Services.Abstract;

namespace Promenade.Services
{
    public class GeoService
    {
        private const int SavedToRawRatio = 5;
        private const int CategoryDuplicateRatio = 10;
        private const int TagDuplicateRatio = 20;
        private readonly double[] _rangeDistances = {0.15, 0.67, 1, 2};
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
            else
            {
                
            }

            return user;
        }

        public State GetState(string userId)
        {
            if (!_memoryCache.TryGetValue(userId, out State state))
            {
                state = new State
                {
                    User = LoadUser(userId),
                    Coordinates = new GeoPoint(),
                    IsNearPoi = false,
                    Poi = null,
                    Route = null
                };
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
            
            // store categories and tagsweights
            var catWeights = ConvertToWeights(lastCategories);
            var tagWeights = ConvertToWeights(lastTags);
            
            double GetThreshold(Poi p)
            {
                var actualDist = GeoUtils.Distance(p.Coordinates, center);
                var diff = actualDist - distance;
                var coeff = diff > 0 ? FurtherToCloserRatio : 1.0;
                var rawValue = coeff * Math.Sqrt(Math.Abs(diff));
                var savedValue = user.SavedPoiScore(p.Id);
                var categoryDuplicateValue = catWeights.ContainsKey(p.CategoryId) ? catWeights[p.CategoryId] : 0;
                var tagDuplicateValue = tagWeights.ContainsKey(p.FullTagId) ? tagWeights[p.FullTagId] : 0;

                return rawValue
                       + SavedToRawRatio * savedValue
                       + CategoryDuplicateRatio * categoryDuplicateValue
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
                state.User.VisitPoi(state.Poi.Id, state.Poi.CategoryId, state.IsNearPoi);

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
            _memoryCache.Set(state.User.Id, state, new MemoryCacheEntryOptions { SlidingExpiration = TimeSpan.FromMinutes(30) });
        }
    }
}