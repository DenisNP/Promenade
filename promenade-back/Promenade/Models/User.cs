using System.Collections.Generic;
using Newtonsoft.Json;
using Promenade.Geo;
using Promenade.Geo.Models;
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

        public void VisitPoi(Poi poi, bool visited)
        {
            if (PoiSaved.ContainsKey(poi.Id))
            {
                PoiSaved[poi.Id].Number++;
                PoiSaved[poi.Id].Visited = PoiSaved[poi.Id].Visited || visited;
            }
            else
            {
                PoiSaved.Add(poi.Id, new SavedPoi
                {
                    Number = 1,
                    Visited = visited,
                    CategoryId = poi.CategoryId,
                    Coordinates = poi.Coordinates,
                    Description = poi.Description
                });
            }
        }
    }

    public class SavedPoi
    {
        private const int VisitedToSkipRatio = 100;
        
        public int Number { get; set; }
        public bool Visited { get; set; }
        public int CategoryId { get; set; }
        public string Description { get; set; }
        public GeoPoint Coordinates { get; set; }

        public int GetScore()
        {
            return Number * (Visited ? VisitedToSkipRatio : 1);
        }
    }
}