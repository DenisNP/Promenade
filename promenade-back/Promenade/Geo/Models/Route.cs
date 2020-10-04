namespace Promenade.Geo.Models
{
    public class Route
    {
        public int Time { get; set; }
        public int Distance { get; set; }
        public GeoPoint[] Points { get; set; }
    }
}