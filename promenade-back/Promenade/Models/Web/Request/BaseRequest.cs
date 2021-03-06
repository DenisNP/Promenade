﻿using System.Collections.Generic;

namespace Promenade.Models.Web.Request
{
    public class BaseRequest
    {
        public string UserId { get; set; }
        public string Sign { get; set; } = "";
        public Dictionary<string, string> Params { get; set; } = new Dictionary<string, string>();
    }
}