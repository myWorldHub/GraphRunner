using System.Collections.Generic;

namespace GraphRunner
{
    public class ExecutionSetting
    {
        public IDictionary<string,GraphSetting> Graphs;

        public IList<NodeConnection> Connections;

        public IList<Execution> Executions;
    }
}