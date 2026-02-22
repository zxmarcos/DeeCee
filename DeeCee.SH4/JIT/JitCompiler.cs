using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace DeeCee.SH4.JIT;

public unsafe class JitCompiler
{
    private X64Assembler _assembler;
    private RegisterAllocator _allocator;
    private LivenessAnalysis _liveness;
    private BasicBlock _block;

    // Delegate type for generated code
    public delegate void JitBlock(Sh4CpuState* state);

    // Keep track of label locations for patching
    private Dictionary<int, X64Assembler.Label> _labels = new();

    public JitBlock Compile(BasicBlock block)
    {
        _block = block;
        _liveness = new LivenessAnalysis(block);
        // We reserve 40 bytes for pushed registers (RBX, R12, R13, R14, R15) relative to RBP
        _allocator = new RegisterAllocator(_liveness, 40);
        _allocator.Allocate();
        _assembler = new X64Assembler();
        _labels.Clear();

        EmitPrologue();
        EmitBody();
        EmitEpilogue();

        var code = _assembler.GetCode();
        var ptr = NativeMemoryAllocator.Allocate(code.Length);
        Marshal.Copy(code, 0, ptr, code.Length);

        // Flush instruction cache is generally handled by OS but we can't easily call it from here cross-platform without p-invoke.
        // Usually safe on x86/x64 due to coherent caches, but not on ARM. Since target is x64, we are fine.

        return Marshal.GetDelegateForFunctionPointer<JitBlock>(ptr);
    }

    private void EmitPrologue()
    {
        // push rbp
        _assembler.Push(X64Registers.RBP);
        // mov rbp, rsp
        _assembler.Mov64(X64Registers.RBP, X64Registers.RSP);

        // Save callee-saved registers we use
        // We use RBX, R12, R13, R14, R15
        _assembler.Push(X64Registers.RBX);
        _assembler.Push(X64Registers.R12);
        _assembler.Push(X64Registers.R13);
        _assembler.Push(X64Registers.R14);
        _assembler.Push(X64Registers.R15);

        // Move argument (Sh4CpuState*) to R15
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // RCX
            _assembler.Mov64(X64Registers.R15, X64Registers.RCX);
        }
        else
        {
            // RDI
            _assembler.Mov64(X64Registers.R15, X64Registers.RDI);
        }

        // Allocate stack space for locals/spills
        // Align to 16 bytes. We start at 40 (5 regs pushed).
        int localStackSize = _allocator.StackSize - 40;
        if (localStackSize > 0)
        {
            if (localStackSize % 16 != 0) localStackSize += (16 - (localStackSize % 16));
            _assembler.Sub(X64Registers.RSP, localStackSize);
        }

