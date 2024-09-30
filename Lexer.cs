public class Lexer
{
    private readonly string _input;
    private int _position = 0;
    private string currentDataType = null;
    public Lexer(string input)
    {
        _input = input;
    }
    
    // Checar si la posicion está in-bounds, si no, retornar null
    private char CurrentChar => _position < _input.Length ? _input[_position] : '\0'; 
    private void Advance() => _position++;

    public List<Token> GetTokens()
    {
        List<Token> tokens = new List<Token>();

        while (_position < _input.Length)
        {
            if (char.IsWhiteSpace(CurrentChar)) 
            {
                Advance(); // Si es espacio en blanco, ignorar
            }
            else if (char.IsLetter(CurrentChar))
            {
                var idToken = ReadIdentifier();
                
                // Si el identificador es una palabra reservada y tambien un tipo de dato
                if (idToken.Type == TokenType.Keyword && IsDataType(idToken.Value))
                {
                    currentDataType = idToken.Value; // para guardar el tipo de dato si se encuentra en la lista
                    tokens.Add(idToken);
                }
                // Si es un identificador y encontramos un tipo de dato anteriormente
                else if (idToken.Type == TokenType.Identifier && currentDataType != null)
                {
                    tokens.Add(new Token(TokenType.Identifier, idToken.Value, false, currentDataType));
                    currentDataType = null;
                }
                 else
                {
                    tokens.Add(idToken);
                }
            }
            else if (char.IsDigit(CurrentChar))
            {
                tokens.Add(ReadNumber()); // Si es digito leer todo el numero y agregar como numero
            }
            else
            {
                var token = ReadSymbol(); // Si es un simbolo, checar el tipo de simbolo
                if (token != null)
                {
                    tokens.Add(token);

                    if (token.Value == ";" || token.Value == ",") // si acaba la linea de declaracion, reset el tipo de datos
                    {
                        currentDataType = null;
                    }
                } 
            }
        }
        return tokens;
    }
    
    private bool IsDataType(string word)
    {
        return word == "int" || word == "float" || word == "var"; 
    }
    private Token ReadIdentifier()
    {
        string result = "";
        while (char.IsLetterOrDigit(CurrentChar))
        {
            result += CurrentChar;
            Advance();
        }

        bool isKeyword = IsKeyword(result);
        return new Token(isKeyword ? TokenType.Keyword : TokenType.Identifier, result, isKeyword);
    }

    private Token ReadNumber()
    {
        string result = "";
        while (char.IsDigit(CurrentChar))
        {
            result += CurrentChar;
            Advance();
        }
        return new Token(TokenType.Number, result);
    }

    private Token ReadSymbol()
    {
        char current = CurrentChar;
        Advance();

        switch (current)
        {
            case '+': return new Token(TokenType.Operator, "+");
            case '-': return new Token(TokenType.Operator, "-");
            case '*': return new Token(TokenType.Operator, "*");
            case '/':
            if (CurrentChar == '/')  // para checar comentarios
                {
                    Advance();
                    SkipSingleLineComment(); // Saltar linea si lee "//"
                    return null;
                }
                return new Token(TokenType.Operator, "/");
            case ';': return new Token(TokenType.Delimiter, ";");
            case ',': return new Token(TokenType.Delimiter, ",");
            case '(': return new Token(TokenType.Delimiter, "(");
            case ')': return new Token(TokenType.Delimiter, ")");
            case '{': return new Token(TokenType.Delimiter, "{");
            case '}': return new Token(TokenType.Delimiter, "}");
            case '>':
                if (CurrentChar == '=') // Manejar >=
                {
                    Advance();
                    return new Token(TokenType.Operator, ">=");
                }
                return new Token(TokenType.Operator, ">");
            case '<':
                if (CurrentChar == '=') // Manejar <=
                {
                    Advance();
                    return new Token(TokenType.Operator, "<=");
                }
                return new Token(TokenType.Operator, "<");
            case '=':
                if (CurrentChar == '=') // Manejar ==
                {
                    Advance();
                    return new Token(TokenType.Operator, "==");
                }
                return new Token(TokenType.Operator, "=");
            case '!':
                if (CurrentChar == '=') // Manejar !=
                {
                    Advance();
                    return new Token(TokenType.Operator, "!=");
                }
                throw new Exception($"Unknown symbol: {current}");
            default:
                throw new Exception($"Unknown symbol: {current}");
        }
    }
    private void SkipSingleLineComment()
    {
        // Avanzar hasta que el caracter leido sea el final de la linea
        while (CurrentChar != '\n' && CurrentChar != '\0')
        {
            Advance();
        }
       
    }
    private static readonly HashSet<string> keywords = new HashSet<string>
{
    "if", "else", "while", "for", "return", "int", "float", "var"
};

private bool IsKeyword(string word) => keywords.Contains(word);
}
