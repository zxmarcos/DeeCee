namespace DeeCee.SH4;

public class Operand
{
    public OperandKind Kind { get; set; }
    public byte RegNum { get; set; }
    public RegisterType RegType { get; set; }
    
    public ConstantType ConstantType { get; set; }
    public UInt32 UConst32 { get; set; }
    public float ConstFloat { get; set; }
    public double ConstDouble { get; set; }
    public int BlockOffset { get; set; }
    public int VarIndex { get; set; } = -1;

    public Operand? Address { get; set; } = null;
    public MemoryWidth? MemoryWidth { get; set; } = null;

    public Operand()
    {
        Kind = OperandKind.None;
    }

    public Operand(OperandKind kind, byte regNum, RegisterType regType)
    {
        Kind = kind;
        RegNum = regNum;
        RegType = regType;
    }

    public Operand(OperandKind kind)
    {
        Kind = kind;
    }

    public Operand(ConstantType constantType)
    {
        ConstantType = constantType;
        Kind = OperandKind.Constant;
    }

    public static Operand Register(byte regNum, RegisterType regType)
    {
        return new Operand(OperandKind.Register, regNum, regType);
    }
    
    public static Operand LocalVariable(RegisterType regType)
    {
        return new Operand(OperandKind.LocalVariable, 0, regType);
    }
    
    public override string ToString()
    {
        return Kind switch
        {
            OperandKind.None => "None",
        
            OperandKind.Register => $"R{RegNum}",

            OperandKind.Constant => ConstantType switch
            {
                ConstantType.Int32 => $"#{UConst32}",
                ConstantType.Float => $"#{ConstFloat}f",
                ConstantType.Double => $"#{ConstDouble}d",
                _ => $"Const({ConstantType})"
            },

            OperandKind.LocalVariable => VarIndex >= 0
                ? $"v{VarIndex}{(RegType != RegisterType.Int32 ? RegType.ToString().ToLower() : "")}"
                : $"v@{RegType.ToString().ToLower()}",

            OperandKind.Label => $"@{BlockOffset}",
            
            OperandKind.Memory => $"({Address})",

            _ => $"Operand({Kind})"
        };
    }
}