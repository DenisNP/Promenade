using System.Globalization;

namespace Promenade.Geo.Models
{
    public class GeoPoint
    {
        public double Lat { get; set; }
        public double Lng { get; set; }

        public GeoPoint() { }

        public GeoPoint(double lat, double lng)
        {
            Lat = lat;
            Lng = lng;
        }

        public string AsString(char separator = ',')
        {
            var cInfo = new CultureInfo("en-US", false);
            return Lat.ToString(cInfo) + separator + Lng.ToString(cInfo);
        }
    }
}