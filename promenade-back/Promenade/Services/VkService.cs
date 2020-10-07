using System;
using System.Collections.Generic;
using System.Linq;
using Promenade.Services.Abstract;

namespace Promenade.Services
{
    public class VkService : ISocialService
    {
        private readonly string _vkApiSecret;

        public VkService()
        {
            var secret = Environment.GetEnvironmentVariable("PROMENADE_VK_SECRET");
#if DEBUG
            _vkApiSecret = "";
#else
            _vkApiSecret = secret ?? throw new ArgumentException("No VK Secret key!");
#endif
        }

        public bool IsSignValid(string userId, Dictionary<string, string> pars, string sign)
        {
#if DEBUG
            return true;
#endif            
            var parsString = string.Join(
                "&",
                pars.Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value)}").OrderBy(x => x)
            );

            var calculatedSign = Utils.ToBase64(Utils.HashHMAC(_vkApiSecret, parsString));
            return calculatedSign == sign && pars.ContainsKey("vk_user_id") && pars["vk_user_id"] == userId;
        }
    }
}