using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;

public class AnalizadorSintactico
{
    private List<Token> tokens;
    private int pos;
    private Token tokenActual;
    private Dictionary<string, int> tablaSimbolos;  // Almacenar variables y sus valores
    private Stack<NodoExpresion> operandos; // Pila para operandos (nodos de expresiones)
    private Stack<Token> operadores; // Pila para operadores
    private Queue<Token> colaTokens; // cola de todos los Tokens
    public int labelCounter = 1;
    public int tempVarCounter = 1;
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
                
                //Console.WriteLine("\nÁrbol Sintáctico de la Expresión:");
                //ImprimirArbol(expresion); // Imprimir el árbol sintáctico

                //int resultado = EvaluarExpresion(expresion);
                //Console.WriteLine($"Resultado de la expresión: {resultado}");

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
                //Console.WriteLine("\nÁrbol Sintáctico de la Expresión:");
                //ImprimirArbol(expresion); // Imprimir el árbol sintáctico

                //int resultado = EvaluarExpresion(expresion);
                //Console.WriteLine($"Resultado de la expresión: {resultado}");

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
            if (tokens.Count - 1 != pos){
                Avanzar();
            }
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
        while (resultadoCondicion == 1 && inWhileloop == true)
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
                    
                    //Console.WriteLine($"Variable {nombreVariable} = {valor} declarada.");
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
        // resetear los tokens
        pos = 0;
        tokenActual = tokens[pos];
        
        // Convertir a notación postfija.
        List<Token> listapostfijo = ConvertirAPostfija();  

        // Listar los tokens en orden postfijo
        // Console.WriteLine("\nTokens en orden postfijo:");
        // foreach (var token in listapostfijo){
        //     Console.WriteLine(token.Value);
        // }   
        // Console.WriteLine("\nFin de los token.");

        // Crear el arbol sintactico con los tokens postfijos
        var arbol = CrearArbol(listapostfijo);

        // Imprimir el arbol binario sintactico
        ImprimirArbol(arbol);

        // Convertir el arbol binario sintactico a codigo intermedio
        var codigoIntermedio = GenerarCodigoIntermedio(arbol);
        
        // Imprimir el codigo intermedio
        Console.WriteLine(Quadruple.ToIntermediateCode(codigoIntermedio));

