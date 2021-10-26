#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;
using GraphConnectEngine;
using GraphConnectEngine.Graphs.Value;
using GraphConnectEngine.Nodes;
using GraphRunner.Json;

namespace GraphRunner
{
    public class ExecutionEnv
    {
        private readonly IDictionary<int, IGraph> _graphs;
        private readonly INodeConnector _connector;

        public ExecutionEnv()
        {
            _graphs = new Dictionary<int, IGraph>();
            _connector = new NodeConnector();
        }

        public bool AddGraph(GraphSetting setting)
        {
            if (_graphs.ContainsKey(setting.Id)) return false;

            var graph = CreateGraph(setting);

            if (graph != null)
            {
                _graphs[setting.Id] = graph;
                return true;
            }

            return false;
        }

        public bool RemoveGraph(int id)
        {
            if (!_graphs.ContainsKey(id))
                return false;
            
            _graphs[id].Dispose();
            _graphs.Remove(id);
            return true;
        }

        public bool ConnectNode(NodeConnection setting)
        {
            var node1 = GetNode(setting.From);
            var node2 = GetNode(setting.To);

            if (node1 == null || node2 == null)
                return false;

            return setting.Connect ? _connector.ConnectNode(node1, node2) : _connector.DisconnectNode(node1,node2);
        }

        public async Task<bool> Execute(Execution setting)
        {
            if (!_graphs.ContainsKey(setting.GraphId))
            {
                return false;
            }

            var graph = _graphs[setting.GraphId];
            if (graph is MyUpdateGraph updater)
            {
                await updater.Execute();
                return true;
            }

            return false;
        }

        public INode? GetNode(NodeInfo info)
        {
            return GetNode(info.GraphId, info.GetNodeType(), info.Index);
        }

        public INode? GetNode(int graphId, NodeType type, int index)
        {
            if (!_graphs.ContainsKey(graphId))
            {
                return null;
            }

            var graph = _graphs[graphId];

            switch (type)
            {
                case NodeType.InProcess:
                    return graph.InProcessNodes.Count <= index ? null : graph.InProcessNodes[index];
                case NodeType.OutProcess:
                    return graph.OutProcessNodes.Count <= index ? null : graph.OutProcessNodes[index];
                case NodeType.InItem:
                    return graph.InItemNodes.Count <= index ? null : graph.InItemNodes[index];
                case NodeType.OutItem:
                    return graph.OutItemNodes.Count <= index ? null : graph.OutItemNodes[index];
            }

            return null;
        }


        private IGraph? CreateGraph(GraphSetting setting)
        {
            switch (setting.Type)
            {
                case "Updater":
                    return new MyUpdateGraph(_connector, new SerialSender());
                case "PrintText":
                    return new MyDebugTextGraph(_connector);
                case "Value":

                    if (!(setting.Setting.ContainsKey("Type") &&
                          setting.Setting.ContainsKey("Value")))
                        return null;

                    switch (setting.Setting["Type"])
                    {
                        case "Int":
                            return new ValueGraph<int>(_connector, int.Parse(setting.Setting["Value"]));
                    }

                    break;
            }

            return null;
        }
    }
}