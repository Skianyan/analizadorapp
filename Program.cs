using System.ComponentModel.DataAnnotations;
using System.ComponentModel.Design;

public enum TokenType // Constantes utilizadas
{
    Identifier,
    Keyword,
    Number,
    String,
    Operator,
    Delimiter, 

}
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
public class NodoExpresion // definir el nodo y sus dos hijos.
{
    public Token Token { get; }
    public NodoExpresion Izquierda { get; set; }
    public NodoExpresion Derecha { get; set; }

    public NodoExpresion(Token token){ 
        Token = token;
        Izquierda = null;
        Derecha = null;
    }
}

public class AnalizadorSintactico
{
    private List<Token> tokens;
    private int pos;
    private Token tokenActual;

    public AnalizadorSintactico(List<Token> tokens)
    {
        this.tokens = tokens;
        this.pos = 0;
        this.tokenActual = tokens.Count > 0 ? tokens[0] : null;
    }

    private void Avanzar()
    {
        pos++;
        if (pos < tokens.Count)
        {
            tokenActual = tokens[pos];
        }
        else
        {
            tokenActual = null;
        }
    }

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
            //PrintTokens(tokens); // para analizador lexico
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al leer el archivo: {ex.Message}");
        }
    }

    private static void PrintTokens(List<Token> tokens)
    {
        // Header
        string header = $"{"Type",-15} {"Value",-20} {"IsKeyword",-10} {"DataType",-10}";
        Console.WriteLine(header);
        Console.WriteLine(new string('-',header.Length));

        // Tabla
        foreach(var token in tokens){
            string type = token.Type.ToString();
            string value = token.Value;
            string isKeyword = token.IsKeyword ? "Yes" : "No";
            string dataType = token.DataType ?? "-";  // Checar si es null el tipo de dato, usar "-" si es null. (no funciona?) 

            Console.WriteLine($"{type,-15} {value,-20} {isKeyword,-10} {dataType,-10}");
        }
    }
}

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
