namespace DeeCee.SH4;

public class Instruction
{
    public Opcode Opcode { get; }
    public Operand? A {get;}
    public Operand? B {get;}
    public Operand? Destiny { get; }

    public Instruction(Operand? a, Operand? b, Operand? destiny, Opcode opcode)
    {
        A = a;
        B = b;
        Destiny = destiny;
        Opcode = opcode;
    }

    public override string ToString()
    {
        string opA = A?.ToString() ?? "";
        string opB = B?.ToString() ?? "";
        string dst = Destiny?.ToString() ?? "";

        return Opcode switch
        {
            Opcode.COPY         => $"{dst} <- {opA}",
            Opcode.ADD          => $"{dst} <- {opA} + {opB}",
            Opcode.SUB          => $"{dst} <- {opA} - {opB}",
            Opcode.AND          => $"{dst} <- {opA} & {opB}",
            Opcode.OR           => $"{dst} <- {opA} | {opB}",
            Opcode.XOR          => $"{dst} <- {opA} ^ {opB}",
            Opcode.NOT          => $"{dst} <- ~{opA}",
            Opcode.CMP_LT       => $"cmp_lt {opA}, {opB} -> {dst} ",
            Opcode.CMP_GT       => $"cmp_gt {opA}, {opB} -> {dst}",
            Opcode.CMP_GE       => $"cmp_ge {opA}, {opB} -> {dst}",
            Opcode.CMP_GE_SIGN  => $"cmp_ge_sign {opA}, {opB} -> {dst}",
            Opcode.CMP_EQ       => $"cmp_eq {opA}, {opB} -> {dst}",
            Opcode.CMP_NE       => $"cmp_ne {opA}, {opB} -> {dst}",
            Opcode.STORE        => $"store {opA} -> {dst}",
            Opcode.LOAD         => $"load {opA} -> {dst}",
            Opcode.BRANCH       => $"branch {dst}",
            Opcode.BRANCH_TRUE  => $"branch_if_true {opA} -> {dst}",
            Opcode.BRANCH_FALSE => $"branch_if_false {opA} -> {dst}",
            _ => $"[invalid opcode: {Opcode}]"
        };
    }
}