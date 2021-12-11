namespace GraphConnectEngine.CodeGen;

using GraphConnectEngine.Graphs.Event;
using GraphConnectEngine.Nodes;

internal class SequentialCodeGenerator
{

    private GeneratorSetting _setting;

    private IDictionary<string, int> _nameCounts = new Dictionary<string, int>();

    public SequentialCodeGenerator(GeneratorSetting setting)
    {
        _setting = setting;
    }

    /// <summary>
    /// 生成する
    /// </summary>
    /// <param name="updater"></param>
    /// <returns></returns>
    public string Generate(AutoGraph startGraph,INodeConnector connector)
    {
        return Run(startGraph, connector, new Dictionary<int, string>());
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="graph"></param>
    /// <param name="connector"></param>
    /// <param name="variables"></param>
    /// <param name="moveNext"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    string Run(AutoGraph graph, INodeConnector connector, Dictionary<int, string> variables, bool moveNext = true)
    {
        var before = "";
        var after = "";
        var inVariables = new List<string>();
        var outVariables = new List<string>();

        //InNodeの処理
        foreach (var (_,node) in graph.InStringTypeNodes)
        {
            //Outにつながっているか確認する
            if (!connector.TryGetAnotherNode(node, out StringTypeNode another))
            {
                return "";
            }

            //まだ処理されていない(Processがつながっていないであろうノード)
            if (!variables.ContainsKey(another.GetHashCode()))
            {
                //TODO Processチェックすべき
                before += Run((AutoGraph)another.Graph, connector, variables, false);
            }

            inVariables.Add(variables[another.GetHashCode()]);
        }

        //inVariablesの変数を登録する
        for (var i = 0; i < graph.InStringTypeNodes.Count; i++)
        {
            variables[graph.InStringTypeNodes[i].GetHashCode()] = inVariables[i];
        }

        //OutItemNodeの処理
        for (var i = 0; i < graph.OutStringTypeNodes.Count; i++)
        {
            var (_,node) = graph.OutStringTypeNodes[i];
            string varName = CreateVariable(node.Name);

            //登録
            outVariables.Add(varName);
            variables[node.GetHashCode()] = varName;
        }

        //OutProcessNodeの処理
        if (moveNext)
        {
            foreach (var node in graph.OutProcessNodes)
            {
                var others = connector.GetOtherNodes(node);
                if (others.Length != 0)
                {
                    foreach (var another in others)
                    {
                        after += Run((AutoGraph)another.Graph, connector,variables);
                    }
                }
            }
        }

        return _setting.Format(graph.GraphName, inVariables, outVariables, graph.Args, before, after);
    }

    private string CreateVariable(string name)
    {
        var formatter = _setting.GetVariableFormat(name);

        if (!_nameCounts.ContainsKey(name))
        {
            _nameCounts[name] = 0;
        }
        _nameCounts[name]++;

        return formatter(_nameCounts[name]);
    }
}