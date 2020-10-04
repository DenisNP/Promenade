using System;
using Microsoft.Extensions.Caching.Memory;
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