using System;
using System.Collections.Generic;

namespace GraphRunner
{
    public class GraphSetting
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public IDictionary<string,string> Setting { get; set; }
    }
}