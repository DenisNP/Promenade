using Promenade.Geo;
using Promenade.Geo.Models;

namespace Promenade.Models
{
    public class State
    {
        public User User { get; set; }
        public Poi Poi { get; set; }
        public Route Route { get; set; }
        public GeoPoint Coordinates { get; set; } = new GeoPoint();
        public bool IsNearPoi { get; set; }
    }
}