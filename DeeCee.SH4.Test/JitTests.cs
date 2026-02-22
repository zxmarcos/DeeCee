using NUnit.Framework;
using DeeCee.SH4;
using DeeCee.SH4.JIT;
using System.Runtime.InteropServices;
using System;

namespace DeeCee.SH4.Test;

public unsafe class JitTests
{
    private Sh4CpuState _state;
    private BasicBlock _block;
    private JitCompiler _compiler;

    [SetUp]
    public void Setup()
    {
        _state = new Sh4CpuState();
        _state.Reset();
        _block = new BasicBlock();
        _compiler = new JitCompiler();
    }

    private void Execute()
    {
        var jitFunc = _compiler.Compile(_block);
        fixed (Sh4CpuState* ptr = &_state)
        {
            jitFunc(ptr);
        }
    }

    [Test]
    public void TestAdd()
    {
        // R0 = 10
        // R1 = 20
        // R2 = R0 + R1

        _state.R[0] = 10;
        _state.R[1] = 20;

        // R0 + R1 -> R2
        _block.Add(new Instruction(
            Operand.Register(0, RegisterType.Int32),
            Operand.Register(1, RegisterType.Int32),
            Operand.Register(2, RegisterType.Int32),
            Opcode.ADD
        ));

        Execute();

        Assert.That(_state.R[2], Is.EqualTo(30));
    }

    [Test]
    public void TestImmediateAdd()
    {
        // R0 = 10
        // R0 = R0 + 5

        _state.R[0] = 10;

        _block.Add(new Instruction(
            Operand.Register(0, RegisterType.Int32),
            new Operand(ConstantType.UInt32) { UConst32 = 5 },
            Operand.Register(0, RegisterType.Int32),
            Opcode.ADD
        ));

        Execute();

        Assert.That(_state.R[0], Is.EqualTo(15));
    }

    [Test]
    public void TestBranch()
    {
        // R0 = 10
        // Branch to end
        // R0 = 20 (skipped)
        // End:

        _state.R[0] = 10;

        var label = new Operand(OperandKind.Label) { BlockOffset = 2 }; // Target index 2 (instruction 2)

        _block.Add(new Instruction(label, null, label, Opcode.BRANCH)); // Branch to label

        _block.Add(new Instruction(
            new Operand(ConstantType.UInt32) { UConst32 = 20 },
            null,
            Operand.Register(0, RegisterType.Int32),
            Opcode.COPY // Or ADD if COPY not supported? JIT supports COPY.
        ));

        // Target instruction (index 2)
        _block.Add(new Instruction(
             Operand.Register(0, RegisterType.Int32),
             new Operand(ConstantType.UInt32) { UConst32 = 30 }, // R0 = R0 + 30
             Operand.Register(0, RegisterType.Int32),
             Opcode.ADD
        ));

        Execute();

        // Should be 10 + 30 = 40. Skipped assignment to 20.
        Assert.That(_state.R[0], Is.EqualTo(40));
    }

    [Test]
    public void TestLocals()
    {
        // v0 = 10
        // v1 = 20
        // v2 = v0 + v1
        // R0 = v2

        var v0 = new Operand(OperandKind.LocalVariable) { VarIndex = 0 };
        var v1 = new Operand(OperandKind.LocalVariable) { VarIndex = 1 };
        var v2 = new Operand(OperandKind.LocalVariable) { VarIndex = 2 };

        // COPY 10 -> v0
        _block.Add(new Instruction(
            new Operand(ConstantType.UInt32) { UConst32 = 10 },
            null,
            v0,
            Opcode.COPY
        ));

        // COPY 20 -> v1
        _block.Add(new Instruction(
            new Operand(ConstantType.UInt32) { UConst32 = 20 },
            null,
            v1,
            Opcode.COPY
        ));

        // ADD v0, v1 -> v2
        _block.Add(new Instruction(
            v0,
            v1,
            v2,
            Opcode.ADD
        ));

        // COPY v2 -> R0
        _block.Add(new Instruction(
            v2,
            null,
            Operand.Register(0, RegisterType.Int32),
            Opcode.COPY
        ));

        Execute();

        Assert.That(_state.R[0], Is.EqualTo(30));
    }
}
