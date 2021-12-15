using GraphConnectEngine.Nodes;

namespace GraphConnectEngine.CodeGen;

public class AutoGraph : IGraph
{
    public string Id { get; private set; }

    public string GraphName { get; private set; }

    public IList<InProcessNode> InProcessNodes { get; private set; }

    public IList<OutProcessNode> OutProcessNodes { get; private set; }

    public IList<InItemNode> InItemNodes => throw new NotImplementedException("InItemNodes is disabled on AutoGraph. Please use InStringTypeNodes instead.");

    public IList<OutItemNode> OutItemNodes => throw new NotImplementedException("OutItemNodes is disabled on AutoGraph. Please use OutStringTypeNodes instead.");

    public IList<(string,StringTypeNode)> InStringTypeNodes { get; private set; }

    public IList<(string, StringTypeNode)> OutStringTypeNodes { get; private set; }

    public IList<string> Args { get; private set; }

    public event EventHandler<GraphStatusEventArgs> OnStatusChanged;

    public AutoGraph(string graphType,string graphId,IDictionary<string,string> inItemTypes, IDictionary<string,string> outItemTypes,bool createInProcessNode,int outProcessNodeCounts,IList<string> args)
    {
        Id = graphId;
        GraphName = graphType;

        InProcessNodes = new List<InProcessNode>();
        if (createInProcessNode)
        {
            InProcessNodes.Add(new InProcessNode(this));
        }

        OutProcessNodes = new List<OutProcessNode>();
        for(int i = 0;i < outProcessNodeCounts; i++)
        {
            OutProcessNodes.Add(new OutProcessNode(this));
        }

        InStringTypeNodes = new List<(string, StringTypeNode)>();
        if(inItemTypes != null)
        {
            foreach (var (name,item) in inItemTypes)
            {
                var types = item.Split("|");
                InStringTypeNodes.Add((name,new StringTypeNode(this,name,types)));
            }
        }

        OutStringTypeNodes = new List<(string, StringTypeNode)>();
        if(outItemTypes != null)
        {
            foreach (var (name, item) in outItemTypes)
            {
                OutStringTypeNodes.Add((name, new StringTypeNode(this,name,item)));
            }
        }

        Args = args ?? new List<string>();
    }

    public void Dispose()
    {

    }

    public Task<InvokeResult> Invoke(object sender, ProcessData args)
    {
        throw new NotImplementedException();
    }

    public Task<InvokeResult> InvokeWithoutCheck(ProcessData args, bool callOutProcess, object[] parameters)
    {
        throw new NotImplementedException();
    }

    public Task<ProcessCallResult> OnProcessCall(ProcessData args, object[] parameters)
    {
        throw new NotImplementedException();
    }

    public override string ToString()
    {
        return $"AutoGraph<{Id}> name<{GraphName}>";
    }
}