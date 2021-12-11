
using GraphConnectEngine.CodeGen;

internal static class Sample
{
    public static GeneratorSetting GeneratorSetting = new GeneratorSetting
    {
        Version = "0",
        Name = "Python3",
        Settings = new Dictionary<string, string>
                {
                    { "Mode",           "1" },
                    { "Indent",         "  " },
                    { "VariableNaming", "{name}{count}"},
                    { "Comment",        "# {0}\n" },
                    { "ErrorLevel",     "0" }
                },
        Graphs = new Dictionary<string, GraphDefinition>
        {
            {
                "Main",new GraphDefinition
                {
                    InItem = new Dictionary<string, string>(),
                    OutItem = new Dictionary<string, string>(),
                    Args = new List<string>(),
                    OutProcessNodeCount = 1
                }
            },
            {
                "Value<T>",new GraphDefinition
                {
                    InItem = new Dictionary<string, string>(),
                    OutItem = new Dictionary<string, string>
                    {
                        { "Value","int" }
                    },
                    Args = new List<string>
                    {
                        "value"
                    },
                    OutProcessNodeCount=1
                }
            },
            {
                "AdditionOperator<T1,T2,T3>",new GraphDefinition
                {
                    InItem = new Dictionary<string, string>
                    {
                        { "Value1","int" },
                        { "Value2","int" }
                    },
                    OutItem =  new Dictionary<string, string>
                    {
                        { "Result","int" }
                    },
                    Args=new List<string>(),
                    OutProcessNodeCount = 1
                }
            },
            {
                "Print",new GraphDefinition
                {
                    InItem = new Dictionary<string, string>
                    {
                        { "Text" ,"string|int" }
                    },
                    OutItem = new Dictionary<string, string>(),
                    Args=new List<string>(),
                    OutProcessNodeCount=1
                }
            }
        },
        Programs = new Dictionary<string, string>
                {
                    { "Main",            "if __name__ == \"__main__\":\n{ia}" },
                    { "Value<T>",       "{Out:0} = {Args:0}\n{a}" },
                    { "AdditionOperator<T1,T2,T3>",   "{b}{Out:0} = {In:0} + {In:1}\n{a}" },
                    { "Print",          "{b}print({In:0})\n{a}" }
                }
    };

    public static GraphTopologySetting GraphTopologySetting = new GraphTopologySetting
    {
        Graphs = new Dictionary<string, GraphSetting>
        {
            {
                "main",
                new GraphSetting
                {
                    Type = "Main",
                    Args = new Dictionary<string, string>()
                }
            },
            {
                "value1",
                new GraphSetting
                {
                    Type = "Value<T>",
                    Args = new Dictionary<string, string>
                    {
                        { "Value" , "2" }
                    }
                }
            },
            {
                "value2",
                new GraphSetting
                {
                    Type = "Value<T>",
                    Args = new Dictionary<string, string>
                    {
                        { "Value" , "4" }
                    }
                }
            },
            {
                "add",
                new GraphSetting
                {
                    Type = "AdditionOperator<T1,T2,T3>",
                    Args = new Dictionary<string, string>()
                }
            },
            {
                "print",
                new GraphSetting
                {
                    Type = "Print",
                    Args= new Dictionary<string, string>()
                }
            }
        },
        Connections = new List<string>
        {
            "main:1:0,print:0:0",
            "value1:3:0,add:2:0",
            "value2:3:0,add:2:1",
            "add:3:0,print:2:0"
        }
    };
}