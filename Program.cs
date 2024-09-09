public enum TokenType // Constantes utilizadas
{
    Identifier,
    Keyword,
    Number,
    String,
    Operator,
    Delimiter, 
    Whitespace,
    Comment, 
    EOF 
}
public class Token
{
    public TokenType Type { get; }
    public string Value { get; }

    public Token(TokenType type, string value)
    {
        Type = type;
        Value = value;
    }

    public override string ToString() => $"{Type}: {Value}"; // funcion de impresion de tokens
}

class Program
{
    static void Main(string[] args)
    {
        // Verificar si se ha pasado un argumento
        if (args.Length == 0)
        {
            Console.WriteLine("Error: No se proporcionó un archivo de entrada.");
            return;
        }

        string filePath = args[0];

        // Verificar si el archivo existe
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"Error: El archivo '{filePath}' no existe.");
            return;
        }

        try
        {
            // Leer el contenido del archivo
            string code = File.ReadAllText(filePath);

            // Crear el lexer y generar los tokens
            Lexer lexer = new Lexer(code);
            List<Token> tokens = lexer.GetTokens();

            // Imprimir los tokens
                Console.WriteLine("Type: Value");
                Console.WriteLine("");
            foreach (var token in tokens)
            {
                Console.WriteLine(token);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al leer el archivo: {ex.Message}");
        }
    }
}

public class Lexer
{
    private readonly string _input;
    private int _position = 0;

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
                tokens.Add(ReadIdentifier()); // Si es una letra, checar si es keyword, si no, agregar como identificador
            }
            else if (char.IsDigit(CurrentChar))
            {
                tokens.Add(ReadNumber()); // Si es digito leer todo el numero y agregar como numero
            }
            else
            {
                tokens.Add(ReadSymbol()); // Si es un simbolo, checar el tipo de simbolo
            }
        }

        tokens.Add(new Token(TokenType.EOF, "EOF")); 
        return tokens;
    }

    private Token ReadIdentifier()
    {
        string result = "";
        while (char.IsLetterOrDigit(CurrentChar))
        {
            result += CurrentChar;
            Advance();
        }

        if (IsKeyword(result))
            return new Token(TokenType.Keyword, result);

        return new Token(TokenType.Identifier, result);
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
            case '+':
                return new Token(TokenType.Operator, "+");
            case '-':
                return new Token(TokenType.Operator, "-");
            case '*':
                return new Token(TokenType.Operator, "*");
            case '/':
            if (CurrentChar == '/') 
                {
                    Advance();
                    SkipSingleLineComment(); // Saltar linea si lee "//"
                    return null; // Retornar null, no generar token
                }
                return new Token(TokenType.Operator, "/");
            case ';':
                return new Token(TokenType.Delimiter, ";");
            case '(':
                return new Token(TokenType.Delimiter, "(");
            case ')':
                return new Token(TokenType.Delimiter, ")");
            case '{':
                return new Token(TokenType.Delimiter, "{");
            case '}':
                return new Token(TokenType.Delimiter, "}");
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
    private bool IsKeyword(string word)
    {
        // Palabras reservadas
        string[] keywords = { "if", "else", "while", "for", "return" };
        // Comparar si la palabra de entrada (word) se encuentra dentro de (keywords)
        return Array.Exists(keywords, keyword => keyword == word);
    }
}