using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Promenade.Geo.Models;

namespace Promenade.Geo
{
    public class Mapbox : IDisposable
    {
        private const string DefaultUrl = "https://api.mapbox.com/directions/v5/mapbox/";
        private string _token;
        private string _url;
        private ILogger _logger;

        public Mapbox(ILogger logger = null, string url = DefaultUrl)
        {
            _token = Environment.GetEnvironmentVariable("MAPBOX_TOKEN");
            _url = url;
            _logger = logger;
        }

        public Route Walk(GeoPoint from, GeoPoint to)
        {
            if (string.IsNullOrEmpty(_token)) throw new ArgumentException("No Mapbox token");

            var coords = Uri.EscapeDataString($"{from.AsString(',')};{to.AsString(',')}");
            var fullUrl = $"{_url}walking/{coords}?alternatives=false&geometries=geojson&steps=false&access_token={_token}";

            var response = Utils.GetRequest(fullUrl, _logger);
            try
            {
                var data = JsonConvert.DeserializeObject<MapboxResult>(response, Utils.ConverterSettings);
                var route = data.Routes.First();

                return new Route
                {
                    Distance = (int) Math.Round(route.Distance),
                    Time = (int) Math.Round(route.Duration),
                    Points = route.Geometry.Coordinates.Select(c => new GeoPoint(c[0], c[1])).ToArray()
                };
            }
            catch (Exception _)
            {
                _logger?.LogError(response);
                return null;
            }
        }

        public void SetToken(string token)
        {
            _token = token;
        } 
        
        public void Dispose()
        {
            _logger = null;
            _token = "";
            _url = "";
        }

        private class MapboxResult
        {
            public MapboxRoute[] Routes { get; set; }
        }

        private class MapboxRoute
        {
            public MapboxGeometry Geometry { get; set; }
            public double Duration { get; set; }
            public double Distance { get; set; }
        }

        private class MapboxGeometry
        {
            public string Type { get; set; }
            public int[][] Coordinates { get; set; }
        }
    }
}