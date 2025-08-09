namespace DeeCee.SH4;

public unsafe struct Sh4CpuState
{
    public fixed UInt32 R[16];
    public fixed UInt32 RBank[8];
    
    // MACH,MACL,PR,FPSCR,FPUL...
    public UInt32 PC;
    public UInt32 PR;
    public UInt32 GBR;
    public UInt32 SR;
    public UInt32 SSR;
    public UInt32 SPC;
    public UInt32 VBR;
    public UInt32 SGR;
    public UInt32 DBR;
    public UInt32 MACH;
    public UInt32 MACL;


    public bool T
    {
        get => (SR & 1) != 0;
        set => SR = (SR & ~1U) | (value ? 1U: 0);
    }
    
    public bool S
    {
        get => (SR & 2) != 0;
        set => SR = (SR & ~2U) | (value ? 2U: 0);
    }
}