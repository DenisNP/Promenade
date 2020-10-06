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

        public string AsString(char separator = ',', bool reverse = false)
        {
            var cInfo = new CultureInfo("en-US", false);
            return !reverse
                ? Lat.ToString(cInfo) + separator + Lng.ToString(cInfo)
                : Lng.ToString(cInfo) + separator + Lat.ToString(cInfo);
        }
    }
}