namespace DeeCee.SH4.Interpreter;

public struct InterpValue
{
    public enum ValueType
    {
        UInt32,
        Int32,
        Int64,
        UInt64,
        Float,
        Double
    }

    public ValueType Type;
    private ulong _rawBits; // Armazena bits de inteiro ou ponto flutuante

    public static InterpValue FromUInt32(uint value)
        => new() { Type = ValueType.UInt32, _rawBits = value };

    public static InterpValue FromInt32(int value)
        => new() { Type = ValueType.Int32, _rawBits = unchecked((ulong)value) };

    public static InterpValue FromUInt64(ulong value)
        => new() { Type = ValueType.UInt64, _rawBits = value };

    public static InterpValue FromInt64(long value)
        => new() { Type = ValueType.Int64, _rawBits = unchecked((ulong)value) };

    public static InterpValue FromFloat(float value)
        => new() { Type = ValueType.Float, _rawBits = BitConverter.SingleToUInt32Bits(value) };

    public static InterpValue FromDouble(double value)
        => new() { Type = ValueType.Double, _rawBits = (ulong)BitConverter.DoubleToInt64Bits(value) };

    public uint AsUInt32() => (uint)_rawBits;
    public int AsInt32() => unchecked((int)_rawBits);
    public ulong AsUInt64() => _rawBits;
    public long AsInt64() => unchecked((long)_rawBits);
    public float AsFloat() => BitConverter.UInt32BitsToSingle((uint)_rawBits);
    public double AsDouble() => BitConverter.Int64BitsToDouble((long)_rawBits);

    public bool IsInteger()
        => Type == ValueType.Int32 || Type == ValueType.Int64 ||
           Type == ValueType.UInt32 || Type == ValueType.UInt64;

    public bool IsFloat()
        => Type == ValueType.Float || Type == ValueType.Double;
}