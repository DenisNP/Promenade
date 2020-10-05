using System.Collections.Generic;
using Newtonsoft.Json;
using Promenade.Models.Abstract;

namespace Promenade.Models
{
    public class User : IIdentity
    {
        public string Id { get; set; }
        public CategoryForUser[] Categories { get; set; }
        
        [JsonIgnore]
        public Dictionary<string, int> PoiSaved { get; set; } = new Dictionary<string, int>();
    }
}