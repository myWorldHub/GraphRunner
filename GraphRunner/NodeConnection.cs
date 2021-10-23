using System;

namespace GraphRunner
{
    public class NodeConnection
    {
        public bool Connect;
        public Tuple<NodeInfo, NodeInfo> Pair;
    }
    
    public class NodeInfo
    {
        public string GraphId;
        public int Type;
        public int Index;
    }
}