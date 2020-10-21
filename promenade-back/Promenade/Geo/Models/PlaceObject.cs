using Newtonsoft.Json;
using System;

namespace Promenade.Geo.Models
{
    public partial class PlaceObject
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("object_type")]
        public long ObjectType { get; set; }

        [JsonProperty("language_id")]
        public long LanguageId { get; set; }

        [JsonProperty("language_iso")]
        public string LanguageIso { get; set; }

        [JsonProperty("language_name")]
        public string LanguageName { get; set; }

        [JsonProperty("urlhtml")]
        public string Urlhtml { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("wikipedia")]
        public string Wikipedia { get; set; }

        [JsonProperty("is_building")]
        public bool IsBuilding { get; set; }

        [JsonProperty("is_region")]
        public bool IsRegion { get; set; }

        [JsonProperty("is_deleted")]
        public bool IsDeleted { get; set; }

        [JsonProperty("tags")]
        public Tag[] Tags { get; set; }

        [JsonProperty("pl")]
        public double Pl { get; set; }

        [JsonProperty("photos")]
        public Photo[] Photos { get; set; }

        [JsonProperty("comments")]
        public object[] Comments { get; set; }
    }

    public partial class Photo
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("size")]
        public long Size { get; set; }

        [JsonProperty("status")]
        public long Status { get; set; }

        [JsonProperty("object_id")]
        public long ObjectId { get; set; }

        [JsonProperty("user_id")]
        public long UserId { get; set; }

        [JsonProperty("user_name")]
        public string UserName { get; set; }

        [JsonProperty("time")]
        public long Time { get; set; }

        [JsonProperty("time_str")]
        public string TimeStr { get; set; }

        [JsonProperty("last_user_id")]
        public long LastUserId { get; set; }

        [JsonProperty("last_user_name")]
        public string LastUserName { get; set; }

        [JsonProperty("960_url")]
        public string The960Url { get; set; }

        [JsonProperty("1280_url")]
        public string The1280Url { get; set; }

        [JsonProperty("big_url")]
        public string BigUrl { get; set; }

        [JsonProperty("thumbnail_url")]
        public string ThumbnailUrl { get; set; }

        [JsonProperty("thumbnailRetina_url")]
        public string ThumbnailRetinaUrl { get; set; }

        [JsonProperty("full_url", NullValueHandling = NullValueHandling.Ignore)]
        public string FullUrl { get; set; }
    }

    public partial class Tag
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }
    }
}
