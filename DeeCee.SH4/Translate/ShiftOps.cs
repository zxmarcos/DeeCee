namespace DeeCee.SH4.Translate;

public static class ShiftOps
{
    public static void Rotcl(Sh4EmitterContext ir)
    {
        var tBit = ir.AllocateLocal();
        var nReg = ir.GetReg(ir.Op.N());
        
        ir.Copy(ir.GetT(), tBit);
        ir.If(ir.And(nReg, ir.Constant(0x80000000)), ir.SetT, ir.ClearT);

        var result = ir.ShiftLeft(nReg, ir.Constant(1));
        ir.If(tBit, () =>
        {
            result = ir.Or(result, ir.Constant(1));
        });
        ir.SetReg(ir.Op.N(), result);
    }
    
    public static void Rotcr(Sh4EmitterContext ir)
    {
        var tBit = ir.AllocateLocal();
        var nReg = ir.GetReg(ir.Op.N());
        
        ir.Copy(ir.GetT(), tBit);
        ir.If(ir.And(nReg, ir.Constant(1)), ir.SetT, ir.ClearT);

        var result = ir.ShiftRight(nReg, ir.Constant(1));
        ir.If(tBit, () =>
        {
            result = ir.Or(result, ir.Constant(0x80000000));
        });
        ir.SetReg(ir.Op.N(), result);
    }
    
    public static void Rotl(Sh4EmitterContext ir)
    {
        var nReg = ir.GetReg(ir.Op.N());
        ir.If(ir.And(nReg, ir.Constant(0x80000000)), ir.SetT, ir.ClearT);
        ir.SetReg(ir.Op.N(), ir.RotateLeft(nReg, ir.Constant(1)));
    }
    
    public static void Rotr(Sh4EmitterContext ir)
    {
        var nReg = ir.GetReg(ir.Op.N());
        ir.If(ir.And(nReg, ir.Constant(1)), ir.SetT, ir.ClearT);
        ir.SetReg(ir.Op.N(), ir.RotateRight(nReg, ir.Constant(1)));
    }
    
    public static void Shal(Sh4EmitterContext ir)
    {
        var nReg = ir.GetReg(ir.Op.N());
        ir.If(ir.And(nReg, ir.Constant(0x80000000)), ir.SetT, ir.ClearT);
        ir.SetReg(ir.Op.N(), ir.ShiftLeft(nReg, ir.Constant(1)));
    }
    
    public static void Shar(Sh4EmitterContext ir)
    {
        var nReg = ir.GetReg(ir.Op.N());
        ir.If(ir.And(nReg, ir.Constant(1)), ir.SetT, ir.ClearT);
        ir.SetReg(ir.Op.N(), ir.ShiftRightArithmetic(nReg, ir.Constant(1)));
    }
    
    public static void Shad(Sh4EmitterContext ir)
    {
        var n = ir.Op.N();
        var m = ir.Op.M();
        
        var mReg = ir.GetReg(m);
        var nReg = ir.GetReg(n);
        
        // int sgn = R[m] & 0x80000000;
        var sgn = ir.And(mReg, ir.Constant(0x80000000));
        
        // if (sgn == 0)
        ir.If(ir.CompareEqual(sgn, ir.Constant(0)), () =>
        {
            // R[n] <<= (R[m] & 0x1F);
            ir.SetReg(n, ir.ShiftLeft(nReg, ir.And(mReg, ir.Constant(0x1F))));
        }, () =>
        {
            // else if ((R[m] & 0x1F) == 0)
            ir.If(ir.CompareEqual(ir.And(mReg, ir.Constant(0x1F)), ir.Constant(0)), () =>
            {
                // if ((R[n] & 0x80000000) == 0)
                ir.If(ir.And(nReg, ir.Constant(0x80000000)), () =>
                {
                    // R[n] = 0;
                    ir.SetReg(n, ir.Constant(~0));
                }, () =>
                {
                    // R[n] = 0xFFFFFFFF;
                    ir.SetReg(n, ir.Constant(0));
                });
            }, () =>
            {
                //R[n]=(signed)R[n] >> ((~R[m] & 0x1F)+1)
                var samt = ir.Add(ir.And(ir.Not(mReg), ir.Constant(0x1F)), ir.Constant(1));
                ir.SetReg(n, ir.ShiftRightArithmetic(nReg, samt));
            });
        });
    }
    
    public static void Shld(Sh4EmitterContext ir)
    {
        var n = ir.Op.N();
        var m = ir.Op.M();
        
        var mReg = ir.GetReg(m);
        var nReg = ir.GetReg(n);
        
        // int sgn = R[m] & 0x80000000;
        var sgn = ir.And(mReg, ir.Constant(0x80000000));
        
        // if (sgn == 0)
        ir.If(ir.CompareEqual(sgn, ir.Constant(0)), () =>
        {
            // R[n] <<= (R[m] & 0x1F);
            ir.SetReg(n, ir.ShiftLeft(nReg, ir.And(mReg, ir.Constant(0x1F))));
        }, () =>
        {
            // else if ((R[m] & 0x1F) == 0)
            ir.If(ir.CompareEqual(ir.And(mReg, ir.Constant(0x1F)), ir.Constant(0)), () =>
            {
                // R[n] = 0;
                ir.SetReg(n, ir.Constant(0));
            }, () =>
            {
                //R[n]=(unsigned)R[n] >> ((~R[m] & 0x1F)+1)
                var samt = ir.Add(ir.And(ir.Not(mReg), ir.Constant(0x1F)), ir.Constant(1));
                ir.SetReg(n, ir.ShiftRight(nReg, samt));
            });
        });
    }
    
    public static void Shll(Sh4EmitterContext ir)
    {
        var nReg = ir.GetReg(ir.Op.N());
        ir.If(ir.And(nReg, ir.Constant(0x80000000)), ir.SetT, ir.ClearT);
        ir.SetReg(ir.Op.N(), ir.ShiftLeft(nReg, ir.Constant(1)));
    }
    
    public static void Shll2(Sh4EmitterContext ir)
    {
        var n = ir.Op.N();
        ir.SetReg(n, ir.ShiftLeft(ir.GetReg(n), ir.Constant(2)));;
    }
    
    public static void Shll8(Sh4EmitterContext ir)
    {
        var n = ir.Op.N();
        ir.SetReg(n, ir.ShiftLeft(ir.GetReg(n), ir.Constant(8)));
    }
    
    public static void Shll16(Sh4EmitterContext ir)
    {
        var n = ir.Op.N();
        ir.SetReg(n, ir.ShiftLeft(ir.GetReg(n), ir.Constant(16)));
    }
    
    public static void Shlr(Sh4EmitterContext ir)
    {
        var nReg = ir.GetReg(ir.Op.N());
        ir.If(ir.And(nReg, ir.Constant(1)), ir.SetT, ir.ClearT);
        ir.SetReg(ir.Op.N(), ir.ShiftRight(nReg, ir.Constant(1)));
    }
    
    public static void Shlr2(Sh4EmitterContext ir)
    {
        var n = ir.Op.N();
        ir.SetReg(n, ir.ShiftRight(ir.GetReg(n), ir.Constant(2)));
    }
    
    public static void Shlr8(Sh4EmitterContext ir)
    {
        var n = ir.Op.N();
        ir.SetReg(n, ir.ShiftRight(ir.GetReg(n), ir.Constant(8)));
    }
    
    public static void Shlr16(Sh4EmitterContext ir)
    {
        var n = ir.Op.N();
        ir.SetReg(n, ir.ShiftRight(ir.GetReg(n), ir.Constant(16)));
    }
    
    
}