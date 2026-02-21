namespace DeeCee.SH4.Translate;

public static class FloatOps
{
    // LDS Rm, FPSCR
    public static void LdsFpscr(Sh4EmitterContext ir)
    {
        ir.SetFPSCR(ir.GetReg(ir.Op.N()));
    }
    
    // FMOV FRm, FRn
    public static void Fmov(Sh4EmitterContext ir)
    {
        ir.SetFReg(ir.Op.N(), ir.GetFReg(ir.Op.M()));
    }
}