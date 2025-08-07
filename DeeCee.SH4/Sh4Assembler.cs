using System.Diagnostics;

namespace DeeCee.SH4;

public class Sh4Assembler
{
    private static UInt16 RegFormat(byte msb, byte n, byte m, byte lsb) =>
        (UInt16)(((msb & 0xF) << 12) | ((n & 0xF) << 8) | ((m & 0xF) << 4) | (lsb & 0xF));
    private static UInt16 ImmFormat(byte msb, byte n, byte imm) => (UInt16)(((msb & 0xF) << 12) | ((n & 0xF) << 8) | (imm & 0xFF));
    private static UInt16 NibbleFormat(byte b3, byte b2, byte b1, byte b0) =>
        (UInt16)(((b3 & 0xF) << 12) | ((b2 & 0xF) << 8) | ((b1 & 0xF) << 4) | (b0 & 0xF));
    private static UInt16 DispFormat(UInt16 msb, UInt16 disp) => (UInt16)((msb << 8) | (disp & 0xFF));
    private static UInt16 Disp12Format(byte msb, UInt16 disp) => (UInt16)(((msb & 0xF) << 12) | (disp & 0xFFF));

    // MOV instructions
    public static UInt16 MOVI(byte n, sbyte imm) => ImmFormat(0b1110, n, (byte)imm);
    public static UInt16 MOVWL_PCR(byte n, byte disp) => ImmFormat(0b1001, n, disp);
    public static UInt16 MOVLL_PCR(byte n, byte disp) => ImmFormat(0b1101, n, disp);
    public static UInt16 MOV(byte m, byte n) => RegFormat(0b0110, n, m, 0b0011);
    public static UInt16 MOVBS_IND(byte m, byte n) => RegFormat(0b0010, n, m, 0b0000);
    public static UInt16 MOVWS_IND(byte m, byte n) => RegFormat(0b0010, n, m, 0b0001);
    public static UInt16 MOVLS_IND(byte m, byte n) => RegFormat(0b0010, n, m, 0b0010);
    public static UInt16 MOVBL_IND(byte m, byte n) => RegFormat(0b0110, n, m, 0b0000);
    public static UInt16 MOVWL_IND(byte m, byte n) => RegFormat(0b0110, n, m, 0b0001);
    public static UInt16 MOVLL_IND(byte m, byte n) => RegFormat(0b0110, n, m, 0b0010);
    public static UInt16 MOVBS_DEC(byte m, byte n) => RegFormat(0b0010, n, m, 0b0100);
    public static UInt16 MOVWS_DEC(byte m, byte n) => RegFormat(0b0010, n, m, 0b0101);
    public static UInt16 MOVLS_DEC(byte m, byte n) => RegFormat(0b0010, n, m, 0b0110);
    public static UInt16 MOVBL_INC(byte m, byte n) => RegFormat(0b0110, n, m, 0b0100);
    public static UInt16 MOVWL_INC(byte m, byte n) => RegFormat(0b0110, n, m, 0b0101);
    public static UInt16 MOVLL_INC(byte m, byte n) => RegFormat(0b0110, n, m, 0b0110);
    public static UInt16 MOVBS_OFF(byte n, byte disp) => DispFormat(0b10000000, (UInt16)((n << 4) | (disp & 0xF)));
    public static UInt16 MOVWS_OFF(byte n, byte disp) => DispFormat(0b10000001, (UInt16)((n << 4) | (disp & 0xF)));
    public static UInt16 MOVLS_OFF(byte m, byte n, byte disp) => NibbleFormat(0b0001, n, m, disp);
    public static UInt16 MOVBL_OFF(byte m, byte disp) => DispFormat(0b10000100, (UInt16)((m << 4) | (disp & 0xF)));
    public static UInt16 MOVWL_OFF(byte m, byte disp) => DispFormat(0b10000101, (UInt16)((m << 4) | (disp & 0xF)));
    public static UInt16 MOVLL_OFF(byte m, byte n, byte disp) => NibbleFormat(0b0101, n, m, disp);
    public static UInt16 MOVBS_IDX(byte m, byte n) => RegFormat(0b0000, n, m, 0b0100);
    public static UInt16 MOVWS_IDX(byte m, byte n) => RegFormat(0b0000, n, m, 0b0101);
    public static UInt16 MOVLS_IDX(byte m, byte n) => RegFormat(0b0000, n, m, 0b0110);
    public static UInt16 MOVBL_IDX(byte m, byte n) => RegFormat(0b0000, n, m, 0b1100);
    public static UInt16 MOVWL_IDX(byte m, byte n) => RegFormat(0b0000, n, m, 0b1101);
    public static UInt16 MOVLL_IDX(byte m, byte n) => RegFormat(0b0000, n, m, 0b1110);
    public static UInt16 MOVBS_GBR(byte disp) => DispFormat(0b11000000, disp);
    public static UInt16 MOVWS_GBR(byte disp) => DispFormat(0b11000001, disp);
    public static UInt16 MOVLS_GBR(byte disp) => DispFormat(0b11000010, disp);
    public static UInt16 MOVBL_GBR(byte disp) => DispFormat(0b11000100, disp);
    public static UInt16 MOVWL_GBR(byte disp) => DispFormat(0b11000101, disp);
    public static UInt16 MOVLL_GBR(byte disp) => DispFormat(0b11000110, disp);
    public static UInt16 MOVA(byte disp) => DispFormat(0b11000111, disp);
    public static UInt16 MOVT(byte n) => RegFormat(0b0000, n, 0b0010, 0b1001);
    public static UInt16 SWAPB(byte m, byte n) => RegFormat(0b0110, n, m, 0b1000);
    public static UInt16 SWAPW(byte m, byte n) => RegFormat(0b0110, n, m, 0b1001);
    public static UInt16 XTRCT(byte m, byte n) => RegFormat(0b0010, n, m, 0b1101);

