#nullable enable
using System.Collections.Generic;

namespace GraphRunner.Json
{
    public class GraphSetting
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public IDictionary<string,string> Setting { get; set; }
    }
}