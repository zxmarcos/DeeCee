namespace DeeCee.SH4.Translate;

public static class FlagOps
{
    public static void SetS(Sh4EmitterContext ir)
    {
        ir.SetS();
    }
    
    public static void ClrS(Sh4EmitterContext ir)
    {
        ir.ClearS();
    }
    
    public static void SetT(Sh4EmitterContext ir)
    {
        ir.SetT();
    }
    
    public static void ClrT(Sh4EmitterContext ir)
    {
        ir.ClearT();
    }
    
}