    // Arithmetic instructions
    public static UInt16 ADD(byte m, byte n) => RegFormat(0b0011, n, m, 0b1100);
    public static UInt16 ADDI(byte n, sbyte imm) => ImmFormat(0b0111, n, (byte)imm);
    public static UInt16 ADDC(byte m, byte n) => RegFormat(0b0011, n, m, 0b1110);
    public static UInt16 ADDV(byte m, byte n) => RegFormat(0b0011, n, m, 0b1111);
    public static UInt16 SUB(byte m, byte n) => RegFormat(0b0011, n, m, 0b1000);
    public static UInt16 SUBC(byte m, byte n) => RegFormat(0b0011, n, m, 0b1010);
    public static UInt16 SUBV(byte m, byte n) => RegFormat(0b0011, n, m, 0b1011);
    public static UInt16 NEG(byte m, byte n) => RegFormat(0b0110, n, m, 0b1011);
    public static UInt16 NEGC(byte m, byte n) => RegFormat(0b0110, n, m, 0b1010);

    // Compare instructions
    public static UInt16 CMPEQI(sbyte imm) => ImmFormat(0b10001000, 0, (byte)imm);
    public static UInt16 CMPEQ(byte m, byte n) => RegFormat(0b0011, n, m, 0b0000);
    public static UInt16 CMPHS(byte m, byte n) => RegFormat(0b0011, n, m, 0b0010);
    public static UInt16 CMPGE(byte m, byte n) => RegFormat(0b0011, n, m, 0b0011);
    public static UInt16 CMPHI(byte m, byte n) => RegFormat(0b0011, n, m, 0b0110);
    public static UInt16 CMPGT(byte m, byte n) => RegFormat(0b0011, n, m, 0b0111);
    public static UInt16 CMPPZ(byte n) => RegFormat(0b0100, n, 0b0001, 0b0001);
    public static UInt16 CMPPL(byte n) => RegFormat(0b0100, n, 0b0001, 0b0101);
    public static UInt16 CMPSTR(byte m, byte n) => RegFormat(0b0010, n, m, 0b1100);

    // Division instructions
    public static UInt16 DIV0S(byte m, byte n) => RegFormat(0b0010, n, m, 0b0111);
    public static UInt16 DIV0U() => (UInt16)0x0019;
    public static UInt16 DIV1(byte m, byte n) => RegFormat(0b0011, n, m, 0b0100);

