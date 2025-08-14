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

    public static void Sub(Sh4EmitterContext context)
    {
        var mReg = context.GetReg(context.Op.M());
        var nReg = context.GetReg(context.Op.N());
        
        var result = context.Sub(nReg, mReg);
        context.SetReg(context.Op.N(), result);
    }
    
    public static void SubC(Sh4EmitterContext ir)
    {
        var mReg = ir.GetReg(ir.Op.M());
        var nReg = ir.GetReg(ir.Op.N());
        
        // tmp0 = R[n]
        var tmp0 = ir.AllocateLocal();
        ir.Copy(nReg, tmp0);
        
        // tmp1 = R[n] - R[m] - T
        var tmp1 = ir.Sub(nReg, mReg);
        var result = ir.Sub(tmp1, ir.GetT());
        ir.SetReg(ir.Op.N(), result);

        ir.If(ir.CompareLesser(tmp0, tmp1),
            ir.SetT,
            ir.ClearT);
        ir.If(ir.CompareLesser(tmp1, result), ir.SetT);
    }
    
    public static void SubV(Sh4EmitterContext ir)
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
        // R[n] -= R[m]
        ir.SetReg(n, ir.Sub(ir.GetReg(n), ir.GetReg(m)));
        
        ir.If(ir.CompareGreaterOrEqualSigned(ir.GetReg(n), zero), () => ir.SetZero(ans),
            () => ir.SetOne(ans));
        
        // ans += dest
        ans = ir.Add(ans, dest);
        // if (src == 1)
        ir.If(ir.CompareEqual(src, ir.Constant(1)), () =>
        {
            // if (ans == 1) T = 1; else T = 0
            ir.If(ir.CompareEqual(ans, ir.Constant(1)), ir.SetT, ir.ClearT);
        }, ir.ClearT);
    }
    
    public static void Neg(Sh4EmitterContext ir)
    {
        var mReg = ir.GetReg(ir.Op.M());
        ir.SetReg(ir.Op.N(), ir.Sub(ir.Constant(0), mReg));
    }
    
    public static void NegC(Sh4EmitterContext ir)
    {
        var mReg = ir.GetReg(ir.Op.M());
        // tmp = 0-R[m]
        var tmp = ir.Sub(ir.Constant(0), mReg);
        // R[n] = temp-T
        ir.SetReg(ir.Op.N(), ir.Sub(tmp, ir.GetT()));
        // if (0<temp) T = 1 else T = 0
        ir.If(ir.CompareGreaterOrEqualSigned(tmp, ir.Constant(0)), ir.SetT, ir.ClearT);
        // if (temp<R[n]) T=1
        ir.If(ir.CompareGreaterOrEqualSigned(ir.GetReg(ir.Op.N()), tmp), ir.SetT);
    }

    public static void ClrMac(Sh4EmitterContext ir)
    {
        ir.SetZero(ir.GetMACH());
        ir.SetZero(ir.GetMACL());
    }
    
    public static void Dt(Sh4EmitterContext ir)
    {
        var n = ir.Op.N();
        var tmp = ir.Sub(ir.GetReg(n), ir.Constant(1));
        ir.SetReg(n, tmp);
        ir.If(ir.IsZero(tmp), ir.SetT, ir.ClearT);
    }
    
    public static void Mulu(Sh4EmitterContext ir)
    {
        var mReg = ir.GetReg(ir.Op.M());
        var nReg = ir.GetReg(ir.Op.N());

        var wordMask = ir.Constant(0xFFFFu);
        var result = ir.Multiply(ir.And(nReg, wordMask), ir.And(mReg, wordMask));
        ir.SetMACL(result);
    }
    
    public static void Muls(Sh4EmitterContext ir)
    {
        var mReg = ir.GetReg(ir.Op.M());
        var nReg = ir.GetReg(ir.Op.N());

        var wordMask = ir.Constant(0xFFFFu);
        var result = ir.MultiplySigned(ir.SignExtend16(ir.And(nReg, wordMask)), ir.SignExtend16(ir.And(mReg, wordMask)));
        ir.SetMACL(result);
    }
    
    public static void Mull(Sh4EmitterContext ir)
    {
        var mReg = ir.GetReg(ir.Op.M());
        var nReg = ir.GetReg(ir.Op.N());
        
        var result = ir.Multiply(nReg, mReg);
        ir.SetMACL(result);
    }
    
    public static void Div0s(Sh4EmitterContext ir)
    {
        var mReg = ir.GetReg(ir.Op.M());
        var nReg = ir.GetReg(ir.Op.N());
        
        ir.If(ir.And(nReg, ir.Constant(0x8000_0000)), ir.ClearQ, ir.SetQ);
        ir.If(ir.And(mReg, ir.Constant(0x8000_0000)), ir.ClearM, ir.SetM);
        ir.If(ir.CompareEqual(ir.GetM(), ir.GetQ()), ir.ClearT, ir.SetT);
    }
    
    public static void Div0u(Sh4EmitterContext ir)
    {
        ir.ClearT();
        ir.ClearM();
        ir.ClearQ();
    }
}