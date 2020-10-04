using Promenade.Models;
using Promenade.Services.Abstract;

namespace Promenade.Services
{
    public class GeoService
    {
        private readonly IDbService _dbService;
        private readonly ContentService _contentService;

        public GeoService(IDbService dbService, ContentService contentService)
        {
            _dbService = dbService;
            _contentService = contentService;
        }
        
        public User LoadUser(string userId)
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
    }
}