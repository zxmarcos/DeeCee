namespace DeeCee.SH4.Translate;

public static class ArithmeticOps
{
    public static void Add(Sh4EmitterContext context)
    {
        var mReg = context.GetReg(context.Op.M());
        var nReg = context.GetReg(context.Op.N());
        
        var result = context.Add(mReg, nReg);
        context.SetReg(context.Op.N(), result);
    }
    
    public static void AddI(Sh4EmitterContext context)
    {
        var nReg = context.GetReg(context.Op.N());

        var simm = context.Constant(context.Op.SImm32());
        var result = context.Add(simm, nReg);
        context.SetReg(context.Op.N(), result);
    }
    
    public static void AddC(Sh4EmitterContext ir)
    {
        var mReg = ir.GetReg(ir.Op.M());
        var nReg = ir.GetReg(ir.Op.N());
        
        // tmp0 = R[n]
        var tmp0 = ir.AllocateLocal();
        ir.Copy(nReg, tmp0);
        
        // tmp1 = R[m] + R[n] + T
        var tmp1 = ir.Add(mReg, nReg);
        var result = ir.Add(tmp1, ir.GetT());
        ir.SetReg(ir.Op.N(), result);

        ir.If(ir.CompareGreater(tmp0, tmp1),
            ir.SetT,
            ir.ClearT);
        ir.If(ir.CompareGreater(tmp1, result), ir.SetT);
    }

    public static void AddV(Sh4EmitterContext ir)
    {
        var dest = ir.AllocateLocal();
        var src = ir.AllocateLocal();
        var ans = ir.AllocateLocal();

        var n = ir.Op.N();
        var m = ir.Op.M();
        var zero = ir.Constant(0);
        
        ir.If(ir.CompareGreaterOrEqualSigned(ir.GetReg(n), zero), () => ir.SetZero(dest),
            () => ir.SetOne(dest));
        
        ir.If(ir.CompareGreaterOrEqualSigned(ir.GetReg(m), zero), () => ir.SetZero(src),
            () => ir.SetOne(src));

        src = ir.Add(src, dest);
        // R[n] += R[m]
        ir.SetReg(n, ir.Add(ir.GetReg(n), ir.GetReg(m)));
        
        ir.If(ir.CompareGreaterOrEqualSigned(ir.GetReg(n), zero), () => ir.SetZero(ans),
            () => ir.SetOne(ans));
        
        // ans += dest
        ans = ir.Add(ans, dest);
        // if (src == 0 || src == 2)
        ir.If(ir.Or(ir.CompareEqual(src, zero), ir.CompareEqual(src, ir.Constant(2))), () =>
        {
            // if (ans == 1) T = 1; else T = 0
            ir.If(ir.CompareEqual(ans, ir.Constant(1)), ir.SetT, ir.ClearT);
        }, ir.ClearT);
    }
}