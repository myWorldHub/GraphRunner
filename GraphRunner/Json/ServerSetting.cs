using System.Collections.Generic;

namespace GraphRunner.Json
{
    public class ServerSetting
    {
        public int Port { get; set; }
        public IList<BindSetting> Bindings { get; set; }
    }
}