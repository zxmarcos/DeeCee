using System.Runtime.CompilerServices;
using DeeCee.SH4.Translate;

namespace DeeCee.SH4;

public class Sh4FrontEnd : Sh4BaseCpu
{
    public Sh4EmitterContext Context { get; } = new Sh4EmitterContext();

    private delegate void OpcodeDecodeHandler();

    private readonly OpcodeDecodeHandler[] _opcodeDecoders = new OpcodeDecodeHandler[16];

    public Sh4FrontEnd()
    {
        _opcodeDecoders[0] = Op0000;
        _opcodeDecoders[1] = Op0001;
        _opcodeDecoders[2] = Op0010;
        _opcodeDecoders[3] = Op0011;
        _opcodeDecoders[4] = Op0100;
        _opcodeDecoders[5] = Op0101;
        _opcodeDecoders[6] = Op0110;
        _opcodeDecoders[7] = Op0111;
        _opcodeDecoders[8] = Op1000;
        _opcodeDecoders[9] = Op1001;
        _opcodeDecoders[10] = Op1010;
        _opcodeDecoders[11] = Op1011;
        _opcodeDecoders[12] = Op1100;
        _opcodeDecoders[13] = Op1101;
        _opcodeDecoders[14] = Op1110;
        _opcodeDecoders[15] = Op1111;
    }

    public void Step()
    {
        Compile(FetchOpcode(_state.PC));
        _state.PC += 2;
    }

    public void Compile(UInt16 value)
    {
        var opcode = new Sh4Opcode(value);
        Context.Op = opcode;
        _opcodeDecoders[opcode.Part(3)]();
    }

    public UInt16 FetchOpcode(UInt32 addr)
    {
        return 0;
    }

    public UInt32 ReadMemory32(UInt32 addr)
    {
        return 0;
    }

    private void Op0000()
    {
        var opcode = (ushort)(Context.Op.Value & 0xFFF);
        switch (opcode)
        {
            case 0b0000_0000_1000:
                // CLRT
                FlagOps.ClrT(Context);
                return;
            case 0b0000_0000_1001:
                // NOP
                break;
            case 0b0000_0000_1011:
                // RTS
                BranchOps.Rts(Context);
                return;
            case 0b0000_0001_1000:
                // SETT
                FlagOps.SetT(Context);
                return;
            case 0b0000_0001_1001:
                // DIV0U
                break;
            case 0b0000_0001_1011:
                // SLEEP
                break;
            case 0b0000_0010_1000:
                // CLRMAC
                ArithmeticOps.ClrMac(Context);
                return;
            case 0b0000_0010_1011:
                // RTE
                break;
            case 0b0000_0011_1000:
                // LDTLB
                break;
            case 0b0000_0100_1000:
                // CLRS
                FlagOps.ClrS(Context);
                return;
            case 0b0000_0101_1000:
                // SETS
                FlagOps.SetS(Context);
                return;

            case var v when (v & 0b0000_1111_1111) == 0b0000_0000_0010:
                // STCSR
                ControlOps.StcSr(Context); return;
                break;
            case var v when (v & 0b0000_1111_1111) == 0b0000_0000_0011:
                // BSRF
                BranchOps.Bsrf(Context);
                return;
            case var v when (v & 0b0000_1111_1111) == 0b0000_0000_1010:
                ControlOps.StsMach(Context); return;
                // STSMACH
                break;
            case var v when (v & 0b0000_1111_1111) == 0b0000_0001_0010:
                ControlOps.StcGbr(Context); return;
                // STCGBR
                break;
            case var v when (v & 0b0000_1111_1111) == 0b0000_0001_1010:
                ControlOps.StsMacl(Context); return;
                // STSMACL
                break;
            case var v when (v & 0b0000_1111_1111) == 0b0000_0010_0010:
                ControlOps.StcVbr(Context); return;
                // STCVBR
                break;
            case var v when (v & 0b0000_1111_1111) == 0b0000_0010_0011:
                // BRAF
                BranchOps.Braf(Context);
                return;
            case var v when (v & 0b0000_1111_1111) == 0b0000_0010_1001:
                // MOVT
                break;
            case var v when (v & 0b0000_1111_1111) == 0b0000_0010_1010:
                ControlOps.StsPr(Context); return;
                // STSPR
                break;
            case var v when (v & 0b0000_1111_1111) == 0b0000_0011_0010:
                ControlOps.StcSsr(Context); return;
                // STCSSR
                break;
            case var v when (v & 0b0000_1111_1111) == 0b0000_0011_1010:
                ControlOps.StcSgr(Context); return;
                // STCSGR
                break;
            case var v when (v & 0b0000_1111_1111) == 0b0000_0100_0010:
                ControlOps.StcSpc(Context); return;
                // STCSPC
                break;
            case var v when (v & 0b0000_1111_1111) == 0b0000_1000_0011:
                // PREF
                break;
            case var v when (v & 0b0000_1111_1111) == 0b0000_1001_0011:
                // OCBI
                break;
            case var v when (v & 0b0000_1111_1111) == 0b0000_1010_0011:
                // OCBP
                break;
            case var v when (v & 0b0000_1111_1111) == 0b0000_1011_0011:
                // OCBWB
                break;
            case var v when (v & 0b0000_1111_1111) == 0b0000_1100_0011:
                // MOVCAL
                break;
            case var v when (v & 0b0000_1111_1111) == 0b0000_1111_1010:
                ControlOps.StcDbr(Context); return;
                // STCDBR
                break;

            case var v when (v & 0b0000_1000_1111) == 0b0000_1000_0010:
                // STCRBANK
                break;

            case var v when (v & 0b0000_1111_1111) == 0b0000_0000_0100:
                // MOVBS_IDX
                break;
            case var v when (v & 0b0000_1111_1111) == 0b0000_0000_0101:
                // MOVWS_IDX
                break;
            case var v when (v & 0b0000_1111_1111) == 0b0000_0000_0110:
                // MOVLS_IDX
                break;
            case var v when (v & 0b0000_1111_1111) == 0b0000_0000_0111:
                // MULL
                break;
            case var v when (v & 0b0000_1111_1111) == 0b0000_0000_1100:
                // MOVBL_IDX
                break;
            case var v when (v & 0b0000_1111_1111) == 0b0000_0000_1101:
                // MOVWL_IDX
                break;
            case var v when (v & 0b0000_1111_1111) == 0b0000_0000_1110:
                // MOVLL_IDX
                break;
            case var v when (v & 0b0000_1111_1111) == 0b0000_0000_1111:
                // MACL
                break;
        }

        throw new NotImplementedException();
    }

