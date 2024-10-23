public class AnalizadorSintactico
{
    private List<Token> tokens;
    private int pos;
    private Token tokenActual;
    private Dictionary<string, int> tablaSimbolos;  // Almacenar variables y sus valores
    private Stack<NodoExpresion> operandos; // Pila para operandos (nodos de expresiones)
    private Stack<Token> operadores; // Pila para operadores
    private Queue<Token> colaTokens; // cola de todos los Tokens

    public int initialPosition = 0;
    public bool inWhileloop = false;

    public AnalizadorSintactico(List<Token> tokens)
    {
        this.tokens = tokens;
        this.pos = 0;
        this.tokenActual = tokens.Count > 0 ? tokens[0] : null;
        this.tablaSimbolos = new Dictionary<string, int>(); // Inicializar tabla de símbolos
        this.operandos = new Stack<NodoExpresion>();
        this.operadores = new Stack<Token>();
        this.colaTokens = new Queue<Token>(); // inicializar cola de tokens
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

// Funciones Helper
    private void Avanzar()
    {
        if (tokenActual != null)
        {
            colaTokens.Enqueue(tokenActual);  // Guardar el token en la pila al ser leído.
        }

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

    private void SaltarBloque()
    {
        int contadorLlaves = 1;
        Avanzar();  // Avanzar al siguiente token
        while (contadorLlaves > 0 && tokenActual != null)
        {
           
            if (tokenActual.Value == "{")
            {
                contadorLlaves++;  // Encontramos otra llave de apertura
            }
            else if (tokenActual.Value == "}")
            {
                contadorLlaves--;  // Encontramos una llave de cierre
            }
             Avanzar();  // Avanzar al siguiente token
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



    // Metodos de Procesamiento
    // Método para procesar múltiples instrucciones

    public void ParsearWhile()
    {
        while(tokenActual != null && inWhileloop == true)
        {
            if (tokenActual.Type == TokenType.Keyword && isNumericType(tokenActual.Value))
            {
                ProcesarDeclaracion(); // Procesamos la declaración de variables
            }
            else if (tokenActual.Type == TokenType.Keyword && tokenActual.Value == "if")
            {
                ProcesarIf();  // Procesamos la sentencia "if"
            }
            else if (tokenActual.Type == TokenType.Keyword && tokenActual.Value == "while")
            {
                ProcesarWhile();  // Procesamos la sentencia "While"
            }
            else
            {
                NodoExpresion expresion = Expresion(); // Procesamos la expresión cuando no es un "if"
                
                Console.WriteLine("\nÁrbol Sintáctico de la Expresión:");
                //ImprimirArbol(expresion); // Imprimir el árbol sintáctico

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
                inWhileloop = false;
            }
        }
    }
    public void ParsearInstrucciones()
    {
        while (tokenActual != null)
        {
            if (tokenActual.Type == TokenType.Keyword && isNumericType(tokenActual.Value))
            {
                ProcesarDeclaracion(); // Procesamos la declaración de variables

            }
            else if (tokenActual.Type == TokenType.Keyword && (tokenActual.Value == "if" || tokenActual.Value == "else"))
            {
                ProcesarIf();  // Procesamos la sentencia "if"

            }
            else if (tokenActual.Type == TokenType.Keyword && tokenActual.Value == "while")
            {
                ProcesarWhile();  // Procesamos la sentencia "While"
                
            }
            else if (inWhileloop == false && tokenActual.Value == "}"){
                break;
            }
            else
            {
                NodoExpresion expresion = Expresion(); // Procesamos la expresión cuando no es un "if"
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

    private void ProcesarIf()
    {
        Avanzar();  // Avanzar para pasar el "if"
        if (tokenActual.Value != "(")
        {
            Error("Se esperaba '(' después de 'if'.");
        }
        Avanzar();  // Avanzar para pasar el paréntesis de apertura

        // Procesar la condición del 'if' (la expresión debe terminar al encontrar ')')
        NodoExpresion condicion = Expresion(true);  // El parámetro 'true' indica que es una condición de 'if'

        if (tokenActual.Value != ")")
        {
            Error("Se esperaba ')' después de la condición.");
        }
        Avanzar();  // Avanzar para pasar el paréntesis de cierre

        // Evaluar la condición
        int resultadoCondicion = EvaluarExpresion(condicion);

        Console.WriteLine($"Condición evaluada: {resultadoCondicion}");

        if (resultadoCondicion == 1 && inWhileloop == true)
        {
            // Si la condición es verdadera, procesamos el bloque "if"
            if (tokenActual.Value == "{")
            {
                Avanzar();  // Avanzar para pasar el corchete de apertura
                ParsearWhile();  // Procesar el bloque de instrucciones
            }
        }
        else if (resultadoCondicion == 1)
        {
            // Si la condición es verdadera, procesamos el bloque "if"
            if (tokenActual.Value == "{")
            {
                Avanzar();  // Avanzar para pasar el corchete de apertura
                ParsearInstrucciones();  // Procesar el bloque de instrucciones
            }
            Avanzar();
            if (tokenActual.Value == "else"){
                SaltarBloque();  // Saltar el bloque "else"
            }
        }
        else
        {
            // Si la condición es falsa, saltar hasta el bloque else
            if (tokenActual.Value == "{")
            {
                SaltarBloque();  // Saltar el bloque "if"
            }
            if (tokenActual.Value == "else")
            {
                Avanzar();  // Avanzar para pasar "else"
                if (tokenActual.Value == "{")
                {
                    Avanzar();  // Avanzar para pasar el corchete de apertura
                    ParsearInstrucciones();  // Procesar el bloque "else"
                }
            }
            if (tokenActual.Value == "}")
            {
                Avanzar();  // Avanzar para pasar el corchete de cierre
            }
            else
            {
                Error("Se esperaba '}' al final del bloque 'else'.");
            }
        }
    }

   private void ProcesarWhile()
    {
        inWhileloop = true;
        Avanzar();  // Avanzamos para pasar la palabra clave "while"
        if (tokenActual.Value != "(")
        {
            Error("Se esperaba '(' después de 'while'.");
        }
        Avanzar();  // Avanzamos para pasar el paréntesis de apertura

        // Procesar la condición del 'while' (la expresión debe terminar al encontrar ')')
        NodoExpresion condicion = Expresion(true);  // El parámetro 'true' indica que es una condición de 'while'

        if (tokenActual.Value != ")")
        {
            Error("Se esperaba ')' después de la condición.");
        }
        Avanzar();  // Avanzamos para pasar el paréntesis de cierre

        // Evaluar la condición inicial
        int resultadoCondicion = EvaluarExpresion(condicion);

        initialPosition = this.pos;
        // Repetir mientras la condición sea verdadera
        while (resultadoCondicion == 1)
        {
            inWhileloop = true;
            this.pos = initialPosition;
            tokenActual = tokens[pos];
            // Procesar el bloque de instrucciones dentro del while
            if (tokenActual.Value == "{")
            {
                Avanzar();  // Avanzamos para pasar el corchete de apertura
                
                // Procesar las instrucciones dentro del ciclo
                ParsearWhile();

                // Después de ejecutar el bloque de instrucciones, reevaluamos la condición
                resultadoCondicion = EvaluarExpresion(condicion);

                // Si la condición es falsa, salimos del ciclo
                if (resultadoCondicion != 1)
                {
                    inWhileloop = false;
                    break;
                }
            }
            else
            {
                Error("Se esperaba '{' al inicio del ciclo 'while'.");
            }
        }
        
        Console.WriteLine($"Condición evaluada: {resultadoCondicion}");
        if (tokenActual.Value != "}" == inWhileloop == true){
            Avanzar();
        }
        inWhileloop = false;
    }


    // Modificar este método para usar la pila (Shunting Yard)
    private NodoExpresion Expresion(bool esCondicionIf = false)
    {
        while (tokenActual != null && (esCondicionIf ? tokenActual.Value != ")" : tokenActual.Value != ";"))
        {
            if (tokenActual.Type == TokenType.Number || tokenActual.Type == TokenType.Identifier)
            {
                operandos.Push(new NodoExpresion(tokenActual)); // Si es número o identificador, lo metemos en la pila de operandos
                Avanzar();
            }
            else if (tokenActual.Type == TokenType.Operator)
            {
                while (operadores.Count > 0 && Precedencia(operadores.Peek()) >= Precedencia(tokenActual) && operadores.Peek().Value != "(")
                {
                    ProcesarOperador(); // Procesamos operadores de mayor o igual precedencia, excepto si hay un paréntesis de apertura
                }
                operadores.Push(tokenActual); // Añadimos el operador a la pila
                Avanzar();
            }
            else if (tokenActual.Value == "(")
            {
                operadores.Push(tokenActual); // Añadimos el paréntesis de apertura
                Avanzar();
            }
            else if (tokenActual.Value == ")")
            {
                if (!esCondicionIf)
                {
                    // Si no estamos en una condición de 'if', el paréntesis de cierre es un error
                    Error("Paréntesis de cierre inesperado.");
                }
                // Si estamos en una condición de 'if', simplemente terminamos el bucle
                break;
            }
            else if (tokenActual.Value == ";")
            {
                // En el caso de una expresión normal, si encontramos un ';', terminamos el bucle
                break;
            }
            else
            {
                Error("Token inesperado en la expresión: " + tokenActual.Value);
            }
        }

        // Procesar cualquier operador restante en la pila
        while (operadores.Count > 0)
        {
            ProcesarOperador();
        }

        // El nodo final en la pila de operandos será la raíz del árbol de la expresión
        if (operandos.Count == 0)
        {
            Error("Expresión inválida: no se encontraron operandos.");
        }

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

    // Método para procesar declaraciones de variables
    public void ProcesarDeclaracion()
    {
        if (tokenActual.Type == TokenType.Keyword && isNumericType(tokenActual.Value))
        {
            Avanzar();
            if (tokenActual.Type == TokenType.Identifier) // Si el token es de tipo identifier
            {
                string nombreVariable = tokenActual.Value;
                Avanzar();

                if (tokenActual.Value == "=")  // Si el token es una asignación
                {
                    Avanzar();
                    NodoExpresion expresion = Expresion();
                    //ImprimirArbol(expresion);
                    // Evaluar la expresión (suponiendo que solo tenemos números, no expresiones)
                    int valor = EvaluarExpresion(expresion);
                    tablaSimbolos[nombreVariable] = valor; // Guardar la variable y su valor en la tabla de símbolos
                    
                    Console.WriteLine($"Variable {nombreVariable} = {valor} declarada.");
                }

                if (tokenActual.Value == ";") // Checar que la declaración acabe con ;
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

        // Si el nodo es un número, lo devolvemos
        if (nodo.Token.Type == TokenType.Number)
        {
            return int.Parse(nodo.Token.Value);
        }

        // Si es una variable, buscamos su valor en la tabla de símbolos
        if (nodo.Token.Type == TokenType.Identifier)
        {
            if (tablaSimbolos.ContainsKey(nodo.Token.Value))
            {
                return tablaSimbolos[nodo.Token.Value]; // Retornamos el valor de la variable
            }
            else
            {
                Error("Variable no declarada: " + nodo.Token.Value);
            }
        }

        // Si es una operación, realizamos la evaluación
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
            case "=":
            if (nodo.Izquierda.Token.Type == TokenType.Identifier)
            {
                string nombreVariable = nodo.Izquierda.Token.Value;
                int nuevoValor = derecha;
                tablaSimbolos[nombreVariable] = nuevoValor;  // Actualizamos el valor de la variable
                return nuevoValor;
            }
            else
            {
                Error("La asignación debe hacerse a una variable.");
            }
            return izquierda = derecha;
            case "<":
                if (izquierda < derecha) return 1; else return 0;
            case ">":
               if (izquierda > derecha) return 1; else return 0;
            case "<=":
                if (izquierda <= derecha) return 1; else return 0;
            case ">=":
               if (izquierda >= derecha) return 1; else return 0;   
            case "==":
               if (izquierda == derecha) return 1; else return 0;                  

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
    // COSAS PARA GENERAR EL ARBOL SINTACTICO
    public void FinalizarAnalisis()
    {
        // reset los tokens
        pos = 0;
        tokenActual = tokens[pos];
        
        List<Token> listapostfijo = ConvertirAPostfija();  // Convertir a notación postfija.
        Console.WriteLine("\nTokens en orden postfijo:");
        foreach (var token in listapostfijo){
            Console.WriteLine(token.Value);
        }   
        //var arbol = ConstruirArbol(listapostfijo);
        Console.WriteLine("\nÁrbol Sintáctico Generado:");
        NodoExpresion Raiz = new NodoExpresion(null){
            Izquierda = null,
            Derecha = null
        };
        foreach (var token in listapostfijo){
            InsertaNodo(token,ref Raiz);
        } 
        
        //ImprimirArbol(arbol);
    }

    public List<Token> ConvertirAPostfija()
    {
        Stack<Token> operadores = new Stack<Token>();  // Pila de operadores
        Stack<Token> bloques = new Stack<Token>();    // Pila de Bloques
        List<Token> salida = new List<Token>();      // Cola de salida

        while (tokenActual != null)
        {
            if (tokenActual.Type == TokenType.Number || tokenActual.Type == TokenType.Identifier || tokenActual.Type == TokenType.Keyword)
            {
                salida.Add(tokenActual);  // Añadir operandos a la salida
                Avanzar();
            }
            else if (tokenActual.Value == "(")
            {
                operadores.Push(tokenActual);  // Apilar paréntesis de apertura
                Avanzar();
            }
            else if (tokenActual.Value == ")")
            {
                // Desapilar hasta encontrar el paréntesis de apertura
                while (operadores.Count > 0 && operadores.Peek().Value != "(")
                {
                    salida.Add(operadores.Pop());
                    Avanzar();
                }
                if (operadores.Count == 0 || operadores.Pop().Value != "(")
                {
                    Error("Paréntesis desbalanceados.");
                }
            }
            else if (tokenActual.Value == "{")
            {
                bloques.Push(tokenActual);  // Apilar paréntesis de apertura
                Avanzar();
            }
            else if (tokenActual.Value == "}")
            {
                // Desapilar hasta encontrar el paréntesis de apertura
                while (operadores.Count > 0 && operadores.Peek().Value != "{")
                {
                    salida.Add(operadores.Pop());
                    Avanzar();
                }
                if (bloques.Count == 0 || bloques.Pop().Value != "{")
                {
                    Error("Llaves desbalanceadas.");
                }
                Avanzar();
            }
            else if (tokenActual.Type == TokenType.Operator)
            {
                // Apilar operadores según su precedencia
                while (operadores.Count > 0 &&
                    Precedencia(operadores.Peek()) >= Precedencia(tokenActual) &&
                    operadores.Peek().Value != "(")
                {
                    salida.Add(operadores.Pop());
                    //Avanzar();
                }
                operadores.Push(tokenActual);
                Avanzar();
            }
            else if (tokenActual.Value == "if")
            {
                operadores.Push(tokenActual);  // Apilar "if" para controlar el flujo
            }
            else if (tokenActual.Value == "else")
            {
                // Mover los tokens del bloque "if" al bloque "else"
                while (operadores.Count > 0 && operadores.Peek().Value != "if")
                {
                    salida.Add(operadores.Pop());
                    Avanzar();
                }
            }
            else if (tokenActual.Value == ";"){
                while (operadores.Count > 0 && operadores.Peek().Value != "{")
                {
                    salida.Add(operadores.Pop());
                    Avanzar();
                }
            }
        }

        while (operadores.Count > 0)
        {
            Token operador = operadores.Pop();
            if (operador.Value == "(" || operador.Value == "{" )
            {
                Error("Bloques desbalanceados.");
            }
            salida.Add(operador);
            Avanzar();
        }

        return salida;
    }

     public void InsertaNodo(Token token, ref NodoExpresion nodo){
          if (nodo == null)
            {
                NodoExpresion newnode = new NodoExpresion(token);
                // CAMBIO 2

                if (Raiz == null)
                    Raiz = Nodo;
            }
            else if (token < Nodo.Dato)
                InsertaNodo(token, ref nodo.Izquierda);
            else if (token > nodo.Value)
                InsertaNodo(token, ref nodo.Derecha);      
    }

    public NodoExpresion ConstruirArbol(List<Token> tokens)
    {
        Stack<NodoExpresion> pila = new Stack<NodoExpresion>();

        foreach (var token in tokens)
        {
            if (token.Type == TokenType.Number || token.Type == TokenType.Identifier)
            {
                // Crear un nodo con el token como raiz
                pila.Push(new NodoExpresion(token));
            }
            else if (token.Type == TokenType.Operator)
            {
                NodoExpresion derecha = pila.Pop();
                NodoExpresion izquierda = pila.Pop();

                // crear un nuevo nodo 
                NodoExpresion nuevoNodo = new NodoExpresion(token)
                {
                    Izquierda = izquierda,
                    Derecha = derecha
                };

                pila.Push(nuevoNodo);
            }
        }

        // regresar la raiz..
        return pila.Pop();
    }

}

/*
    Agregar todos los token a una pila al momento de ser evaluados para crear un arbol al final

    -- crear un stack con todos los token leidos
    -- recorrerlo de manera postfija para generar un arbol sintactico
*/