        // Load mapped SH4 registers
        for (int i = 0; i < 16; i++)
        {
            var loc = _allocator.GetLocation(i);
            if (loc.HasValue)
            {
                if (loc.Value.Type == LocationType.Register)
                {
                    // Load R[i] from [R15 + i*4] to Register
                    _assembler.Mov(loc.Value.Register, X64Registers.R15, i * 4);
                }
                else
                {
                    // Spill to stack: Load from State to Temp (RAX), store to Stack [RBP + offset]
                    _assembler.Mov(X64Registers.RAX, X64Registers.R15, i * 4);
                    _assembler.Mov(X64Registers.RBP, loc.Value.StackOffset, X64Registers.RAX);
                }
            }
        }
    }

    private void EmitEpilogue()
    {
        // Store mapped SH4 registers back
        for (int i = 0; i < 16; i++)
        {
            var loc = _allocator.GetLocation(i);
            if (loc.HasValue)
            {
                if (loc.Value.Type == LocationType.Register)
                {
                    // Store Register to R[i] at [R15 + i*4]
                    _assembler.Mov(X64Registers.R15, i * 4, loc.Value.Register);
                }
                else
                {
                    // Load from Stack [RBP + offset] to Temp (RAX), store to State
                    _assembler.Mov(X64Registers.RAX, X64Registers.RBP, loc.Value.StackOffset);
                    _assembler.Mov(X64Registers.R15, i * 4, X64Registers.RAX);
                }
            }
        }

        // Restore stack
        int localStackSize = _allocator.StackSize - 40;
        if (localStackSize > 0)
        {
            if (localStackSize % 16 != 0) localStackSize += (16 - (localStackSize % 16));
            _assembler.Add(X64Registers.RSP, localStackSize);
        }

        _assembler.Pop(X64Registers.R15);
        _assembler.Pop(X64Registers.R14);
        _assembler.Pop(X64Registers.R13);
        _assembler.Pop(X64Registers.R12);
        _assembler.Pop(X64Registers.RBX);

        _assembler.Pop(X64Registers.RBP);
        _assembler.Ret();
    }

    private void EmitBody()
    {
        for (int i = 0; i < _block.Instructions.Count; i++)
        {
            var instr = _block.Instructions[i];

            // Define label for this instruction if needed (target of branch)
            // But IR uses Label operands which point to instruction index or BlockOffset?
            // OperandKind.Label has BlockOffset.
            // If instructions are targetted by index, we need to map index to label.
            // But usually blocks are straight-line except for branches at end.
            // Intra-block branches? The IR supports labels.
            // OperandKind.Label is used in BRANCH instructions.
            // Wait, does BasicBlock contain labels as instructions? No.
            // BRANCH takes an Operand of type Label.

            // We need to map BlockOffset to code offset.
            // BlockOffset is likely instruction index.

            if (!_labels.ContainsKey(i)) _labels[i] = new X64Assembler.Label();
            _assembler.Bind(_labels[i]);

            EmitInstruction(instr);
        }
    }

    private void EmitInstruction(Instruction instr)
    {
        switch (instr.Opcode)
        {
            case Opcode.ADD:
                EmitAlu(instr, (dst, src) => _assembler.Add(dst, src), (dst, imm) => _assembler.Add(dst, imm));
                break;
            case Opcode.SUB:
                EmitAlu(instr, (dst, src) => _assembler.Sub(dst, src), (dst, imm) => _assembler.Sub(dst, imm));
                break;
            case Opcode.AND:
                EmitAlu(instr, (dst, src) => _assembler.And(dst, src), (dst, imm) => _assembler.And(dst, imm));
                break;
            case Opcode.OR:
                EmitAlu(instr, (dst, src) => _assembler.Or(dst, src), (dst, imm) => _assembler.Or(dst, imm));
                break;
            case Opcode.XOR:
                EmitAlu(instr, (dst, src) => _assembler.Xor(dst, src), (dst, imm) => _assembler.Xor(dst, imm));
                break;
            case Opcode.NOT:
                EmitUnary(instr, dst => _assembler.Not(dst));
                break;
            case Opcode.COPY:
                EmitCopy(instr);
                break;
            case Opcode.SHL:
                EmitShift(instr, (dst, imm) => _assembler.Shl(dst, imm), dst => _assembler.Shl(dst));
                break;
            case Opcode.SHR:
                EmitShift(instr, (dst, imm) => _assembler.Shr(dst, imm), dst => _assembler.Shr(dst));
                break;
            case Opcode.SAR:
                EmitShift(instr, (dst, imm) => _assembler.Sar(dst, imm), dst => _assembler.Sar(dst));
                break;
            case Opcode.CMP_EQ:
                EmitCmp(instr, label => _assembler.Je(label), label => _assembler.Jne(label));
                break;
            case Opcode.CMP_NE:
                EmitCmp(instr, label => _assembler.Jne(label), label => _assembler.Je(label));
                break;
            case Opcode.CMP_LT: // Signed Less
                EmitCmp(instr, label => _assembler.Jl(label), label => _assembler.Jge(label));
                break;
            case Opcode.CMP_GE: // Signed Greater Equal
                EmitCmp(instr, label => _assembler.Jge(label), label => _assembler.Jl(label));
                break;
            case Opcode.CMP_GT: // Signed Greater
                EmitCmp(instr, label => _assembler.Jg(label), label => _assembler.Jle(label));
                break;
            // Unsigned comparisons? IR has CMP_LT/GT etc. Are they signed or unsigned?
            // Opcode.cs: CMP_LT, CMP_GT, CMP_GT_SIGN, CMP_GE, CMP_GE_SIGN.
            // So CMP_LT/CMP_GT/CMP_GE are likely unsigned?
            // Wait, Instruction.cs: "cmp_lt", "cmp_gt", "cmp_gt_sign".
            // Convention: default is usually unsigned in some IRs, signed in others.
            // SH4 instructions: CMP/EQ, CMP/GE (signed), CMP/GT (signed), CMP/HI (unsigned >), CMP/HS (unsigned >=).
            // So CMP_GE_SIGN maps to SH4 CMP/GE.
            // CMP_GE maps to SH4 CMP/HS (unsigned).
            // Let's assume:
            // CMP_GE -> Unsigned (JAE)
            // CMP_GT -> Unsigned (JA)
            // CMP_LT -> Unsigned (JB)
            // CMP_GE_SIGN -> Signed (JGE)
            // CMP_GT_SIGN -> Signed (JG)

            // Re-mapping:
            // Opcode.CMP_LT -> Unsigned Less (JB)
            // Opcode.CMP_GT -> Unsigned Greater (JA)
            // Opcode.CMP_GE -> Unsigned Greater Equal (JAE)

            // Opcode.CMP_GT_SIGN -> Signed Greater (JG)
            // Opcode.CMP_GE_SIGN -> Signed Greater Equal (JGE)

            case Opcode.CMP_GT_SIGN:
                EmitCmp(instr, label => _assembler.Jg(label), label => _assembler.Jle(label));
                break;
            case Opcode.CMP_GE_SIGN:
                EmitCmp(instr, label => _assembler.Jge(label), label => _assembler.Jl(label));
                break;

            case Opcode.BRANCH:
                EmitBranch(instr);
                break;
            case Opcode.BRANCH_TRUE:
                // Conditional branch based on previous comparison?
                // IR semantics: BRANCH_TRUE A -> jump to A if condition is true.
                // Condition is usually stored in T flag or implicit?
                // Opcode.CMP_* usually set "Destiny" to 1 or 0?
                // Instruction.cs says: "cmp_eq opA, opB -> dst"
                // So result is in dst.
                // BRANCH_TRUE opA -> jump if opA != 0.
                EmitBranchTrue(instr);
                break;
            case Opcode.BRANCH_FALSE:
                EmitBranchFalse(instr);
                break;

            case Opcode.LOAD:
                EmitLoad(instr);
                break;
            case Opcode.STORE:
                EmitStore(instr);
                break;

            default:
                // throw new NotImplementedException($"Opcode {instr.Opcode} not implemented");
                break;
        }
    }

    private void EmitCopy(Instruction instr)
    {
        // dst <- src
        LoadToRegister(instr.A, X64Registers.RAX);
        StoreFromRegister(instr.Destiny, X64Registers.RAX);
    }

    private void EmitAlu(Instruction instr, Action<X64Registers, X64Registers> emitReg, Action<X64Registers, int> emitImm)
    {
        LoadToRegister(instr.A, X64Registers.RAX);
        if (instr.B.Kind == OperandKind.Constant)
        {
            emitImm(X64Registers.RAX, (int)instr.B.UConst32);
        }
        else
        {
            LoadToRegister(instr.B, X64Registers.RCX);
            emitReg(X64Registers.RAX, X64Registers.RCX);
        }
        StoreFromRegister(instr.Destiny, X64Registers.RAX);
    }

    private void EmitUnary(Instruction instr, Action<X64Registers> emit)
    {
        LoadToRegister(instr.A, X64Registers.RAX);
        emit(X64Registers.RAX);
        StoreFromRegister(instr.Destiny, X64Registers.RAX);
    }

    private void EmitShift(Instruction instr, Action<X64Registers, int> emitImm, Action<X64Registers> emitCl)
    {
        LoadToRegister(instr.A, X64Registers.RAX);
        if (instr.B.Kind == OperandKind.Constant)
        {
            emitImm(X64Registers.RAX, (int)instr.B.UConst32);
        }
        else
        {
            LoadToRegister(instr.B, X64Registers.RCX); // RCX is required for CL
            emitCl(X64Registers.RAX);
        }
        StoreFromRegister(instr.Destiny, X64Registers.RAX);
    }

    private void EmitCmp(Instruction instr, Action<X64Assembler.Label> jumpIfTrue, Action<X64Assembler.Label> jumpIfFalse)
    {
        // cmp_op A, B -> Dest
        // Result is boolean (1 or 0)

        LoadToRegister(instr.A, X64Registers.RAX);
        if (instr.B.Kind == OperandKind.Constant)
        {
            _assembler.Cmp(X64Registers.RAX, (int)instr.B.UConst32);
        }
        else
        {
            LoadToRegister(instr.B, X64Registers.RCX);
            _assembler.Cmp(X64Registers.RAX, X64Registers.RCX);
        }

        // Use setcc? Or conditional jumps?
        // setcc is better for producing 0/1 result.
        // But Assembler has Setne/Sete but maybe not Setg/Setl etc.
        // Let's use conditional jumps to set 0 or 1.

        var labelTrue = new X64Assembler.Label();
        var labelEnd = new X64Assembler.Label();

        jumpIfTrue(labelTrue);
        _assembler.Mov(X64Registers.RAX, 0); // False
        _assembler.Jmp(labelEnd);
        _assembler.Bind(labelTrue);
        _assembler.Mov(X64Registers.RAX, 1); // True
        _assembler.Bind(labelEnd);

        StoreFromRegister(instr.Destiny, X64Registers.RAX);
    }

    private void EmitBranch(Instruction instr)
    {
        // Unconditional branch to label
        // instr.Destiny is usually not set for Branch?
        // instr.A should be label?
        // Wait, Instruction.ToString says: "branch {dst}" ? No, usually "branch {label}".
        // Opcode.BRANCH: Instruction.cs says "branch {dst}" in switch.
        // OperandKind.Label has BlockOffset.

        // Let's assume instr.Destiny is the target if it's a label.
        // Wait, Instruction.cs: Opcode.BRANCH -> $"branch {dst}"
        // But Opcode.BRANCH_TRUE -> $"branch_if_true {opA} -> {dst}"
        // dst is the label operand?

        int targetIndex = instr.Destiny?.BlockOffset ?? 0;
        if (!_labels.ContainsKey(targetIndex)) _labels[targetIndex] = new X64Assembler.Label();
        _assembler.Jmp(_labels[targetIndex]);
    }

    private void EmitBranchTrue(Instruction instr)
    {
        // A is condition, Destiny is target
        LoadToRegister(instr.A, X64Registers.RAX);
        _assembler.Cmp(X64Registers.RAX, 0);

        int targetIndex = instr.Destiny?.BlockOffset ?? 0;
        if (!_labels.ContainsKey(targetIndex)) _labels[targetIndex] = new X64Assembler.Label();

        _assembler.Jne(_labels[targetIndex]); // Jump if != 0 (True)
    }

    private void EmitBranchFalse(Instruction instr)
    {
        // A is condition, Destiny is target
        LoadToRegister(instr.A, X64Registers.RAX);
        _assembler.Cmp(X64Registers.RAX, 0);

        int targetIndex = instr.Destiny?.BlockOffset ?? 0;
        if (!_labels.ContainsKey(targetIndex)) _labels[targetIndex] = new X64Assembler.Label();

        _assembler.Je(_labels[targetIndex]); // Jump if == 0 (False)
    }

    private void EmitLoad(Instruction instr)
    {
        throw new NotImplementedException("Memory access (LOAD) not supported in JIT yet");
    }

    private void EmitStore(Instruction instr)
    {
        throw new NotImplementedException("Memory access (STORE) not supported in JIT yet");
    }

    private void LoadToRegister(Operand op, X64Registers target)
    {
        if (op.Kind == OperandKind.Constant)
        {
            _assembler.Mov(target, (int)op.UConst32);
            return;
        }

        int id = _liveness.GetId(op);
        if (id == -1) return; // Should not happen for vars/regs

        var loc = _allocator.GetLocation(id);
        if (loc.HasValue)
        {
            if (loc.Value.Type == LocationType.Register)
            {
                if (loc.Value.Register != target)
                    _assembler.Mov(target, loc.Value.Register);
            }
            else
            {
                // Stack (spilled local)
                // [RBP + offset]
                _assembler.Mov(target, X64Registers.RBP, loc.Value.StackOffset);
            }
        }
        else
        {
            // Not mapped by allocator (unmapped local or architectural reg?)
            // If architectural reg (0-15), we mapped them in Prologue.
            // But if allocator decided to spill them? Then loc.Type == Stack.
            // If allocator didn't map them at all (e.g. not live), we might need to load from R15 (memory state).
            // But if LivenessAnalysis says it's not live, we don't need to load it?
            // Safer to load from memory if ID < 16.

            if (id < 16)
            {
                _assembler.Mov(target, X64Registers.R15, id * 4);
            }
        }
    }

    private void StoreFromRegister(Operand op, X64Registers source)
    {
        if (op == null) return;
        int id = _liveness.GetId(op);
        if (id == -1) return;

        var loc = _allocator.GetLocation(id);
        if (loc.HasValue)
        {
            if (loc.Value.Type == LocationType.Register)
            {
                if (loc.Value.Register != source)
                    _assembler.Mov(loc.Value.Register, source);
            }
            else
            {
                _assembler.Mov(X64Registers.RBP, loc.Value.StackOffset, source);
            }
        }
        else
        {
            if (id < 16)
            {
                _assembler.Mov(X64Registers.R15, id * 4, source);
            }
        }
    }
}
