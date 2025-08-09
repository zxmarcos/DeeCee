using NUnit.Framework;
using System;

namespace DeeCee.SH4.Test
{
    /// <summary>
    /// Testes para instruções de extensão de sinal (sign-extend) e extensão de zero (zero-extend).
    /// </summary>
    public unsafe class ExtOpsTest
    {
        private Sh4CpuState _state;
        private Sh4FrontEnd _fe;

        [SetUp]
        public void Setup()
        {
            _state = new Sh4CpuState();
            _fe = new Sh4FrontEnd();
        }

        private void CompileInstruction(ushort instruction)
        {
            _fe.Context.Block.Clear();
            _fe.Compile(instruction);
        }

        /// <summary>
        /// Define um estado inicial para os registradores, executa o bloco compilado
        /// e verifica o resultado com uma asserção customizada.
        /// </summary>
        private void ExecuteAndAssert(Action setup, Action<Sh4CpuState> assertion)
        {
            fixed (Sh4CpuState* statePtr = &_state)
            {
                var interpreter = new Sh4Interpreter(statePtr);
                setup();
                interpreter.Execute(_fe.Context.Block);
                assertion(_state);
            }
        }

        [Test]
        public void TestExtsb() // Sign-Extend Byte
        {
            CompileInstruction(Sh4Assembler.EXTSB(1, 2)); // EXTS.B R1, R2

            // Testa com byte positivo (bit 7 = 0)
            ExecuteAndAssert(
                () => _state.R[1] = 0xFFFFFF7F, // 127
                s => Assert.That(s.R[2], Is.EqualTo(0x0000007F)));

            // Testa com byte negativo (bit 7 = 1)
            ExecuteAndAssert(
                () => _state.R[1] = 0xFFFFFF80, // -128
                s => Assert.That(s.R[2], Is.EqualTo(0xFFFFFF80)));

            // Testa com -1
            ExecuteAndAssert(
                () => _state.R[1] = 0x000000FF, // -1
                s => Assert.That(s.R[2], Is.EqualTo(0xFFFFFFFF)));
        }

        [Test]
        public void TestExtsw() // Sign-Extend Word
        {
            CompileInstruction(Sh4Assembler.EXTSW(1, 2)); // EXTS.W R1, R2

            // Testa com word positiva (bit 15 = 0)
            ExecuteAndAssert(
                () => _state.R[1] = 0xFFFF7FFF, // 32767
                s => Assert.That(s.R[2], Is.EqualTo(0x00007FFF)));

            // Testa com word negativa (bit 15 = 1)
            ExecuteAndAssert(
                () => _state.R[1] = 0xFFFF8000, // -32768
                s => Assert.That(s.R[2], Is.EqualTo(0xFFFF8000)));

            // Testa com -1
            ExecuteAndAssert(
                () => _state.R[1] = 0x0000FFFF, // -1
                s => Assert.That(s.R[2], Is.EqualTo(0xFFFFFFFF)));
        }

        [Test]
        public void TestExtub() // Zero-Extend Byte
        {
            CompileInstruction(Sh4Assembler.EXTUB(1, 2)); // EXTU.B R1, R2

            // Testa com byte positivo
            ExecuteAndAssert(
                () => _state.R[1] = 0xFFFFFF7F,
                s => Assert.That(s.R[2], Is.EqualTo(0x0000007F)));

            // Testa com byte cujo bit 7 é 1 (deve ser tratado como unsigned)
            ExecuteAndAssert(
                () => _state.R[1] = 0xFFFFFF80,
                s => Assert.That(s.R[2], Is.EqualTo(0x00000080)));
            
            // Testa a operação "in-place" (Rn é o mesmo que Rm)
            CompileInstruction(Sh4Assembler.EXTUB(1, 1));
            ExecuteAndAssert(
                () => _state.R[1] = 0xABCDEF12,
                s => Assert.That(s.R[1], Is.EqualTo(0x00000012)));
        }

        [Test]
        public void TestExtuw() // Zero-Extend Word
        {
            CompileInstruction(Sh4Assembler.EXTUW(1, 2)); // EXTU.W R1, R2

            // Testa com word positiva
            ExecuteAndAssert(
                () => _state.R[1] = 0xFFFF7FFF,
                s => Assert.That(s.R[2], Is.EqualTo(0x00007FFF)));

            // Testa com word cujo bit 15 é 1 (deve ser tratado como unsigned)
            ExecuteAndAssert(
                () => _state.R[1] = 0xFFFF8000,
                s => Assert.That(s.R[2], Is.EqualTo(0x00008000)));

            // Testa a operação "in-place" (Rn é o mesmo que Rm)
            CompileInstruction(Sh4Assembler.EXTUW(1, 1));
            ExecuteAndAssert(
                () => _state.R[1] = 0xABCDEF12,
                s => Assert.That(s.R[1], Is.EqualTo(0x0000EF12)));
        }
    }
}