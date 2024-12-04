using System.ComponentModel.DataAnnotations;
using System.ComponentModel.Design;

public enum TokenType
{
    Identifier,
    Keyword,
    Number,
    String,
    Operator,
    Delimiter, 
    Separator,
}
class Program
{
    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Error: No se proporcionó un archivo de entrada.");
            return;
        }

        string filePath = args[0];

        if (!File.Exists(filePath))
        {
            Console.WriteLine($"Error: El archivo '{filePath}' no existe.");
            return;
        }

        try
        {
            string code = File.ReadAllText(filePath);

            Lexer lexer = new Lexer(code);
            List<Token> tokens = lexer.GetTokens();

            AnalizadorSintactico parser = new AnalizadorSintactico(tokens);
            parser.ParsearInstrucciones();
            parser.FinalizarAnalisis();
            parser.ImprimirTablaSimbolos();
            PrintTokens(tokens);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al procesar el archivo: {ex.Message}");
        }
    }

    public static void PrintTokens(List<Token> tokens)
    {
        string header = $"{"Type",-15} {"Value",-20} {"IsKeyword",-10} {"DataType",-10}";
        Console.WriteLine(header);
        Console.WriteLine(new string('-', header.Length));

        foreach (var token in tokens)
        {
            string type = token.Type.ToString();
            string value = token.Value;
            string isKeyword = token.IsKeyword ? "Yes" : "No";
            string dataType = string.IsNullOrEmpty(token.DataType) ? "-" : token.DataType; // Verificación para nulos y vacíos.

            Console.WriteLine($"{type,-15} {value,-20} {isKeyword,-10} {dataType,-10}");
        }
    }
}