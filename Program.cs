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

            // Procesar todas las instrucciones
            parser.ParsearInstrucciones();

            parser.ImprimirTablaSimbolos(); // imprime los simbolos y sus valores.
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al procesar el archivo: {ex.Message}");
        }
    }
}
