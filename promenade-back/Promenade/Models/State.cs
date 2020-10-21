using System;
using System.Collections.Generic;
using Newtonsoft.Json;
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
        public Poi[] Visited { get; set; }
        
        [JsonIgnore] 
        public List<IdFoundRecord> LastCategoriesFound { get; set; } = new List<IdFoundRecord>();
        [JsonIgnore]
        public List<IdFoundRecord> LastTagsFound { get; set; } = new List<IdFoundRecord>();
        [JsonIgnore] 
        public CachedResult CachedResult { get; set; }
    }

    public class IdFoundRecord
    {
        public int Id { get; set; }
        public DateTime Time { get; set; }
        public int Order { get; set; }
    }

    public class CachedResult
    {
        private const double DistanceTolerance = 0.02;
        
        public GeoPoint Coordinates { get; set; }
        public int RangeId { get; set; }
        public List<Poi> Pois { get; set; }

        public bool IsEqual(GeoPoint coordinates, int rangeId)
        {
            if (RangeId != rangeId) return false;
            var dist = GeoUtils.Distance(Coordinates, coordinates);
            return dist <= DistanceTolerance;
        }
    }
}