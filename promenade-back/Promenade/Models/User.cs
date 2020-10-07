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
        public Dictionary<string, SavedPoi> PoiSaved { get; set; } = new Dictionary<string, SavedPoi>();

        public int SavedPoiScore(string poiId)
        {
            return PoiSaved.ContainsKey(poiId) ? PoiSaved[poiId].GetScore() : 0;
        }

        public void VisitPoi(string poiId, int categoryId, bool visited)
        {
            if (PoiSaved.ContainsKey(poiId))
            {
                PoiSaved[poiId].Number++;
                PoiSaved[poiId].Visited = PoiSaved[poiId].Visited || visited;
            }
            else
            {
                PoiSaved.Add(poiId, new SavedPoi
                {
                    Number = 1,
                    Visited = visited,
                    CategoryId = categoryId
                });
            }
        }
    }

    public class SavedPoi
    {
        private const int VisitedToSkipRatio = 10;
        
        public int Number { get; set; }
        public bool Visited { get; set; }
        public int CategoryId { get; set; }

        public int GetScore()
        {
            return Number * (Visited ? VisitedToSkipRatio : 1);
        }
    }
}