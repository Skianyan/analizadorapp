public class AnalizadorSintactico
{
    private List<Token> tokens;
    private int pos;
    private Token tokenActual;
    private Dictionary<string, int> tablaSimbolos;  // Almacenar variables y sus valores
    private Stack<NodoExpresion> operandos; // Pila para operandos (nodos de expresiones)
    private Stack<Token> operadores; // Pila para operadores

    public AnalizadorSintactico(List<Token> tokens)
    {
        this.tokens = tokens;
        this.pos = 0;
        this.tokenActual = tokens.Count > 0 ? tokens[0] : null;
        this.tablaSimbolos = new Dictionary<string, int>(); // Inicializar tabla de símbolos
        this.operandos = new Stack<NodoExpresion>();
        this.operadores = new Stack<Token>();
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

    public bool isNumericType(string value)
    {
        string[] numericTypes = { "var", "int", "float", "numeric" };
        return Array.Exists(numericTypes, type => type.Equals(value, StringComparison.OrdinalIgnoreCase));
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

    // Modificar este método para usar la pila (Shunting Yard Algorithm)
    private NodoExpresion Expresion()
    {
        while (tokenActual != null && tokenActual.Value != ";")
        {
            if (tokenActual.Type == TokenType.Number || tokenActual.Type == TokenType.Identifier)
            {
                operandos.Push(new NodoExpresion(tokenActual)); // Si es número o identificador, lo metemos en la pila de operandos
                Avanzar();
            }
            else if (tokenActual.Type == TokenType.Operator)
            {
                while (operadores.Count > 0 && Precedencia(operadores.Peek()) >= Precedencia(tokenActual))
                {
                    ProcesarOperador(); // Procesamos los operadores de mayor o igual precedencia
                }
                operadores.Push(tokenActual); // Añadimos el operador a la pila
                Avanzar();
            }
            else if (tokenActual.Value == "(")
            {
                operadores.Push(tokenActual); // Añadimos el paréntesis izquierdo
                Avanzar();
            }
            else if (tokenActual.Value == ")")
            {
                while (operadores.Peek().Value != "(")
                {
                    ProcesarOperador(); // Procesar todos los operadores hasta el paréntesis izquierdo
                }
                operadores.Pop(); // Quitamos el paréntesis izquierdo
                Avanzar();
            }
        }

        // Procesar cualquier operador restante en la pila
        while (operadores.Count > 0)
        {
            ProcesarOperador();
        }

        // El nodo final en la pila de operandos será la raíz del árbol de la expresión
        return operandos.Pop();
    }

    private void ProcesarOperador()
    {
        Token operador = operadores.Pop();
        NodoExpresion derecha = operandos.Pop();
        NodoExpresion izquierda = operandos.Pop();

        NodoExpresion nuevoNodo = new NodoExpresion(operador)
        {
            Izquierda = izquierda,
            Derecha = derecha
        };

        operandos.Push(nuevoNodo);
    }

    private int Precedencia(Token operador)
    {
        switch (operador.Value)
        {
            case "+":
            case "-":
                return 1;
            case "*":
            case "/":
                return 2;
            default:
                return 0;
        }
    }

    // método para procesar declaraciones de variables
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

        // si el nodo es un identificador (variable), buscamos su valor en la tabla de símbolos
        if (nodo.Token.Type == TokenType.Identifier)
        {
            if (tablaSimbolos.ContainsKey(nodo.Token.Value))
            {
                return tablaSimbolos[nodo.Token.Value];
            }
            else
            {
                Error($"Variable no declarada: {nodo.Token.Value}");
            }
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

    public static void ImprimirArbol(NodoExpresion nodo, int nivel = 0)
    {
        if (nodo != null)
        {
            ImprimirArbol(nodo.Derecha, nivel + 1);
            Console.WriteLine(new string(' ', nivel * 4) + nodo.Token.Value);
            ImprimirArbol(nodo.Izquierda, nivel + 1);
        }
    }
}
