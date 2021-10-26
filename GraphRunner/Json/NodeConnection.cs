namespace GraphRunner.Json
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

        public NodeType GetNodeType()
        {
            switch (Type)
            {
                case 0:
                    return NodeType.InProcess;
                case 1:
                    return NodeType.OutProcess;
                case 2:
                    return NodeType.InItem;
                case 3:
                    return NodeType.OutItem;
            }
            return NodeType.Undefined;
        }
    }

    public enum NodeType
    {
        InProcess,
        OutProcess,
        InItem,
        OutItem,
        Undefined
    }
}