    private void Op0001()
    {
        throw new NotImplementedException();
    }

    private void Op0010()
    {
        switch (Context.Op.Part(0))
        {
            // AND Rm,Rn
            case 0b1001:
                BitwiseOps.And(Context);
                return;
            // OR Rm,Rn
            case 0b1011:
                BitwiseOps.Or(Context);
                return;
            // XOR Rm,Rn
            case 0b1010:
                BitwiseOps.Xor(Context);
                return;
            // TST Rm,Rn
            case 0b1000:
                BitwiseOps.Tst(Context);
                return;
            // CMP/STR Rm,Rn
            case 0b1100:
                CompareOps.CmpStr(Context);
                return;
        }

        throw new NotImplementedException();
    }

    private void Op0011()
    {
        switch (Context.Op.Part(0))
        {
            // CMP/EQ Rm,Rn
            case 0b0000:
                CompareOps.CmpEq(Context);
                return;
            // CMP/GE Rm,Rn
            case 0b0011:
                CompareOps.CmpGe(Context);
                return;
            // CMP/GT Rm,Rn
            case 0b0111:
                CompareOps.CmpGt(Context);
                return;
            // CMP/HI Rm,Rn
            case 0b0110:
                CompareOps.CmpHi(Context);
                return;
            // CMP/HS Rm,Rn
            case 0b0010:
                CompareOps.CmpHs(Context);
                return;

            // ADD Rm,Rn
            case 0b1100:
                ArithmeticOps.Add(Context);
                return;
            // ADDC Rm,Rn
            case 0b1110:
                ArithmeticOps.AddC(Context);
                return;
            // ADDV Rm,Rn
            case 0b1111:
                ArithmeticOps.AddV(Context);
                return;
            // SUB Rm,Rn
            case 0b1000:
                ArithmeticOps.Sub(Context);
                return;
            // SUBC Rm,Rn
            case 0b1010:
                ArithmeticOps.SubC(Context);
                return;
            // SUBV Rm,Rn
            case 0b1011:
                ArithmeticOps.SubV(Context);
                return;
        }

        throw new NotImplementedException();
    }

