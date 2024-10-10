public class Token
{
    public TokenType Type { get; set; }
    public string Value { get; set; }
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