namespace Promenade.Models.Web.Request
{
    public class CoordinatesRequest : BaseRequest
    {
        public double Lat { get; set; }
        public double Lng { get; set; }
    }
}