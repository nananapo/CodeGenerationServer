using GraphConnectEngine.Nodes;

namespace GraphConnectEngine.CodeGen;
public class StringTypeNode : Node
{
    public readonly string Name;

    public readonly bool IsInNode;

    public readonly string OutItemType;

    public readonly IList<string> InItemTypes;

    public StringTypeNode(IGraph graph, string name, string outItemType)
    {
        Graph = graph;
        Name = name;
        IsInNode = false;
        OutItemType = outItemType;
    }

    public StringTypeNode(IGraph graph, string name, IList<string> inItemTypes)
    {
        Graph = graph;
        Name = name;
        IsInNode = true;
        InItemTypes = inItemTypes;
    }

    public override bool CanAttach(INodeConnector connector, INode onode)
    {
        if (onode is StringTypeNode node)
        {
            if (node.IsInNode == IsInNode)
            {
                return false;
            }

            if (IsInNode)
            {
                if (InItemTypes.Contains(node.OutItemType))
                {
                    return !connector.TryGetAnotherNode(this, out var _);
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return node.InItemTypes.Contains(OutItemType);
            }
        }
        return false;
    }

    public override string ToString()
    {
        return $"StringTypeNode<{IsInNode}> {(IsInNode ? InItemTypes.Join(",") : OutItemType)}";
    }
}