public class NodoExpresion
{
    public Token Token { get; }
    public NodoExpresion Izquierda { get; set; }
    public NodoExpresion Derecha { get; set; }

    public NodoExpresion(Token token)
    { 
        Token = token;
        Izquierda = null;
        Derecha = null;
    }
}