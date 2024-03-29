﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Promenade.Geo.Models;

namespace Promenade.Geo
{
    public class Overpass : IDisposable
    {
        public const ElementType AllTypes = ElementType.Node | ElementType.Relation | ElementType.Way;
        private const int DefaultTimeout = 25;
        private const string DefaultUrl = "https://overpass-api.de/api/interpreter";
        private readonly string[] _nameTagsPriority = {"name", "name:ru", "title", "description", "subject" };

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
            var response = Utils.PostRequest(_url, query);
            var pois = new List<Poi>();

            try
            {
                var data = JsonConvert.DeserializeObject<OverpassResponse>(response, Utils.ConverterSettings);
                if (data == null) throw new IOException($"Empty or wrong result from overpass: {response}");

                var i = 0;
                while (i < data.Elements.Count)
                {
                    var el = data.Elements[i];
                    if (el.Tags == null || el.Tags.Count == 0)
                    {
                        // node element or other empty
                        i++;
                        continue;
                    }

                    Poi poi = null;
                    if (HasCoordinates(el))
                    {
                        // single coordinated element with tags, save it
                        data.Elements.RemoveAt(i);
                        poi = new Poi { Coordinates = new GeoPoint(el.Lat, el.Lon) };
                    }
                    else if (el.Nodes != null && el.Nodes.Length > 0)
                    {
                        // multinode element, calculate closest coordinates and save it
                        data.Elements.RemoveAt(i);
                        var nodeCoordinates = new List<GeoPoint>();
                        foreach (var node in el.Nodes)
                        {
                            var nIdx = data.Elements.FindIndex(x => x.Type == "node" && x.Id == node);
                            if (nIdx >= 0 && HasCoordinates(data.Elements[nIdx]))
                            {
                                // node found
                                var point = new GeoPoint(data.Elements[nIdx].Lat, data.Elements[nIdx].Lon);
                                nodeCoordinates.Add(point);
                                
                                // remove element from list
                                if (nIdx < i) i--;
                                data.Elements.RemoveAt(nIdx);
                            }
                        }

                        // set coordinates to poi if found
                        if (nodeCoordinates.Count > 0)
                        {
                            var center = GeoUtils.MidPoint(boundsTopLeft, boundsBottomRight);
                            var closest = nodeCoordinates.MinBy(n => GeoUtils.Distance(n, center));
                            poi = new Poi { Coordinates = closest };
                        }
                    }
                    else
                    {
                        // unknown element, skip
                        i++;
                    }

                    // save poi
                    if (poi != null)
                    {
                        poi.Id = el.Id.ToString();
                        poi.Tags = el.Tags.Select(x => x).ToArray();
                        poi.Description = ExtractName(el);
                        pois.Add(poi);
                    }
                }

                return pois;
            }
            catch (Exception e)
            {
                Console.WriteLine(query);
                Console.WriteLine(response);
                Console.WriteLine(e);
                return new List<Poi>();
            }
        }

        private bool HasCoordinates(Element el)
        {
            return Math.Abs(el.Lat) > 0.00001 && Math.Abs(el.Lon) > 0.00001;
        }

        private string ExtractName(Element el)
        {
            foreach (var tag in _nameTagsPriority)
            {
                if (el.Tags.ContainsKey(tag)) return el.Tags[tag];
            }

            return "";
        }

        private string ConstructQuery()
        {
            var boundsLower = new GeoPoint(
                Math.Min(_boundsTopLeft.Lat, _boundsBottomRight.Lat),
                Math.Min(_boundsTopLeft.Lng, _boundsBottomRight.Lng)
            );
            var boundsHigher = new GeoPoint(
                Math.Max(_boundsTopLeft.Lat, _boundsBottomRight.Lat),
                Math.Max(_boundsTopLeft.Lng, _boundsBottomRight.Lng)
            );
            
            var q = $"[out:json][timeout:{_timeout}];(";
            q += string.Join("", _query.Select(x => x.Construct(boundsLower, boundsHigher)));
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
        public int CategoryId { get; set; }
        public int FullTagId { get; set; }
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

        public string Construct(GeoPoint boundsLower, GeoPoint boundsHigher)
        {
            var bounds = $"({boundsLower.AsString(',')},{boundsHigher.AsString(',')})";
            var q = "";

            if ((_elementType & ElementType.Node) > 0) q += $"node{TagsToString()}{bounds};";
            if ((_elementType & ElementType.Way) > 0) q += $"way{TagsToString()}{bounds};";
            if ((_elementType & ElementType.Relation) > 0) q += $"relation{TagsToString()}{bounds};";

            return q;
        }
    }
}