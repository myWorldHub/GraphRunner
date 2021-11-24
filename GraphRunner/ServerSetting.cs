using System.Collections.Generic;

namespace GraphRunner
{
    public class ServerSetting
    {
        public int Port { get; set; }

        /// <summary>
        /// path -> graph 
        /// </summary>
        public Dictionary<string,int> Bindings { get; set; }
    }
}