namespace GraphConnectEngine.CodeGen;

#nullable disable

public class GraphTopologySetting
{

    public Dictionary<string, GraphSetting> Graphs { get; set; }

    public List<string> Connections { get; set; }

    public static bool TryParseConnection(string str, out NodeData node1, out NodeData node2)
    {
        var nodeStrs = str.Split(',');

        if (nodeStrs.Length != 2)
        {
            node1 = null;
            node2 = null;
            return false;
        }

        var infoStrs1 = nodeStrs[0].Split(":");
        var infoStrs2 = nodeStrs[1].Split(":");

        if (infoStrs1.Length != 3 || infoStrs2.Length != 3)
        {
            node1 = null;
            node2 = null;
            return false;
        }

        var graph1 = infoStrs1[0];
        var graph2 = infoStrs2[0];

        if (!int.TryParse(infoStrs1[1], out var typeInt1) ||
            !int.TryParse(infoStrs1[2], out var index1) ||
            !int.TryParse(infoStrs2[1], out var typeInt2) ||
            !int.TryParse(infoStrs2[2], out var index2))
        {
            node1 = null;
            node2 = null;
            return false;
        }

        var type1 = NodeData.IntToNodeType(typeInt1);
        var type2 = NodeData.IntToNodeType(typeInt2);

        if (type1 == NodeType.Undefined || type2 == NodeType.Undefined)
        {
            node1 = null;
            node2 = null;
            return false;
        }

        node1 = new NodeData
        {
            GraphId = graph1,
            NodeType = type1,
            Index = index1
        };
        node2 = new NodeData
        {
            GraphId = graph2,
            NodeType = type2,
            Index = index2
        };

        return true;
    }

}

public class GraphSetting
{
    public string Type { get; set; }
    public Dictionary<string, string> Args { get; set; }
}

public class NodeData
{
    public string GraphId;
    public NodeType NodeType;
    public int Index;

    public static NodeType IntToNodeType(int type)
    {
        switch (type)
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

    public INode ToNode(AutoGraph graph)
    {
        switch (NodeType)
        {
            case NodeType.InProcess:
                return graph.InProcessNodes.Count <= Index ? null : graph.InProcessNodes[Index];
            case NodeType.OutProcess:
                return graph.OutProcessNodes.Count <= Index ? null : graph.OutProcessNodes[Index];
            case NodeType.InItem:
                return graph.InStringTypeNodes.Count <= Index ? null : graph.InStringTypeNodes[Index].Item2;
            case NodeType.OutItem:
                return graph.OutStringTypeNodes.Count <= Index ? null : graph.OutStringTypeNodes[Index].Item2;
        }
        return null;
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