    // Multiplication instructions
    public static UInt16 DMULS(byte m, byte n) => RegFormat(0b0011, n, m, 0b1101);
    public static UInt16 DMULU(byte m, byte n) => RegFormat(0b0011, n, m, 0b0101);
    public static UInt16 MULL(byte m, byte n) => RegFormat(0b0000, n, m, 0b0111);
    public static UInt16 MULS(byte m, byte n) => RegFormat(0b0010, n, m, 0b1111);
    public static UInt16 MULU(byte m, byte n) => RegFormat(0b0010, n, m, 0b1110);
    public static UInt16 MACL(byte m, byte n) => RegFormat(0b0000, n, m, 0b1111);
    public static UInt16 MACW(byte m, byte n) => RegFormat(0b0100, n, m, 0b1111);

    // Logical instructions
    public static UInt16 AND(byte m, byte n) => RegFormat(0b0010, n, m, 0b1001);
    public static UInt16 ANDI(sbyte imm) => ImmFormat(0b1100, 0b1001, (byte)imm);
    public static UInt16 ANDB(byte imm) => ImmFormat(0b1100, 0b1101, imm);
    public static UInt16 NOT(byte m, byte n) => RegFormat(0b0110, n, m, 0b0111);
    public static UInt16 OR(byte m, byte n) => RegFormat(0b0010, n, m, 0b1011);
    public static UInt16 ORI(sbyte imm) => ImmFormat(0b1100, 0b1011, (byte)imm);
    public static UInt16 ORB(byte imm) => ImmFormat(0b1100, 0b1111, imm);
    public static UInt16 TST(byte m, byte n) => RegFormat(0b0010, n, m, 0b1000);
    public static UInt16 TSTI(sbyte imm) => ImmFormat(0b1100, 0b1000, (byte)imm);
    public static UInt16 TSTB(byte imm) => ImmFormat(0b1100, 0b1100, imm);
    public static UInt16 XOR(byte m, byte n) => RegFormat(0b0010, n, m, 0b1010);
    public static UInt16 XORI(sbyte imm) => ImmFormat(0b1100, 0b1010, (byte)imm);
    public static UInt16 XORB(byte imm) => ImmFormat(0b1100, 0b1110, imm);
    public static UInt16 TAS(byte n) => RegFormat(0b0100, n, 0b0001, 0b1011);

    // Shift and rotate instructions
    public static UInt16 ROTL(byte n) => RegFormat(0b0100, n, 0b0000, 0b0100);
    public static UInt16 ROTR(byte n) => RegFormat(0b0100, n, 0b0000, 0b0101);
    public static UInt16 ROTCL(byte n) => RegFormat(0b0100, n, 0b0010, 0b0100);
    public static UInt16 ROTCR(byte n) => RegFormat(0b0100, n, 0b0010, 0b0101);
    public static UInt16 SHAD(byte m, byte n) => RegFormat(0b0100, n, m, 0b1100);
    public static UInt16 SHAL(byte n) => RegFormat(0b0100, n, 0b0010, 0b0000);
    public static UInt16 SHAR(byte n) => RegFormat(0b0100, n, 0b0010, 0b0001);
    public static UInt16 SHLD(byte m, byte n) => RegFormat(0b0100, n, m, 0b1101);
    public static UInt16 SHLL(byte n) => RegFormat(0b0100, n, 0b0000, 0b0000);
    public static UInt16 SHLR(byte n) => RegFormat(0b0100, n, 0b0000, 0b0001);
    public static UInt16 SHLL2(byte n) => RegFormat(0b0100, n, 0b0000, 0b1000);
    public static UInt16 SHLR2(byte n) => RegFormat(0b0100, n, 0b0000, 0b1001);
    public static UInt16 SHLL8(byte n) => RegFormat(0b0100, n, 0b0001, 0b1000);
    public static UInt16 SHLR8(byte n) => RegFormat(0b0100, n, 0b0001, 0b1001);
    public static UInt16 SHLL16(byte n) => RegFormat(0b0100, n, 0b0010, 0b1000);
    public static UInt16 SHLR16(byte n) => RegFormat(0b0100, n, 0b0010, 0b1001);

