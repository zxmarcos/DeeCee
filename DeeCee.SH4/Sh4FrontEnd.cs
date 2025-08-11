using System.Runtime.CompilerServices;
using DeeCee.SH4.Translate;

namespace DeeCee.SH4;

public class Sh4FrontEnd : Sh4BaseCpu
{
    public Sh4EmitterContext Context { get; } = new();
    
    public void Compile(UInt16 value)
    {
        var opcode = new Sh4Opcode(value);
        Context.Op = opcode;
        Sh4OpcodeTable.GetInstruction(opcode.Value).Emit(Context);
    }

}