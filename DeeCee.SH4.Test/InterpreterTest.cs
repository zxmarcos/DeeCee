using NUnit.Framework;
using DeeCee.SH4.Interpreter;
using DeeCee.SH4;
using System.Collections.Generic;

namespace DeeCee.SH4.Test
{
    public unsafe class InterpreterTest
    {
        private Sh4CpuState _state;
        private TestMemory _memory;

        [SetUp]
        public void Setup()
        {
            _state = new Sh4CpuState();
            _memory = new TestMemory(1024);
        }

        private void Execute(BasicBlock block)
        {
            fixed (Sh4CpuState* statePtr = &_state)
            {
                var interpreter = new Interpreter.Interpreter(statePtr);
                interpreter.Memory = _memory;
                interpreter.Execute(block);
            }
        }

        // Helper to create Register Operand
        private Operand R(int regNum) => Operand.Register((byte)regNum, RegisterType.UInt32);

        // Helper to create Constant Operand
        private Operand Imm(uint val) => new Operand(ConstantType.UInt32) { UConst32 = val };

        // Helper to create Local Variable Operand
        private Operand Var(int index) => new Operand(OperandKind.LocalVariable) { VarIndex = index, RegType = RegisterType.UInt32 };

        // Helper to create Label Operand
        private Operand Label(int offset) => new Operand(OperandKind.Label) { BlockOffset = offset };

        // Helper to create Memory Operand
        private Operand Mem(uint address, MemoryWidth width) => new Operand(OperandKind.Memory) { Address = Imm(address), MemoryWidth = width };

        private BasicBlock Block(params Instruction[] instructions)
        {
            var block = new BasicBlock();
            foreach (var instr in instructions)
            {
                block.Add(instr);
            }
            // Count local variables
            int maxVar = -1;
            foreach (var instr in instructions)
            {
                if (instr.Destiny?.Kind == OperandKind.LocalVariable) maxVar = System.Math.Max(maxVar, instr.Destiny.VarIndex);
                if (instr.A?.Kind == OperandKind.LocalVariable) maxVar = System.Math.Max(maxVar, instr.A.VarIndex);
                if (instr.B?.Kind == OperandKind.LocalVariable) maxVar = System.Math.Max(maxVar, instr.B.VarIndex);
            }
            block.LocalVariableCount = maxVar + 1;
            return block;
        }

        [Test]
        public void TestAdd()
        {
            var block = Block(new Instruction(R(1), Imm(10), R(2), Opcode.ADD));
            _state.R[1] = 20;
            Execute(block);
            Assert.That(_state.R[2], Is.EqualTo(30));

            // Overflow (wrapping)
            block = Block(new Instruction(R(1), Imm(1), R(2), Opcode.ADD));
            _state.R[1] = uint.MaxValue;
            Execute(block);
            Assert.That(_state.R[2], Is.EqualTo(0));
        }

        [Test]
        public void TestSub()
        {
            var block = Block(new Instruction(R(1), Imm(10), R(2), Opcode.SUB));
            _state.R[1] = 30;
            Execute(block);
            Assert.That(_state.R[2], Is.EqualTo(20));

            // Underflow (wrapping)
            block = Block(new Instruction(R(1), Imm(1), R(2), Opcode.SUB));
            _state.R[1] = 0;
            Execute(block);
            Assert.That(_state.R[2], Is.EqualTo(uint.MaxValue));
        }

        [Test]
        public void TestMul() // Unsigned multiplication
        {
            var block = Block(new Instruction(R(1), R(2), R(3), Opcode.MUL));
            _state.R[1] = 10;
            _state.R[2] = 20;
            Execute(block);
            Assert.That(_state.R[3], Is.EqualTo(200));

            // Overflow
             _state.R[1] = 0x10000;
            _state.R[2] = 0x10000;
            Execute(block);
            Assert.That(_state.R[3], Is.EqualTo(0)); // 0x10000 * 0x10000 = 0x100000000 -> 0 (32-bit)
        }

