namespace GraphConnectEngine.CodeGen;
internal interface IToken
{
    public static IToken operator +(IToken token1, IToken token2)
    {
        return token1;
    }

    public static IToken operator +(IToken token, string str)
    {
        return token;
    }
}