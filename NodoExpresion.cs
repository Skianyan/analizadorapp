public class NodoExpresion // definir el nodo y sus dos hijos.
{
    public Token Token { get; }
    public NodoExpresion Izquierda { get; set; }
    public NodoExpresion Derecha { get; set; }
    public string Identificador { get; set; }

    public NodoExpresion(Token token){ 
        Token = token;
        Izquierda = null;
        Derecha = null;
        Identificador = null;
    }

    public NodoExpresion(string identificador){
        Token = null;
        Izquierda = null;
        Derecha = null;
        Identificador = identificador;
    }
}