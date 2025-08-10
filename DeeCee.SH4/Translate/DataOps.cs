namespace DeeCee.SH4.Translate;

public static class DataOps
{
    // ************ MOVe data *****************
    
    /* MOV Rm,Rn */
    public static void Mov(Sh4EmitterContext ir)
    {
        ir.SetReg(ir.Op.N(), ir.GetReg(ir.Op.M()));
    }
    
    /* MOV.B Rm,@Rn */
    public static void MovBs(Sh4EmitterContext ir)
    {
        var mReg = ir.GetReg(ir.Op.M());
        var nReg = ir.GetReg(ir.Op.N());
        ir.Store(ir.Memory(nReg, MemoryWidth.Byte), mReg);
    }
    
    /* MOV.W Rm,@Rn */
    public static void MovWs(Sh4EmitterContext ir)
    {
        var mReg = ir.GetReg(ir.Op.M());
        var nReg = ir.GetReg(ir.Op.N());
        ir.Store(ir.Memory(nReg, MemoryWidth.Word), mReg);
    }
    
    /* MOV.L Rm,@Rn */
    public static void MovLs(Sh4EmitterContext ir)
    {
        var mReg = ir.GetReg(ir.Op.M());
        var nReg = ir.GetReg(ir.Op.N());
        ir.Store(ir.Memory(nReg), mReg);
    }
    
    /* MOV.B @Rm,Rn */
    public static void MovBl(Sh4EmitterContext ir)
    {
        var mReg = ir.GetReg(ir.Op.M());
        var ea = ir.Memory(mReg, MemoryWidth.Byte);
        var data = ir.SignExtend8(ir.Load(ea));;
        ir.SetReg(ir.Op.N(), data);
    }
    
    /* MOV.W @Rm,Rn */
    public static void MovWl(Sh4EmitterContext ir)
    {
        var mReg = ir.GetReg(ir.Op.M());
        var ea = ir.Memory(mReg, MemoryWidth.Word);
        var data = ir.SignExtend16(ir.Load(ea));;
        ir.SetReg(ir.Op.N(), data);
    }
    
    /* MOV.L @Rm,Rn */
    public static void MovLl(Sh4EmitterContext ir)
    {
        var mReg = ir.GetReg(ir.Op.M());
        var ea = ir.Memory(mReg);
        var data = ir.Load(ea);
        ir.SetReg(ir.Op.N(), data);
    }
    
    /* MOV.B Rm,@-Rn */
    public static void MovBm(Sh4EmitterContext ir)
    {
        var mReg = ir.GetReg(ir.Op.M());
        var nReg = ir.GetReg(ir.Op.N());
        var addr = ir.Sub(nReg, ir.Constant(1));
        ir.Store(ir.Memory(addr, MemoryWidth.Byte), mReg);
        ir.SetReg(ir.Op.N(), addr);
    }
    
    /* MOV.W Rm,@-Rn */
    public static void MovWm(Sh4EmitterContext ir)
    {
        var mReg = ir.GetReg(ir.Op.M());
        var nReg = ir.GetReg(ir.Op.N());
        var addr = ir.Sub(nReg, ir.Constant(2));
        ir.Store(ir.Memory(addr, MemoryWidth.Word), mReg);
        ir.SetReg(ir.Op.N(), addr);
    }
    
    /* MOV.L Rm,@-Rn */
    public static void MovLm(Sh4EmitterContext ir)
    {
        var mReg = ir.GetReg(ir.Op.M());
        var nReg = ir.GetReg(ir.Op.N());
        var addr = ir.Sub(nReg, ir.Constant(4));
        ir.Store(ir.Memory(addr), mReg);
        ir.SetReg(ir.Op.N(), addr);
    }
    
    /* MOV.B @Rm+,Rn */
    public static void MovBp(Sh4EmitterContext ir)
    {
        var mReg = ir.GetReg(ir.Op.M());
        var ea = ir.Memory(mReg, MemoryWidth.Byte);;
        var data = ir.Load(ea);
        ir.SetReg(ir.Op.N(), ir.SignExtend8(data));
        if (ir.Op.M() != ir.Op.N())
        {
            ir.SetReg(ir.Op.M(), ir.Add(mReg, ir.Constant(1)));;
        }
    }
    
    /* MOV.W @Rm+,Rn */
    public static void MovWp(Sh4EmitterContext ir)
    {
        var mReg = ir.GetReg(ir.Op.M());
        var ea = ir.Memory(mReg, MemoryWidth.Word);
        var data = ir.Load(ea);
        ir.SetReg(ir.Op.N(), ir.SignExtend16(data));
        if (ir.Op.M() != ir.Op.N())
        {
            ir.SetReg(ir.Op.M(), ir.Add(mReg, ir.Constant(2)));
        }
    }
    
