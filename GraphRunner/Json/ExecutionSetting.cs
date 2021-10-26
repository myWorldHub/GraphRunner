using System.Collections.Generic;

namespace GraphRunner.Json
{
    public class ExecutionSetting
    {
        public IList<GraphSetting> Graphs { get; set; }

        public IList<NodeConnection> Connections { get; set; }

        public IList<Execution> Executions { get; set; }
    }
}