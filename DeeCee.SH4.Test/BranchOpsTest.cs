using NUnit.Framework;
using System;

namespace DeeCee.SH4.Test
{
    /// <summary>
    /// Testes para instruções de desvio de fluxo (branch, jump, subroutine).
    /// </summary>
    public unsafe class BranchOpsTest
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
        public void TestBtAndBts() // Branch if True
        {
            // O comportamento de BTS é o mesmo de BT pois o delay slot não está implementado.
            CompileInstruction(Sh4Assembler.BT(10));

            // Caso 1: Desvio tomado (T=1)
            // PC = (PC inicial + 4) + (deslocamento * 2)
            // PC = (0x1000 + 4) + (10 * 2) = 0x1004 + 20 = 0x1018
            ExecuteAndAssert(
                () => { _state.PC = 0x1000; _state.T = true; },
                s => Assert.That(s.PC, Is.EqualTo(0x1018)));

            // Caso 2: Desvio não tomado (T=0)
            // PC deve permanecer inalterado pela lógica do desvio.
            ExecuteAndAssert(
                () => { _state.PC = 0x1000; _state.T = false; },
                s => Assert.That(s.PC, Is.EqualTo(0x1000)));
        }

        [Test]
        public void TestBfAndBfs() // Branch if False
        {
            CompileInstruction(Sh4Assembler.BF(-20)); // Deslocamento negativo

            // Caso 1: Desvio tomado (T=0)
            ExecuteAndAssert(
                () => { _state.PC = 0x1000; _state.T = false; },
                s => Assert.That(s.PC, Is.EqualTo(0x1000 + 4 + (-20 * 2))));

            // Caso 2: Desvio não tomado (T=1)
            ExecuteAndAssert(
                () => { _state.PC = 0x1000; _state.T = true; },
                s => Assert.That(s.PC, Is.EqualTo(0x1000)));
        }

        [Test]
        public void TestBraAndBsr() // Branch Always & Branch to Subroutine
        {
            // BRA
            CompileInstruction(Sh4Assembler.BRA(50));
            ExecuteAndAssert(
                () => { _state.PC = 0x2000; _state.PR = 0; },
                s =>
                {
                    // PC = (0x2000 + 4) + (50 * 2) = 0x2068
                    Assert.That(s.PC, Is.EqualTo(0x2068));
                    Assert.That(s.PR, Is.EqualTo(0)); // PR não deve ser modificado
                });

            // BSR
            CompileInstruction(Sh4Assembler.BSR(50));
            ExecuteAndAssert(
                () => { _state.PC = 0x2000; _state.PR = 0; },
                s =>
                {
                    // PR = PC + 4 = 0x2004
                    // PC = (0x2000 + 4) + (50 * 2) = 0x2068
                    Assert.That(s.PC, Is.EqualTo(0x2068));
                    Assert.That(s.PR, Is.EqualTo(0x2004));
                });
        }
        
        [Test]
        public void TestBrafAndBsrf() // Branch Always Far & Branch to Subroutine Far
        {
            // BRAF
            CompileInstruction(Sh4Assembler.BRAF(5)); // Desvio para endereço em R5
            ExecuteAndAssert(
                () => { _state.PC = 0x3000; _state.R[5] = 0x150; },
                s =>
                {
                    // PC = (PC + 4) + R[5] = (0x3000 + 4) + 0x150 = 0x3154
                    Assert.That(s.PC, Is.EqualTo(0x3154));
                });
            
            // BSRF
            CompileInstruction(Sh4Assembler.BSRF(5));
            ExecuteAndAssert(
                () => { _state.PC = 0x3000; _state.R[5] = 0x150; },
                s =>
                {
                    // PR = PC + 4 = 0x3004
                    // PC = (PC + 4) + R[5] = 0x3154
                    Assert.That(s.PR, Is.EqualTo(0x3004));
                    Assert.That(s.PC, Is.EqualTo(0x3154));
                });
        }

        [Test]
        public void TestJmpAndJsr() // Jump & Jump to Subroutine
        {
            // JMP
            CompileInstruction(Sh4Assembler.JMP(10)); // Salta para o endereço em R10
            ExecuteAndAssert(
                () => { _state.R[10] = 0xABCD0000; _state.PR = 0; },
                s =>
                {
                    Assert.That(s.PC, Is.EqualTo(0xABCD0000));
                    Assert.That(s.PR, Is.EqualTo(0));
                });

            // JSR
            CompileInstruction(Sh4Assembler.JSR(10));
            ExecuteAndAssert(
                () => { _state.PC = 0x8000; _state.R[10] = 0xABCD0000; },
                s =>
                {
                    // PR = PC + 4 = 0x8004
                    Assert.That(s.PR, Is.EqualTo(0x8004));
                    Assert.That(s.PC, Is.EqualTo(0xABCD0000));
                });
        }
        
        [Test]
        public void TestRts() // Return from Subroutine
        {
            CompileInstruction(Sh4Assembler.RTS());
            ExecuteAndAssert(
                () => _state.PR = 0xCAFEBABE,
                s => Assert.That(s.PC, Is.EqualTo(0xCAFEBABE)));
        }
    }
}