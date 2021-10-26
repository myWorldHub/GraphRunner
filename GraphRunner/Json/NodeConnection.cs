namespace GraphRunner
{
    public class NodeConnection
    {
        public bool Connect { get; set; }
        public NodeInfo From { get; set; }
        public NodeInfo To { get; set; }
    }

    public class NodeInfo
    {
        public int GraphId { get; set; }
        public int Type { get; set; }
        public int Index { get; set; }

        public override string ToString()
        {
            return GraphId+"."+Type+"."+Index;
        }
    }

    public enum NodeType
    {
        InProcess,
        OutProcess,
        InItem,
        OutItem
    }
}