using System.Diagnostics;

namespace DeeCee.SH4.Translate;

public class Sh4EmitterContext : EmitterContext
{
    internal Sh4Opcode Op { get; set; } = new Sh4Opcode();

    public enum RegConstants : byte
    {
        R0Bank = 16,
        RnBank = R0Bank + 7,
        // Float registers
        FR0_Bank0,
        FR1_Bank0,
        FR2_Bank0,
        FR3_Bank0,
        FR4_Bank0,
        FR5_Bank0,
        FR6_Bank0,
        FR7_Bank0,
        FR8_Bank0,
        FR9_Bank0,
        FR10_Bank0,
        FR11_Bank0,
        FR12_Bank0,
        FR13_Bank0,
        FR14_Bank0,
        FR15_Bank0,
        FR0_Bank1,
        FR1_Bank1,
        FR2_Bank1,
        FR3_Bank1,
        FR4_Bank1,
        FR5_Bank1,
        FR6_Bank1,
        FR7_Bank1,
        FR8_Bank1,
        FR9_Bank1,
        FR10_Bank1,
        FR11_Bank1,
        FR12_Bank1,
        FR13_Bank1,
        FR14_Bank1,
        FR15_Bank1,

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
        FPSCR
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

    public void SetFReg(byte i, Operand value)
    {
        Debug.Assert(i < 32, "Invalid register index");
        SetReg((byte)(i + (int)RegConstants.FR0_Bank0), value);
    }
    
    public Operand GetFReg(byte i)
    {
        Debug.Assert(i < 32, "Invalid register index");
        return GetReg((byte)(i + (int)RegConstants.FR0_Bank0));
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
    
    
    // --- FPSCR ---
    public void SetFPSCR(Operand value)
    {
        SetReg((byte)RegConstants.FPSCR, And(value, Constant(0x003FFFFFu)));
    }

    public Operand GetFPSCR()
    {
        return GetReg((byte)RegConstants.FPSCR);
    }

    public void NextInstruction()
    {
        SetPC(Add(GetPC(), Constant(2)));
    }
    
}