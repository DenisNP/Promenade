using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Promenade.Geo.Models;

namespace Promenade.Geo
{
    public class Overpass : IDisposable
    {
        public const ElementType AllTypes = ElementType.Node | ElementType.Relation | ElementType.Way;
        private const int DefaultTimeout = 25;
        private const string DefaultUrl = "http://overpass-api.de/api/interpreter";

        private string _url;
        private readonly int _timeout;

        private List<QueryClause> _query = new List<QueryClause>();
        private GeoPoint _boundsTopLeft;
        private GeoPoint _boundsBottomRight;

        public Overpass(string url = DefaultUrl, int timeout = DefaultTimeout)
        {
            _url = url;
            _timeout = timeout;
        }

        public Overpass AddClause(ElementType elementType, params KeyValuePair<string, string>[] tags)
        {
            _query.Add(new QueryClause(elementType, tags));
            return this;
        }

        public List<Poi> Execute(GeoPoint boundsTopLeft, GeoPoint boundsBottomRight)
        {
            _boundsTopLeft = boundsTopLeft;
            _boundsBottomRight = boundsBottomRight;

            var query = ConstructQuery();
            var response = Utils.MakeRequest(_url, query);
            var data = JsonConvert.DeserializeObject<OverpassResponse>(response, Utils.ConverterSettings);

            return null;
        }

        private string ConstructQuery()
        {
            var q = $"[out:json][timeout:{_timeout}];(";
            q += string.Join("", _query.Select(x => x.Construct(_boundsTopLeft, _boundsBottomRight)));
            q += ");out body;>;out skel qt;";
            return q;
        }

        public void Dispose()
        {
            _query = null;
            _boundsTopLeft = null;
            _boundsBottomRight = null;
            _url = "";
        }

        private class OverpassResponse
        {
            public List<Element> Elements { get; set; }
        }

        private class Element
        {
            public string Type { get; set; }
            public long Id { get; set; }
            public long[] Nodes { get; set; }
            public Dictionary<string, string> Tags { get; set; }
            public double Lat { get; set; }
            public double Lon { get; set; }
        }
    }

    public class Poi
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public KeyValuePair<string, string>[] Tags { get; set; }
        public GeoPoint Coordinates { get; set; }
    }

    [Flags]
    public enum ElementType
    {
        Node = 1,
        Way = 2,
        Relation = 4,
    }

    public class QueryClause
    {
        private readonly ElementType _elementType;
        private readonly KeyValuePair<string, string>[] _tags;

        public QueryClause(ElementType elementType, KeyValuePair<string, string>[] tags)
        {
            if (elementType == 0) throw new ArgumentException("Empty ElementType not allowed");
            
            _elementType = elementType;
            _tags = tags;
        }

        private string TagsToString()
        {
            return $"[{string.Join("][", _tags.Select(t => $"{t.Key}={t.Value}"))}]";
        }

        public string Construct(GeoPoint boundsTopLeft, GeoPoint boundsBottomRight)
        {
            var bounds = $"({boundsTopLeft.AsString(',')},{boundsBottomRight.AsString(',')})";
            var q = "";

            if ((_elementType & ElementType.Node) > 0) q += $"node{TagsToString()}{bounds};";
            if ((_elementType & ElementType.Way) > 0) q += $"way{TagsToString()}{bounds};";
            if ((_elementType & ElementType.Relation) > 0) q += $"relation{TagsToString()}{bounds};";

            return q;
        }
    }
}