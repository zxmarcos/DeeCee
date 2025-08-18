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
    
    public bool Q
    {
        get => (SR & (1U << 8)) != 0;
        set => SR = (SR & ~(1U << 8)) | (value ? (1U << 8): 0);
    }
    
    public bool M
    {
        get => (SR & (1U << 9)) != 0;
        set => SR = (SR & ~(1U << 9)) | (value ? (1U << 9): 0);
    }

    public override string ToString()
    {
        var sb = new System.Text.StringBuilder();

        // GPRs
        for (int i = 0; i < 16; i++)
        {
            sb.Append($"R{i:D2}={R[i]:X8}");
            sb.Append(i % 4 == 3 ? '\n' : ' ');
        }

        // Banked registers
        for (int i = 0; i < 8; i++)
        {
            sb.Append($"RB{i}={RBank[i]:X8}");
            sb.Append(i % 4 == 3 ? '\n' : ' ');
        }
        if (sb.Length > 0 && sb[^1] != '\n') sb.Append('\n');

        // Special registers
        sb.AppendFormat("PC ={0:X8} PR ={1:X8} GBR={2:X8} SR ={3:X8}\n", PC, PR, GBR, SR);
        sb.AppendFormat("SSR={0:X8} SPC={1:X8} VBR={2:X8}\n", SSR, SPC, VBR);
        sb.AppendFormat("SGR={0:X8} DBR={1:X8} MACH={2:X8} MACL={3:X8}\n", SGR, DBR, MACH, MACL);

        // Flags (derivados de SR)
        sb.AppendFormat("FLAGS: T={0} S={1} Q={2} M={3}\n", T ? 1 : 0, S ? 1 : 0, Q ? 1 : 0, M ? 1 : 0);

        return sb.ToString();
    }

}