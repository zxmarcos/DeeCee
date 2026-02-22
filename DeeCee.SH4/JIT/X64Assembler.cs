using System;
using System.Collections.Generic;

namespace DeeCee.SH4.JIT;

public class X64Assembler
{
    private List<byte> _code = new();

    public byte[] GetCode() => _code.ToArray();

    public int CurrentOffset => _code.Count;

    private void EmitByte(byte b) => _code.Add(b);
    public void EmitInt32(int value)
    {
        _code.Add((byte)value);
        _code.Add((byte)(value >> 8));
        _code.Add((byte)(value >> 16));
        _code.Add((byte)(value >> 24));
    }
    public void EmitInt64(long value)
    {
        EmitInt32((int)value);
        EmitInt32((int)(value >> 32));
    }

    private void EmitRexW(X64Registers reg, X64Registers rm) => EmitRexW((int)reg, (int)rm);

    private void EmitRexW(int reg, int rm)
    {
        byte rex = 0x48; // 0100 1000 (W=1)
        if ((reg & 8) != 0) rex |= 4; // R
        if ((rm & 8) != 0) rex |= 1; // B
        EmitByte(rex);
    }

    private void EmitRex(int reg, int rm)
    {
        byte rex = 0x40;
        bool needed = false;
        if ((reg & 8) != 0) { rex |= 4; needed = true; }
        if ((rm & 8) != 0) { rex |= 1; needed = true; }
        if (needed) EmitByte(rex);
    }

    private void EmitModRM(int mod, int reg, int rm)
    {
        EmitByte((byte)((mod << 6) | ((reg & 7) << 3) | (rm & 7)));
    }

    // MOV dest, src
    public void Mov(X64Registers dest, X64Registers src)
    {
        EmitRex((int)dest, (int)src);
        EmitByte(0x8B);
        EmitModRM(3, (int)dest, (int)src);
    }

    public void Mov64(X64Registers dest, X64Registers src)
    {
        EmitRexW((int)dest, (int)src);
        EmitByte(0x8B);
        EmitModRM(3, (int)dest, (int)src);
    }

    // MOV dest, imm64
    public void Mov(X64Registers dest, long immediate)
    {
        EmitRexW(0, (int)dest);
        EmitByte((byte)(0xB8 + ((int)dest & 7)));
        EmitInt64(immediate);
    }

    // MOV dest, imm32
    public void Mov(X64Registers dest, int immediate)
    {
        EmitRex(0, (int)dest);
        EmitByte((byte)(0xB8 + ((int)dest & 7)));
        EmitInt32(immediate);
    }

    // MOV [base + offset], src
    public void Mov(X64Registers baseReg, int offset, X64Registers src)
    {
        // 32-bit store
        EmitRex((int)src, (int)baseReg);
        EmitByte(0x89);
        EmitMemModRM(src, baseReg, offset);
    }

    // MOV [base + offset], src (64-bit)
    public void Mov64(X64Registers baseReg, int offset, X64Registers src)
    {
        EmitRexW((int)src, (int)baseReg);
        EmitByte(0x89);
        EmitMemModRM(src, baseReg, offset);
    }

    // MOV dest, [base + offset]
    public void Mov(X64Registers dest, X64Registers baseReg, int offset)
    {
        // 32-bit load
        EmitRex((int)dest, (int)baseReg);
        EmitByte(0x8B);
        EmitMemModRM(dest, baseReg, offset);
    }

    // MOV dest, [base + offset] (64-bit)
    public void Mov64(X64Registers dest, X64Registers baseReg, int offset)
    {
        EmitRexW((int)dest, (int)baseReg);
        EmitByte(0x8B);
        EmitMemModRM(dest, baseReg, offset);
    }

    private void EmitMemModRM(X64Registers reg, X64Registers baseReg, int offset)
    {
        if (offset == 0 && (int)baseReg != (int)X64Registers.RBP && (int)baseReg != (int)X64Registers.R13)
        {
            EmitModRM(0, (int)reg, (int)baseReg);
            if ((int)baseReg == (int)X64Registers.RSP || (int)baseReg == (int)X64Registers.R12) EmitByte(0x24);
        }
        else if (offset >= -128 && offset <= 127)
        {
            EmitModRM(1, (int)reg, (int)baseReg);
            if ((int)baseReg == (int)X64Registers.RSP || (int)baseReg == (int)X64Registers.R12) EmitByte(0x24);
            EmitByte((byte)offset);
        }
        else
        {
            EmitModRM(2, (int)reg, (int)baseReg);
            if ((int)baseReg == (int)X64Registers.RSP || (int)baseReg == (int)X64Registers.R12) EmitByte(0x24);
            EmitInt32(offset);
        }
    }

