using System.Runtime.CompilerServices;

namespace DeeCee.SH4;

public struct Sh4Opcode
{
    public readonly UInt16 Value;

    public Sh4Opcode(ushort value)
    {
        this.Value = value;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte Part(byte nibble) => (byte)((Value >> (nibble * 4)) & 0xF);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte N() => (byte)((Value >> 8) & 0xF);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte M() => (byte)((Value >> 4) & 0xF);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Int32 SImm32() => (sbyte)(Value & 0xFF);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte Imm8() => (byte)(Value & 0xFF);
}