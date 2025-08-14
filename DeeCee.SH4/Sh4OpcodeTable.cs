using DeeCee.SH4.Translate;

namespace DeeCee.SH4;

public class Sh4OpcodeTable
{
    public delegate void EmitHandler(Sh4EmitterContext ir);

    public delegate string DisasmHandler(ushort opcode);

    private class InstructionMeta
    {
        public ushort Mask;
        public ushort BitPattern;
        public string Pattern;
        public DisasmHandler Disasm;
        public EmitHandler Emit;

        public InstructionMeta(string pattern, EmitHandler emit, string template)
        {
            Pattern = pattern;
            Emit = emit;
        }
    }

    public class Instruction
    {
        public DisasmHandler Disasm;
        public EmitHandler Emit;
    }

    static List<InstructionMeta> _metaInstructions;
    static Instruction[] _lookupTable;

    public static Instruction GetInstruction(ushort opcode)
    {
        return _lookupTable[opcode];
    }

    private static void Add(InstructionMeta meta)
    {
        var (mask, pattern) = ParsePattern(meta.Pattern);
        meta.BitPattern = pattern;
        meta.Mask = mask;
        _metaInstructions.Add(meta);
    }

    static Sh4OpcodeTable()
    {
        // @formatter:off
        #region "SH4 Opcode Table"
        _metaInstructions = new List<InstructionMeta>();
        Add(new InstructionMeta("1110nnnniiiiiiii", DataOps.MovI              ,"mov #imm8, rn"));
        Add(new InstructionMeta("1001nnnndddddddd", DataOps.MovWi             ,"mov.w @(disp:8,pc), rn"));
        Add(new InstructionMeta("1101nnnndddddddd", DataOps.MovLi             ,"mov.l @(disp:8,pc), rn"));
        Add(new InstructionMeta("0110nnnnmmmm0011", DataOps.Mov               ,"mov rm, rn"));
        Add(new InstructionMeta("0010nnnnmmmm0000", DataOps.MovBs             ,"mov.b rm, @rn"));
        Add(new InstructionMeta("0010nnnnmmmm0001", DataOps.MovWs             ,"mov.w rm, @rn"));
        Add(new InstructionMeta("0010nnnnmmmm0010", DataOps.MovLs             ,"mov.l rm, @rn"));
        Add(new InstructionMeta("0110nnnnmmmm0000", DataOps.MovBl             ,"mov.b @rm, rn"));
        Add(new InstructionMeta("0110nnnnmmmm0001", DataOps.MovWl             ,"mov.w @rm, rn"));
        Add(new InstructionMeta("0110nnnnmmmm0010", DataOps.MovLl             ,"mov.l @rm, rn"));
        Add(new InstructionMeta("0010nnnnmmmm0100", DataOps.MovBm             ,"mov.b rm,@-rn"));
        Add(new InstructionMeta("0010nnnnmmmm0101", DataOps.MovWm             ,"mov.w rm,@-rn"));
        Add(new InstructionMeta("0010nnnnmmmm0110", DataOps.MovLm             ,"mov.l rm,@-rn"));
        Add(new InstructionMeta("0110nnnnmmmm0100", DataOps.MovBp             ,"mov.b @rm+,rn"));
        Add(new InstructionMeta("0110nnnnmmmm0101", DataOps.MovWp             ,"mov.w @rm+,rn"));
        Add(new InstructionMeta("0110nnnnmmmm0110", DataOps.MovLp             ,"mov.l @rm+,rn"));
        Add(new InstructionMeta("10000000nnnndddd", DataOps.MovBs4            ,"mov.b r0, @(disp:4,rm)"));
        Add(new InstructionMeta("10000001nnnndddd", DataOps.MovWs4            ,"mov.w r0, @(disp:4,rm)"));
        Add(new InstructionMeta("0001nnnnmmmmdddd", DataOps.MovLs4            ,"mov.l rm, @(disp:4,rn)"));
        Add(new InstructionMeta("10000100mmmmdddd", DataOps.MovBl4            ,"mov.b @(disp:4,rm), r0"));
        Add(new InstructionMeta("10000101mmmmdddd", DataOps.MovWl4            ,"mov.w @(disp:4,rm), r0"));
        Add(new InstructionMeta("0101nnnnmmmmdddd", DataOps.MovLl4            ,"mov.l @(disp:4,rm), rn"));
        Add(new InstructionMeta("0000nnnnmmmm0100", DataOps.MovBs0            ,"mov.b rm, @(r0,rn)"));
        Add(new InstructionMeta("0000nnnnmmmm0101", DataOps.MovWs0            ,"mov.w rm, @(r0,rn)"));
        Add(new InstructionMeta("0000nnnnmmmm0110", DataOps.MovLs0            ,"mov.l rm, @(r0,rn)"));
        Add(new InstructionMeta("0000nnnnmmmm1100", DataOps.MovBl0            ,"mov.b @(r0,rm), rn"));
        Add(new InstructionMeta("0000nnnnmmmm1101", DataOps.MovWl0            ,"mov.w @(r0,rm), rn"));
        Add(new InstructionMeta("0000nnnnmmmm1110", DataOps.MovLl0            ,"mov.l @(r0,rm), rn"));
        Add(new InstructionMeta("11000000dddddddd", DataOps.MovBsg            ,"mov.b r0, @(disp:8,gbr)"));
        Add(new InstructionMeta("11000001dddddddd", DataOps.MovWsg            ,"mov.w r0, @(disp:8,gbr)"));
        Add(new InstructionMeta("11000010dddddddd", DataOps.MovLsg            ,"mov.l r0, @(disp:8,gbr)"));
        Add(new InstructionMeta("11000100dddddddd", DataOps.MovBlg            ,"mov.b @(disp:8,gbr), r0"));
        Add(new InstructionMeta("11000101dddddddd", DataOps.MovWlg            ,"mov.w @(disp:8,gbr), r0"));
        Add(new InstructionMeta("11000110dddddddd", DataOps.MovLlg            ,"mov.l @(disp:8,gbr), r0"));
        Add(new InstructionMeta("11000111dddddddd", DataOps.MovA              ,"mova (disp:8,pc), r0"));
        Add(new InstructionMeta("0000nnnn00101001", DataOps.MovT              ,"movt rn"));
        Add(new InstructionMeta("0110nnnnmmmm1000", DataOps.Swapb             ,"swap.b rm, rn"));
        Add(new InstructionMeta("0110nnnnmmmm1001", DataOps.Swapw             ,"swap.w rm, rn"));
        Add(new InstructionMeta("0010nnnnmmmm1101", DataOps.Xtrct             ,"xtrct rm, rn"));
        Add(new InstructionMeta("0011nnnnmmmm1100", ArithmeticOps.Add         ,"add rm, rn"));
        Add(new InstructionMeta("0111nnnniiiiiiii", ArithmeticOps.AddI        ,"add #imm8, rn"));
        Add(new InstructionMeta("0011nnnnmmmm1110", ArithmeticOps.AddC        ,"addc rm, rn"));
        Add(new InstructionMeta("0011nnnnmmmm1111", ArithmeticOps.AddV        ,"addv rm, rn"));
        Add(new InstructionMeta("10001000iiiiiiii", CompareOps.CmpEqI         ,"cmp/eq #imm8, r0"));
        Add(new InstructionMeta("0011nnnnmmmm0000", CompareOps.CmpEq          ,"cmp/eq rm, rn"));
        Add(new InstructionMeta("0011nnnnmmmm0010", CompareOps.CmpHs          ,"cmp/hs rm, rn"));
        Add(new InstructionMeta("0011nnnnmmmm0011", CompareOps.CmpGe          ,"cmp/ge rm, rn"));
        Add(new InstructionMeta("0011nnnnmmmm0110", CompareOps.CmpHi          ,"cmp/hi rm, rn"));
        Add(new InstructionMeta("0011nnnnmmmm0111", CompareOps.CmpGt          ,"cmp/gt rm, rn"));
        Add(new InstructionMeta("0100nnnn00010001", CompareOps.CmpPz          ,"cmp/pz rn"));
        Add(new InstructionMeta("0100nnnn00010101", CompareOps.CmpPl          ,"cmp/pl rn"));
        Add(new InstructionMeta("0010nnnnmmmm1100", CompareOps.CmpStr         ,"cmp/str rm, rn"));
        Add(new InstructionMeta("0010nnnnmmmm0111", UnknownOps.Unimplemented  ,"div0s rm, rn"));
        Add(new InstructionMeta("0000000000011001", UnknownOps.Unimplemented  ,"div0u"));
        Add(new InstructionMeta("0011nnnnmmmm0100", UnknownOps.Unimplemented  ,"div1 rm, rn"));
        Add(new InstructionMeta("0011nnnnmmmm1101", UnknownOps.Unimplemented  ,"dmuls.l rm, rn"));
        Add(new InstructionMeta("0011nnnnmmmm0101", UnknownOps.Unimplemented  ,"dmulu.l rm, rn"));
        Add(new InstructionMeta("0100nnnn00010000", ArithmeticOps.Dt          ,"dt rn"));
        Add(new InstructionMeta("0110nnnnmmmm1110", ExtOps.Extsb              ,"exts.b rm, rn"));
        Add(new InstructionMeta("0110nnnnmmmm1111", ExtOps.Extsw              ,"exts.w rm, rn"));
        Add(new InstructionMeta("0110nnnnmmmm1100", ExtOps.Extub              ,"extu.b rm, rn"));
        Add(new InstructionMeta("0110nnnnmmmm1101", ExtOps.Extuw              ,"extu.w rm, rn"));
        Add(new InstructionMeta("0000nnnnmmmm1111", UnknownOps.Unimplemented  ,"mac.l @rm+, @rn+"));
        Add(new InstructionMeta("0100nnnnmmmm1111", UnknownOps.Unimplemented  ,"mac.w @rm+, @rn+"));
        Add(new InstructionMeta("0000nnnnmmmm0111", ArithmeticOps.Mull        ,"mul.l rm, rn"));
        Add(new InstructionMeta("0010nnnnmmmm1111", ArithmeticOps.Muls        ,"muls rm, rn"));
        Add(new InstructionMeta("0010nnnnmmmm1110", ArithmeticOps.Mulu        ,"mulu rm, rn"));
        Add(new InstructionMeta("0110nnnnmmmm1011", ArithmeticOps.Neg         ,"neg rm, rn"));
        Add(new InstructionMeta("0110nnnnmmmm1010", ArithmeticOps.NegC        ,"negc rm, rn"));
        Add(new InstructionMeta("0011nnnnmmmm1000", ArithmeticOps.Sub         ,"sub rm, rn"));
        Add(new InstructionMeta("0011nnnnmmmm1010", ArithmeticOps.SubC        ,"subc rm, rn"));
        Add(new InstructionMeta("0011nnnnmmmm1011", ArithmeticOps.SubV        ,"subv rm, rn"));
        Add(new InstructionMeta("0010nnnnmmmm1001", BitwiseOps.And            ,"and rm, rn"));
        Add(new InstructionMeta("11001001iiiiiiii", BitwiseOps.AndI           ,"and #imm8, r0"));
        Add(new InstructionMeta("11001101iiiiiiii", BitwiseOps.AndB           ,"and.b #imm8, @(r0,gbr)"));
        Add(new InstructionMeta("0110nnnnmmmm0111", BitwiseOps.Not            ,"not rm, rn"));
        Add(new InstructionMeta("0010nnnnmmmm1011", BitwiseOps.Or             ,"or rm, rn"));
        Add(new InstructionMeta("11001011iiiiiiii", BitwiseOps.OrI            ,"or #imm8, r0"));
        Add(new InstructionMeta("11001111iiiiiiii", BitwiseOps.OrB            ,"or.b #imm8, @(r0,gbr)"));
        Add(new InstructionMeta("0100nnnn00011011", BitwiseOps.Tas            ,"tas.b @rn"));
        Add(new InstructionMeta("0010nnnnmmmm1000", BitwiseOps.Tst            ,"tst rm, rn"));
        Add(new InstructionMeta("11001000iiiiiiii", BitwiseOps.TstI           ,"tst #imm8, r0"));
        Add(new InstructionMeta("11001100iiiiiiii", BitwiseOps.TstB           ,"tst.b #imm8, @(r0,gbr)"));
        Add(new InstructionMeta("0010nnnnmmmm1010", BitwiseOps.Xor            ,"xor rm, rn"));
        Add(new InstructionMeta("11001010iiiiiiii", BitwiseOps.XorI           ,"xor #imm8, r0"));
        Add(new InstructionMeta("11001110iiiiiiii", BitwiseOps.XorB           ,"xor.b #imm8, @(r0,gbr)"));
        Add(new InstructionMeta("0100nnnn00000100", ShiftOps.Rotl             ,"rotl rn"));
        Add(new InstructionMeta("0100nnnn00000101", ShiftOps.Rotr             ,"rotr rn"));
        Add(new InstructionMeta("0100nnnn00100100", ShiftOps.Rotcl            ,"rotcl rn"));
        Add(new InstructionMeta("0100nnnn00100101", ShiftOps.Rotcr            ,"rotcr rn"));
        Add(new InstructionMeta("0100nnnnmmmm1100", ShiftOps.Shad             ,"shad rm, rn"));
        Add(new InstructionMeta("0100nnnn00100000", ShiftOps.Shal             ,"shal rn"));
        Add(new InstructionMeta("0100nnnn00100001", ShiftOps.Shar             ,"shar rn"));
        Add(new InstructionMeta("0100nnnnmmmm1101", ShiftOps.Shld             ,"shld rm, rn"));
        Add(new InstructionMeta("0100nnnn00000000", ShiftOps.Shll             ,"shll rn"));
        Add(new InstructionMeta("0100nnnn00000001", ShiftOps.Shlr             ,"shlr rn"));
        Add(new InstructionMeta("0100nnnn00001000", ShiftOps.Shll2            ,"shll2 rn"));
        Add(new InstructionMeta("0100nnnn00001001", ShiftOps.Shlr2            ,"shlr2 rn"));
        Add(new InstructionMeta("0100nnnn00011000", ShiftOps.Shll8            ,"shll8 rn"));
        Add(new InstructionMeta("0100nnnn00011001", ShiftOps.Shlr8            ,"shlr8 rn"));
        Add(new InstructionMeta("0100nnnn00101000", ShiftOps.Shll16           ,"shll16 rn"));
        Add(new InstructionMeta("0100nnnn00101001", ShiftOps.Shlr16           ,"shlr16 rn"));
        Add(new InstructionMeta("10001011dddddddd", BranchOps.Bf              ,"bf disp:8"));
        Add(new InstructionMeta("10001111dddddddd", BranchOps.Bfs             ,"bfs disp:8"));
        Add(new InstructionMeta("10001001dddddddd", BranchOps.Bt              ,"bt disp:8"));
        Add(new InstructionMeta("10001101dddddddd", BranchOps.Bts             ,"bts disp:8"));
        Add(new InstructionMeta("1010dddddddddddd", BranchOps.Bra             ,"bra disp:12"));
        Add(new InstructionMeta("0000nnnn00100011", BranchOps.Braf            ,"braf rn"));
        Add(new InstructionMeta("1011dddddddddddd", BranchOps.Bsr             ,"bsr disp:12"));
        Add(new InstructionMeta("0000nnnn00000011", BranchOps.Bsrf            ,"bsrf rn"));
        Add(new InstructionMeta("0100nnnn00101011", BranchOps.Jmp             ,"jmp @rm"));
        Add(new InstructionMeta("0100nnnn00001011", BranchOps.Jsr             ,"jsr @rn"));
        Add(new InstructionMeta("0000000000001011", BranchOps.Rts             ,"rts"));
        Add(new InstructionMeta("0000000000101000", ArithmeticOps.ClrMac      ,"clrmac"));
        Add(new InstructionMeta("0000000001001000", FlagOps.ClrS              ,"clrs"));
        Add(new InstructionMeta("0000000000001000", FlagOps.ClrT              ,"clrt"));
        Add(new InstructionMeta("0100mmmm00001110", ControlOps.LdcSr          ,"ldc rn, sr"));
        Add(new InstructionMeta("0100mmmm00011110", ControlOps.LdcGbr         ,"ldc rn, gbr"));
        Add(new InstructionMeta("0100mmmm00101110", ControlOps.LdcVbr         ,"ldc rn, vbr"));
        Add(new InstructionMeta("0100mmmm00111110", ControlOps.LdcSsr         ,"ldc rn, ssr"));
        Add(new InstructionMeta("0100mmmm01001110", ControlOps.LdcSpc         ,"ldc rn, spc"));
        Add(new InstructionMeta("0100mmmm11111010", ControlOps.LdcDbr         ,"ldc rn, dbr"));
        Add(new InstructionMeta("0100mmmm1nnn1110", ControlOps.LdcRbank       ,"ldc.l rn, rn_bank"));
        Add(new InstructionMeta("0100mmmm00000111", ControlOps.LdcmSr         ,"ldc.l @rn+, sr"));
        Add(new InstructionMeta("0100mmmm00010111", ControlOps.LdcmGbr        ,"ldc.l @rn+, gbr"));
        Add(new InstructionMeta("0100mmmm00100111", ControlOps.LdcmVbr        ,"ldc.l @rn+, vbr"));
        Add(new InstructionMeta("0100mmmm00110111", ControlOps.LdcmSsr        ,"ldc.l @rn+, ssr"));
        Add(new InstructionMeta("0100mmmm01000111", ControlOps.LdcmSpc        ,"ldc.l @rn+, spc"));
        Add(new InstructionMeta("0100mmmm11110110", ControlOps.LdcmDbr        ,"ldc.l @rn+, dbr"));
        Add(new InstructionMeta("0100mmmm1nnn0111", ControlOps.LdcmRbank      ,"ldc.l @rn+, rm_bank"));
        Add(new InstructionMeta("0100mmmm00001010", ControlOps.LdsMach        ,"lds rn, mach"));
        Add(new InstructionMeta("0100mmmm00011010", ControlOps.LdsMacl        ,"lds rn, macl"));
        Add(new InstructionMeta("0100mmmm00101010", ControlOps.LdsPr          ,"lds rn, pr"));
        Add(new InstructionMeta("0100mmmm00000110", ControlOps.LdsmMach       ,"lds.l @rn+, mach"));
        Add(new InstructionMeta("0100mmmm00010110", ControlOps.LdsmMacl       ,"lds.l @rn+, macl"));
        Add(new InstructionMeta("0100mmmm00100110", ControlOps.LdsmPr         ,"lds.l @rn+, pr"));
        Add(new InstructionMeta("0000000000111000", UnknownOps.Unimplemented  ,"ldtlb"));
        Add(new InstructionMeta("0000nnnn11000011", UnknownOps.Unimplemented  ,"movca.l r0, @rn"));
        Add(new InstructionMeta("0000000000001001", UnknownOps.Unimplemented  ,"nop"));
        Add(new InstructionMeta("0000nnnn10010011", UnknownOps.Unimplemented  ,"ocbi"));
        Add(new InstructionMeta("0000nnnn10100011", UnknownOps.Unimplemented  ,"ocbp"));
        Add(new InstructionMeta("0000nnnn10110011", UnknownOps.Unimplemented  ,"ocbwb"));
        Add(new InstructionMeta("0000nnnn10000011", UnknownOps.Unimplemented  ,"pref @rn"));
        Add(new InstructionMeta("0000000000101011", UnknownOps.Unimplemented  ,"rte"));
        Add(new InstructionMeta("0000000001011000", FlagOps.SetS              ,"sets"));
        Add(new InstructionMeta("0000000000011000", FlagOps.SetT              ,"sett"));
        Add(new InstructionMeta("0000000000011011", UnknownOps.Unimplemented  ,"sleep"));
        Add(new InstructionMeta("0000nnnn00000010", ControlOps.StcSr          ,"stc sr, rn"));
        Add(new InstructionMeta("0000nnnn00010010", ControlOps.StcGbr         ,"stc gbr, rn"));
        Add(new InstructionMeta("0000nnnn00100010", ControlOps.StcVbr         ,"stc vbr, rn"));
        Add(new InstructionMeta("0000nnnn00110010", ControlOps.StcSsr         ,"stc ssr, rn"));
        Add(new InstructionMeta("0000nnnn01000010", ControlOps.StcSpc         ,"stc spc, rn"));
        Add(new InstructionMeta("0000nnnn00111010", ControlOps.StcSgr         ,"stc sgr, rn"));
        Add(new InstructionMeta("0000nnnn11111010", ControlOps.StcDbr         ,"stc dbr, rn"));
        Add(new InstructionMeta("0000nnnn1mmm0010", ControlOps.StcRbank       ,"stc rm_bank, rn"));
        Add(new InstructionMeta("0100nnnn00000011", ControlOps.StcmSr         ,"stc.l sr, @-rn"));
        Add(new InstructionMeta("0100nnnn00010011", ControlOps.StcmGbr        ,"stc.l gbr, @-rn"));
        Add(new InstructionMeta("0100nnnn00100011", ControlOps.StcmVbr        ,"stc.l vbr, @-rn"));
        Add(new InstructionMeta("0100nnnn00110011", ControlOps.StcmSsr        ,"stc.l ssr, @-rn"));
        Add(new InstructionMeta("0100nnnn01000011", ControlOps.StcmSpc        ,"stc.l spc, @-rn"));
        Add(new InstructionMeta("0100nnnn00110010", ControlOps.StcmSgr        ,"stc.l sgr, @-rn"));
        Add(new InstructionMeta("0100nnnn11110010", ControlOps.StcmDbr        ,"stc.l dbr, @-rn"));
        Add(new InstructionMeta("0100nnnn1mmm0011", ControlOps.StcmRbank      ,"stc.l rm_bank, @-rn"));
        Add(new InstructionMeta("0000nnnn00001010", ControlOps.StsMach        ,"sts mach, rn"));
        Add(new InstructionMeta("0000nnnn00011010", ControlOps.StsMacl        ,"sts macl, rn"));
        Add(new InstructionMeta("0000nnnn00101010", ControlOps.StsPr          ,"sts pr, rn"));
        Add(new InstructionMeta("0100nnnn00000010", ControlOps.StsmMach       ,"sts.l mach, @-rn"));
        Add(new InstructionMeta("0100nnnn00010010", ControlOps.StsmMacl       ,"sts.l macl, @-rn"));
        Add(new InstructionMeta("0100nnnn00100010", ControlOps.StsmPr         ,"sts.l pr, @-rn"));
        Add(new InstructionMeta("11000011iiiiiiii", UnknownOps.Unimplemented  ,"trapa #imm8"));
        Add(new InstructionMeta("1111nnnn10001101", UnknownOps.Unimplemented  ,"fldi0 frn"));
        Add(new InstructionMeta("1111nnnn10011101", UnknownOps.Unimplemented  ,"fldi1 frn"));
        Add(new InstructionMeta("1111nnnnmmmm1100", UnknownOps.Unimplemented  ,"fmov frm, frn"));
        Add(new InstructionMeta("1111nnnnmmmm1000", UnknownOps.Unimplemented  ,"fmov.s @(rm), frn"));
        Add(new InstructionMeta("1111nnnnmmmm0110", UnknownOps.Unimplemented  ,"fmov.s @(r0,rm), frn"));
        Add(new InstructionMeta("1111nnnnmmmm1010", UnknownOps.Unimplemented  ,"fmov.s frm, @rn"));
        Add(new InstructionMeta("1111nnnnmmmm0111", UnknownOps.Unimplemented  ,"fmov.s frm, @(r0,rn)"));
        Add(new InstructionMeta("1111nnnnmmmm1011", UnknownOps.Unimplemented  ,"fmov.s frm, @-rn"));
        Add(new InstructionMeta("1111nnnnmmmm1001", UnknownOps.Unimplemented  ,"fmov.s @rm+, frn"));
        Add(new InstructionMeta("1111mmmm00011101", UnknownOps.Unimplemented  ,"flds frn, fpul"));
        Add(new InstructionMeta("1111nnnn00001101", UnknownOps.Unimplemented  ,"fsts fpul, frn"));
        Add(new InstructionMeta("1111nnnn01011101", UnknownOps.Unimplemented  ,"fabs frn"));
        Add(new InstructionMeta("1111nnnn01111101", UnknownOps.Unimplemented  ,"fsrra frn"));
        Add(new InstructionMeta("1111nnnnmmmm0000", UnknownOps.Unimplemented  ,"fadd frm, frn"));
        Add(new InstructionMeta("1111nnnnmmmm0100", UnknownOps.Unimplemented  ,"fcmp/eq frm, frn"));
        Add(new InstructionMeta("1111nnnnmmmm0101", UnknownOps.Unimplemented  ,"fcmp/gt frm, frn"));
        Add(new InstructionMeta("1111nnnnmmmm0011", UnknownOps.Unimplemented  ,"fdiv frm, frn"));
        Add(new InstructionMeta("1111nnnn00101101", UnknownOps.Unimplemented  ,"float fpul, frn"));
        Add(new InstructionMeta("1111nnnnmmmm1110", UnknownOps.Unimplemented  ,"fmac fr0, frm, frn"));
        Add(new InstructionMeta("1111nnnnmmmm0010", UnknownOps.Unimplemented  ,"fmul frm, frn"));
        Add(new InstructionMeta("1111nnnn01001101", UnknownOps.Unimplemented  ,"fneg frn"));
        Add(new InstructionMeta("1111nnnn01101101", UnknownOps.Unimplemented  ,"fsqrt frn"));
        Add(new InstructionMeta("1111nnnnmmmm0001", UnknownOps.Unimplemented  ,"fsub frm, frn"));
        Add(new InstructionMeta("1111mmmm00111101", UnknownOps.Unimplemented  ,"ftrc frn, fpul"));
        Add(new InstructionMeta("1111mmmm10111101", UnknownOps.Unimplemented  ,"fcnvds drn, fpul"));
        Add(new InstructionMeta("1111nnnn10101101", UnknownOps.Unimplemented  ,"fcnvsd fpul, drn"));
        Add(new InstructionMeta("0100mmmm01101010", UnknownOps.Unimplemented  ,"lds rn, fpscr"));
        Add(new InstructionMeta("0100mmmm01011010", UnknownOps.Unimplemented  ,"lds rn, fpul"));
        Add(new InstructionMeta("0100mmmm01100110", UnknownOps.Unimplemented  ,"lds.l @rn+, fpscr"));
        Add(new InstructionMeta("0100mmmm01010110", UnknownOps.Unimplemented  ,"lds.l @rn+, fpul"));
        Add(new InstructionMeta("0000nnnn01101010", UnknownOps.Unimplemented  ,"sts fpscr, rn"));
        Add(new InstructionMeta("0000nnnn01011010", UnknownOps.Unimplemented  ,"sts fpul, rn"));
        Add(new InstructionMeta("0100nnnn01100010", UnknownOps.Unimplemented  ,"sts.l fpscr, @-rn"));
        Add(new InstructionMeta("0100nnnn01010010", UnknownOps.Unimplemented  ,"sts.l fpul, @-rn"));
        Add(new InstructionMeta("1111nnmm11101101", UnknownOps.Unimplemented  ,"fipr fvm, fvn"));
        Add(new InstructionMeta("1111nnn011111101", UnknownOps.Unimplemented  ,"fsca fpul, drn"));
        Add(new InstructionMeta("1111nn0111111101", UnknownOps.Unimplemented  ,"ftrv xmtrx, fvn"));
        Add(new InstructionMeta("1111101111111101", UnknownOps.Unimplemented  ,"frchg"));
        Add(new InstructionMeta("1111001111111101", UnknownOps.Unimplemented  ,"fschg"));
        #endregion
        // @formatter:on
        
        BuildLookupTable();
    }
    