    // Branch instructions
    public static UInt16 BF(sbyte disp) => DispFormat(0b10001011, (byte)disp);
    public static UInt16 BFS(sbyte disp) => DispFormat(0b10001111, (byte)disp);
    public static UInt16 BT(sbyte disp) => DispFormat(0b10001001, (byte)disp);
    public static UInt16 BTS(sbyte disp) => DispFormat(0b10001101, (byte)disp);
    public static UInt16 BRA(Int16 disp) => Disp12Format(0b1010, (UInt16)(disp & 0xFFF));
    public static UInt16 BRAF(byte n) => RegFormat(0b0000, n, 0b0010, 0b0011);
    public static UInt16 BSR(Int16 disp) => Disp12Format(0b1011, (UInt16)(disp & 0xFFF));
    public static UInt16 BSRF(byte n) => RegFormat(0b0000, n, 0b0000, 0b0011);
    public static UInt16 JMP(byte m) => RegFormat(0b0100, m, 0b0010, 0b1011);
    public static UInt16 JSR(byte n) => RegFormat(0b0100, n, 0b0000, 0b1011);
    public static UInt16 RTS() => (UInt16)0x000B;

    // System control instructions
    public static UInt16 CLRMAC() => (UInt16)0x0028;
    public static UInt16 CLRS() => (UInt16)0x0048;
    public static UInt16 CLRT() => (UInt16)0x0008;
    public static UInt16 NOP() => (UInt16)0x0009;
    public static UInt16 RTE() => (UInt16)0x002B;
    public static UInt16 SETS() => (UInt16)0x0058;
    public static UInt16 SETT() => (UInt16)0x0018;
    public static UInt16 SLEEP() => (UInt16)0x001B;
    public static UInt16 TRAPA(byte imm) => ImmFormat(0b1100, 0b0011, imm);

    // Load/Store control registers
    public static UInt16 LDCSR(byte m) => RegFormat(0b0100, m, 0b0000, 0b1110);
    public static UInt16 LDCGBR(byte m) => RegFormat(0b0100, m, 0b0001, 0b1110);
    public static UInt16 LDCVBR(byte m) => RegFormat(0b0100, m, 0b0010, 0b1110);
    public static UInt16 LDCSSR(byte m) => RegFormat(0b0100, m, 0b0011, 0b1110);
    public static UInt16 LDCSPC(byte m) => RegFormat(0b0100, m, 0b0100, 0b1110);
    public static UInt16 LDCDBR(byte m) => RegFormat(0b0100, m, 0b1111, 0b1010);
    public static UInt16 LDCRBANK(byte m, byte n) => RegFormat(0b0100, m, (byte)(0b1000 | (n & 0x7)), 0b1110);
    public static UInt16 LDCMSR(byte m) => RegFormat(0b0100, m, 0b0000, 0b0111);
    public static UInt16 LDCMGBR(byte m) => RegFormat(0b0100, m, 0b0001, 0b0111);
    public static UInt16 LDCMVBR(byte m) => RegFormat(0b0100, m, 0b0010, 0b0111);
    public static UInt16 LDCMSSR(byte m) => RegFormat(0b0100, m, 0b0011, 0b0111);
    public static UInt16 LDCMSPC(byte m) => RegFormat(0b0100, m, 0b0100, 0b0111);
    public static UInt16 LDCMDBR(byte m) => RegFormat(0b0100, m, 0b1111, 0b0110);
    public static UInt16 LDCMRBANK(byte m, byte n) => RegFormat(0b0100, m, (byte)(0b1000 | (n & 0x7)), 0b0111);