    // ALU operations
    private void EmitAlu(int opcodeDigit, X64Registers dest, X64Registers src)
    {
        EmitRex((int)src, (int)dest);
        EmitByte((byte)((opcodeDigit << 3) | 1)); // 0x01 for ADD, 0x29 for SUB, etc. Wait, standard is 0x01, 0x09 etc.
        // ALU:
        // ADD: 01 /r
        // OR:  09 /r
        // ADC: 11 /r
        // SBB: 19 /r
        // AND: 21 /r
        // SUB: 29 /r
        // XOR: 31 /r
        // CMP: 39 /r

        // I will implement explicitly to avoid confusion.
    }

    public void Add(X64Registers dest, X64Registers src) { EmitRex((int)src, (int)dest); EmitByte(0x01); EmitModRM(3, (int)src, (int)dest); }
    public void Or(X64Registers dest, X64Registers src)  { EmitRex((int)src, (int)dest); EmitByte(0x09); EmitModRM(3, (int)src, (int)dest); }
    public void Adc(X64Registers dest, X64Registers src) { EmitRex((int)src, (int)dest); EmitByte(0x11); EmitModRM(3, (int)src, (int)dest); }
    public void Sbb(X64Registers dest, X64Registers src) { EmitRex((int)src, (int)dest); EmitByte(0x19); EmitModRM(3, (int)src, (int)dest); }
    public void And(X64Registers dest, X64Registers src) { EmitRex((int)src, (int)dest); EmitByte(0x21); EmitModRM(3, (int)src, (int)dest); }
    public void Sub(X64Registers dest, X64Registers src) { EmitRex((int)src, (int)dest); EmitByte(0x29); EmitModRM(3, (int)src, (int)dest); }
    public void Xor(X64Registers dest, X64Registers src) { EmitRex((int)src, (int)dest); EmitByte(0x31); EmitModRM(3, (int)src, (int)dest); }
    public void Cmp(X64Registers dest, X64Registers src) { EmitRex((int)src, (int)dest); EmitByte(0x39); EmitModRM(3, (int)src, (int)dest); }

    // ALU Immediate
    // 81 /digit imm32
    // 83 /digit imm8
    private void EmitAluImm(int digit, X64Registers dest, int immediate)
    {
        EmitRex(0, (int)dest);
        if (immediate >= -128 && immediate <= 127)
        {
            EmitByte(0x83);
            EmitModRM(3, digit, (int)dest);
            EmitByte((byte)immediate);
        }
        else
        {
            EmitByte(0x81);
            EmitModRM(3, digit, (int)dest);
            EmitInt32(immediate);
        }
    }

    public void Add(X64Registers dest, int imm) => EmitAluImm(0, dest, imm);
    public void Or(X64Registers dest, int imm)  => EmitAluImm(1, dest, imm);
    public void Adc(X64Registers dest, int imm) => EmitAluImm(2, dest, imm);
    public void Sbb(X64Registers dest, int imm) => EmitAluImm(3, dest, imm);
    public void And(X64Registers dest, int imm) => EmitAluImm(4, dest, imm);
    public void Sub(X64Registers dest, int imm) => EmitAluImm(5, dest, imm);
    public void Xor(X64Registers dest, int imm) => EmitAluImm(6, dest, imm);
    public void Cmp(X64Registers dest, int imm) => EmitAluImm(7, dest, imm);

    public void Not(X64Registers dest)
    {
        EmitRex(0, (int)dest);
        EmitByte(0xF7);
        EmitModRM(3, 2, (int)dest);
    }

    public void Neg(X64Registers dest)
    {
        EmitRex(0, (int)dest);
        EmitByte(0xF7);
        EmitModRM(3, 3, (int)dest);
    }

    public void Push(X64Registers reg)
    {
        if ((int)reg >= 8) EmitByte(0x41);
        EmitByte((byte)(0x50 + ((int)reg & 7)));
    }

    public void Pop(X64Registers reg)
    {
        if ((int)reg >= 8) EmitByte(0x41);
        EmitByte((byte)(0x58 + ((int)reg & 7)));
    }

