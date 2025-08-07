using System.Runtime.CompilerServices;
using DeeCee.SH4.Translate;

namespace DeeCee.SH4;

public class Sh4FrontEnd : Sh4BaseCpu
{
    public Sh4EmitterContext Context { get; } = new Sh4EmitterContext();

    private delegate void OpcodeDecodeHandler();

    private readonly OpcodeDecodeHandler[] _opcodeDecoders = new OpcodeDecodeHandler[16];

    public Sh4FrontEnd()
    {
        _opcodeDecoders[0] = Op0000;
        _opcodeDecoders[1] = Op0001;
        _opcodeDecoders[2] = Op0010;
        _opcodeDecoders[3] = Op0011;
        _opcodeDecoders[4] = Op0100;
        _opcodeDecoders[5] = Op0101;
        _opcodeDecoders[6] = Op0110;
        _opcodeDecoders[7] = Op0111;
        _opcodeDecoders[8] = Op1000;
        _opcodeDecoders[9] = Op1001;
        _opcodeDecoders[10] = Op1010;
        _opcodeDecoders[11] = Op1011;
        _opcodeDecoders[12] = Op1100;
        _opcodeDecoders[13] = Op1101;
        _opcodeDecoders[14] = Op1110;
        _opcodeDecoders[15] = Op1111;
    }
 
    public void Step()
    {
        Compile(FetchOpcode(_state.PC));
        _state.PC += 2;
    }

    public void Compile(UInt16 value)
    {
        var opcode = new Sh4Opcode(value);
        Context.Op = opcode;
        _opcodeDecoders[opcode.Part(3)]();
    }

    public UInt16 FetchOpcode(UInt32 addr)
    {
        return 0;
    }

    public UInt32 ReadMemory32(UInt32 addr)
    {
        return 0;
    }

    private void Op0000() { }
    private void Op0001() { }

    private void Op0010()
    {
        switch (Context.Op.Part(0))
        {
            case 0b1001: BitwiseOps.And(Context);
                return;
            case 0b1011: BitwiseOps.Or(Context);
                return;
            case 0b1010: BitwiseOps.Xor(Context);
                return;
        }
    }

    private void Op0011()
    {
        switch (Context.Op.Part(0))
        {
            // ADD Rm,Rn
            case 0b1100: ArithmeticOps.Add(Context); return;
            // ADDC Rm,Rn
            case 0b1110: ArithmeticOps.AddC(Context); return;
            // ADDV Rm,Rn
            case 0b1111: ArithmeticOps.AddV(Context); return;
            // SUB Rm,Rn
            case 0b1000: ArithmeticOps.Sub(Context); return;
            // SUBC Rm,Rn
            case 0b1010: ArithmeticOps.SubC(Context); return;
        }
    }

    private void Op0100() { }
    private void Op0101() { }

    private void Op0110()
    {
        switch (Context.Op.Part(0))
        {
            case 0b0111: BitwiseOps.Not(Context);
                return;
        }
    }

    private void Op0111()
    {
        ArithmeticOps.AddI(Context);
    }

    private void Op1000() { }
    private void Op1001() { }
    private void Op1010() { }
    private void Op1011() { }

    private void Op1100()
    {
        switch (Context.Op.Part(2))
        {
            case 0b1001: BitwiseOps.AndI(Context);
                return;
            case 0b1101: BitwiseOps.AndB(Context);
                return;
            
            case 0b1011: BitwiseOps.OrI(Context);
                return;
            case 0b1111: BitwiseOps.OrB(Context);
                return;
            
            case 0b1010: BitwiseOps.XorI(Context);
                return;
            case 0b1110: BitwiseOps.XorB(Context);
                return;
        }
    }
    private void Op1101() { }
    private void Op1110() { }
    private void Op1111() { }
    
}