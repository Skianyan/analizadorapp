public class Quadruple {
    public string Operator { get; set; }
    public string Operand1 { get; set; }
    public string Operand2 { get; set; }
    public string Result { get; set; }

    public Quadruple(string op, string operand1, string operand2, string result) {
        Operator = op;
        Operand1 = operand1;
        Operand2 = operand2;
        Result = result;
    }
    public override string ToString() {
        // Crear los strings para los diferentes escenarios
        if (Operator == "=" && Operand2 == null)
            return $"{Result} = {Operand1}";
        else if (Operator == "if")
            return $"if {Operand1} goto {Result}";
        else if (Operator == "ifnot")
            return $"ifnot {Operand1} goto {Result}";
        else if (Operator == "goto")
            return $"goto {Result}";
        else if (Operator == "label")
            return $"{Result}:";
        else
            return $"{Result} = {Operand1} {Operator} {Operand2}";
    }

    public static string ToIntermediateCode(List<Quadruple> quadruples) {
        var code = new System.Text.StringBuilder();

        foreach (var quad in quadruples) {
            code.AppendLine(quad.ToString());
        }

        return code.ToString();
    }

    public string toTableLine() {
        return $"{Operator,-10} | {Operand1,-10} | {Operand2,-10} | {Result,-10}";
    }
    
    public static string ToTable(List<Quadruple> quadruples) {
        var table = new System.Text.StringBuilder();
        table.AppendLine($"{"Operator",-10} | {"Operand1",-10} | {"Operand2",-10} | {"Result",-10}");
        table.AppendLine(new string('-', 45));

        foreach (var quad in quadruples) {
            table.AppendLine(quad.toTableLine());
        }

        return table.ToString();
    }

    public static string ToAssembly(List<Quadruple> quadruples) {
        //para crear un string con todas las lineas de asm
        var assemblyCode = new System.Text.StringBuilder();
        
        foreach (var quad in quadruples) {
            switch (quad.Operator) {
                case "=":
                    assemblyCode.AppendLine($"MOV {quad.Result}, {quad.Operand1}");
                    break;
                case "+":
                    assemblyCode.AppendLine($"MOV EAX, {quad.Operand1}");
                    assemblyCode.AppendLine($"ADD EAX, {quad.Operand2}");
                    assemblyCode.AppendLine($"MOV {quad.Result}, EAX");
                    break;
                case "-":
                    assemblyCode.AppendLine($"MOV EAX, {quad.Operand1}");
                    assemblyCode.AppendLine($"SUB EAX, {quad.Operand2}");
                    assemblyCode.AppendLine($"MOV {quad.Result}, EAX");
                    break;
                case ">":
                    assemblyCode.AppendLine($"CMP {quad.Operand1}, {quad.Operand2}");
                    assemblyCode.AppendLine($"JG {quad.Result}");
                    break;
                case "<":
                    assemblyCode.AppendLine($"CMP {quad.Operand1}, {quad.Operand2}");
                    assemblyCode.AppendLine($"JL {quad.Result}");
                    break;
                case ">=":
                    assemblyCode.AppendLine($"CMP {quad.Operand1}, {quad.Operand2}");
                    assemblyCode.AppendLine($"JGE {quad.Result}");
                    break;
                case "<=":
                    assemblyCode.AppendLine($"CMP {quad.Operand1}, {quad.Operand2}");
                    assemblyCode.AppendLine($"JLE {quad.Result}");
                    break;
                case "if":
                    assemblyCode.AppendLine($"CMP {quad.Operand1}, 0");
                    assemblyCode.AppendLine($"JNE {quad.Result}");
                    break;
                case "ifnot":
                    assemblyCode.AppendLine($"CMP {quad.Operand1}, 0");
                    assemblyCode.AppendLine($"JE {quad.Result}");
                    break;
                case "goto":
                    assemblyCode.AppendLine($"JMP {quad.Result}");
                    break;
                case "label":
                    assemblyCode.AppendLine($"{quad.Result}:");
                    break;
                default:
                    throw new InvalidOperationException($"Operador desconocido: {quad.Operator}");
            }
        }

        return assemblyCode.ToString();
    }


}