    public void Ret() => EmitByte(0xC3);

    // Shifts
    // D3 /4 : SHL r/m32, CL
    // D3 /5 : SHR r/m32, CL
    // D3 /7 : SAR r/m32, CL
    // C1 /4 ib : SHL r/m32, imm8

    public void Shl(X64Registers dest, int imm) { EmitRex(0, (int)dest); EmitByte(0xC1); EmitModRM(3, 4, (int)dest); EmitByte((byte)imm); }
    public void Shr(X64Registers dest, int imm) { EmitRex(0, (int)dest); EmitByte(0xC1); EmitModRM(3, 5, (int)dest); EmitByte((byte)imm); }
    public void Sar(X64Registers dest, int imm) { EmitRex(0, (int)dest); EmitByte(0xC1); EmitModRM(3, 7, (int)dest); EmitByte((byte)imm); }

    public void Shl(X64Registers dest) { EmitRex(0, (int)dest); EmitByte(0xD3); EmitModRM(3, 4, (int)dest); } // uses CL
    public void Shr(X64Registers dest) { EmitRex(0, (int)dest); EmitByte(0xD3); EmitModRM(3, 5, (int)dest); } // uses CL
    public void Sar(X64Registers dest) { EmitRex(0, (int)dest); EmitByte(0xD3); EmitModRM(3, 7, (int)dest); } // uses CL

    // Jumps
    // For now simple short jumps or near jumps.
    // I need to support label patching.

    public void Jmp(int relativeOffset)
    {
        EmitByte(0xE9);
        EmitInt32(relativeOffset - 4); // rel32 is relative to next instruction
    }

    // Conditional Jumps 0F 8x cw
    public void Je(int relativeOffset) { EmitByte(0x0F); EmitByte(0x84); EmitInt32(relativeOffset - 4); }
    public void Jne(int relativeOffset) { EmitByte(0x0F); EmitByte(0x85); EmitInt32(relativeOffset - 4); }

    // SetCC
    // 0F 9x
    public void Setne(X64Registers dest)
    {
        EmitRex(0, (int)dest); // Optional if using r8l etc.
        EmitByte(0x0F);
        EmitByte(0x95);
        EmitModRM(3, 0, (int)dest);
    }

    public void Sete(X64Registers dest)
    {
        EmitRex(0, (int)dest);
        EmitByte(0x0F);
        EmitByte(0x94);
        EmitModRM(3, 0, (int)dest);
    }

    public class Label
    {
        public int Offset { get; set; } = -1;
        public List<int> PatchOffsets { get; } = new();
    }

    public void Bind(Label label)
    {
        label.Offset = _code.Count;
        foreach (var patch in label.PatchOffsets)
        {
            int rel = label.Offset - (patch + 4);
            _code[patch] = (byte)rel;
            _code[patch+1] = (byte)(rel >> 8);
            _code[patch+2] = (byte)(rel >> 16);
            _code[patch+3] = (byte)(rel >> 24);
        }
        label.PatchOffsets.Clear();
    }

    public void Jmp(Label label)
    {
        EmitByte(0xE9);
        if (label.Offset != -1)
        {
            EmitInt32(label.Offset - (_code.Count + 4));
        }
        else
        {
            label.PatchOffsets.Add(_code.Count);
            EmitInt32(0);
        }
    }

    private void EmitJcc(Label label, byte opcode)
    {
        EmitByte(0x0F);
        EmitByte(opcode);
        if (label.Offset != -1)
        {
            EmitInt32(label.Offset - (_code.Count + 4));
        }
        else
        {
            label.PatchOffsets.Add(_code.Count);
            EmitInt32(0);
        }
    }

    public void Je(Label label) => EmitJcc(label, 0x84);
    public void Jne(Label label) => EmitJcc(label, 0x85);
    public void Jl(Label label) => EmitJcc(label, 0x8C);
    public void Jge(Label label) => EmitJcc(label, 0x8D);
    public void Jle(Label label) => EmitJcc(label, 0x8E);
    public void Jg(Label label) => EmitJcc(label, 0x8F);

    // Unsigned
    public void Jb(Label label) => EmitJcc(label, 0x82);
    public void Jae(Label label) => EmitJcc(label, 0x83);
    public void Jbe(Label label) => EmitJcc(label, 0x86);
    public void Ja(Label label) => EmitJcc(label, 0x87);

}
