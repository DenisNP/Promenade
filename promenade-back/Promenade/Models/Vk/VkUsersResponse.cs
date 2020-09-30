using Newtonsoft.Json;

namespace Promenade.Models.Vk
{
    public class VkUsersResponse
    {
        [JsonProperty("response")]
        public VkUser[] Response { get; set; }
    }
}