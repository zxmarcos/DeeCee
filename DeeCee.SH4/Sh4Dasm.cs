namespace DeeCee.SH4;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class SH4Dasm
{
    // Estrutura para representar uma instrução decodificada
    public struct DecodedInstruction
    {
        public string Mnemonic { get; set; }
        public string FullInstruction { get; set; }
        public ushort Opcode { get; set; }
        public bool IsValid { get; set; }
    }

    // Estrutura interna para padrões de instruções
    private struct InstructionPattern
    {
        public string Template { get; set; }
        public ushort Mask { get; set; }
        public ushort Pattern { get; set; }
        public Func<ushort, string> Formatter { get; set; }
    }

    private readonly List<InstructionPattern> _instructions;
    private readonly InstructionPattern?[] _lookupTable;

    public SH4Dasm()
    {
        _instructions = new List<InstructionPattern>();
        _lookupTable = new InstructionPattern?[65536];
        InitializeInstructions();
        BuildLookupTable();
    }
    
    /// <summary>
    /// Pré-calcula e armazena o padrão de instrução para cada opcode possível.
    /// Isso evita a busca linear no método Disassemble.
    /// </summary>
    private void BuildLookupTable()
    {
        // Itera sobre cada opcode possível de 0x0000 a 0xFFFF.
        for (int i = 0; i <= ushort.MaxValue; i++)
        {
            ushort opcode = (ushort)i;
            
            // Encontra a primeira instrução que corresponde ao opcode.
            // A ordem em InitializeInstructions() é importante, pois as instruções
            // mais específicas devem vir primeiro.
            foreach (var instruction in _instructions)
            {
                if ((opcode & instruction.Mask) == instruction.Pattern)
                {
                    _lookupTable[opcode] = instruction;
                    break; // Encontrou o padrão, vai para o próximo opcode.
                }
            }
        }
    }
    
    private void InitializeInstructions()
    {
        // Instruções MOV
        AddInstruction("mov #imm8, rn", "1110nnnniiiiiiii", (op) => {
            int rn = (op >> 8) & 0xF;
            int imm8 = op & 0xFF;
            int sext_imm = (imm8 & 0x80) != 0 ? (int)(imm8 | 0xFFFFFF00) : imm8;
            return $"mov #{sext_imm}, r{rn}";
        });

        AddInstruction("mov.w @(disp:8,pc), rn", "1001nnnndddddddd", (op) => {
            int rn = (op >> 8) & 0xF;
            int disp = op & 0xFF;
            return $"mov.w @({disp * 2},pc), r{rn}";
        });

        AddInstruction("mov.l @(disp:8,pc), rn", "1101nnnndddddddd", (op) => {
            int rn = (op >> 8) & 0xF;
            int disp = op & 0xFF;
            return $"mov.l @({disp * 4},pc), r{rn}";
        });

        AddInstruction("mov rm, rn", "0110nnnnmmmm0011", (op) => {
            int rn = (op >> 8) & 0xF;
            int rm = (op >> 4) & 0xF;
            return $"mov r{rm}, r{rn}";
        });

        AddInstruction("mov.b rm, @rn", "0010nnnnmmmm0000", (op) => {
            int rn = (op >> 8) & 0xF;
            int rm = (op >> 4) & 0xF;
            return $"mov.b r{rm}, @r{rn}";
        });

        AddInstruction("mov.w rm, @rn", "0010nnnnmmmm0001", (op) => {
            int rn = (op >> 8) & 0xF;
            int rm = (op >> 4) & 0xF;
            return $"mov.w r{rm}, @r{rn}";
        });

        AddInstruction("mov.l rm, @rn", "0010nnnnmmmm0010", (op) => {
            int rn = (op >> 8) & 0xF;
            int rm = (op >> 4) & 0xF;
            return $"mov.l r{rm}, @r{rn}";
        });

        AddInstruction("mov.b @rm, rn", "0110nnnnmmmm0000", (op) => {
            int rn = (op >> 8) & 0xF;
            int rm = (op >> 4) & 0xF;
            return $"mov.b @r{rm}, r{rn}";
        });

        AddInstruction("mov.w @rm, rn", "0110nnnnmmmm0001", (op) => {
            int rn = (op >> 8) & 0xF;
            int rm = (op >> 4) & 0xF;
            return $"mov.w @r{rm}, r{rn}";
        });

        AddInstruction("mov.l @rm, rn", "0110nnnnmmmm0010", (op) => {
            int rn = (op >> 8) & 0xF;
            int rm = (op >> 4) & 0xF;
            return $"mov.l @r{rm}, r{rn}";
        });

        AddInstruction("mov.b rm,@-rn", "0010nnnnmmmm0100", (op) => {
            int rn = (op >> 8) & 0xF;
            int rm = (op >> 4) & 0xF;
            return $"mov.b r{rm}, @-r{rn}";
        });

        AddInstruction("mov.w rm,@-rn", "0010nnnnmmmm0101", (op) => {
            int rn = (op >> 8) & 0xF;
            int rm = (op >> 4) & 0xF;
            return $"mov.w r{rm}, @-r{rn}";
        });

        AddInstruction("mov.l rm,@-rn", "0010nnnnmmmm0110", (op) => {
            int rn = (op >> 8) & 0xF;
            int rm = (op >> 4) & 0xF;
            return $"mov.l r{rm}, @-r{rn}";
        });

        AddInstruction("mov.b @rm+,rn", "0110nnnnmmmm0100", (op) => {
            int rn = (op >> 8) & 0xF;
            int rm = (op >> 4) & 0xF;
            return $"mov.b @r{rm}+, r{rn}";
        });

        AddInstruction("mov.w @rm+,rn", "0110nnnnmmmm0101", (op) => {
            int rn = (op >> 8) & 0xF;
            int rm = (op >> 4) & 0xF;
            return $"mov.w @r{rm}+, r{rn}";
        });

        AddInstruction("mov.l @rm+,rn", "0110nnnnmmmm0110", (op) => {
            int rn = (op >> 8) & 0xF;
            int rm = (op >> 4) & 0xF;
            return $"mov.l @r{rm}+, r{rn}";
        });

        // Instruções MOV com deslocamento
        AddInstruction("mov.b r0, @(disp:4,rm)", "10000000nnnndddd", (op) => {
            int rm = (op >> 4) & 0xF;
            int disp = op & 0xF;
            return $"mov.b r0, @({disp},r{rm})";
        });

        AddInstruction("mov.w r0, @(disp:4,rm)", "10000001nnnndddd", (op) => {
            int rm = (op >> 4) & 0xF;
            int disp = op & 0xF;
            return $"mov.w r0, @({disp * 2},r{rm})";
        });

        AddInstruction("mov.l rm, @(disp:4,rn)", "0001nnnnmmmmdddd", (op) => {
            int rn = (op >> 8) & 0xF;
            int rm = (op >> 4) & 0xF;
            int disp = op & 0xF;
            return $"mov.l r{rm}, @({disp * 4},r{rn})";
        });

        AddInstruction("mov.b @(disp:4,rm), r0", "10000100mmmmdddd", (op) => {
            int rm = (op >> 4) & 0xF;
            int disp = op & 0xF;
            return $"mov.b @({disp},r{rm}), r0";
        });

        AddInstruction("mov.w @(disp:4,rm), r0", "10000101mmmmdddd", (op) => {
            int rm = (op >> 4) & 0xF;
            int disp = op & 0xF;
            return $"mov.w @({disp * 2},r{rm}), r0";
        });

        AddInstruction("mov.l @(disp:4,rm), rn", "0101nnnnmmmmdddd", (op) => {
            int rn = (op >> 8) & 0xF;
            int rm = (op >> 4) & 0xF;
            int disp = op & 0xF;
            return $"mov.l @({disp * 4},r{rm}), r{rn}";
        });

        // Instruções MOV com r0 como índice
        AddInstruction("mov.b rm, @(r0,rn)", "0000nnnnmmmm0100", (op) => {
            int rn = (op >> 8) & 0xF;
            int rm = (op >> 4) & 0xF;
            return $"mov.b r{rm}, @(r0,r{rn})";
        });

        AddInstruction("mov.w rm, @(r0,rn)", "0000nnnnmmmm0101", (op) => {
            int rn = (op >> 8) & 0xF;
            int rm = (op >> 4) & 0xF;
            return $"mov.w r{rm}, @(r0,r{rn})";
        });

        AddInstruction("mov.l rm, @(r0,rn)", "0000nnnnmmmm0110", (op) => {
            int rn = (op >> 8) & 0xF;
            int rm = (op >> 4) & 0xF;
            return $"mov.l r{rm}, @(r0,r{rn})";
        });

        AddInstruction("mov.b @(r0,rm), rn", "0000nnnnmmmm1100", (op) => {
            int rn = (op >> 8) & 0xF;
            int rm = (op >> 4) & 0xF;
            return $"mov.b @(r0,r{rm}), r{rn}";
        });

        AddInstruction("mov.w @(r0,rm), rn", "0000nnnnmmmm1101", (op) => {
            int rn = (op >> 8) & 0xF;
            int rm = (op >> 4) & 0xF;
            return $"mov.w @(r0,r{rm}), r{rn}";
        });

        AddInstruction("mov.l @(r0,rm), rn", "0000nnnnmmmm1110", (op) => {
            int rn = (op >> 8) & 0xF;
            int rm = (op >> 4) & 0xF;
            return $"mov.l @(r0,r{rm}), r{rn}";
        });

        // Instruções MOV com GBR
        AddInstruction("mov.b r0, @(disp:8,gbr)", "11000000dddddddd", (op) => {
            int disp = op & 0xFF;
            return $"mov.b r0, @({disp},gbr)";
        });

        AddInstruction("mov.w r0, @(disp:8,gbr)", "11000001dddddddd", (op) => {
            int disp = op & 0xFF;
            return $"mov.w r0, @({disp * 2},gbr)";
        });

        AddInstruction("mov.l r0, @(disp:8,gbr)", "11000010dddddddd", (op) => {
            int disp = op & 0xFF;
            return $"mov.l r0, @({disp * 4},gbr)";
        });

        AddInstruction("mov.b @(disp:8,gbr), r0", "11000100dddddddd", (op) => {
            int disp = op & 0xFF;
            return $"mov.b @({disp},gbr), r0";
        });

        AddInstruction("mov.w @(disp:8,gbr), r0", "11000101dddddddd", (op) => {
            int disp = op & 0xFF;
            return $"mov.w @({disp * 2},gbr), r0";
        });

        AddInstruction("mov.l @(disp:8,gbr), r0", "11000110dddddddd", (op) => {
            int disp = op & 0xFF;
            return $"mov.l @({disp * 4},gbr), r0";
        });

        // Outras instruções MOV
        AddInstruction("mova (disp:8,pc), r0", "11000111dddddddd", (op) => {
            int disp = op & 0xFF;
            return $"mova @({disp * 4},pc), r0";
        });

        AddInstruction("movt rn", "0000nnnn00101001", (op) => {
            int rn = (op >> 8) & 0xF;
            return $"movt r{rn}";
        });

        // Instruções de manipulação de dados
        AddInstruction("swap.b rm, rn", "0110nnnnmmmm1000", (op) => {
            int rn = (op >> 8) & 0xF;
            int rm = (op >> 4) & 0xF;
            return $"swap.b r{rm}, r{rn}";
        });

        AddInstruction("swap.w rm, rn", "0110nnnnmmmm1001", (op) => {
            int rn = (op >> 8) & 0xF;
            int rm = (op >> 4) & 0xF;
            return $"swap.w r{rm}, r{rn}";
        });

        AddInstruction("xtrct rm, rn", "0010nnnnmmmm1101", (op) => {
            int rn = (op >> 8) & 0xF;
            int rm = (op >> 4) & 0xF;
            return $"xtrct r{rm}, r{rn}";
        });

        // Instruções aritméticas
        AddInstruction("add rm, rn", "0011nnnnmmmm1100", (op) => {
            int rn = (op >> 8) & 0xF;
            int rm = (op >> 4) & 0xF;
            return $"add r{rm}, r{rn}";
        });

        AddInstruction("add #imm8, rn", "0111nnnniiiiiiii", (op) => {
            int rn = (op >> 8) & 0xF;
            int imm8 = op & 0xFF;
            int sext_imm = (imm8 & 0x80) != 0 ? (int)(imm8 | 0xFFFFFF00) : imm8;
            return $"add #{sext_imm}, r{rn}";
        });

        AddInstruction("addc rm, rn", "0011nnnnmmmm1110", (op) => {
            int rn = (op >> 8) & 0xF;
            int rm = (op >> 4) & 0xF;
            return $"addc r{rm}, r{rn}";
        });

        AddInstruction("addv rm, rn", "0011nnnnmmmm1111", (op) => {
            int rn = (op >> 8) & 0xF;
            int rm = (op >> 4) & 0xF;
            return $"addv r{rm}, r{rn}";
        });

        // Instruções de comparação
        AddInstruction("cmp/eq #imm8, r0", "10001000iiiiiiii", (op) => {
            int imm8 = op & 0xFF;
            int sext_imm = (imm8 & 0x80) != 0 ? (int)(imm8 | 0xFFFFFF00) : imm8;
            return $"cmp/eq #{sext_imm}, r0";
        });

        AddInstruction("cmp/eq rm, rn", "0011nnnnmmmm0000", (op) => {
            int rn = (op >> 8) & 0xF;
            int rm = (op >> 4) & 0xF;
            return $"cmp/eq r{rm}, r{rn}";
        });

        AddInstruction("cmp/hs rm, rn", "0011nnnnmmmm0010", (op) => {
            int rn = (op >> 8) & 0xF;
            int rm = (op >> 4) & 0xF;
            return $"cmp/hs r{rm}, r{rn}";
        });

        AddInstruction("cmp/ge rm, rn", "0011nnnnmmmm0011", (op) => {
            int rn = (op >> 8) & 0xF;
            int rm = (op >> 4) & 0xF;
            return $"cmp/ge r{rm}, r{rn}";
        });

        AddInstruction("cmp/hi rm, rn", "0011nnnnmmmm0110", (op) => {
            int rn = (op >> 8) & 0xF;
            int rm = (op >> 4) & 0xF;
            return $"cmp/hi r{rm}, r{rn}";
        });

        AddInstruction("cmp/gt rm, rn", "0011nnnnmmmm0111", (op) => {
            int rn = (op >> 8) & 0xF;
            int rm = (op >> 4) & 0xF;
            return $"cmp/gt r{rm}, r{rn}";
        });

        AddInstruction("cmp/pz rn", "0100nnnn00010001", (op) => {
            int rn = (op >> 8) & 0xF;
            return $"cmp/pz r{rn}";
        });

        AddInstruction("cmp/pl rn", "0100nnnn00010101", (op) => {
            int rn = (op >> 8) & 0xF;
            return $"cmp/pl r{rn}";
        });

        AddInstruction("cmp/str rm, rn", "0010nnnnmmmm1100", (op) => {
            int rn = (op >> 8) & 0xF;
            int rm = (op >> 4) & 0xF;
            return $"cmp/str r{rm}, r{rn}";
        });

        // Instruções de divisão
        AddInstruction("div0s rm, rn", "0010nnnnmmmm0111", (op) => {
            int rn = (op >> 8) & 0xF;
            int rm = (op >> 4) & 0xF;
            return $"div0s r{rm}, r{rn}";
        });

        AddInstruction("div0u", "0000000000011001", (op) => "div0u");

        AddInstruction("div1 rm, rn", "0011nnnnmmmm0100", (op) => {
            int rn = (op >> 8) & 0xF;
            int rm = (op >> 4) & 0xF;
            return $"div1 r{rm}, r{rn}";
        });

        // Instruções de multiplicação
        AddInstruction("dmuls.l rm, rn", "0011nnnnmmmm1101", (op) => {
            int rn = (op >> 8) & 0xF;
            int rm = (op >> 4) & 0xF;
            return $"dmuls.l r{rm}, r{rn}";
        });

        AddInstruction("dmulu.l rm, rn", "0011nnnnmmmm0101", (op) => {
            int rn = (op >> 8) & 0xF;
            int rm = (op >> 4) & 0xF;
            return $"dmulu.l r{rm}, r{rn}";
        });

        AddInstruction("mul.l rm, rn", "0000nnnnmmmm0111", (op) => {
            int rn = (op >> 8) & 0xF;
            int rm = (op >> 4) & 0xF;
            return $"mul.l r{rm}, r{rn}";
        });

        AddInstruction("muls rm, rn", "0010nnnnmmmm1111", (op) => {
            int rn = (op >> 8) & 0xF;
            int rm = (op >> 4) & 0xF;
            return $"muls r{rm}, r{rn}";
        });

        AddInstruction("mulu rm, rn", "0010nnnnmmmm1110", (op) => {
            int rn = (op >> 8) & 0xF;
            int rm = (op >> 4) & 0xF;
            return $"mulu r{rm}, r{rn}";
        });

        // Outras instruções aritméticas
        AddInstruction("dt rn", "0100nnnn00010000", (op) => {
            int rn = (op >> 8) & 0xF;
            return $"dt r{rn}";
        });

        AddInstruction("neg rm, rn", "0110nnnnmmmm1011", (op) => {
            int rn = (op >> 8) & 0xF;
            int rm = (op >> 4) & 0xF;
            return $"neg r{rm}, r{rn}";
        });

        AddInstruction("negc rm, rn", "0110nnnnmmmm1010", (op) => {
            int rn = (op >> 8) & 0xF;
            int rm = (op >> 4) & 0xF;
            return $"negc r{rm}, r{rn}";
        });

        AddInstruction("sub rm, rn", "0011nnnnmmmm1000", (op) => {
            int rn = (op >> 8) & 0xF;
            int rm = (op >> 4) & 0xF;
            return $"sub r{rm}, r{rn}";
        });

        AddInstruction("subc rm, rn", "0011nnnnmmmm1010", (op) => {
            int rn = (op >> 8) & 0xF;
            int rm = (op >> 4) & 0xF;
            return $"subc r{rm}, r{rn}";
        });

        AddInstruction("subv rm, rn", "0011nnnnmmmm1011", (op) => {
            int rn = (op >> 8) & 0xF;
            int rm = (op >> 4) & 0xF;
            return $"subv r{rm}, r{rn}";
        });

        // Instruções de extensão de sinal
        AddInstruction("exts.b rm, rn", "0110nnnnmmmm1110", (op) => {
            int rn = (op >> 8) & 0xF;
            int rm = (op >> 4) & 0xF;
            return $"exts.b r{rm}, r{rn}";
        });

        AddInstruction("exts.w rm, rn", "0110nnnnmmmm1111", (op) => {
            int rn = (op >> 8) & 0xF;
            int rm = (op >> 4) & 0xF;
            return $"exts.w r{rm}, r{rn}";
        });

        AddInstruction("extu.b rm, rn", "0110nnnnmmmm1100", (op) => {
            int rn = (op >> 8) & 0xF;
            int rm = (op >> 4) & 0xF;
            return $"extu.b r{rm}, r{rn}";
        });

        AddInstruction("extu.w rm, rn", "0110nnnnmmmm1101", (op) => {
            int rn = (op >> 8) & 0xF;
            int rm = (op >> 4) & 0xF;
            return $"extu.w r{rm}, r{rn}";
        });

        // Instruções lógicas
        AddInstruction("and rm, rn", "0010nnnnmmmm1001", (op) => {
            int rn = (op >> 8) & 0xF;
            int rm = (op >> 4) & 0xF;
            return $"and r{rm}, r{rn}";
        });

        AddInstruction("and #imm8, r0", "11001001iiiiiiii", (op) => {
            int imm8 = op & 0xFF;
            return $"and #0x{imm8:X2}, r0";
        });

        AddInstruction("not rm, rn", "0110nnnnmmmm0111", (op) => {
            int rn = (op >> 8) & 0xF;
            int rm = (op >> 4) & 0xF;
            return $"not r{rm}, r{rn}";
        });

        AddInstruction("or rm, rn", "0010nnnnmmmm1011", (op) => {
            int rn = (op >> 8) & 0xF;
            int rm = (op >> 4) & 0xF;
            return $"or r{rm}, r{rn}";
        });

        AddInstruction("or #imm8, r0", "11001011iiiiiiii", (op) => {
            int imm8 = op & 0xFF;
            return $"or #0x{imm8:X2}, r0";
        });

        AddInstruction("tst rm, rn", "0010nnnnmmmm1000", (op) => {
            int rn = (op >> 8) & 0xF;
            int rm = (op >> 4) & 0xF;
            return $"tst r{rm}, r{rn}";
        });

        AddInstruction("tst #imm8, r0", "11001000iiiiiiii", (op) => {
            int imm8 = op & 0xFF;
            return $"tst #0x{imm8:X2}, r0";
        });

        AddInstruction("xor rm, rn", "0010nnnnmmmm1010", (op) => {
            int rn = (op >> 8) & 0xF;
            int rm = (op >> 4) & 0xF;
            return $"xor r{rm}, r{rn}";
        });

        AddInstruction("xor #imm8, r0", "11001010iiiiiiii", (op) => {
            int imm8 = op & 0xFF;
            return $"xor #0x{imm8:X2}, r0";
        });

        // Instruções de deslocamento e rotação
        AddInstruction("rotl rn", "0100nnnn00000100", (op) => {
            int rn = (op >> 8) & 0xF;
            return $"rotl r{rn}";
        });

        AddInstruction("rotr rn", "0100nnnn00000101", (op) => {
            int rn = (op >> 8) & 0xF;
            return $"rotr r{rn}";
        });

        AddInstruction("rotcl rn", "0100nnnn00100100", (op) => {
            int rn = (op >> 8) & 0xF;
            return $"rotcl r{rn}";
        });

        AddInstruction("rotcr rn", "0100nnnn00100101", (op) => {
            int rn = (op >> 8) & 0xF;
            return $"rotcr r{rn}";
        });

        AddInstruction("shad rm, rn", "0100nnnnmmmm1100", (op) => {
            int rn = (op >> 8) & 0xF;
            int rm = (op >> 4) & 0xF;
            return $"shad r{rm}, r{rn}";
        });

        AddInstruction("shal rn", "0100nnnn00100000", (op) => {
            int rn = (op >> 8) & 0xF;
            return $"shal r{rn}";
        });

        AddInstruction("shar rn", "0100nnnn00100001", (op) => {
            int rn = (op >> 8) & 0xF;
            return $"shar r{rn}";
        });

        AddInstruction("shld rm, rn", "0100nnnnmmmm1101", (op) => {
            int rn = (op >> 8) & 0xF;
            int rm = (op >> 4) & 0xF;
            return $"shld r{rm}, r{rn}";
        });

        AddInstruction("shll rn", "0100nnnn00000000", (op) => {
            int rn = (op >> 8) & 0xF;
            return $"shll r{rn}";
        });

        AddInstruction("shlr rn", "0100nnnn00000001", (op) => {
            int rn = (op >> 8) & 0xF;
            return $"shlr r{rn}";
        });

        AddInstruction("shll2 rn", "0100nnnn00001000", (op) => {
            int rn = (op >> 8) & 0xF;
            return $"shll2 r{rn}";
        });

        AddInstruction("shlr2 rn", "0100nnnn00001001", (op) => {
            int rn = (op >> 8) & 0xF;
            return $"shlr2 r{rn}";
        });

        AddInstruction("shll8 rn", "0100nnnn00011000", (op) => {
            int rn = (op >> 8) & 0xF;
            return $"shll8 r{rn}";
        });

        AddInstruction("shlr8 rn", "0100nnnn00011001", (op) => {
            int rn = (op >> 8) & 0xF;
            return $"shlr8 r{rn}";
        });

        AddInstruction("shll16 rn", "0100nnnn00101000", (op) => {
            int rn = (op >> 8) & 0xF;
            return $"shll16 r{rn}";
        });

        AddInstruction("shlr16 rn", "0100nnnn00101001", (op) => {
            int rn = (op >> 8) & 0xF;
            return $"shlr16 r{rn}";
        });

        // Instruções de branch
        AddInstruction("bf disp:8", "10001011dddddddd", (op) => {
            int disp = op & 0xFF;
            int sext_disp = (disp & 0x80) != 0 ? (int)(disp | 0xFFFFFF00) : disp;
            return $"bf 0x{sext_disp * 2:X}";
        });

        AddInstruction("bfs disp:8", "10001111dddddddd", (op) => {
            int disp = op & 0xFF;
            int sext_disp = (disp & 0x80) != 0 ? (int)(disp | 0xFFFFFF00) : disp;
            return $"bf/s 0x{sext_disp * 2:X}";
        });

        AddInstruction("bt disp:8", "10001001dddddddd", (op) => {
            int disp = op & 0xFF;
            int sext_disp = (disp & 0x80) != 0 ? (int)(disp | 0xFFFFFF00) : disp;
            return $"bt 0x{sext_disp * 2:X}";
        });

        AddInstruction("bts disp:8", "10001101dddddddd", (op) => {
            int disp = op & 0xFF;
            int sext_disp = (disp & 0x80) != 0 ? (int)(disp | 0xFFFFFF00) : disp;
            return $"bt/s 0x{sext_disp * 2:X}";
        });

        AddInstruction("bra disp:12", "1010dddddddddddd", (op) => {
            int disp = op & 0xFFF;
            int sext_disp = (disp & 0x800) != 0 ? (int)(disp | 0xFFFFF000) : disp;
            return $"bra 0x{sext_disp * 2:X}";
        });

        AddInstruction("braf rn", "0000nnnn00100011", (op) => {
            int rn = (op >> 8) & 0xF;
            return $"braf r{rn}";
        });

        AddInstruction("bsr disp:12", "1011dddddddddddd", (op) => {
            int disp = op & 0xFFF;
            int sext_disp = (disp & 0x800) != 0 ? (int)(disp | 0xFFFFF000) : disp;
            return $"bsr 0x{sext_disp * 2:X}";
        });

        AddInstruction("bsrf rn", "0000nnnn00000011", (op) => {
            int rn = (op >> 8) & 0xF;
            return $"bsrf r{rn}";
        });

        AddInstruction("jmp @rm", "0100nnnn00101011", (op) => {
            int rm = (op >> 8) & 0xF;
            return $"jmp @r{rm}";
        });

        AddInstruction("jsr @rn", "0100nnnn00001011", (op) => {
            int rn = (op >> 8) & 0xF;
            return $"jsr @r{rn}";
        });

        AddInstruction("rts", "0000000000001011", (op) => "rts");

        // Instruções de controle do sistema
        AddInstruction("clrmac", "0000000000101000", (op) => "clrmac");
        AddInstruction("clrs", "0000000001001000", (op) => "clrs");
        AddInstruction("clrt", "0000000000001000", (op) => "clrt");
        AddInstruction("nop", "0000000000001001", (op) => "nop");
        AddInstruction("rte", "0000000000101011", (op) => "rte");
        AddInstruction("sets", "0000000001011000", (op) => "sets");
        AddInstruction("sett", "0000000000011000", (op) => "sett");
        AddInstruction("sleep", "0000000000011011", (op) => "sleep");
        AddInstruction("ldtlb", "0000000000111000", (op) => "ldtlb");

        // Instruções de controle de registros
        AddInstruction("ldc rn, sr", "0100mmmm00001110", (op) => {
            int rn = (op >> 8) & 0xF;
            return $"ldc r{rn}, sr";
        });

        AddInstruction("ldc rn, gbr", "0100mmmm00011110", (op) => {
            int rn = (op >> 8) & 0xF;
            return $"ldc r{rn}, gbr";
        });

        AddInstruction("ldc rn, vbr", "0100mmmm00101110", (op) => {
            int rn = (op >> 8) & 0xF;
            return $"ldc r{rn}, vbr";
        });

        AddInstruction("ldc rn, ssr", "0100mmmm00111110", (op) => {
            int rn = (op >> 8) & 0xF;
            return $"ldc r{rn}, ssr";
        });

        AddInstruction("ldc rn, spc", "0100mmmm01001110", (op) => {
            int rn = (op >> 8) & 0xF;
            return $"ldc r{rn}, spc";
        });

        AddInstruction("ldc rn, dbr", "0100mmmm11111010", (op) => {
            int rn = (op >> 8) & 0xF;
            return $"ldc r{rn}, dbr";
        });

        AddInstruction("ldc.l @rn+, sr", "0100mmmm00000111", (op) => {
            int rn = (op >> 8) & 0xF;
            return $"ldc.l @r{rn}+, sr";
        });

        AddInstruction("ldc.l @rn+, gbr", "0100mmmm00010111", (op) => {
            int rn = (op >> 8) & 0xF;
            return $"ldc.l @r{rn}+, gbr";
        });

        AddInstruction("ldc.l @rn+, vbr", "0100mmmm00100111", (op) => {
            int rn = (op >> 8) & 0xF;
            return $"ldc.l @r{rn}+, vbr";
        });

        AddInstruction("ldc.l @rn+, ssr", "0100mmmm00110111", (op) => {
            int rn = (op >> 8) & 0xF;
            return $"ldc.l @r{rn}+, ssr";
        });

        AddInstruction("ldc.l @rn+, spc", "0100mmmm01000111", (op) => {
            int rn = (op >> 8) & 0xF;
            return $"ldc.l @r{rn}+, spc";
        });

        AddInstruction("ldc.l @rn+, dbr", "0100mmmm11110110", (op) => {
            int rn = (op >> 8) & 0xF;
            return $"ldc.l @r{rn}+, dbr";
        });

        AddInstruction("lds rn, mach", "0100mmmm00001010", (op) => {
            int rn = (op >> 8) & 0xF;
            return $"lds r{rn}, mach";
        });

        AddInstruction("lds rn, macl", "0100mmmm00011010", (op) => {
            int rn = (op >> 8) & 0xF;
            return $"lds r{rn}, macl";
        });

        AddInstruction("lds rn, pr", "0100mmmm00101010", (op) => {
            int rn = (op >> 8) & 0xF;
            return $"lds r{rn}, pr";
        });

        AddInstruction("lds.l @rn+, mach", "0100mmmm00000110", (op) => {
            int rn = (op >> 8) & 0xF;
            return $"lds.l @r{rn}+, mach";
        });

        AddInstruction("lds.l @rn+, macl", "0100mmmm00010110", (op) => {
            int rn = (op >> 8) & 0xF;
            return $"lds.l @r{rn}+, macl";
        });

        AddInstruction("lds.l @rn+, pr", "0100mmmm00100110", (op) => {
            int rn = (op >> 8) & 0xF;
            return $"lds.l @r{rn}+, pr";
        });

        // Store control register instructions
        AddInstruction("stc sr, rn", "0000nnnn00000010", (op) => {
            int rn = (op >> 8) & 0xF;
            return $"stc sr, r{rn}";
        });

        AddInstruction("stc gbr, rn", "0000nnnn00010010", (op) => {
            int rn = (op >> 8) & 0xF;
            return $"stc gbr, r{rn}";
        });

        AddInstruction("stc vbr, rn", "0000nnnn00100010", (op) => {
            int rn = (op >> 8) & 0xF;
            return $"stc vbr, r{rn}";
        });

        AddInstruction("stc ssr, rn", "0000nnnn00110010", (op) => {
            int rn = (op >> 8) & 0xF;
            return $"stc ssr, r{rn}";
        });

        AddInstruction("stc spc, rn", "0000nnnn01000010", (op) => {
            int rn = (op >> 8) & 0xF;
            return $"stc spc, r{rn}";
        });

        AddInstruction("stc sgr, rn", "0000nnnn00111010", (op) => {
            int rn = (op >> 8) & 0xF;
            return $"stc sgr, r{rn}";
        });

        AddInstruction("stc dbr, rn", "0000nnnn11111010", (op) => {
            int rn = (op >> 8) & 0xF;
            return $"stc dbr, r{rn}";
        });

        AddInstruction("stc.l sr, @-rn", "0100nnnn00000011", (op) => {
            int rn = (op >> 8) & 0xF;
            return $"stc.l sr, @-r{rn}";
        });

        AddInstruction("stc.l gbr, @-rn", "0100nnnn00010011", (op) => {
            int rn = (op >> 8) & 0xF;
            return $"stc.l gbr, @-r{rn}";
        });

        AddInstruction("stc.l vbr, @-rn", "0100nnnn00100011", (op) => {
            int rn = (op >> 8) & 0xF;
            return $"stc.l vbr, @-r{rn}";
        });

        AddInstruction("stc.l ssr, @-rn", "0100nnnn00110011", (op) => {
            int rn = (op >> 8) & 0xF;
            return $"stc.l ssr, @-r{rn}";
        });

        AddInstruction("stc.l spc, @-rn", "0100nnnn01000011", (op) => {
            int rn = (op >> 8) & 0xF;
            return $"stc.l spc, @-r{rn}";
        });

        AddInstruction("stc.l sgr, @-rn", "0100nnnn00110010", (op) => {
            int rn = (op >> 8) & 0xF;
            return $"stc.l sgr, @-r{rn}";
        });

        AddInstruction("stc.l dbr, @-rn", "0100nnnn11110010", (op) => {
            int rn = (op >> 8) & 0xF;
            return $"stc.l dbr, @-r{rn}";
        });

        AddInstruction("sts mach, rn", "0000nnnn00001010", (op) => {
            int rn = (op >> 8) & 0xF;
            return $"sts mach, r{rn}";
        });

        AddInstruction("sts macl, rn", "0000nnnn00011010", (op) => {
            int rn = (op >> 8) & 0xF;
            return $"sts macl, r{rn}";
        });

        AddInstruction("sts pr, rn", "0000nnnn00101010", (op) => {
            int rn = (op >> 8) & 0xF;
            return $"sts pr, r{rn}";
        });

        AddInstruction("sts.l mach, @-rn", "0100nnnn00000010", (op) => {
            int rn = (op >> 8) & 0xF;
            return $"sts.l mach, @-r{rn}";
        });

        AddInstruction("sts.l macl, @-rn", "0100nnnn00010010", (op) => {
            int rn = (op >> 8) & 0xF;
            return $"sts.l macl, @-r{rn}";
        });

        AddInstruction("sts.l pr, @-rn", "0100nnnn00100010", (op) => {
            int rn = (op >> 8) & 0xF;
            return $"sts.l pr, @-r{rn}";
        });

        // Outras instruções
        AddInstruction("tas.b @rn", "0100nnnn00011011", (op) => {
            int rn = (op >> 8) & 0xF;
            return $"tas.b @r{rn}";
        });

        AddInstruction("trapa #imm8", "11000011iiiiiiii", (op) => {
            int imm8 = op & 0xFF;
            return $"trapa #0x{imm8:X2}";
        });

        AddInstruction("movca.l r0, @rn", "0000nnnn11000011", (op) => {
            int rn = (op >> 8) & 0xF;
            return $"movca.l r0, @r{rn}";
        });

        AddInstruction("ocbi @rn", "0000nnnn10010011", (op) => {
            int rn = (op >> 8) & 0xF;
            return $"ocbi @r{rn}";
        });

        AddInstruction("ocbp @rn", "0000nnnn10100011", (op) => {
            int rn = (op >> 8) & 0xF;
            return $"ocbp @r{rn}";
        });

        AddInstruction("ocbwb @rn", "0000nnnn10110011", (op) => {
            int rn = (op >> 8) & 0xF;
            return $"ocbwb @r{rn}";
        });

        AddInstruction("pref @rn", "0000nnnn10000011", (op) => {
            int rn = (op >> 8) & 0xF;
            return $"pref @r{rn}";
        });

        // Instruções de ponto flutuante
        AddInstruction("fldi0 frn", "1111nnnn10001101", (op) => {
            int frn = (op >> 8) & 0xF;
            return $"fldi0 fr{frn}";
        });

        AddInstruction("fldi1 frn", "1111nnnn10011101", (op) => {
            int frn = (op >> 8) & 0xF;
            return $"fldi1 fr{frn}";
        });

        AddInstruction("fmov frm, frn", "1111nnnnmmmm1100", (op) => {
            int frn = (op >> 8) & 0xF;
            int frm = (op >> 4) & 0xF;
            return $"fmov fr{frm}, fr{frn}";
        });

        AddInstruction("fmov.s @rm, frn", "1111nnnnmmmm1000", (op) => {
            int frn = (op >> 8) & 0xF;
            int rm = (op >> 4) & 0xF;
            return $"fmov.s @r{rm}, fr{frn}";
        });

        AddInstruction("fmov.s @(r0,rm), frn", "1111nnnnmmmm0110", (op) => {
            int frn = (op >> 8) & 0xF;
            int rm = (op >> 4) & 0xF;
            return $"fmov.s @(r0,r{rm}), fr{frn}";
        });

        AddInstruction("fmov.s frm, @rn", "1111nnnnmmmm1010", (op) => {
            int rn = (op >> 8) & 0xF;
            int frm = (op >> 4) & 0xF;
            return $"fmov.s fr{frm}, @r{rn}";
        });

        AddInstruction("fmov.s frm, @(r0,rn)", "1111nnnnmmmm0111", (op) => {
            int rn = (op >> 8) & 0xF;
            int frm = (op >> 4) & 0xF;
            return $"fmov.s fr{frm}, @(r0,r{rn})";
        });

        AddInstruction("fmov.s frm, @-rn", "1111nnnnmmmm1011", (op) => {
            int rn = (op >> 8) & 0xF;
            int frm = (op >> 4) & 0xF;
            return $"fmov.s fr{frm}, @-r{rn}";
        });

        AddInstruction("fmov.s @rm+, frn", "1111nnnnmmmm1001", (op) => {
            int frn = (op >> 8) & 0xF;
            int rm = (op >> 4) & 0xF;
            return $"fmov.s @r{rm}+, fr{frn}";
        });

        AddInstruction("flds frn, fpul", "1111mmmm00011101", (op) => {
            int frn = (op >> 8) & 0xF;
            return $"flds fr{frn}, fpul";
        });

        AddInstruction("fsts fpul, frn", "1111nnnn00001101", (op) => {
            int frn = (op >> 8) & 0xF;
            return $"fsts fpul, fr{frn}";
        });

        AddInstruction("fabs frn", "1111nnnn01011101", (op) => {
            int frn = (op >> 8) & 0xF;
            return $"fabs fr{frn}";
        });

        AddInstruction("fadd frm, frn", "1111nnnnmmmm0000", (op) => {
            int frn = (op >> 8) & 0xF;
            int frm = (op >> 4) & 0xF;
            return $"fadd fr{frm}, fr{frn}";
        });

        AddInstruction("fcmp/eq frm, frn", "1111nnnnmmmm0100", (op) => {
            int frn = (op >> 8) & 0xF;
            int frm = (op >> 4) & 0xF;
            return $"fcmp/eq fr{frm}, fr{frn}";
        });

        AddInstruction("fcmp/gt frm, frn", "1111nnnnmmmm0101", (op) => {
            int frn = (op >> 8) & 0xF;
            int frm = (op >> 4) & 0xF;
            return $"fcmp/gt fr{frm}, fr{frn}";
        });

        AddInstruction("fdiv frm, frn", "1111nnnnmmmm0011", (op) => {
            int frn = (op >> 8) & 0xF;
            int frm = (op >> 4) & 0xF;
            return $"fdiv fr{frm}, fr{frn}";
        });

        AddInstruction("float fpul, frn", "1111nnnn00101101", (op) => {
            int frn = (op >> 8) & 0xF;
            return $"float fpul, fr{frn}";
        });

        AddInstruction("fmac fr0, frm, frn", "1111nnnnmmmm1110", (op) => {
            int frn = (op >> 8) & 0xF;
            int frm = (op >> 4) & 0xF;
            return $"fmac fr0, fr{frm}, fr{frn}";
        });

        AddInstruction("fmul frm, frn", "1111nnnnmmmm0010", (op) => {
            int frn = (op >> 8) & 0xF;
            int frm = (op >> 4) & 0xF;
            return $"fmul fr{frm}, fr{frn}";
        });

        AddInstruction("fneg frn", "1111nnnn01001101", (op) => {
            int frn = (op >> 8) & 0xF;
            return $"fneg fr{frn}";
        });

        AddInstruction("fsqrt frn", "1111nnnn01101101", (op) => {
            int frn = (op >> 8) & 0xF;
            return $"fsqrt fr{frn}";
        });

        AddInstruction("fsub frm, frn", "1111nnnnmmmm0001", (op) => {
            int frn = (op >> 8) & 0xF;
            int frm = (op >> 4) & 0xF;
            return $"fsub fr{frm}, fr{frn}";
        });

        AddInstruction("ftrc frn, fpul", "1111mmmm00111101", (op) => {
            int frn = (op >> 8) & 0xF;
            return $"ftrc fr{frn}, fpul";
        });

        AddInstruction("fsrra frn", "1111nnnn01111101", (op) => {
            int frn = (op >> 8) & 0xF;
            return $"fsrra fr{frn}";
        });

        // Instruções de conversão de ponto flutuante
        AddInstruction("fcnvds drn, fpul", "1111nnn010111101", (op) => {
            int drn = ((op >> 9) & 0x7) << 1;
            return $"fcnvds dr{drn}, fpul";
        });

        AddInstruction("fcnvsd fpul, drn", "1111nnn010101101", (op) => {
            int drn = ((op >> 9) & 0x7) << 1;
            return $"fcnvsd fpul, dr{drn}";
        });

        // Instruções de controle de ponto flutuante
        AddInstruction("lds rn, fpscr", "0100mmmm01101010", (op) => {
            int rn = (op >> 8) & 0xF;
            return $"lds r{rn}, fpscr";
        });

        AddInstruction("lds rn, fpul", "0100mmmm01011010", (op) => {
            int rn = (op >> 8) & 0xF;
            return $"lds r{rn}, fpul";
        });

        AddInstruction("lds.l @rn+, fpscr", "0100mmmm01100110", (op) => {
            int rn = (op >> 8) & 0xF;
            return $"lds.l @r{rn}+, fpscr";
        });

        AddInstruction("lds.l @rn+, fpul", "0100mmmm01010110", (op) => {
            int rn = (op >> 8) & 0xF;
            return $"lds.l @r{rn}+, fpul";
        });

        AddInstruction("sts fpscr, rn", "0000nnnn01101010", (op) => {
            int rn = (op >> 8) & 0xF;
            return $"sts fpscr, r{rn}";
        });

        AddInstruction("sts fpul, rn", "0000nnnn01011010", (op) => {
            int rn = (op >> 8) & 0xF;
            return $"sts fpul, r{rn}";
        });

        AddInstruction("sts.l fpscr, @-rn", "0100nnnn01100010", (op) => {
            int rn = (op >> 8) & 0xF;
            return $"sts.l fpscr, @-r{rn}";
        });

        AddInstruction("sts.l fpul, @-rn", "0100nnnn01010010", (op) => {
            int rn = (op >> 8) & 0xF;
            return $"sts.l fpul, @-r{rn}";
        });

        // Instruções vetoriais
        AddInstruction("fipr fvm, fvn", "1111nnmm11101101", (op) => {
            int fvn = ((op >> 10) & 0x3) << 2;
            int fvm = ((op >> 8) & 0x3) << 2;
            return $"fipr fv{fvm}, fv{fvn}";
        });

        AddInstruction("fsca fpul, drn", "1111nnn011111101", (op) => {
            int drn = ((op >> 9) & 0x7) << 1;
            return $"fsca fpul, dr{drn}";
        });

        AddInstruction("ftrv xmtrx, fvn", "1111nn0111111101", (op) => {
            int fvn = ((op >> 10) & 0x3) << 2;
            return $"ftrv xmtrx, fv{fvn}";
        });

        AddInstruction("frchg", "1111101111111101", (op) => "frchg");
        AddInstruction("fschg", "1111001111111101", (op) => "fschg");

        // Instruções MAC
        AddInstruction("mac.l @rm+, @rn+", "0000nnnnmmmm1111", (op) => {
            int rn = (op >> 8) & 0xF;
            int rm = (op >> 4) & 0xF;
            return $"mac.l @r{rm}+, @r{rn}+";
        });

        AddInstruction("mac.w @rm+, @rn+", "0100nnnnmmmm1111", (op) => {
            int rn = (op >> 8) & 0xF;
            int rm = (op >> 4) & 0xF;
            return $"mac.w @r{rm}+, @r{rn}+";
        });

        // Instruções lógicas com GBR
        AddInstruction("and.b #imm8, @(r0,gbr)", "11001101iiiiiiii", (op) => {
            int imm8 = op & 0xFF;
            return $"and.b #0x{imm8:X2}, @(r0,gbr)";
        });

        AddInstruction("or.b #imm8, @(r0,gbr)", "11001111iiiiiiii", (op) => {
            int imm8 = op & 0xFF;
            return $"or.b #0x{imm8:X2}, @(r0,gbr)";
        });

        AddInstruction("tst.b #imm8, @(r0,gbr)", "11001100iiiiiiii", (op) => {
            int imm8 = op & 0xFF;
            return $"tst.b #0x{imm8:X2}, @(r0,gbr)";
        });

        AddInstruction("xor.b #imm8, @(r0,gbr)", "11001110iiiiiiii", (op) => {
            int imm8 = op & 0xFF;
            return $"xor.b #0x{imm8:X2}, @(r0,gbr)";
        });
    }

    private void AddInstruction(string template, string pattern, Func<ushort, string> formatter)
    {
        var (mask, patternValue) = ParsePattern(pattern);
        _instructions.Add(new InstructionPattern
        {
            Template = template,
            Mask = mask,
            Pattern = patternValue,
            Formatter = formatter
        });
    }

    private (ushort mask, ushort pattern) ParsePattern(string binaryPattern)
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
                    // pattern bit remains 0
                    break;
                case '1':
                    mask |= (ushort)(1 << bitPos);
                    pattern |= (ushort)(1 << bitPos);
                    break;
                case 'n':
                case 'm':
                case 'd':
                case 'i':
                    // Variable bits - don't set mask
                    break;
            }
        }

        return (mask, pattern);
    }

    /// <summary>
    /// Decodifica um opcode de 16 bits em uma instrução SH-4
    /// </summary>
    /// <param name="opcode">O opcode de 16 bits a ser decodificado</param>
    /// <returns>Uma estrutura DecodedInstruction contendo informações sobre a instrução</returns>
    public DecodedInstruction Disassemble(ushort opcode)
    {
        var instructionPattern = _lookupTable[opcode];

        if (instructionPattern.HasValue)
        {
            try
            {
                var instruction = instructionPattern.Value;
                string formattedInstruction = instruction.Formatter(opcode);
                return new DecodedInstruction
                {
                    Mnemonic = ExtractMnemonic(formattedInstruction),
                    FullInstruction = formattedInstruction,
                    Opcode = opcode,
                    IsValid = true
                };
            }
            catch
            {
                // Se houver erro na formatação, trata como desconhecido.
                // Isso pode acontecer se a lógica do formatador falhar.
            }
        }

        // Instrução não reconhecida
        return new DecodedInstruction
        {
            Mnemonic = "unknown",
            FullInstruction = $"unknown 0x{opcode:X4}",
            Opcode = opcode,
            IsValid = false
        };
    }

    /// <summary>
    /// Decodifica um array de opcodes
    /// </summary>
    /// <param name="opcodes">Array de opcodes de 16 bits</param>
    /// <returns>Array de instruções decodificadas</returns>
    public DecodedInstruction[] DisassembleBlock(ushort[] opcodes)
    {
        var results = new DecodedInstruction[opcodes.Length];
        for (int i = 0; i < opcodes.Length; i++)
        {
            results[i] = Disassemble(opcodes[i]);
        }
        return results;
    }

    /// <summary>
    /// Decodifica um bloco de bytes (little-endian) em instruções
    /// </summary>
    /// <param name="data">Array de bytes contendo os opcodes</param>
    /// <param name="startOffset">Offset inicial no array</param>
    /// <param name="count">Número de instruções (2 bytes cada) a decodificar</param>
    /// <returns>Array de instruções decodificadas</returns>
    public DecodedInstruction[] DisassembleBytes(byte[] data, int startOffset = 0, int count = -1)
    {
        if (count == -1)
            count = (data.Length - startOffset) / 2;

        var opcodes = new ushort[count];
        for (int i = 0; i < count; i++)
        {
            int byteIndex = startOffset + (i * 2);
            if (byteIndex + 1 < data.Length)
            {
                // SH-4 usa little-endian para instruções
                opcodes[i] = (ushort)(data[byteIndex] | (data[byteIndex + 1] << 8));
            }
        }

        return DisassembleBlock(opcodes);
    }

    /// <summary>
    /// Decodifica instruções com endereços base para branches relativos
    /// </summary>
    /// <param name="opcodes">Array de opcodes</param>
    /// <param name="baseAddress">Endereço base para cálculo de branches</param>
    /// <returns>Array de instruções decodificadas com endereços absolutos para branches</returns>
    public DecodedInstruction[] DisassembleWithAddresses(ushort[] opcodes, uint baseAddress)
    {
        var results = new DecodedInstruction[opcodes.Length];
        
        for (int i = 0; i < opcodes.Length; i++)
        {
            var instruction = Disassemble(opcodes[i]);
            uint currentAddress = baseAddress + (uint)(i * 2);
            
            // Ajustar branches para mostrar endereços absolutos
            if (instruction.IsValid && IsBranchInstruction(instruction.Mnemonic))
            {
                instruction.FullInstruction = CalculateAbsoluteBranch(instruction.FullInstruction, currentAddress);
            }
            
            results[i] = instruction;
        }
        
        return results;
    }

    private string ExtractMnemonic(string fullInstruction)
    {
        int spaceIndex = fullInstruction.IndexOf(' ');
        return spaceIndex > 0 ? fullInstruction.Substring(0, spaceIndex) : fullInstruction;
    }

    private bool IsBranchInstruction(string mnemonic)
    {
        return mnemonic == "bf" || mnemonic == "bf/s" || mnemonic == "bt" || mnemonic == "bt/s" ||
               mnemonic == "bra" || mnemonic == "bsr";
    }

    private string CalculateAbsoluteBranch(string instruction, uint currentAddress)
    {
        // Extrai o deslocamento da instrução e calcula o endereço absoluto
        int hexIndex = instruction.IndexOf("0x");
        if (hexIndex == -1) return instruction;

        string hexPart = instruction.Substring(hexIndex + 2);
        if (int.TryParse(hexPart, System.Globalization.NumberStyles.HexNumber, null, out int displacement))
        {
            uint targetAddress = currentAddress + 4 + (uint)displacement; // PC + 4 + displacement
            return instruction.Substring(0, hexIndex) + $"0x{targetAddress:X8}";
        }

        return instruction;
    }

    /// <summary>
    /// Obtém informações sobre todas as instruções suportadas
    /// </summary>
    /// <returns>Lista com templates de todas as instruções</returns>
    public List<string> GetSupportedInstructions()
    {
        return _instructions.Select(i => i.Template).ToList();
    }

    /// <summary>
    /// Verifica se um opcode é uma instrução válida
    /// </summary>
    /// <param name="opcode">O opcode a verificar</param>
    /// <returns>True se a instrução for reconhecida</returns>
    public bool IsValidInstruction(ushort opcode)
    {
        return Disassemble(opcode).IsValid;
    }
}