    private void Op0100()
    {
        switch (Context.Op.Value & 0b0000_1111_1111)
        {
            // Instruções com prefixo mmmm
            case 0b0000_0000_0110: ControlOps.LdsmMach(Context); return; // LDSMMACH
            case 0b0000_0000_0111: ControlOps.LdcmSr(Context); return; // LDCMSR
            case 0b0000_0000_1010: ControlOps.LdsMach(Context); return; // LDSMACH
            case 0b0000_0000_1110: ControlOps.LdcSr(Context); return; // LDCSR
            case 0b0000_0001_0110: ControlOps.LdsmMacl(Context); return; // LDSMMACL
            case 0b0000_0001_0111: ControlOps.LdcmGbr(Context); return; // LDCMGBR
            case 0b0000_0001_1010: ControlOps.LdsMacl(Context); return; // LDSMACL
            case 0b0000_0001_1110: ControlOps.LdcGbr(Context); return; // LDCGBR
            case 0b0000_0010_0110: ControlOps.LdsmPr(Context); return; // LDSMPR
            case 0b0000_0010_0111: ControlOps.LdcmVbr(Context); return; // LDCMVBR
            case 0b0000_0010_1010: ControlOps.LdsPr(Context); return; // LDSPR
            case 0b0000_0010_1110: ControlOps.LdcVbr(Context); return; // LDCVBR
            case 0b0000_0011_0111: ControlOps.LdcmSsr(Context); return; // LDCMSSR
            case 0b0000_0011_1110: ControlOps.LdcSsr(Context); return; // LDCSSR
            case 0b0000_0100_0111: ControlOps.LdcmSpc(Context); return; // LDCMSPC
            case 0b0000_0100_1110: ControlOps.LdcSpc(Context); return; // LDCSPC
            case 0b0000_1111_0110: ControlOps.LdcmDbr(Context); return; // LDCMDBR
            case 0b0000_1111_1010: ControlOps.LdcDbr(Context); return; // LDCDBR

            // Instruções com prefixo nnnn
            case 0b0000_0000_0000:
                ShiftOps.Shll(Context);
                return; // SHLL
            case 0b0000_0000_0001:
                ShiftOps.Shlr(Context);
                return; // SHLR
            case 0b0000_0000_0010: ControlOps.StsmMach(Context); return; // STSMMACH
            case 0b0000_0000_0011: ControlOps.StcmSr(Context); return; // STCMSR
            case 0b0000_0000_0100:
                ShiftOps.Rotl(Context);
                return; // ROTL
            case 0b0000_0000_0101:
                ShiftOps.Rotr(Context);
                return; // ROTR
            case 0b0000_0000_1000:
                ShiftOps.Shll2(Context);
                return; // SHLL2
            case 0b0000_0000_1001:
                ShiftOps.Shlr2(Context);
                return; // SHLR2
            case 0b0000_0000_1011:
                BranchOps.Jsr(Context);
                return; // JSR
            case 0b0000_0001_0000:
                ArithmeticOps.Dt(Context);
                return; // DT
            case 0b0000_0001_0001:
                CompareOps.CmpPz(Context);
                return; // CMPPZ
            case 0b0000_0001_0010: ControlOps.StsmMacl(Context); return; // STSMMACL
            case 0b0000_0001_0011: ControlOps.StcmGbr(Context); return; // STCMGBR
            case 0b0000_0001_0101:
                CompareOps.CmpPl(Context);
                return; // CMPPL
            case 0b0000_0001_1000:
                ShiftOps.Shll8(Context);
                return; // SHLL8
            case 0b0000_0001_1001:
                ShiftOps.Shlr8(Context);
                return; // SHLR8
            case 0b0000_0001_1011: break; // TAS
            case 0b0000_0010_0000:
                ShiftOps.Shal(Context);
                return; // SHAL
            case 0b0000_0010_0001:
                ShiftOps.Shar(Context);
                return; // SHAR
            case 0b0000_0010_0010: ControlOps.StsmPr(Context); return; // STSMPR
            case 0b0000_0010_0011: ControlOps.StcmVbr(Context); return; // STCMVBR
            case 0b0000_0010_0100:
                ShiftOps.Rotcl(Context);
                return; // ROTCL
            case 0b0000_0010_0101:
                ShiftOps.Rotcr(Context);
                return; // ROTCR
            case 0b0000_0010_1000:
                ShiftOps.Shll16(Context);
                return; // SHLL16
            case 0b0000_0010_1001:
                ShiftOps.Shlr16(Context);
                return; // SHLR16
            case 0b0000_0010_1011:
                BranchOps.Jmp(Context);
                return; // JMP
            case 0b0000_0011_0010: ControlOps.StcmSgr(Context); return; // STCMSGR
            case 0b0000_0011_0011: ControlOps.StcmSsr(Context); return; // STCMSSR
            case 0b0000_0100_0011: ControlOps.StcmSpc(Context); return; // STCMSPC
            case 0b0000_1111_0010: ControlOps.StcmDbr(Context); return; // STCMDBR

            default:
                // Verifica instruções com padrões especiais
                if ((Context.Op.Value & 0b0000_1000_1111) == 0b0000_0000_0111) // LDCMRBANK mmmm1nnn0111
                {
                    // LDCMRBANK
                }
                else if ((Context.Op.Value & 0b0000_1000_1111) == 0b0000_0000_1110) // LDCRBANK mmmm1nnn1110
                {
                    // LDCRBANK
                }
                else if ((Context.Op.Value & 0b0000_1000_1111) == 0b0000_0000_0011) // STCMRBANK nnnn1mmm0011
                {
                    // STCMRBANK
                }
                else if ((Context.Op.Value & 0b0000_0000_1111) == 0b0000_0000_1100) // SHAD nnnnmmmm1100
                {
                    ShiftOps.Shad(Context); // SHAD
                    return;
                }
                else if ((Context.Op.Value & 0b0000_0000_1111) == 0b0000_0000_1101) // SHLD nnnnmmmm1101
                {
                    ShiftOps.Shld(Context); // SHLD
                    return;
                }
                else if ((Context.Op.Value & 0b0000_0000_1111) == 0b0000_0000_1111) // MACW nnnnmmmm1111
                {
                    // MACW
                }

                break;
        }

        throw new NotImplementedException();
    }

