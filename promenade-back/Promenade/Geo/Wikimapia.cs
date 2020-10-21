using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Promenade.Geo.Models;
using System;
using System.Linq;

namespace Promenade.Geo
{
    public class Wikimapia : IDisposable
    {
        private const string DefaultUrl = "http://api.wikimapia.org/";
        private const string _getNearest = "place.getnearest";
        private const string _getById = "place.getbyid";
        private const int _maxDistance = 50;
        private string _token;
        private string _url;
        private ILogger _logger;

        public Wikimapia(ILogger logger = null, string url = DefaultUrl)
        {
            _token = Environment.GetEnvironmentVariable("WIKIMAPIA_TOKEN");
            _url = url;
            _logger = logger;
        }

        public void Dispose()
        {
            _logger = null;
            _token = "";
            _url = "";
        }

        public Place[] GetAllPlaces(GeoPoint point)
        {
            if (string.IsNullOrEmpty(_token)) throw new ArgumentException("No Wikimapia token");

            var getNearestUrl = $"{_url}?key={_token}&function={_getNearest}&lat={point.Lat.ToString().Replace(",", ".")}&lon={point.Lng.ToString().Replace(",", ".")}&format=json";

            var response = Utils.GetRequest(getNearestUrl, _logger);
            try
            {
                var data = JsonConvert.DeserializeObject<WikiObject>(response, Utils.ConverterSettings);
                var places = data.Places;

                return places.Where(p => p.Distance < _maxDistance).ToArray();
            }
            catch (Exception _)
            {
                _logger?.LogError(response);
                return null;
            }
        }

        public PlaceCard GetPlace(string id)
        {
            if (string.IsNullOrEmpty(_token)) throw new ArgumentException("No Wikimapia token");

            var getByIdUrl = $"{_url}?key={_token}&function={_getById}&id={id}&format=json&language=ru&data_blocks=main%2Cphotos%2Ccomments%2C";

            var response = Utils.GetRequest(getByIdUrl, _logger);
            try
            {
                var data = JsonConvert.DeserializeObject<PlaceObject>(response, Utils.ConverterSettings);

                return new PlaceCard {
                    Title = data.Title,
                    Description = data.Description,
                    ImageUrls = data.Photos.Select(x => x.The960Url).ToArray(),
                    WikipediaUrl = data.Wikipedia
                };
            }
            catch (Exception _)
            {
                _logger?.LogError(response);
                return null;
            }

        }

    }
}
