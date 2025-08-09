namespace DeeCee.SH4.Translate;

public class ControlOps
{
    public static void LdcSr(Sh4EmitterContext ir)
    {
        ir.SetSR(ir.And(ir.GetReg(ir.Op.N()), ir.Constant(0x700083F3)));
    }
    
    public static void StcSr(Sh4EmitterContext ir)
    {
        ir.SetReg(ir.Op.N(), ir.GetSR());
    }
    
    public static void LdcGbr(Sh4EmitterContext ir)
    {
        ir.SetGBR(ir.GetReg(ir.Op.N()));
    }
    
    public static void StcGbr(Sh4EmitterContext ir)
    {
        ir.SetReg(ir.Op.N(), ir.GetGBR());
    }
    
    public static void LdcVbr(Sh4EmitterContext ir)
    {
        ir.SetVBR(ir.GetReg(ir.Op.N()));
    }
    
    public static void StcVbr(Sh4EmitterContext ir)
    {
        ir.SetReg(ir.Op.N(), ir.GetVBR());
    }
    
    public static void LdcSsr(Sh4EmitterContext ir)
    {
        ir.SetSSR(ir.GetReg(ir.Op.N()));
    }
    
    public static void StcSsr(Sh4EmitterContext ir)
    {
        ir.SetReg(ir.Op.N(), ir.GetSSR());
    }
    
    public static void LdcSpc(Sh4EmitterContext ir)
    {
        ir.SetSPC(ir.GetReg(ir.Op.N()));
    }
    
    public static void StcSpc(Sh4EmitterContext ir)
    {
        ir.SetReg(ir.Op.N(), ir.GetSPC());
    }
    
    public static void LdcSgr(Sh4EmitterContext ir)
    {
        ir.SetSGR(ir.GetReg(ir.Op.N()));
    }
    
    public static void StcSgr(Sh4EmitterContext ir)
    {
        ir.SetReg(ir.Op.N(), ir.GetSGR());
    }
    
    public static void LdcDbr(Sh4EmitterContext ir)
    {
        ir.SetDBR(ir.GetReg(ir.Op.N()));
    }
    
    public static void StcDbr(Sh4EmitterContext ir)
    {
        ir.SetReg(ir.Op.N(), ir.GetDBR());
    }
    
    public static void LdcRbank(Sh4EmitterContext ir)
    {
        ir.SetBankedReg(ir.Op.M() & 7, ir.GetReg(ir.Op.N()));
    }
    
    public static void StcRbank(Sh4EmitterContext ir)
    {
        ir.SetReg(ir.Op.N(), ir.GetBankedReg(ir.Op.M() & 7));
    }
    
    
    public static void LdcmSr(Sh4EmitterContext ir)
    {
        var ea = ir.Memory(ir.GetReg(ir.Op.N()));
        var data = ir.Load(ea);
        ir.SetSR(ir.And(data, ir.Constant(0x700083F3)));
        ir.SetReg(ir.Op.N(), ir.Add(ir.GetReg(ir.Op.N()), ir.Constant(4)));
    }
    
    public static void StcmSr(Sh4EmitterContext ir)
    {
        var addr = ir.Sub(ir.GetReg(ir.Op.N()), ir.Constant(4));
        ir.SetReg(ir.Op.N(), addr);
        var ea = ir.Memory(addr);
        ir.Store(ea, ir.GetSR());
    }
    
    public static void LdcmGbr(Sh4EmitterContext ir)
    {
        var ea = ir.Memory(ir.GetReg(ir.Op.N()));
        var data = ir.Load(ea);
        ir.SetGBR(data);
        ir.SetReg(ir.Op.N(), ir.Add(ir.GetReg(ir.Op.N()), ir.Constant(4)));
    }
    
    public static void StcmGbr(Sh4EmitterContext ir)
    {
        var addr = ir.Sub(ir.GetReg(ir.Op.N()), ir.Constant(4));
        ir.SetReg(ir.Op.N(), addr);
        var ea = ir.Memory(addr);
        ir.Store(ea, ir.GetGBR());
    }
    
    public static void LdcmVbr(Sh4EmitterContext ir)
    {
        var ea = ir.Memory(ir.GetReg(ir.Op.N()));
        var data = ir.Load(ea);
        ir.SetVBR(data);
        ir.SetReg(ir.Op.N(), ir.Add(ir.GetReg(ir.Op.N()), ir.Constant(4)));
    }
    
    public static void StcmVbr(Sh4EmitterContext ir)
    {
        var addr = ir.Sub(ir.GetReg(ir.Op.N()), ir.Constant(4));
        ir.SetReg(ir.Op.N(), addr);
        var ea = ir.Memory(addr);
        ir.Store(ea, ir.GetVBR());
    }
    
    public static void LdcmSsr(Sh4EmitterContext ir)
    {
        var ea = ir.Memory(ir.GetReg(ir.Op.N()));
        var data = ir.Load(ea);
        ir.SetSSR(data);
        ir.SetReg(ir.Op.N(), ir.Add(ir.GetReg(ir.Op.N()), ir.Constant(4)));
    }
    
    public static void StcmSsr(Sh4EmitterContext ir)
    {
        var addr = ir.Sub(ir.GetReg(ir.Op.N()), ir.Constant(4));
        ir.SetReg(ir.Op.N(), addr);
        var ea = ir.Memory(addr);
        ir.Store(ea, ir.GetSSR());
    }
    
