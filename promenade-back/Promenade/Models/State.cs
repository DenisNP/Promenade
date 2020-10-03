using Promenade.Geo;

namespace Promenade.Models
{
    public class State
    {
        public User User { get; set; }
        public Poi Poi { get; set; }
        public Route Route { get; set; }
        public bool IsNearPoi { get; set; }
    }
}