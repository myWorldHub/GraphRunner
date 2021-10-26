#nullable enable
using System.Collections.Generic;
using GraphConnectEngine;
using GraphConnectEngine.Nodes;

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

        public bool CreateGraph(GraphSetting setting)
        {
            if (_graphs.ContainsKey(setting.Id)) return false;

            var graph = setting.Create(_connector);

            if (graph != null)
            {
                _graphs[setting.Id] = graph;
                return true;
            }

            return false;
        }

        public bool RemoveGraph(string id)
        {
            //TODO
            return true;
        }

        public bool ConnectNode(NodeConnection setting)
        {
            return true;
        }

        public int Execute(Execution setting)
        {
            //TODO
            return 0;
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
    }
}