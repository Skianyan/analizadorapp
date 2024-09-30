public class Token
{
    public TokenType Type { get; }
    public string Value { get; }
    public string DataType { get; }
    public bool IsKeyword{ get; }

    public Token(TokenType type, string value, bool isKeyword = false, string dataType = "")
    {
        Type = type;
        Value = value;
        IsKeyword = isKeyword;
        DataType = dataType;
    }
}