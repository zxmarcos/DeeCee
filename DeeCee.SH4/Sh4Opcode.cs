using System.Runtime.CompilerServices;

namespace DeeCee.SH4;

public struct Sh4Opcode
{
    private readonly UInt16 value;

    public Sh4Opcode(ushort value)
    {
        this.value = value;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte Part(byte nibble) => (byte)((value >> (nibble * 4)) & 0xF);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte N() => (byte)((value >> 8) & 0xF);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte M() => (byte)((value >> 4) & 0xF);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Int32 SImm32() => (sbyte)(value & 0xFF);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte Imm8() => (byte)(value & 0xFF);
}