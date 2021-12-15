#pragma warning disable CS8618
/// <summary>
///
// TODO 名前で型ヒントを与えたい
/// </summary>
internal class GraphDefinition
{
    // TODO デフォルト値の設定
    // TODO 名前で代入させたい
    /// <summary>
    /// Inは|で複数の型を指定できる
    /// デフォルトはノード無し
    /// 順序を保証する
    /// </summary>
    public Dictionary<string,string> InItem { get; set; }

    /// <summary>
    /// 型を指定する
    /// デフォルトはノード無し
    /// </summary>
    public Dictionary<string,string> OutItem { get; set; }

    /// <summary>
    /// InProcessを持つかどうか
    /// </summary>
    public bool InProcessNode { get; set; }

    /// <summary>
    /// OutProcessNodeの数を指定する
    /// デフォルト値は1
    /// </summary>
    public int OutProcessNodeCount { get; set; }

    // TODO 名前で代入させたい
    /// <summary>
    /// 引数として値を受け取る時の名前を指定する
    /// デフォルトは引数なし
    /// </summary>
    public List<string> Args { get; set; }

}