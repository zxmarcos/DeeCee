namespace DeeCee.SH4.Translate;

public static class ExtOps
{
    public static void Extsb(Sh4EmitterContext ir)
    {
        var mReg = ir.GetReg(ir.Op.M());
        ir.SetReg(ir.Op.N(), ir.SignExtend8(mReg));
    }
    
    public static void Extsw(Sh4EmitterContext ir)
    {
        var mReg = ir.GetReg(ir.Op.M());
        ir.SetReg(ir.Op.N(), ir.SignExtend16(mReg));
    }
    
    public static void Extub(Sh4EmitterContext ir)
    {
        var mReg = ir.GetReg(ir.Op.M());
        ir.SetReg(ir.Op.N(), ir.ZeroExtend8(mReg));
    }
    
    public static void Extuw(Sh4EmitterContext ir)
    {
        var mReg = ir.GetReg(ir.Op.M());
        ir.SetReg(ir.Op.N(), ir.ZeroExtend16(mReg));
    }
}