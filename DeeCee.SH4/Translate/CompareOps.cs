namespace DeeCee.SH4.Translate;

public static class CompareOps
{
    public static void CmpEqI(Sh4EmitterContext ir)
    {
        ir.If(ir.CompareEqual(ir.GetReg(0), ir.Constant(ir.Op.SImm32())), ir.SetT, ir.ClearT);
    }
    
    public static void CmpEq(Sh4EmitterContext ir)
    {
        var mReg = ir.GetReg(ir.Op.M());
        var nReg = ir.GetReg(ir.Op.N());
        
        ir.If(ir.CompareEqual(nReg, mReg), ir.SetT, ir.ClearT);
    }
    
    public static void CmpGe(Sh4EmitterContext ir)
    {
        var mReg = ir.GetReg(ir.Op.M());
        var nReg = ir.GetReg(ir.Op.N());
        
        ir.If(ir.CompareGreaterOrEqualSigned(nReg, mReg), ir.SetT, ir.ClearT);
    }
    
    public static void CmpGt(Sh4EmitterContext ir)
    {
        var mReg = ir.GetReg(ir.Op.M());
        var nReg = ir.GetReg(ir.Op.N());
        
        ir.If(ir.CompareGreaterSigned(nReg, mReg), ir.SetT, ir.ClearT);
    }
    
    public static void CmpHi(Sh4EmitterContext ir)
    {
        var mReg = ir.GetReg(ir.Op.M());
        var nReg = ir.GetReg(ir.Op.N());
        
        ir.If(ir.CompareGreater(nReg, mReg), ir.SetT, ir.ClearT);
    }
    
    public static void CmpHs(Sh4EmitterContext ir)
    {
        var mReg = ir.GetReg(ir.Op.M());
        var nReg = ir.GetReg(ir.Op.N());
        
        ir.If(ir.CompareGreaterOrEqual(nReg, mReg), ir.SetT, ir.ClearT);
    }
    
    public static void CmpPl(Sh4EmitterContext ir)
    {
        var nReg = ir.GetReg(ir.Op.N());
        
        ir.If(ir.CompareGreaterSigned(nReg, ir.Constant(0)), ir.SetT, ir.ClearT);
    }
    
    public static void CmpPz(Sh4EmitterContext ir)
    {
        var nReg = ir.GetReg(ir.Op.N());
        
        ir.If(ir.CompareGreaterOrEqualSigned(nReg, ir.Constant(0)), ir.SetT, ir.ClearT);
    }
    
    public static void CmpStr(Sh4EmitterContext ir)
    {
        var nReg = ir.GetReg(ir.Op.N());
        var mReg = ir.GetReg(ir.Op.M());
        
        var temp = ir.Xor(nReg, mReg);
        var zero = ir.Constant(0);
        var c1 = ir.CompareEqual(ir.And(temp, ir.Constant(0xFF000000)), zero);
        var c2 = ir.CompareEqual(ir.And(temp, ir.Constant(0xFF0000)), zero);
        var c3 = ir.CompareEqual(ir.And(temp, ir.Constant(0xFF00)), zero);
        var c4 = ir.CompareEqual(ir.And(temp, ir.Constant(0xFF)), zero);
        var equal = ir.Or(ir.Or(c1, c2), ir.Or(c3, c4));
        ir.If(ir.CompareEqual(equal, ir.Constant(1)), ir.SetT, ir.ClearT);
    }
}