        [Test]
        public void TestMuls() // Signed multiplication
        {
             var block = Block(new Instruction(R(1), R(2), R(3), Opcode.MULS));
            _state.R[1] = unchecked((uint)-2); // 0xFFFFFFFE
            _state.R[2] = 3;
            Execute(block);
            Assert.That((int)_state.R[3], Is.EqualTo(-6));
        }

        [Test]
        public void TestBitwise()
        {
            // AND
            var block = Block(new Instruction(R(1), R(2), R(3), Opcode.AND));
            _state.R[1] = 0b1100;
            _state.R[2] = 0b1010;
            Execute(block);
            Assert.That(_state.R[3], Is.EqualTo(0b1000));

            // OR
            block = Block(new Instruction(R(1), R(2), R(3), Opcode.OR));
            Execute(block);
            Assert.That(_state.R[3], Is.EqualTo(0b1110));

            // XOR
            block = Block(new Instruction(R(1), R(2), R(3), Opcode.XOR));
            Execute(block);
            Assert.That(_state.R[3], Is.EqualTo(0b0110));

            // NOT
            block = Block(new Instruction(R(1), null, R(3), Opcode.NOT));
            _state.R[1] = 0;
            Execute(block);
            Assert.That(_state.R[3], Is.EqualTo(uint.MaxValue));
        }

        [Test]
        public void TestShift()
        {
            // SHL
            var block = Block(new Instruction(R(1), Imm(2), R(2), Opcode.SHL));
            _state.R[1] = 1;
            Execute(block);
            Assert.That(_state.R[2], Is.EqualTo(4));

            // SHR (Unsigned)
            block = Block(new Instruction(R(1), Imm(1), R(2), Opcode.SHR));
            _state.R[1] = 0x80000000;
            Execute(block);
            Assert.That(_state.R[2], Is.EqualTo(0x40000000));

            // SAR (Signed)
            block = Block(new Instruction(R(1), Imm(1), R(2), Opcode.SAR));
            _state.R[1] = 0x80000000;
            Execute(block);
            Assert.That(_state.R[2], Is.EqualTo(0xC0000000));

            // ROL
            block = Block(new Instruction(R(1), Imm(1), R(2), Opcode.ROL));
            _state.R[1] = 0x80000000;
            Execute(block);
            Assert.That(_state.R[2], Is.EqualTo(1));

            // ROR
            block = Block(new Instruction(R(1), Imm(1), R(2), Opcode.ROR));
            _state.R[1] = 1;
            Execute(block);
            Assert.That(_state.R[2], Is.EqualTo(0x80000000));
        }

        [Test]
        public void TestExtension()
        {
             // SIGN_EXT8
            var block = Block(new Instruction(R(1), null, R(2), Opcode.SIGN_EXT8));
            _state.R[1] = 0xFF; // -1 as byte
            Execute(block);
            Assert.That(_state.R[2], Is.EqualTo(uint.MaxValue));

            _state.R[1] = 0x7F; // 127 as byte
            Execute(block);
            Assert.That(_state.R[2], Is.EqualTo(0x7F));

            // SIGN_EXT16
            block = Block(new Instruction(R(1), null, R(2), Opcode.SIGN_EXT16));
            _state.R[1] = 0xFFFF; // -1 as short
            Execute(block);
            Assert.That(_state.R[2], Is.EqualTo(uint.MaxValue));

            // ZERO_EXT8
            block = Block(new Instruction(R(1), null, R(2), Opcode.ZERO_EXT8));
            _state.R[1] = 0xFFFFFFFF;
            Execute(block);
            Assert.That(_state.R[2], Is.EqualTo(0xFF));

             // ZERO_EXT16
            block = Block(new Instruction(R(1), null, R(2), Opcode.ZERO_EXT16));
            _state.R[1] = 0xFFFFFFFF;
            Execute(block);
            Assert.That(_state.R[2], Is.EqualTo(0xFFFF));
        }

