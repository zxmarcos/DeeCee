namespace DeeCee.SH4.Translate;

public class Sh4EmitterContext : EmitterContext
{
    internal Sh4Opcode Op { get; set; } = new Sh4Opcode();

    public enum RegConstants : byte
    {
        PC = 16,
        SR,
        GBR,
        PR
    };
    
    public Operand GetT()
    {
        var sr = GetReg((byte)RegConstants.SR);
        return And(sr, Constant(1));
    }

    public void SetT()
    {
        SetSR(Or(GetSR(), Constant(1)));
    }
    
    public void SetS()
    {
        SetSR(Or(GetSR(), Constant(2)));
    }
    
    public void ClearT()
    {
        SetSR(And(GetSR(), Constant(~1)));
    }
    
    public void ClearS()
    {
        SetSR(And(GetSR(), Constant(~2)));
    }

    public Operand GetSR()
    {
        return GetReg((byte)RegConstants.SR);
    }
    
    public Operand GetGBR()
    {
        return GetReg((byte)RegConstants.GBR);
    }

    public void SetSR(Operand sr)
    {
        SetReg((byte)RegConstants.SR, sr);
    }
    
    public Operand GetPC()
    {
        return GetReg((byte)RegConstants.PC);
    }
    
    public void SetPC(Operand pc)
    {
        SetReg((byte)RegConstants.PC, pc);
    }

    // op = 1
    public void SetOne(Operand op)
    {
        Store(op, Constant(1));
    }
    // op = 0
    public void SetZero(Operand op)
    {
        Store(op, Constant(0));
    }
    
    public void SetPR(Operand pr)
    {
        SetReg((byte)RegConstants.PR, pr);
    }
    
    public Operand GetPR()
    {
        return GetReg((byte)RegConstants.PR);
    }
}