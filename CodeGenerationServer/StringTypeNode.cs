using GraphConnectEngine.Nodes;

namespace GraphConnectEngine.CodeGen;
public class StringTypeNode : Node
{
    public readonly string Name;

    public readonly bool IsInNode;

    public readonly string OutItemType;

    public readonly IList<string> InItemTypes;

    public StringTypeNode(string name, string outItemType)
    {
        Name = name;
        IsInNode = false;
        OutItemType = outItemType;
    }

    public StringTypeNode(string name, IList<string> inItemTypes)
    {
        Name = name;
        IsInNode = true;
        InItemTypes = inItemTypes;
    }

    public override bool CanAttach(INodeConnector connector, INode onode)
    {
        if(onode is StringTypeNode node)
        {
            if(node.IsInNode == IsInNode)
            {
                return false;
            }

            if (IsInNode)
            {
                return InItemTypes.Contains(node.OutItemType);
            }
            else
            {
                return node.InItemTypes.Contains(OutItemType);
            }
        }
        return false;
    }
}