        [Test]
        public void TestComparison()
        {
            // CMP_EQ
            var block = Block(new Instruction(R(1), R(2), R(3), Opcode.CMP_EQ));
            _state.R[1] = 10;
            _state.R[2] = 10;
            Execute(block);
            Assert.That(_state.R[3], Is.EqualTo(1));

            _state.R[2] = 11;
            Execute(block);
            Assert.That(_state.R[3], Is.EqualTo(0));

            // CMP_NE
            block = Block(new Instruction(R(1), R(2), R(3), Opcode.CMP_NE));
            _state.R[1] = 10;
            _state.R[2] = 11;
            Execute(block);
            Assert.That(_state.R[3], Is.EqualTo(1));

            // CMP_LT (Unsigned)
            block = Block(new Instruction(R(1), R(2), R(3), Opcode.CMP_LT));
            _state.R[1] = 10;
            _state.R[2] = 20;
            Execute(block);
            Assert.That(_state.R[3], Is.EqualTo(1));

            _state.R[1] = 20;
            Execute(block);
            Assert.That(_state.R[3], Is.EqualTo(0));

            // CMP_GT (Unsigned)
            block = Block(new Instruction(R(1), R(2), R(3), Opcode.CMP_GT));
            _state.R[1] = 20;
            _state.R[2] = 10;
            Execute(block);
            Assert.That(_state.R[3], Is.EqualTo(1));

            // CMP_GT_SIGN (Signed)
            block = Block(new Instruction(R(1), R(2), R(3), Opcode.CMP_GT_SIGN));
            _state.R[1] = unchecked((uint)-1); // -1
            _state.R[2] = 10;
            Execute(block);
            Assert.That(_state.R[3], Is.EqualTo(0)); // -1 is not > 10

            _state.R[1] = 10;
            _state.R[2] = unchecked((uint)-1);
            Execute(block);
            Assert.That(_state.R[3], Is.EqualTo(1)); // 10 > -1

            // CMP_GE (Unsigned)
            block = Block(new Instruction(R(1), R(2), R(3), Opcode.CMP_GE));
            _state.R[1] = 10;
            _state.R[2] = 10;
            Execute(block);
            Assert.That(_state.R[3], Is.EqualTo(1));

             // CMP_GE_SIGN (Signed)
            block = Block(new Instruction(R(1), R(2), R(3), Opcode.CMP_GE_SIGN));
            _state.R[1] = unchecked((uint)-1); // -1
            _state.R[2] = unchecked((uint)-1); // -1
            Execute(block);
            Assert.That(_state.R[3], Is.EqualTo(1));
        }

        [Test]
        public void TestBranch()
        {
            // BRANCH
            // R1 = 1
            // BRANCH target
            // R1 = 2 (skipped)
            // target:
            // R1 = 3
            var block = new BasicBlock();
            block.Add(new Instruction(Imm(1), null, R(1), Opcode.COPY));
            block.Add(new Instruction(null, null, Label(3), Opcode.BRANCH)); // Jump to instruction index 3
            block.Add(new Instruction(Imm(2), null, R(1), Opcode.COPY));
            block.Add(new Instruction(Imm(3), null, R(1), Opcode.COPY)); // Index 3

            block.LocalVariableCount = 0;

            Execute(block);
            Assert.That(_state.R[1], Is.EqualTo(3));
        }

        [Test]
        public void TestBranchTrue()
        {
            // R2 = 1 (True)
            // BRANCH_TRUE R2, target
            // R1 = 2 (Skipped)
            // target:
            // R1 = 3
            var block = new BasicBlock();
            block.Add(new Instruction(Imm(1), null, R(1), Opcode.COPY)); // Init R1
            block.Add(new Instruction(Imm(1), null, R(2), Opcode.COPY)); // Condition True
            block.Add(new Instruction(R(2), null, Label(4), Opcode.BRANCH_TRUE)); // Jump to 4 if R2 != 0
            block.Add(new Instruction(Imm(2), null, R(1), Opcode.COPY));
            block.Add(new Instruction(Imm(3), null, R(1), Opcode.COPY)); // Index 4

            block.LocalVariableCount = 0;
            Execute(block);
            Assert.That(_state.R[1], Is.EqualTo(3));

             // Condition False (Corrected)
            block = new BasicBlock();
            block.Add(new Instruction(Imm(1), null, R(1), Opcode.COPY)); // Init R1
            block.Add(new Instruction(Imm(0), null, R(2), Opcode.COPY)); // Condition False
            block.Add(new Instruction(R(2), null, Label(4), Opcode.BRANCH_TRUE)); // Should NOT jump
            block.Add(new Instruction(Imm(2), null, R(1), Opcode.COPY)); // Should execute, R1=2
            block.Add(new Instruction(null, null, Label(6), Opcode.BRANCH)); // Jump to end to skip next instruction
            block.Add(new Instruction(Imm(3), null, R(1), Opcode.COPY)); // Index 5, should be skipped by branch above

            Execute(block);
            Assert.That(_state.R[1], Is.EqualTo(2));
        }

