using System;
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
            if (state.Poi != null) Stop(userId);
            
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
            var distance = rangeId switch
            {
                0 => 0.3,
                1 => 0.67,
                2 => 1,
                3 => 2,
                _ => throw new ArgumentOutOfRangeException(nameof(rangeId), "RangeId should be from 0 to 3")
            };
            var topLeft = GeoUtils.FindPointAtDistanceFrom(center, -Math.PI / 4, distance);
            var bottomRight = GeoUtils.FindPointAtDistanceFrom(center, 3 * Math.PI / 4, distance);
            
            // get points
            var pois = overpass.Execute(topLeft, bottomRight);
            
            throw new NotImplementedException();
        }
        
        public State Move(string userId, double lat, double lng)
        {
            throw new NotImplementedException();
        }
        
        public State Stop(string userId)
        {
            throw new NotImplementedException();
        }
        
        public State ToggleCategory(string userId, int categoryId)
        {
            throw new NotImplementedException();
        }
    }
}