    /* MOV.L @Rm+,Rn */
    public static void MovLp(Sh4EmitterContext ir)
    {
        var mReg = ir.GetReg(ir.Op.M());
        var ea = ir.Memory(mReg);
        var data = ir.Load(ea);
        ir.SetReg(ir.Op.N(), data);
        if (ir.Op.M() != ir.Op.N())
        {
            ir.SetReg(ir.Op.M(), ir.Add(mReg, ir.Constant(4)));
        }
    }
    
    /* MOV.B Rm,@(R0,Rn) */
    public static void MovBs0(Sh4EmitterContext ir)
    {
        var mReg = ir.GetReg(ir.Op.M());
        var nReg = ir.GetReg(ir.Op.N());
        var addr = ir.Add(nReg, ir.GetReg(0));
        ir.Store(ir.Memory(addr, MemoryWidth.Byte), mReg);
    }
    
    /* MOV.W Rm,@(R0,Rn) */
    public static void MovWs0(Sh4EmitterContext ir)
    {
        var mReg = ir.GetReg(ir.Op.M());
        var nReg = ir.GetReg(ir.Op.N());
        var addr = ir.Add(nReg, ir.GetReg(0));
        ir.Store(ir.Memory(addr, MemoryWidth.Word), mReg);
    }
    
    /* MOV.L Rm,@(R0,Rn) */
    public static void MovLs0(Sh4EmitterContext ir)
    {
        var mReg = ir.GetReg(ir.Op.M());
        var nReg = ir.GetReg(ir.Op.N());
        var addr = ir.Add(nReg, ir.GetReg(0));
        ir.Store(ir.Memory(addr), mReg);
    }
    
    /* MOV.B @(R0,Rm),Rn */
    public static void MovBl0(Sh4EmitterContext ir)
    {
        var mReg = ir.GetReg(ir.Op.M());
        var ea = ir.Memory(ir.Add(mReg, ir.GetReg(0)), MemoryWidth.Byte);;
        var data = ir.Load(ea);
        ir.SetReg(ir.Op.N(), ir.SignExtend8(data));
    }
    
    /* MOV.W @(R0,Rm),Rn */
    public static void MovWl0(Sh4EmitterContext ir)
    {
        var mReg = ir.GetReg(ir.Op.M());
        var ea = ir.Memory(ir.Add(mReg, ir.GetReg(0)), MemoryWidth.Word);;
        var data = ir.Load(ea);
        ir.SetReg(ir.Op.N(), ir.SignExtend16(data));
    }
    
    /* MOV.L @(R0,Rm),Rn */
    public static void MovLl0(Sh4EmitterContext ir)
    {
        var mReg = ir.GetReg(ir.Op.M());
        var ea = ir.Memory(ir.Add(mReg, ir.GetReg(0)));;
        var data = ir.Load(ea);
        ir.SetReg(ir.Op.N(), data);
    }
    
    // ************ MOVe constant value *****************
    
    /* MOV #imm8,Rn */
    public static void MovI(Sh4EmitterContext ir)
    {
        ir.SetReg(ir.Op.N(), ir.Constant(ir.Op.SImm32()));
    }
    
    /* MOV.W @(disp,PC),Rn */
    public static void MovWi(Sh4EmitterContext ir)
    {
        var mReg = ir.GetReg(ir.Op.M());
        var displace = ir.Constant(4 + ir.Op.Imm8() * 2);
        var ea = ir.Memory(ir.Add(ir.GetPC(), displace), MemoryWidth.Word);;
        var data = ir.Load(ea);
        ir.SetReg(ir.Op.N(), ir.SignExtend16(data));
    }
    
    /* MOV.L @(disp,PC),Rn */
    public static void MovLi(Sh4EmitterContext ir)
    {
        var mReg = ir.GetReg(ir.Op.M());
        var displace = ir.Constant(4 + ir.Op.Imm8() * 4);
        var maskedPc = ir.And(ir.GetPC(), ir.Constant(0xFFFFFFFC));
        var ea = ir.Memory(ir.Add(maskedPc, displace), MemoryWidth.Word);;
        var data = ir.Load(ea);
        ir.SetReg(ir.Op.N(), data);
    }
    
    // ************ MOVe global data *****************
    
    /* MOV.B @(disp,GBR),R0 */
    public static void MovBlg(Sh4EmitterContext ir)
    {
        var addr = ir.Add(ir.GetGBR(), ir.Constant(ir.Op.Imm8()));
        var ea = ir.Memory(addr, MemoryWidth.Byte);
        var data = ir.Load(ea);
        ir.SetReg(0, ir.SignExtend8(data));
    }
    
    /* MOV.W @(disp,GBR),R0 */
    public static void MovWlg(Sh4EmitterContext ir)
    {
        var addr = ir.Add(ir.GetGBR(), ir.Constant(ir.Op.Imm8() * 2));
        var ea = ir.Memory(addr, MemoryWidth.Word);
        var data = ir.Load(ea);
        ir.SetReg(0, ir.SignExtend16(data));
    }
    
