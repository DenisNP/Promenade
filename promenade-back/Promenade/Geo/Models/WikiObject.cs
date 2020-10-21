using System;
using Newtonsoft.Json;

namespace Promenade.Geo.Models
{
    public class WikiObject
    {
        [JsonProperty("language")]
        public string Language { get; set; }

        [JsonProperty("places")]
        public Place[] Places { get; set; }

        [JsonProperty("found")]
        public long Found { get; set; }

        [JsonProperty("count")]
        public long Count { get; set; }
    }


    public partial class Place
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("language_id")]
        public long LanguageId { get; set; }

        [JsonProperty("language")]
        public string Language { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }

        [JsonProperty("urlhtml")]
        public string Urlhtml { get; set; }

        [JsonProperty("distance")]
        public long Distance { get; set; }

        [JsonProperty("location")]
        public Location Location { get; set; }

        [JsonProperty("polygon")]
        public Polygon[] Polygon { get; set; }
    }

    public partial class Location
    {
        [JsonProperty("north")]
        public double North { get; set; }

        [JsonProperty("south")]
        public double South { get; set; }

        [JsonProperty("east")]
        public double East { get; set; }

        [JsonProperty("west")]
        public double West { get; set; }

        [JsonProperty("lon")]
        public double Lon { get; set; }

        [JsonProperty("lat")]
        public double Lat { get; set; }
    }

    public partial class Polygon
    {
        [JsonProperty("x")]
        public double X { get; set; }

        [JsonProperty("y")]
        public double Y { get; set; }
    }


}