        // Imprimir la tabla de los cuadruplos
        Console.WriteLine(Quadruple.ToTable(codigoIntermedio));
    }

    public List<Quadruple> GenerarCodigoIntermedio(NodoExpresion root)
    {
        List<Quadruple> codigoIntermedio = new List<Quadruple>(); // Lista de codigo intermedio
        GenerarCodigo(root, codigoIntermedio);
        return codigoIntermedio;
    }

    private string GenerarCodigo(NodoExpresion nodo, List<Quadruple> codigoIntermedio)
    {
        if (nodo == null) return "";

        // Procesar los nodos de la main branch, siempre seran entre 'main' o ';'
        if (nodo.Token.Value == ";" || nodo.Token.Value == "main") {
            GenerarCodigo(nodo.Izquierda, codigoIntermedio);  // Procesar branch izquierdo
            GenerarCodigo(nodo.Derecha, codigoIntermedio);    // Procesar branch derecho
            return "";
        }

        // Asignaciones de variables
        if (nodo.Token.Value == "=") {
        string variableName = nodo.Izquierda.Token.Value;
        
        // Generate right-hand side expression and store result in a temporary variable
        string rightOperand = GenerarCodigo(nodo.Derecha, codigoIntermedio);
        string tempVar = $"t{tempVarCounter++}";

        // Create a Quadruple for the temporary assignment
        codigoIntermedio.Add(new Quadruple("=", rightOperand, null, tempVar));

        // Create a Quadruple for the final assignment to the target variable
        codigoIntermedio.Add(new Quadruple("=", tempVar, null, variableName));
            return variableName;
        }

        // Operadores Aritmeticos
        // solo operadores que utilizan dos operandos, como '+', '-', '*', '/'
        if (nodo.Token.Type == TokenType.Operator) {
        string leftOperand = GenerarCodigo(nodo.Izquierda, codigoIntermedio);
        string rightOperand = GenerarCodigo(nodo.Derecha, codigoIntermedio);
        string tempVar = $"t{tempVarCounter++}";

        // Create a Quadruple for the operation
        codigoIntermedio.Add(new Quadruple(nodo.Token.Value, leftOperand, rightOperand, tempVar));

        return tempVar;
        }

        // Manejar sentencias IF
        if (nodo.Token.Value == "if") {
        string condition = GenerarCodigo(nodo.Izquierda, codigoIntermedio);
        string trueLabel = $"L{labelCounter++}";
        string endLabel = $"L{labelCounter++}";

        // Crear cuadruplo si la condicion es 'if'
        codigoIntermedio.Add(new Quadruple("if", condition, null, trueLabel));

        // generar codigo para 'else' 
        if (nodo.Derecha != null && nodo.Derecha.Token.Value == "else") {
            NodoExpresion elseNode = nodo.Derecha;
            // generar codigo para bloque else
            GenerarCodigo(elseNode.Izquierda, codigoIntermedio);  
            codigoIntermedio.Add(new Quadruple("goto", null, null, endLabel));
            codigoIntermedio.Add(new Quadruple("label", null, null, trueLabel));
            // generar codigo para bloque 'if'
            GenerarCodigo(nodo.Izquierda, codigoIntermedio);      
            codigoIntermedio.Add(new Quadruple("label", null, null, endLabel));
        } else {
            codigoIntermedio.Add(new Quadruple("label", null, null, trueLabel));
            // generar codigo para bloque 'if' 
            GenerarCodigo(nodo.Izquierda, codigoIntermedio);      
            codigoIntermedio.Add(new Quadruple("label", null, null, endLabel));
        }
        return "";
        }

        // Manejar sentencias while
        if (nodo.Token.Value == "while") {
            string startLabel = $"L{labelCounter++}";
            string endLabel = $"L{labelCounter++}";

            // Label al inicio del loop
            codigoIntermedio.Add(new Quadruple("label", null, null, startLabel));

            string condition = GenerarCodigo(nodo.Izquierda, codigoIntermedio);
            
            // Salto condicional al final del loop
            codigoIntermedio.Add(new Quadruple("ifnot", condition, null, endLabel));

            // Codigo para el cuerpo del loop (right child)
            GenerarCodigo(nodo.Derecha, codigoIntermedio);

            // Salto al inicio del loop
            codigoIntermedio.Add(new Quadruple("goto", null, null, startLabel));
            
            // Label al final del loop
            codigoIntermedio.Add(new Quadruple("label", null, null, endLabel));

            return "";
        }

        return nodo.Token.Value;
    }

    public List<Token> ConvertirAPostfija()
    {
        Stack<Token> operadores = new Stack<Token>();  // Pila de operadores
        List<Token> salida = new List<Token>();        // Cola de salida

        while (tokenActual != null)
        {
            if (tokenActual.Type == TokenType.Number || tokenActual.Type == TokenType.Identifier ||
                (tokenActual.Type == TokenType.Keyword && (tokenActual.Value == "if" || tokenActual.Value == "else" 
                || tokenActual.Value == "while")))
            {
                salida.Add(tokenActual);  // Añadir operandos a la salida
                Avanzar();
            }
            else if (tokenActual.Type == TokenType.Operator)
            {
                // Apilar operadores según su precedencia
                while (operadores.Count > 0 &&
                    Precedencia(operadores.Peek()) >= Precedencia(tokenActual) &&
                    operadores.Peek().Value != "(" && operadores.Peek().Value != "{")
                {
                    salida.Add(operadores.Pop());
                }
                operadores.Push(tokenActual);
                Avanzar();
            }
            else if (tokenActual.Value == "(" || tokenActual.Value == "{")
            {
                operadores.Push(tokenActual);  // Apilar paréntesis o llave de apertura
                salida.Add(tokenActual);      // Incluir en la salida
                Avanzar();
            }
            else if (tokenActual.Value == ")" || tokenActual.Value == "}")
            {
                // Desapilar hasta encontrar el paréntesis o llave de apertura
                string apertura = tokenActual.Value == ")" ? "(" : "{";
                while (operadores.Count > 0 && operadores.Peek().Value != apertura)
                {
                    salida.Add(operadores.Pop());
                }
                if (operadores.Count == 0 || operadores.Pop().Value != apertura)
                {
                    Error("Paréntesis o llaves desbalanceados.");
                }
                salida.Add(tokenActual);  // Incluir paréntesis o llave de cierre en la salida
                Avanzar();
            }
            else if (tokenActual.Value == ";")
            {
                // Vaciar la pila de operadores hasta que esté vacío o hasta encontrar un bloque
                while (operadores.Count > 0 && operadores.Peek().Value != "{")
                {
                    salida.Add(operadores.Pop());
                }
                salida.Add(tokenActual);  // Añadir punto y coma a la salida
                Avanzar();
            }
            else {
                Avanzar();
            }
        }

        // Añadir los operadores restantes en la pila a la salida
        while (operadores.Count > 0)
        {
            Token operador = operadores.Pop();
            if (operador.Value == "(" || operador.Value == "{")
            {
                Error("Bloques desbalanceados.");
            }
            salida.Add(operador);
        }

        return salida;
    }

    public NodoExpresion CrearArbol(List<Token> tokens){
        Stack<NodoExpresion> TokenStack = new Stack<NodoExpresion>();
        NodoExpresion MainNode = new NodoExpresion(new Token(TokenType.String, "main"));
        NodoExpresion mainRightBranch = new NodoExpresion(null);
        NodoExpresion ifHead = new NodoExpresion(null);
        NodoExpresion referenceNode = new NodoExpresion(null);
        referenceNode = MainNode;

        // Checar tokens uno por uno
        for (int i = 0; i < tokens.Count; i++){
            var token = tokens[i];  // token == token actual
            
            if (i < tokens.Count - 1){
                var nextToken = tokens[i+1];    // el siguiente token

                if (referenceNode.Izquierda == null){
                    if (token.Type == TokenType.Number || token.Type == TokenType.Identifier){
                        TokenStack.Push(new NodoExpresion(token));  // agregar el token si es un operando
                    }
                    if (token.Type == TokenType.Operator){
                        handleOp(token,TokenStack);
                    }
                    if (token.Value == ";" && referenceNode.Izquierda == null){
                        NodoExpresion nuevaRamaDerecha = new NodoExpresion(token);
                        referenceNode.Derecha = nuevaRamaDerecha;
                        referenceNode.Izquierda = TokenStack.Pop();
                        referenceNode = referenceNode.Derecha;
                    }

                    // Manejar if - else
                    if (token.Value == "if") {
                        NodoExpresion ifNode = new NodoExpresion(token);
                        ifHead = ifNode;
                        mainRightBranch = referenceNode;

                        if (referenceNode.Izquierda == null)
                            referenceNode.Izquierda = ifNode;
                        else
                            referenceNode.Derecha = ifNode;

                        referenceNode = ifNode;

                        // parsear la condicion del if y el cuerpo del if
                        NodoExpresion conditionNode = ParseCondition(tokens, ref i);
                        NodoExpresion ifBodyNode = ParseBody(tokens, ref i);
                        
                        ifNode.Izquierda = conditionNode;

                        // checar si hay un nodo else despues del if
                        if (i < tokens.Count - 1 && tokens[i + 1].Value == "else") {
                            i++;  // avanzar a else
                            var elseToken = tokens[i];
                            
                            // crear un nodo else
                            NodoExpresion elseNode = new NodoExpresion(elseToken);
                            ifNode.Derecha = elseNode;  // agregar else a la derecha de if

                            // agregar el cuerpo del if a la izquierda del else
                            elseNode.Izquierda = ifBodyNode;

                            // parsear el cuerpo de else
                            NodoExpresion elseBodyNode = ParseBody(tokens, ref i);
                            elseNode.Derecha = elseBodyNode;  // agregar el cuerpo del if como derecha del else
                            

                            //referenceNode = elseNode;  // settear else como nodo de referencia
                        } else {
                            // si no se encuentra un else, agregar cuerpo del if a la derecha del if
                            ifNode.Derecha = ifBodyNode;
                        }
                        /// test block
                            referenceNode = mainRightBranch;
                            NodoExpresion nuevaRamaDerecha = new NodoExpresion(new Token(TokenType.Delimiter,";"));
                            referenceNode.Derecha = nuevaRamaDerecha;
                            referenceNode = referenceNode.Derecha; 
                        ///
                    }
                    // Handle 'while' statements
                    if (token.Value == "while") {
                        NodoExpresion whileNode = new NodoExpresion(token);

                        // Attach 'while' node to the current reference node
                        if (referenceNode.Izquierda == null)
                            referenceNode.Izquierda = whileNode;
                        else
                            referenceNode.Derecha = whileNode;

                        referenceNode = whileNode;

                        // Parse condition and body for 'while'
                        NodoExpresion conditionNode = ParseCondition(tokens, ref i);
                        NodoExpresion whileBodyNode = ParseBody(tokens, ref i);
                        
                        // Attach condition and body to 'while' node
                        whileNode.Izquierda = conditionNode;
                        whileNode.Derecha = whileBodyNode;
                    }
                }
            }

        }
        // Metodo para parsear la condicion del if
        static NodoExpresion ParseCondition(List<Token> tokens, ref int i){
            Stack<NodoExpresion> conditionStack = new Stack<NodoExpresion>();

            i++; // avanzar a '('

            if (tokens[i].Value != "(") {
                throw new Exception("Se esperaba '(' despues de 'if'");
            }

            i++; // Parsear tokens dentro de la condicion

            while (i < tokens.Count && tokens[i].Value != ")") {
                var token = tokens[i];

                // Handle operand tokens (identifiers, numbers)
                if (token.Type == TokenType.Identifier || token.Type == TokenType.Number) {
                    conditionStack.Push(new NodoExpresion(token));
                }
                // Handle operator tokens
                else if (token.Type == TokenType.Operator) {
                    handleOp(token, conditionStack);
                }

                i++; // Move to the next token
            }

            // Ensure we reached the end of the condition
            if (i >= tokens.Count || tokens[i].Value != ")") {
                throw new Exception("Expected ')' at the end of condition");
            }

            // Return the top of the stack as the root node of the condition
            return conditionStack.Count > 0 ? conditionStack.Pop() : null;
        }

        static NodoExpresion ParseBody(List<Token> tokens, ref int i) {
            Stack<NodoExpresion> bodyStack = new Stack<NodoExpresion>();

            i++; // Move to the first token after '{'

            while (i < tokens.Count && tokens[i].Value != "}" && tokens[i].Value != "else") {
                var token = tokens[i];

                if (token.Type == TokenType.Identifier || token.Type == TokenType.Number) {
                    bodyStack.Push(new NodoExpresion(token));
                } else if (token.Type == TokenType.Operator) {
                    handleOp(token, bodyStack);
                } else if (token.Value == ";") {
                    NodoExpresion statementNode = bodyStack.Pop();
                    NodoExpresion newStatementBranch = new NodoExpresion(new Token(TokenType.String, ";"));
                    newStatementBranch.Izquierda = statementNode;

                    if (bodyStack.Count == 0) {
                        bodyStack.Push(newStatementBranch);
                    } else {
                        NodoExpresion currentBody = bodyStack.Pop();
                        currentBody.Derecha = newStatementBranch;
                        bodyStack.Push(currentBody);
                    }
                }
                
                i++; // Move to the next token
            }
            if (i >= tokens.Count || tokens[i].Value != "}") {
                throw new Exception("Expected '}' at the end of body");
            }

            // Regresar el bodyStack
            return bodyStack.Count > 0 ? bodyStack.Pop() : null;
        }

        static void handleOp(Token token, Stack<NodoExpresion> TokenStack){
            NodoExpresion derecha = TokenStack.Pop();
            NodoExpresion izquierda = TokenStack.Pop();

            // crear un nuevo nodo y asignar lo a la izquierda del main
            NodoExpresion nuevoNodo = new NodoExpresion(token)
            {
                Izquierda = izquierda,
                Derecha = derecha
            };
            TokenStack.Push(nuevoNodo);
        }
        return MainNode;
    }
}
