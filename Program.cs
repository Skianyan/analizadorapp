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
        this.tokenActual = tokens.Count > 0 ? tokens[0] : null; // si hay mas de 0 tokens, inicializar en la posicion [0]
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
            PrintTokens(tokens); // para analizador lexico
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