    /* MOV.L @(disp,GBR),R0 */
    public static void MovLlg(Sh4EmitterContext ir)
    {
        var addr = ir.Add(ir.GetGBR(), ir.Constant(ir.Op.Imm8() * 4));
        var ea = ir.Memory(addr, MemoryWidth.Byte);
        var data = ir.Load(ea);
        ir.SetReg(0, data);
    }
    
    /* MOV.B R0,@(disp,GBR) */
    public static void MovBsg(Sh4EmitterContext ir)
    {
        var addr = ir.Add(ir.GetGBR(), ir.Constant(ir.Op.Imm8()));
        var ea = ir.Memory(addr, MemoryWidth.Byte);
        ir.Store(ea, ir.GetReg(0));
    }
    
    /* MOV.W R0,@(disp,GBR) */
    public static void MovWsg(Sh4EmitterContext ir)
    {
        var addr = ir.Add(ir.GetGBR(), ir.Constant(ir.Op.Imm8() * 2));
        var ea = ir.Memory(addr, MemoryWidth.Word);
        ir.Store(ea, ir.GetReg(0));
    }
    
    /* MOV.L R0,@(disp,GBR) */
    public static void MovLsg(Sh4EmitterContext ir)
    {
        var addr = ir.Add(ir.GetGBR(), ir.Constant(ir.Op.Imm8() * 4));
        var ea = ir.Memory(addr);
        ir.Store(ea, ir.GetReg(0));
    }
    
    // ****************  MOVe structure data ******************
    
    /* MOV.B R0,@(disp,Rn) */
    public static void MovBs4(Sh4EmitterContext ir)
    {
        var addr = ir.Add(ir.GetReg(ir.Op.N()), ir.Constant(ir.Op.Imm4()));
        var ea = ir.Memory(addr, MemoryWidth.Byte);
        ir.Store(ea, ir.GetReg(0));
    }
    
    /* MOV.W R0,@(disp,Rn) */
    public static void MovWs4(Sh4EmitterContext ir)
    {
        var addr = ir.Add(ir.GetReg(ir.Op.N()), ir.Constant(ir.Op.Imm4() * 2));
        var ea = ir.Memory(addr, MemoryWidth.Word);
        ir.Store(ea, ir.GetReg(0));
    }
    
    /* MOV.L Rm,@(disp,Rn) */
    public static void MovLs4(Sh4EmitterContext ir)
    {
        var addr = ir.Add(ir.GetReg(ir.Op.N()), ir.Constant(ir.Op.Imm4() * 4));
        var ea = ir.Memory(addr);
        ir.Store(ea, ir.GetReg(ir.Op.M()));
    }
    
    /* MOV.B @(disp,Rm),R0 */
    public static void MovBl4(Sh4EmitterContext ir)
    {
        var addr = ir.Add(ir.GetReg(ir.Op.M()), ir.Constant(ir.Op.Imm4()));
        var ea = ir.Memory(addr, MemoryWidth.Byte);
        var data = ir.Load(ea);
        ir.SetReg(0, ir.SignExtend8(data));
    }
    
    /* MOV.W @(disp,Rm),R0 */
    public static void MovWl4(Sh4EmitterContext ir)
    {
        var addr = ir.Add(ir.GetReg(ir.Op.M()), ir.Constant(ir.Op.Imm4() * 2));
        var ea = ir.Memory(addr, MemoryWidth.Word);
        var data = ir.Load(ea);
        ir.SetReg(0, ir.SignExtend16(data));
    }
    
    /* MOV.W @(disp,Rm),Rn */
    public static void MovLl4(Sh4EmitterContext ir)
    {
        var addr = ir.Add(ir.GetReg(ir.Op.M()), ir.Constant(ir.Op.Imm4() * 4));
        var ea = ir.Memory(addr);
        var data = ir.Load(ea);
        ir.SetReg(ir.Op.N(), data);
    }
    
    // ****************  MOVe effective Address ******************
    
    /* MOVA @(disp,PC),R0 */
    public static void MovA(Sh4EmitterContext ir)
    {
        var displace = ir.Constant(4 + ir.Op.Imm8() * 4);
        var maskedPc = ir.And(ir.GetPC(), ir.Constant(0xFFFFFFFC));
        ir.SetReg(0, ir.Add(maskedPc, displace));
    }
    
    // ****************  Others... ******************
    /* MOVT Rn */
    public static void MovT(Sh4EmitterContext ir)
    {
        ir.SetReg(ir.Op.N(), ir.GetT());
    }
    
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

    public static void MovCal(Sh4EmitterContext context)
    {
        throw new NotImplementedException();
    }
}