public class AnalizadorSintactico
{
    private List<Token> tokens;
    private int pos;
    private Token tokenActual;
    private Dictionary<string, int> tablaSimbolos;  // Almacenar variables y sus valores

    public AnalizadorSintactico(List<Token> tokens)
    {
        this.tokens = tokens;
        this.pos = 0;
        this.tokenActual = tokens.Count > 0 ? tokens[0] : null;
        this.tablaSimbolos = new Dictionary<string, int>(); // Inicializar tabla de símbolos
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
    
    private void Error(string mensaje)
    {
        throw new Exception("Error de tipo: " + mensaje);
    }

    public bool isNumericType(string value){
        string[] numericTypes = {"var", "int", "float", "numeric"};
        bool exists = Array.Exists(numericTypes, type => type.Equals(value, StringComparison.OrdinalIgnoreCase));
        return exists;
    }

    // Método para procesar múltiples instrucciones
    public void ParsearInstrucciones()
    {
        while (tokenActual != null)
        {
            if (tokenActual.Type == TokenType.Keyword && isNumericType(tokenActual.Value))
            {
                ProcesarDeclaracion(); // Procesamos la declaración de variable
            }
            else
            {
                NodoExpresion expresion = Expresion(); // Procesamos la expresión
                Console.WriteLine("\nÁrbol Sintáctico de la Expresión:");
                ImprimirArbol(expresion); // Imprimir el árbol sintáctico

                int resultado = EvaluarExpresion(expresion);
                Console.WriteLine($"Resultado de la expresión: {resultado}");

                if (tokenActual != null && tokenActual.Value == ";")
                {
                    Avanzar(); // Avanzamos sobre el ';'
                }
                else
                {
                    Error("Se esperaba ';' al final de la expresión.");
                }
            }
        }
    }

    private NodoExpresion Expresion()
    {
        NodoExpresion nodo = Termino();

        while (tokenActual != null && (tokenActual.Value == "+" || tokenActual.Value == "-"))
        {
            Token operador = tokenActual;
            Avanzar();
            NodoExpresion nodoDerecho = Termino();
            NodoExpresion nuevoNodo = new NodoExpresion(operador)
            {
                Izquierda = nodo,
                Derecha = nodoDerecho
            };
            nodo = nuevoNodo;
        }
        return nodo;
    }

    private NodoExpresion Termino()
    {   
        NodoExpresion nodo = Factor();

        while (tokenActual != null && (tokenActual.Value == "*" || tokenActual.Value == "/"))
        {
            Token operador = tokenActual;
            Avanzar();
            NodoExpresion nodoDerecho = Factor();
            NodoExpresion nuevoNodo = new NodoExpresion(operador)
            {
                Izquierda = nodo,
                Derecha = nodoDerecho
            };
            nodo = nuevoNodo;
        }

        return nodo;
    }

    private NodoExpresion Factor()
    {
        Token token = tokenActual; //definimos tokenActual como el token en el que estamos

        if (token.Type == TokenType.Number) // si es de tipo numero, regresarlo como esta
        {
            Avanzar();
            return new NodoExpresion(token);
        }
        else if (token.Type == TokenType.Identifier) // si es de tipo identificador
        {
            Avanzar();
            //return new NodoExpresion(token); // retornar el nombre de la variable

            if (tablaSimbolos.ContainsKey(token.Value)) // si el valor se encuentra dentro de la tabla de simbolos, regresar su valor
            {
                return new NodoExpresion(new Token(TokenType.Number, tablaSimbolos[token.Value].ToString()));
            }
            else
            {
                Error("Variable no declarada: " + token.Value);
            }
        }
        else if (token.Value == "(")
        {
            Avanzar();
            NodoExpresion nodo = Expresion();
            if (tokenActual != null && tokenActual.Value == ")")
            {
                Avanzar();
                return nodo;
            }
            else
            {
                Error("Se esperaba ')'");
            }
        }

        Error("Token inesperado");
        return null;
    }

    // metodo para procesar declaraciones de variables
    public void ProcesarDeclaracion()
    {
        if (tokenActual.Type == TokenType.Keyword && isNumericType(tokenActual.Value))
        {
            Avanzar(); 
            if (tokenActual.Type == TokenType.Identifier) // si el token es de tipo identifier
            {
                string nombreVariable = tokenActual.Value;
                Avanzar();

                if (tokenActual.Value == "=")  // si el token es una asignacion
                {
                    Avanzar(); 
                    NodoExpresion expresion = Expresion();

                    // evaluar la expresión (suponiendo que solo tenemos números, no expresiones)
                    int valor = EvaluarExpresion(expresion);
                    tablaSimbolos[nombreVariable] = valor; // guardar la variable y su valor en la tabla de símbolos

                    Console.WriteLine($"Variable {nombreVariable} = {valor} declarada.");
                }

                if (tokenActual.Value == ";") // checar que la declaracion acabe con ;
                {
                    Avanzar();
                }
                else
                {
                    Error("Se esperaba ';' al final de la declaración");
                }
            }
            else
            {
                Error("Se esperaba un identificador de variable");
            }
        }
        else
        {
            Error("Tipo de declaración no soportada");
        }
    }

    // metodo para evaluar una expresión y devolver su valor (no se ocupa?)
    private int EvaluarExpresion(NodoExpresion nodo)
    {
        if (nodo == null)
        {
            return 0;
        }

        // si el nodo es un número, lo devolvemos
        if (nodo.Token.Type == TokenType.Number)
        {
            return int.Parse(nodo.Token.Value);
        }

        // si es una operación, realizamos la evaluacion
        int izquierda = EvaluarExpresion(nodo.Izquierda);
        int derecha = EvaluarExpresion(nodo.Derecha);

        switch (nodo.Token.Value)
        {
            case "+":
                return izquierda + derecha;
            case "-":
                return izquierda - derecha;
            case "*":
                return izquierda * derecha;
            case "/":
                return izquierda / derecha;
            default:
                Error("Operador desconocido: " + nodo.Token.Value);
                return 0;
        }
    }

    // imprimir el arbol sintáctico
    public static void ImprimirArbol(NodoExpresion nodo, int nivel = 0)
    {
        if (nodo != null)
        {
            ImprimirArbol(nodo.Derecha, nivel + 1);
            Console.WriteLine(new string(' ', nivel * 4) + nodo.Token.Value);
            ImprimirArbol(nodo.Izquierda, nivel + 1);
        }
    }
    public void ImprimirTablaSimbolos()
    {
        Console.WriteLine("\nTabla de Símbolos:");
        Console.WriteLine($"{"Variable",-10} {"Valor",-10}");
        Console.WriteLine(new string('-', 20));
        foreach (var simbolo in tablaSimbolos)
        {
            Console.WriteLine($"{simbolo.Key,-10} {simbolo.Value,-10}");
        }
    }

}