    public static UInt16 STCSR(byte n) => RegFormat(0b0000, n, 0b0000, 0b0010);
    public static UInt16 STCGBR(byte n) => RegFormat(0b0000, n, 0b0001, 0b0010);
    public static UInt16 STCVBR(byte n) => RegFormat(0b0000, n, 0b0010, 0b0010);
    public static UInt16 STCSSR(byte n) => RegFormat(0b0000, n, 0b0011, 0b0010);
    public static UInt16 STCSPC(byte n) => RegFormat(0b0000, n, 0b0100, 0b0010);
    public static UInt16 STCSGR(byte n) => RegFormat(0b0000, n, 0b0011, 0b1010);
    public static UInt16 STCDBR(byte n) => RegFormat(0b0000, n, 0b1111, 0b1010);
    public static UInt16 STCRBANK(byte m, byte n) => RegFormat(0b0000, n, (byte)(0b1000 | (m & 0x7)), 0b0010);
    public static UInt16 STCMSR(byte n) => RegFormat(0b0100, n, 0b0000, 0b0011);
    public static UInt16 STCMGBR(byte n) => RegFormat(0b0100, n, 0b0001, 0b0011);
    public static UInt16 STCMVBR(byte n) => RegFormat(0b0100, n, 0b0010, 0b0011);
    public static UInt16 STCMSSR(byte n) => RegFormat(0b0100, n, 0b0011, 0b0011);
    public static UInt16 STCMSPC(byte n) => RegFormat(0b0100, n, 0b0100, 0b0011);
    public static UInt16 STCMSGR(byte n) => RegFormat(0b0100, n, 0b0011, 0b0010);
    public static UInt16 STCMDBR(byte n) => RegFormat(0b0100, n, 0b1111, 0b0010);
    public static UInt16 STCMRBANK(byte m, byte n) => RegFormat(0b0100, n, (byte)(0b1000 | (m & 0x7)), 0b0011);

    // Load/Store system registers
    public static UInt16 LDSMACH(byte m) => RegFormat(0b0100, m, 0b0000, 0b1010);
    public static UInt16 LDSMACL(byte m) => RegFormat(0b0100, m, 0b0001, 0b1010);
    public static UInt16 LDSPR(byte m) => RegFormat(0b0100, m, 0b0010, 0b1010);
    public static UInt16 LDSMMACH(byte m) => RegFormat(0b0100, m, 0b0000, 0b0110);
    public static UInt16 LDSMMACL(byte m) => RegFormat(0b0100, m, 0b0001, 0b0110);
    public static UInt16 LDSMPR(byte m) => RegFormat(0b0100, m, 0b0010, 0b0110);

    public static UInt16 STSMACH(byte n) => RegFormat(0b0000, n, 0b0000, 0b1010);
    public static UInt16 STSMACL(byte n) => RegFormat(0b0000, n, 0b0001, 0b1010);
    public static UInt16 STSPR(byte n) => RegFormat(0b0000, n, 0b0010, 0b1010);
    public static UInt16 STSMMACH(byte n) => RegFormat(0b0100, n, 0b0000, 0b0010);
    public static UInt16 STSMMACL(byte n) => RegFormat(0b0100, n, 0b0001, 0b0010);
    public static UInt16 STSMPR(byte n) => RegFormat(0b0100, n, 0b0010, 0b0010);

    // Extension instructions
    public static UInt16 EXTSB(byte m, byte n) => RegFormat(0b0110, n, m, 0b1110);
    public static UInt16 EXTSW(byte m, byte n) => RegFormat(0b0110, n, m, 0b1111);
    public static UInt16 EXTUB(byte m, byte n) => RegFormat(0b0110, n, m, 0b1100);
    public static UInt16 EXTUW(byte m, byte n) => RegFormat(0b0110, n, m, 0b1101);
    public static UInt16 DT(byte n) => RegFormat(0b0100, n, 0b0001, 0b0000);

    // Cache and TLB instructions
    public static UInt16 LDTLB() => (UInt16)0x0038;
    public static UInt16 MOVCAL(byte n) => RegFormat(0b0000, n, 0b1100, 0b0011);
    public static UInt16 OCBI(byte n) => RegFormat(0b0000, n, 0b1001, 0b0011);
    public static UInt16 OCBP(byte n) => RegFormat(0b0000, n, 0b1010, 0b0011);
    public static UInt16 OCBWB(byte n) => RegFormat(0b0000, n, 0b1011, 0b0011);
    public static UInt16 PREF(byte n) => RegFormat(0b0000, n, 0b1000, 0b0011);
}