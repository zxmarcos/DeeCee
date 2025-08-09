namespace DeeCee.SH4.Translate;

public static class DataOps
{
    public static void Swapb(Sh4EmitterContext ir)
    {
        var mReg = ir.GetReg(ir.Op.M());

        var higher = ir.And(mReg, ir.Constant(0xFFFF0000));
        var upper = ir.ShiftLeft(ir.And(mReg, ir.Constant(0xFF)), ir.Constant(8));
        var lower = ir.ShiftRight(ir.And(mReg, ir.Constant(0xFF00)), ir.Constant(8));
        ir.SetReg(ir.Op.N(), ir.Or(higher, ir.Or(upper, lower)));
    }
    
    public static void Swapw(Sh4EmitterContext ir)
    {
        var mReg = ir.GetReg(ir.Op.M());
        
        var upper = ir.ShiftLeft(ir.And(mReg, ir.Constant(0xFFFF)), ir.Constant(16));
        var lower = ir.ShiftRight(ir.And(mReg, ir.Constant(0xFFFF0000)), ir.Constant(16));
        ir.SetReg(ir.Op.N(), ir.Or(upper, lower));
    }
    
    public static void Xtrct(Sh4EmitterContext ir)
    {
        var mReg = ir.GetReg(ir.Op.M());
        var nReg = ir.GetReg(ir.Op.M());
        
        var upper = ir.ShiftLeft(ir.And(mReg, ir.Constant(0xFFFF)), ir.Constant(16));
        var lower = ir.ShiftRight(ir.And(nReg, ir.Constant(0xFFFF0000)), ir.Constant(16));
        ir.SetReg(ir.Op.N(), ir.Or(upper, lower));
    }
}