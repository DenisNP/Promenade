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
        private const int SavedToRawRatio = 10;
        private readonly double[] _rangeDistances = {0.1, 0.67, 1, 2};
        private const double RadiusToBoundRatio = 1.2;
        private const double FurtherToCloserRatio = 2.0;
        private const double NearDistance = 0.05;
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
            var center = new GeoPoint(lat, lng);
            if (rangeId < 0 || rangeId > _rangeDistances.Length - 1)
                throw new ArgumentOutOfRangeException(nameof(rangeId), $"RangeId should be from 0 to {_rangeDistances.Length - 1}");

            var distance = _rangeDistances[rangeId];
            var topLeft = GeoUtils.FindPointAtDistanceFrom(center, -Math.PI / 4, distance * RadiusToBoundRatio);
            var bottomRight = GeoUtils.FindPointAtDistanceFrom(center, 3 * Math.PI / 4, distance * RadiusToBoundRatio);
            
            // get points
            var pois = overpass.Execute(topLeft, bottomRight);
            var poi = ChoosePoi(center, distance, pois, state.User);

            if (poi == null)
            {
                state.Poi = null;
                state.Route = null;
            }
            else
            {
                var mapbox = new Mapbox();
                var route = mapbox.Walk(center, poi.Coordinates);
                
                state.Poi = poi;
                state.Route = route;

                _contentService.FillEmptyData(state.Poi);
            }

            SetIsNear(state);
            SaveState(state);
            return state;
        }

        private Poi ChoosePoi(GeoPoint center, double distance, List<Poi> pois, User user)
        {
            if (pois.Count == 0) return null;
            double GetThreshold(GeoPoint p, string poiId)
            {
                var actualDist = GeoUtils.Distance(p, center);
                var diff = actualDist - distance;
                var coeff = diff > 0 ? FurtherToCloserRatio : 1.0;
                var rawValue = coeff * Math.Sqrt(Math.Abs(diff));
                var savedValue = user.SavedPoiScore(poiId);

                return rawValue + SavedToRawRatio * savedValue;
            }

            return pois.MinBy(p => GetThreshold(p.Coordinates, p.Id));
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
                _dbService.UpdateAsync(state.User);
                
                // clear point
                state.Poi = null;
                state.Route = null;
                state.IsNearPoi = false;
                
                if (!disableSave) SaveState(state);
            }
                
            return state;
        }
        
        public State ToggleCategory(string userId, int categoryId)
        {
            var state = GetState(userId);
            var category = state.User.Categories.FirstOrDefault(c => c.Id == categoryId);
            
            if (category == null) throw new ArgumentOutOfRangeException(nameof(categoryId));
            
            // cant disable the only enabled category
            if (state.User.Categories.Count(c => c.Enabled) == 1) return state;
            
            // toggle category and save
            category.Enabled = !category.Enabled;
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