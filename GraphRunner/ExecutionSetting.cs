using System.Collections.Generic;

namespace GraphRunner
{
    public class ExecutionSetting
    {
        public IList<GraphSetting> Graphs;

        public IList<NodeConnection> Connections;

        public IList<Execution> Executions;
    }
}