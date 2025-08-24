using System.Runtime.CompilerServices;

namespace DeeCee.SH4.Translate;

public class BranchOps
{

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AdvancePC(Sh4EmitterContext ir)
    {
        // PC = PC+2
        ir.SetPC(ir.Add(ir.GetPC(), ir.Constant(2)));
    }
    
    public static void Bf(Sh4EmitterContext ir)
    {
        ir.If(ir.IsZero(ir.GetT()), () =>
        {
            // PC = PC+4 + disp*2
            var tmp = ir.Add(ir.GetPC(), ir.Constant(4));
            ir.SetPC(ir.Add(tmp, ir.Constant(ir.Op.SImm32() * 2)));
        }, () => AdvancePC(ir)
        );
    }
    
    public static void Bt(Sh4EmitterContext ir)
    {
        ir.If(ir.GetT(), () =>
        {
            // PC = PC+4 + disp*2
            var tmp = ir.Add(ir.GetPC(), ir.Constant(4));
            ir.SetPC(ir.Add(tmp, ir.Constant(ir.Op.SImm32() * 2)));
        }, () => AdvancePC(ir));
    }

    public static void Bfs(Sh4EmitterContext ir)
    {
        ir.If(ir.IsZero(ir.GetT()), () =>
        {
            // PC = PC+4 + disp*2
            var tmp = ir.Add(ir.GetPC(), ir.Constant(2));
            ir.SetPC(ir.Add(tmp, ir.Constant(ir.Op.SImm32() * 2)));
        }, () => AdvancePC(ir));
        // TODO: Delay slot
    }
    
    public static void Bts(Sh4EmitterContext ir)
    {
        ir.If(ir.GetT(), () =>
        {
            // PC = PC+4 + disp*2
            var tmp = ir.Add(ir.GetPC(), ir.Constant(2));
            ir.SetPC(ir.Add(tmp, ir.Constant(ir.Op.SImm32() * 2)));
        }, () => AdvancePC(ir));
        // TODO: Delay slot
    }
    
    public static void Bra(Sh4EmitterContext ir)
    {
        // PC = PC+4 + disp*2
        var disp = ir.Op.Value & 0xFFF;
        if ((disp & (1 << 12)) != 0)
        {
            disp = (int)((UInt32)disp | 0xFFFF_F000u);
        }
        var tmp = ir.Add(ir.GetPC(), ir.Constant(2));
        ir.SetPC(ir.Add(tmp, ir.Constant(disp * 2)));
    }
    
    public static void Braf(Sh4EmitterContext ir)
    {
        var nReg = ir.GetReg(ir.Op.N());
        // PC = PC+4 + Rn
        var tmp = ir.Add(ir.GetPC(), ir.Constant(2));
        ir.SetPC(ir.Add(tmp, nReg));
    }
    
    public static void Bsr(Sh4EmitterContext ir)
    {
        // PC = PC+4 + disp*2
        var disp = ir.Op.Value & 0xFFF;
        if ((disp & (1 << 12)) != 0)
        {
            disp = (int)((UInt32)disp | 0xFFFF_F000u);
        }
        var tmp = ir.Add(ir.GetPC(), ir.Constant(2));
        // PR = PC + 4
        ir.SetPR(tmp);
        ir.SetPC(ir.Add(tmp, ir.Constant(disp * 2)));
    }
    
    public static void Bsrf(Sh4EmitterContext ir)
    {
        var nReg = ir.GetReg(ir.Op.N());
        // PC = PC+4 + Rn
        var tmp = ir.Add(ir.GetPC(), ir.Constant(2));
        ir.SetPR(tmp);
        ir.SetPC(ir.Add(tmp, nReg));
    }
    
    public static void Jmp(Sh4EmitterContext ir)
    {
        var nReg = ir.GetReg(ir.Op.N());
        ir.SetPC(nReg);
    }
    
    public static void Jsr(Sh4EmitterContext ir)
    {
        var nReg = ir.GetReg(ir.Op.N());
        ir.SetPR(ir.Add(ir.GetPC(), ir.Constant(2)));
        ir.SetPC(nReg);
    }
    
    public static void Rts(Sh4EmitterContext ir)
    {
        ir.SetPC(ir.GetPR());
    }
}