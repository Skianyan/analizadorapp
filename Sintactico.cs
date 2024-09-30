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
    
    private void Error(string mensaje)
    {
        throw new Exception("Error de tipo:" + mensaje);
    }

    private NodoExpresion Parsear()
    {
        return Expresion();
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
            // desplegar el nodo creado
            Console.WriteLine("Nuevo termino: ");
            Console.WriteLine("       " + nodoDerecho.Token);
            Console.WriteLine("     /");
            Console.WriteLine(operador);
            Console.WriteLine("     \\");
            Console.WriteLine("       " + nodo.Token);
            
            nodo = nuevoNodo;
        }
        return nodo;
    }

    private NodoExpresion Termino()
    {   
        NodoExpresion nodo = Factor();

        return nodo;
    }

    private NodoExpresion Factor()
}