    private static void BuildLookupTable()
    {
        _lookupTable = new Instruction[ushort.MaxValue + 1];
        // Itera sobre cada opcode possível de 0x0000 a 0xFFFF.
        for (int i = 0; i <= ushort.MaxValue; i++)
        {
            ushort opcode = (ushort)i;
            
            // Encontra a primeira instrução que corresponde ao opcode.
            foreach (var instruction in _metaInstructions)
            {
                if ((opcode & instruction.Mask) == instruction.BitPattern)
                {
                    _lookupTable[opcode] = new Instruction()
                    {
                        Emit = instruction.Emit,
                    };
                    break;
                }
            }
        }
    }
    
    private static (ushort mask, ushort pattern) ParsePattern(string binaryPattern)
    {
        ushort mask = 0;
        ushort pattern = 0;

        for (int i = 0; i < 16; i++)
        {
            char bit = binaryPattern[i];
            int bitPos = 15 - i;

            switch (bit)
            {
                case '0':
                    mask |= (ushort)(1 << bitPos);
                    break;
                case '1':
                    mask |= (ushort)(1 << bitPos);
                    pattern |= (ushort)(1 << bitPos);
                    break;
                case 'n':
                case 'm':
                case 'd':
                case 'i':
                    break;
            }
        }

        return (mask, pattern);
    }
}