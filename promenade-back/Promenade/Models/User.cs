using System.Collections.Generic;
using System.Linq;
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
                    Id = poi.Id,
                    Number = 1,
                    Visited = visited,
                    CategoryId = poi.CategoryId,
                    FullTagId = poi.FullTagId,
                    Coordinates = poi.Coordinates,
                    Description = poi.Description,
                    Tags = poi.Tags,
                });
            }
        }

        public Poi[] GetVisitedAsPoi()
        {
            return PoiSaved.Values
                .Where(p => p.Visited)
                .Select(p => new Poi
                    {
                        Id = p.Id,
                        CategoryId = p.CategoryId,
                        FullTagId = p.FullTagId,
                        Coordinates = p.Coordinates,
                        Description = p.Description,
                        Tags = p.Tags,
                    }
                )
                .ToArray();
        }
    }

    public class SavedPoi : Poi
    {
        private const int VisitedToSkipRatio = 100;
        
        public int Number { get; set; }
        public bool Visited { get; set; }

        public int GetScore()
        {
            return Number * (Visited ? VisitedToSkipRatio : 1);
        }
    }
}