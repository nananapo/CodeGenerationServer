namespace GraphConnectEngine.CodeGen;

using System.Text.RegularExpressions;

#nullable disable

class GeneratorSetting
{
    public string Version { get; set; }

    public string Name { get; set; }

    public Dictionary<string, string> Settings { get; set; }

    public Dictionary<string, GraphDefinition> Graphs { get; set; }

    public Dictionary<string, string> Programs { get; set; }


    private bool _loaded = false;

    private string _indent;

    private string _commentFormat;

    private string _variableFormat;

    public string Format(string id, IList<string> inItems, IList<string> outItems, IList<string> args, string before, string after)
    {

        // 見つからない
        if (!Programs.ContainsKey(id))
        {
            return Comment($"{id} is not defined in setting.");
        }

        var keys = new HashSet<string>();
        foreach (Match m in Regex.Matches(Programs[id], @"\{([^\s]*?)\}"))
        {
            var key = m.Groups[1].Value;
            keys.Add(key);
        }

        var list = new List<string>();
        var format = Programs[id];

        for (var i = 0; i < keys.Count; i++)
        {

            var key = keys.ElementAt(i);

            switch (key)
            {
                case "a":
                    list.Add(after);
                    break;
                case "b":
                    list.Add(before);
                    break;
                case "ia":
                    list.Add(AddIndent(after));
                    break;
                case "ib":
                    list.Add(AddIndent(before));
                    break;
                case "i":
                    list.Add(AddIndent(""));
                    break;
                default:
                    var sp = key.Split(':');
                    switch (sp[0])
                    {
                        case "In":
                            list.Add(GetItem(id, sp, inItems));
                            break;
                        case "Out":
                            list.Add(GetItem(id, sp, outItems));
                            break;
                        case "Args":
                            list.Add(GetItem(id, sp, args));
                            break;
                        default:
                            throw new Exception($"Unknown identifier {key} in {id}");
                    }
                    break;
            }

            format = format.Replace("{" + key + "}", "{" + i + "}");
        }

        try
        {
            var formatted = String.Format(format, args: list.ToArray<object>());
            return formatted;
        }catch (Exception)
        {
            return "##ERROR##";
        }
    }

    private void LoadSetting()
    {
        if (_loaded) return;

        _loaded = true;

        _indent = Settings.ContainsKey("Indent") ? Settings["Indent"] : "";
        _commentFormat = Settings.ContainsKey("Comment") ? Settings["Comment"] : "";

        //Variable naming
        _variableFormat = (Settings.ContainsKey("VariableNaming") ? Settings["VariableNaming"] : "{name}_{random}")
                .Replace("{name}", "{0}")
                .Replace("{Name}", "{1}")
                .Replace("{NAME}", "{2}")
                .Replace("{random}", "{3}")
                .Replace("{RANDOM}", "{4}")
                .Replace("{count}", "{5}");
    }

    public string Comment(string str)
    {
        LoadSetting();
        try
        {
            return string.Format(_commentFormat, str);
        }
        catch (Exception)
        {
            return "##ERROR##";
        }
    }

    public string AddIndent(string str)
    {
        LoadSetting();
        return str.Split('\n').Select(s => _indent + s).Join("\n");
    }

    //カウントを受け取ってフォーマット済みStringを返す関数を返す
    public Func<int, string> GetVariableFormat(string name)
    {
        LoadSetting();

        var lower = name.ToLower();
        var camel = name; // TODO 必ずCamelCaseにする
        var upper = name.ToUpper();

        return (int count) =>
        {
            var rand = "abcdefghijklmnopqrstuvwxyz0123456789".Random(5);
            var RAND = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".Random(5);
            try
            {
                return string.Format(_variableFormat, lower, camel, upper, rand, RAND, count);
            }
            catch (Exception)
            {
                return "##ERROR##";
            }
        };
    }

    private string GetItem(string id, IList<string> sp, IList<string> items)
    {
        if (sp.Count == 1)
        {
            return items[0];
        }
        else
        {
            if (int.TryParse(sp[1], out int index))
            {
                if (index < items.Count)
                {
                    return items[index];
                }
                else
                {
                    throw new IndexOutOfRangeException($"{sp[0]} : {sp[1]} is out of range in {id}");
                }
            }
            else
            {
                throw new Exception($"Failed to parse int : {sp[1]} in {id}");
            }
        }
    }

    public AutoGraph CreateGraph(string id,GraphSetting setting)
    {
        if (!Graphs.ContainsKey(setting.Type))
        {
            return null;
        }

        var gen = Graphs[setting.Type];
        return new AutoGraph(setting.Type,id,gen.InItem,gen.OutItem,gen.InProcessNode,gen.OutProcessNodeCount,setting.Args.Values.ToArray());
    }

}
