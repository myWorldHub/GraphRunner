using System;

namespace GraphRunner
{
    public class NodeConnection
    {
        public bool Connect { get; set; }
        public Tuple<NodeInfo, NodeInfo> Pair { get; set; }
    }

    public class NodeInfo
    {
        public string GraphId { get; set; }
        public int Type { get; set; }
        public int Index { get; set; }
    }
}