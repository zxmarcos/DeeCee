namespace DeeCee.SH4.Interpreter;

public class InterpValue
{
    public enum ValueType
    {
        UInt32,
        Int32,
        Int64,
        UInt64,
        Float,
        Double,
    }
    public ValueType Type { get; set; }
    public object Value { get; set; }

    public bool IsInteger()
    {
        return Type == ValueType.Int32 || Type == ValueType.Int64 || Type == ValueType.UInt32 || Type == ValueType.UInt64;
    }
    
    public bool IsFloat()
    {
        return Type == ValueType.Float || Type == ValueType.Double;
    }
    
    public static InterpValue UInt32(UInt32 value)
    {
        return new InterpValue() { Type = ValueType.UInt32, Value = value };
    }
    
    public static InterpValue UInt64(UInt64 value)
    {
        return new InterpValue() { Type = ValueType.UInt64, Value = value };
    }
    
    public static InterpValue Int32(Int32 value)
    {
        return new InterpValue() { Type = ValueType.Int32, Value = value };
    }
    
    public static InterpValue Int64(Int64 value)
    {
        return new InterpValue() { Type = ValueType.Int64, Value = value };
    }
    
    public static InterpValue Float(float value)
    {
        return new InterpValue() { Type = ValueType.Float, Value = value };
    }
    
    public static InterpValue Double(double value)
    {
        return new InterpValue() { Type = ValueType.Double, Value = value };
    }
}