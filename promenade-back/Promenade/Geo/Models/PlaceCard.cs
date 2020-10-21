using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Promenade.Geo.Models
{
    public class PlaceCard
    {
        public string[] ImageUrls { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }

        public string WikipediaUrl { get; set; }
    }
}
