# Clase para representar los nodos del árbol de expresiones
class NodoExpresion:
    def __init__(self, valor):
        self.valor = valor
        self.izquierda = None
        self.derecha = None

# Analizador Sintáctico
class AnalizadorSintactico:
    def __init__(self, tokens):
        self.tokens = tokens
        self.pos = 0
        self.token_actual = self.tokens[self.pos]
    
    def avanzar(self):
        self.pos += 1
        if self.pos < len(self.tokens):
            self.token_actual = self.tokens[self.pos]
        else:
            self.token_actual = None

    def error(self, mensaje):
        raise Exception("Error de sintaxis: " + mensaje)

    # Método principal para parsear las expresiones
    def parsear(self):
        return self.expresion()

    # Manejo de la precedencia de operadores
    def expresion(self):
        # Primer nivel de precedencia: suma y resta
        nodo = self.termino()
        
        while self.token_actual in ['+', '-']:
            operador = self.token_actual
            self.avanzar()
            nodo_derecho = self.termino()
            nuevo_nodo = NodoExpresion(operador)
            nuevo_nodo.izquierda = nodo
            nuevo_nodo.derecha = nodo_derecho

            print("Nueva expresion: ")
            print("       ", nodo_derecho.valor)
            print("     /")
            print(operador,)
            print("     \\")
            print("       ", nodo.valor)

            nodo = nuevo_nodo
        
        return nodo

    def termino(self):
        # Segundo nivel de precedencia: multiplicación y división
        nodo = self.factor()
        
        while self.token_actual in ['*', '/']:
            operador = self.token_actual
            self.avanzar()
            nodo_derecho = self.factor()
            nuevo_nodo = NodoExpresion(operador)
            nuevo_nodo.izquierda = nodo
            nuevo_nodo.derecha = nodo_derecho
            print("Nuevo termino: ")
            print("       ", nodo_derecho.valor)
            print("     /")
            print(operador,)
            print("     \\")
            print("       ", nodo.valor)
            nodo = nuevo_nodo
        
        return nodo

    def factor(self):
        # Manejo de números o expresiones entre paréntesis
        token = self.token_actual
        
        if token.isdigit():
            self.avanzar()
            return NodoExpresion(token)
        
        elif token == '(':
            self.avanzar()
            nodo = self.expresion()
            if self.token_actual == ')':
                self.avanzar()
                return nodo
            else:
                self.error("Se esperaba ')'")
        else:
            self.error("Token inesperado")

# Función para mostrar el árbol sintáctico de manera legible
def imprimir_arbol(nodo, nivel=0,lado=0):
    if nodo:
        #if (lado == 1): #Izquierda
        #    print('   ' * nivel + "\\")
        imprimir_arbol(nodo.derecha, nivel + 1, 2)
        print('    ' * nivel + str(nodo.valor))


        imprimir_arbol(nodo.izquierda, nivel + 1, 1)
        #if (lado == 2): #derecha
        #    print('   ' * nivel + "/")

# Función principal para ejecutar el parser
def main():
    expresion = "3 + 5 * ( 10 - 2 ) / 4"
    #expresion = "3 + 5 * 2 "
    tokens = expresion.split()  # Tokenización básica

    parser = AnalizadorSintactico(tokens)
    print("Tokens: ", tokens)

    arbol = parser.parsear()
    
    # Mostrar el árbol sintáctico
    print("Árbol Sintáctico:")
    imprimir_arbol(arbol)

if __name__ == "__main__":
    main()