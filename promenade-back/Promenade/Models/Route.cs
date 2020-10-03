using Promenade.Geo.Models;

namespace Promenade.Models
{
    public class Route
    {
        public int Time { get; set; }
        public int Distance { get; set; }
        public GeoPoint[] Points { get; set; }
    }
}