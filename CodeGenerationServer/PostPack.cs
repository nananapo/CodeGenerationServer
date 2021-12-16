namespace GraphConnectEngine.CodeGen;

internal class PostPack
{
    public GeneratorSetting GeneratorSetting { get; set; }

    public GraphTopologySetting GraphTopologySetting { get; set; }

    public string StartGraphId { get; set; }
}

internal class ResultPack
{
    public string GeneratedCode { get; set; }

    public string SyntaxHighlight { get; set; }
}