        [Test]
        public void TestBranchFalse()
        {
             // Condition True (Should NOT jump)
            var block = new BasicBlock();
            block.Add(new Instruction(Imm(1), null, R(1), Opcode.COPY));
            block.Add(new Instruction(Imm(1), null, R(2), Opcode.COPY)); // True
            block.Add(new Instruction(R(2), null, Label(4), Opcode.BRANCH_FALSE)); // Should NOT jump
            block.Add(new Instruction(Imm(2), null, R(1), Opcode.COPY)); // R1=2
            block.Add(new Instruction(null, null, Label(6), Opcode.BRANCH));
            block.Add(new Instruction(Imm(3), null, R(1), Opcode.COPY)); // Index 5

            Execute(block);
            Assert.That(_state.R[1], Is.EqualTo(2));

            // Condition False (Should Jump)
            block = new BasicBlock();
            block.Add(new Instruction(Imm(1), null, R(1), Opcode.COPY));
            block.Add(new Instruction(Imm(0), null, R(2), Opcode.COPY)); // False
            block.Add(new Instruction(R(2), null, Label(4), Opcode.BRANCH_FALSE)); // Should jump to 4
            block.Add(new Instruction(Imm(2), null, R(1), Opcode.COPY));
            block.Add(new Instruction(Imm(3), null, R(1), Opcode.COPY)); // Index 4, R1=3

            Execute(block);
            Assert.That(_state.R[1], Is.EqualTo(3));
        }

        [Test]
        public void TestMemory()
        {
            // STORE/LOAD Byte
            var block = Block(
                new Instruction(Imm(0x12345678), null, Mem(0x100, MemoryWidth.Byte), Opcode.STORE),
                new Instruction(Mem(0x100, MemoryWidth.Byte), null, R(1), Opcode.LOAD)
            );
            Execute(block);
            Assert.That(_state.R[1], Is.EqualTo(0x78));
            Assert.That(_memory.Read8(0x100), Is.EqualTo(0x78));

             // STORE/LOAD Word
            block = Block(
                new Instruction(Imm(0x12345678), null, Mem(0x102, MemoryWidth.Word), Opcode.STORE),
                new Instruction(Mem(0x102, MemoryWidth.Word), null, R(1), Opcode.LOAD)
            );
            Execute(block);
            Assert.That(_state.R[1], Is.EqualTo(0x5678));
            Assert.That(_memory.Read16(0x102), Is.EqualTo(0x5678));

            // STORE/LOAD Dword
            block = Block(
                new Instruction(Imm(0x12345678), null, Mem(0x104, MemoryWidth.Dword), Opcode.STORE),
                new Instruction(Mem(0x104, MemoryWidth.Dword), null, R(1), Opcode.LOAD)
            );
            Execute(block);
            Assert.That(_state.R[1], Is.EqualTo(0x12345678));
            Assert.That(_memory.Read32(0x104), Is.EqualTo(0x12345678));
        }

        [Test]
        public void TestCopy()
        {
            var block = Block(new Instruction(Imm(123), null, R(1), Opcode.COPY));
            Execute(block);
            Assert.That(_state.R[1], Is.EqualTo(123));

            block = Block(new Instruction(R(1), null, R(2), Opcode.COPY));
            _state.R[1] = 456;
            Execute(block);
            Assert.That(_state.R[2], Is.EqualTo(456));
        }
    }
}