    public static void LdcmSpc(Sh4EmitterContext ir)
    {
        var ea = ir.Memory(ir.GetReg(ir.Op.N()));
        var data = ir.Load(ea);
        ir.SetSPC(data);
        ir.SetReg(ir.Op.N(), ir.Add(ir.GetReg(ir.Op.N()), ir.Constant(4)));
    }
    
    public static void StcmSpc(Sh4EmitterContext ir)
    {
        var addr = ir.Sub(ir.GetReg(ir.Op.N()), ir.Constant(4));
        ir.SetReg(ir.Op.N(), addr);
        var ea = ir.Memory(addr);
        ir.Store(ea, ir.GetSPC());
    }
    
    public static void LdcmSgr(Sh4EmitterContext ir)
    {
        var ea = ir.Memory(ir.GetReg(ir.Op.N()));
        var data = ir.Load(ea);
        ir.SetSGR(data);
        ir.SetReg(ir.Op.N(), ir.Add(ir.GetReg(ir.Op.N()), ir.Constant(4)));
    }
    
    public static void StcmSgr(Sh4EmitterContext ir)
    {
        var addr = ir.Sub(ir.GetReg(ir.Op.N()), ir.Constant(4));
        ir.SetReg(ir.Op.N(), addr);
        var ea = ir.Memory(addr);
        ir.Store(ea, ir.GetSGR());
    }
    
    public static void LdcmDbr(Sh4EmitterContext ir)
    {
        var ea = ir.Memory(ir.GetReg(ir.Op.N()));
        var data = ir.Load(ea);
        ir.SetDBR(data);
        ir.SetReg(ir.Op.N(), ir.Add(ir.GetReg(ir.Op.N()), ir.Constant(4)));
    }
    
    public static void StcmDbr(Sh4EmitterContext ir)
    {
        var addr = ir.Sub(ir.GetReg(ir.Op.N()), ir.Constant(4));
        ir.SetReg(ir.Op.N(), addr);
        var ea = ir.Memory(addr);
        ir.Store(ea, ir.GetDBR());
    }
    
    public static void LdcmRbank(Sh4EmitterContext ir)
    {
        var ea = ir.Memory(ir.GetReg(ir.Op.N()));
        var data = ir.Load(ea);
        ir.SetBankedReg(ir.Op.M() & 7, data);
        ir.SetReg(ir.Op.N(), ir.Add(ir.GetReg(ir.Op.N()), ir.Constant(4)));
    }
    
    public static void StcmRbank(Sh4EmitterContext ir)
    {
        var addr = ir.Sub(ir.GetReg(ir.Op.N()), ir.Constant(4));
        ir.SetReg(ir.Op.N(), addr);
        var ea = ir.Memory(addr);
        ir.Store(ea, ir.GetBankedReg(ir.Op.M() & 7));
    }
    
    
    public static void LdsMach(Sh4EmitterContext ir)
    {
        ir.SetMACH(ir.GetReg(ir.Op.N()));
    }
    
    public static void StsMach(Sh4EmitterContext ir)
    {
        ir.SetReg(ir.Op.N(), ir.GetMACH());
    }
    
    public static void LdsMacl(Sh4EmitterContext ir)
    {
        ir.SetMACL(ir.GetReg(ir.Op.N()));
    }
    
    public static void StsMacl(Sh4EmitterContext ir)
    {
        ir.SetReg(ir.Op.N(), ir.GetMACL());
    }
    
    public static void LdsPr(Sh4EmitterContext ir)
    {
        ir.SetPR(ir.GetReg(ir.Op.N()));
    }
    
    public static void StsPr(Sh4EmitterContext ir)
    {
        ir.SetReg(ir.Op.N(), ir.GetPR());
    }
    
    public static void LdsmMach(Sh4EmitterContext ir)
    {
        var ea = ir.Memory(ir.GetReg(ir.Op.N()));
        var data = ir.Load(ea);
        ir.SetMACH(data);
        ir.SetReg(ir.Op.N(), ir.Add(ir.GetReg(ir.Op.N()), ir.Constant(4)));
    }
    
    public static void StsmMach(Sh4EmitterContext ir)
    {
        var addr = ir.Sub(ir.GetReg(ir.Op.N()), ir.Constant(4));
        ir.SetReg(ir.Op.N(), addr);
        var ea = ir.Memory(addr);
        ir.Store(ea, ir.GetMACH());
    }
    
    public static void LdsmMacl(Sh4EmitterContext ir)
    {
        var ea = ir.Memory(ir.GetReg(ir.Op.N()));
        var data = ir.Load(ea);
        ir.SetMACL(data);
        ir.SetReg(ir.Op.N(), ir.Add(ir.GetReg(ir.Op.N()), ir.Constant(4)));
    }
    
    public static void StsmMacl(Sh4EmitterContext ir)
    {
        var addr = ir.Sub(ir.GetReg(ir.Op.N()), ir.Constant(4));
        ir.SetReg(ir.Op.N(), addr);
        var ea = ir.Memory(addr);
        ir.Store(ea, ir.GetMACL());
    }
    
    public static void LdsmPr(Sh4EmitterContext ir)
    {
        var ea = ir.Memory(ir.GetReg(ir.Op.N()));
        var data = ir.Load(ea);
        ir.SetPR(data);
        ir.SetReg(ir.Op.N(), ir.Add(ir.GetReg(ir.Op.N()), ir.Constant(4)));
    }
    
    public static void StsmPr(Sh4EmitterContext ir)
    {
        var addr = ir.Sub(ir.GetReg(ir.Op.N()), ir.Constant(4));
        ir.SetReg(ir.Op.N(), addr);
        var ea = ir.Memory(addr);
        ir.Store(ea, ir.GetPR());
    }
}