    private void Op0101()
    {
        throw new NotImplementedException();
    }

    private void Op0110()
    {
        switch (Context.Op.Part(0))
        {
            // NEGC Rm, Rn
            case 0b1010:
                ArithmeticOps.NegC(Context);
                return;
            // NEG Rm, Rn
            case 0b1011:
                ArithmeticOps.Neg(Context);
                return;

            // EXTU.B Rm, Rn
            case 0b1100:
                ExtOps.Extub(Context);
                return;
            // EXTU.W Rm, Rn
            case 0b1101:
                ExtOps.Extuw(Context);
                return;
            // EXTS.B Rm, Rn
            case 0b1110:
                ExtOps.Extsb(Context);
                return;
            // EXTS.W Rm, Rn
            case 0b1111:
                ExtOps.Extsw(Context);
                return;
            // NOT Rm, Rn
            case 0b0111:
                BitwiseOps.Not(Context);
                return;
        }

        throw new NotImplementedException();
    }

    private void Op0111()
    {
        ArithmeticOps.AddI(Context);
    }

    private void Op1000()
    {
        switch (Context.Op.Part(2))
        {
            // CMP/EQ #imm,R0
            case 0b1000:
                CompareOps.CmpEqI(Context);
                return;
            // BF disp
            case 0b1011:
                BranchOps.Bf(Context);
                return;
            // BFS disp
            case 0b1111:
                BranchOps.Bfs(Context);
                return;
            // BT disp
            case 0b1001:
                BranchOps.Bt(Context);
                return;
            // BTS disp
            case 0b1101:
                BranchOps.Bts(Context);
                return;
        }

        throw new NotImplementedException();
    }

    private void Op1001()
    {
        throw new NotImplementedException();
    }

    private void Op1010()
    {
        // BRA disp
        BranchOps.Bra(Context);
    }

    private void Op1011()
    {
        // BSR disp
        BranchOps.Bsr(Context);
    }

    private void Op1100()
    {
        switch (Context.Op.Part(2))
        {
            // AND #imm,R0
            case 0b1001:
                BitwiseOps.AndI(Context);
                return;
            // AND #imm,(R0+GBR)
            case 0b1101:
                BitwiseOps.AndB(Context);
                return;

            // OR #imm,R0
            case 0b1011:
                BitwiseOps.OrI(Context);
                return;
            // OR #imm,(R0+GBR)
            case 0b1111:
                BitwiseOps.OrB(Context);
                return;
            // XOR #imm,R0
            case 0b1010:
                BitwiseOps.XorI(Context);
                return;
            // XOR #imm,(R0+GBR)
            case 0b1110:
                BitwiseOps.XorB(Context);
                return;
            // TST #imm,R0
            case 0b1000:
                BitwiseOps.TstI(Context);
                return;
            // TST #imm,(R0+GBR)
            case 0b1100:
                BitwiseOps.TstB(Context);
                return;
        }

        throw new NotImplementedException();
    }

    private void Op1101()
    {
        throw new NotImplementedException();
    }

    private void Op1110()
    {
        throw new NotImplementedException();
    }

    private void Op1111()
    {
        throw new NotImplementedException();
    }
}