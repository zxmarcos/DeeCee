namespace DeeCee.SH4.Translate;

public static class BitwiseOps
{
    public static void And(Sh4EmitterContext ir)
    {
        var mReg = ir.GetReg(ir.Op.M());
        var nReg = ir.GetReg(ir.Op.N());
        
        var result = ir.And(mReg, nReg);
        ir.SetReg(ir.Op.N(), result);
    }
    
    public static void AndI(Sh4EmitterContext ir)
    {
        var result = ir.And(ir.GetReg(0), ir.Constant((UInt32) ir.Op.Imm8()));
        ir.SetReg(0, result);
    }

    public static void AndB(Sh4EmitterContext ir)
    {
        var ea = ir.Memory(ir.Add(ir.GetGBR(), ir.GetReg(0)), MemoryWidth.Dword);
        var data = ir.Load(ea);
        var result = ir.And(data, ir.Constant((UInt32) ir.Op.Imm8()));
        ir.Store(ea, result);
    }
    
    public static void Or(Sh4EmitterContext ir)
    {
        var mReg = ir.GetReg(ir.Op.M());
        var nReg = ir.GetReg(ir.Op.N());
        
        var result = ir.Or(mReg, nReg);
        ir.SetReg(ir.Op.N(), result);
    }
    
    public static void OrI(Sh4EmitterContext ir)
    {
        var result = ir.Or(ir.GetReg(0), ir.Constant((UInt32) ir.Op.Imm8()));
        ir.SetReg(0, result);
    }
    
    public static void OrB(Sh4EmitterContext ir)
    {
        var ea = ir.Memory(ir.Add(ir.GetGBR(), ir.GetReg(0)));
        var data = ir.Load(ea);
        var result = ir.Or(data, ir.Constant((UInt32) ir.Op.Imm8()));
        ir.Store(ea, result);
    }
    
    public static void Xor(Sh4EmitterContext context)
    {
        var mReg = context.GetReg(context.Op.M());
        var nReg = context.GetReg(context.Op.N());
        
        var result = context.Xor(mReg, nReg);
        context.SetReg(context.Op.N(), result);
    }
    
    public static void XorI(Sh4EmitterContext ir)
    {
        var result = ir.Xor(ir.GetReg(0), ir.Constant((UInt32) ir.Op.Imm8()));
        ir.SetReg(0, result);
    }
    
    public static void XorB(Sh4EmitterContext ir)
    {
        var ea = ir.Memory(ir.Add(ir.GetGBR(), ir.GetReg(0)));
        var data = ir.Load(ea);
        var result = ir.Xor(data, ir.Constant((UInt32) ir.Op.Imm8()));
        ir.Store(ea, result);
    }
    
    public static void Not(Sh4EmitterContext context)
    {
        var mReg = context.GetReg(context.Op.M());
        context.SetReg(context.Op.N(), context.Not(mReg));
    }
}