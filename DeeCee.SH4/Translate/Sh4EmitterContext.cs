using System.Diagnostics;

namespace DeeCee.SH4.Translate;

public class Sh4EmitterContext : EmitterContext
{
    internal Sh4Opcode Op { get; set; } = new Sh4Opcode();

    public enum RegConstants : byte
    {
        R0Bank = 16,
        RnBank = R0Bank + 7,
        PC,
        SR,
        GBR,
        PR,
        SSR,
        SPC,
        VBR,
        SGR,
        DBR,
        MACH,
        MACL,
    };
    
    public Operand GetT()
    {
        var sr = GetReg((byte)RegConstants.SR);
        return And(sr, Constant(1));
    }

    public Operand GetQ()
    {
        throw new NotImplementedException();
    }
    
    public Operand GetM()
    {
        throw new NotImplementedException();
    }

    public void SetT()
    {
        SetSR(Or(GetSR(), Constant(1)));
    }
    
    public void SetS()
    {
        SetSR(Or(GetSR(), Constant(2)));
    }
    
    public void SetQ()
    {
        throw new NotImplementedException();
    }
    
    public void SetM()
    {
        throw new NotImplementedException();
    }
    
    public void ClearT()
    {
        SetSR(And(GetSR(), Constant(~1)));
    }
    
    public void ClearS()
    {
        SetSR(And(GetSR(), Constant(~2)));
    }
    
    public void ClearQ()
    {
        throw new NotImplementedException();
    }
    
    public void ClearM()
    {
        throw new NotImplementedException();
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
    
    
    // --- PC (Program Counter) ---
    public void SetPC(Operand pc)
    {
        SetReg((byte)RegConstants.PC, pc);
    }

    public Operand GetPC()
    {
        return GetReg((byte)RegConstants.PC);
    }

    // --- SR (Status Register) ---
    public void SetSR(Operand sr)
    {
        SetReg((byte)RegConstants.SR, sr);
    }

    public Operand GetSR()
    {
        return GetReg((byte)RegConstants.SR);
    }

    // --- GBR (Global Base Register) ---
    public void SetGBR(Operand gbr)
    {
        SetReg((byte)RegConstants.GBR, gbr);
    }

    public Operand GetGBR()
    {
        return GetReg((byte)RegConstants.GBR);
    }

    // --- PR (Procedure Register) ---
    public void SetPR(Operand pr)
    {
        SetReg((byte)RegConstants.PR, pr);
    }

    public Operand GetPR()
    {
        return GetReg((byte)RegConstants.PR);
    }

    // --- SSR (Saved Status Register) ---
    public void SetSSR(Operand ssr)
    {
        SetReg((byte)RegConstants.SSR, ssr);
    }

    public Operand GetSSR()
    {
        return GetReg((byte)RegConstants.SSR);
    }

    // --- SPC (Saved Program Counter) ---
    public void SetSPC(Operand spc)
    {
        SetReg((byte)RegConstants.SPC, spc);
    }

    public Operand GetSPC()
    {
        return GetReg((byte)RegConstants.SPC);
    }

    // --- VBR (Vector Base Register) ---
    public void SetVBR(Operand vbr)
    {
        SetReg((byte)RegConstants.VBR, vbr);
    }

    public Operand GetVBR()
    {
        return GetReg((byte)RegConstants.VBR);
    }

    // --- SGR (Saved General Register) ---
    public void SetSGR(Operand sgr)
    {
        SetReg((byte)RegConstants.SGR, sgr);
    }

    public Operand GetSGR()
    {
        return GetReg((byte)RegConstants.SGR);
    }

    // --- DBR (Debug Base Register) ---
    public void SetDBR(Operand dbr)
    {
        SetReg((byte)RegConstants.DBR, dbr);
    }

    public Operand GetDBR()
    {
        return GetReg((byte)RegConstants.DBR);
    }

    // --- MACH (Multiply and Accumulate High) ---
    public void SetMACH(Operand mach)
    {
        SetReg((byte)RegConstants.MACH, mach);
    }

    public Operand GetMACH()
    {
        return GetReg((byte)RegConstants.MACH);
    }

    // --- MACL (Multiply and Accumulate Low) ---
    public void SetMACL(Operand macl)
    {
        SetReg((byte)RegConstants.MACL, macl);
    }

    public Operand GetMACL()
    {
        return GetReg((byte)RegConstants.MACL);
    }

    public void SetBankedReg(int i, Operand value)
    {
        Debug.Assert(i < 8, "Invalid register index");
        SetReg((byte)(i + (int)RegConstants.R0Bank), value);
    }
    
    public Operand GetBankedReg(int i)
    {
        Debug.Assert(i < 8, "Invalid register index");
        return GetReg((byte)(i + (int)RegConstants.R0Bank));
    }

    public void NextInstruction()
    {
        SetPC(Add(GetPC(), Constant(2)));
    }
}