using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GraphConnectEngine;
using GraphConnectEngine.Graphs;
using GraphConnectEngine.Graphs.Event;
using GraphConnectEngine.Graphs.Value;
using GraphConnectEngine.Nodes;

namespace GraphRunner
{
    public class GraphSetting
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public IDictionary<string,string> Setting { get; set; }

        public IGraph? Create(INodeConnector connector)
        {
            switch (Type)
            {
                case "Updater":
                    return new UpdaterGraph(connector, new SerialSender());
                case "PrintText":
                    return new DebugTextGraph(connector, (str) => { return Task.FromResult(true);});
                case "Value":
                    
                    if (!(Setting.ContainsKey("Type") &&
                          Setting.ContainsKey("Value")))
                        return null;
                    
                    switch (Setting["Type"])
                    {
                        case "Int":
                            return new ValueGraph<int>(connector, int.Parse(Setting["Value"]));
                    }
                    
                    break;
            }

            return null;
        }
    }
}