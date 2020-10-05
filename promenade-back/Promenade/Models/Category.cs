using System.Collections.Generic;

namespace Promenade.Models
{
    public class Category : CategoryMeta
    {
        public string Placeholder { get; set; }
        public bool DefaultEnabled { get; set; }
        public TagData[] Tags { get; set; }
    }

    public class TagData
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public string Name { get; set; }

        public KeyValuePair<string, string> ToKvPair()
        {
            return new KeyValuePair<string, string>(Key, Value);
        }
    }
}