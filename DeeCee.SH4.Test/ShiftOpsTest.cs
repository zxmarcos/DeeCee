using NUnit.Framework;
using System;
using System.Diagnostics;

namespace DeeCee.SH4.Test
{
    /// <summary>
    /// Testes para instruções de deslocamento e rotação de bits.
    /// </summary>
    public unsafe class ShiftOpsTest
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
            Console.WriteLine(_fe.Context.Block);
        }

        /// <summary>
        /// Define um estado inicial para registradores e flags, executa o bloco compilado
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
        public void TestRotcl() // Rotate Left through Carry
        {
            CompileInstruction(Sh4Assembler.ROTCL(1));

            // T=0, MSB=0 -> T=0, LSB=0
            ExecuteAndAssert(
                () => { _state.R[1] = 0x40000000; _state.T = false; },
                s => { Assert.That(s.R[1], Is.EqualTo(0x80000000)); Assert.That(s.T, Is.False); });

            // T=0, MSB=1 -> T=1, LSB=0
            ExecuteAndAssert(
                () => { _state.R[1] = 0x97654321; _state.T = false; },
                s => { Assert.That(s.R[1], Is.EqualTo(0x2ECA8642)); Assert.That(s.T, Is.True); });

            // T=1, MSB=0 -> T=0, LSB=1
            ExecuteAndAssert(
                () => { _state.R[1] = 0x72345678; _state.T = true; },
                s => { Assert.That(s.R[1], Is.EqualTo(0xE468ACF1)); Assert.That(s.T, Is.False); });
            
            // T=1, MSB=1 -> T=1, LSB=1
            ExecuteAndAssert(
                () => { _state.R[1] = 0x97654321; _state.T = true; },
                s => { Assert.That(s.R[1], Is.EqualTo(0x2ECA8643)); Assert.That(s.T, Is.True); });
        }

        [Test]
        public void TestRotcr() // Rotate Right through Carry
        {
            CompileInstruction(Sh4Assembler.ROTCR(1));

            // T=0, LSB=0 -> T=0, MSB=0
            ExecuteAndAssert(
                () => { _state.R[1] = 0x12345678; _state.T = false; },
                s => { Assert.That(s.R[1], Is.EqualTo(0x091A2B3C)); Assert.That(s.T, Is.False); });

            // T=0, LSB=1 -> T=1, MSB=0
            ExecuteAndAssert(
                () => { _state.R[1] = 0x87654321; _state.T = false; },
                s => { Assert.That(s.R[1], Is.EqualTo(0x43B2A190)); Assert.That(s.T, Is.True); });

            // T=1, LSB=0 -> T=0, MSB=1
            ExecuteAndAssert(
                () => { _state.R[1] = 0x12345678; _state.T = true; },
                s => { Assert.That(s.R[1], Is.EqualTo(0x891A2B3C)); Assert.That(s.T, Is.False); });

            // T=1, LSB=1 -> T=1, MSB=1
            ExecuteAndAssert(
                () => { _state.R[1] = 0x87654321; _state.T = true; },
                s => { Assert.That(s.R[1], Is.EqualTo(0xC3B2A190)); Assert.That(s.T, Is.True); });
        }
        
        [Test]
        public void TestRotlAndRotr()
        {
            // ROTL
            CompileInstruction(Sh4Assembler.ROTL(1));
            ExecuteAndAssert(
                () => _state.R[1] = 0x87654321,
                s => { Assert.That(s.R[1], Is.EqualTo(0x0ECA8643)); Assert.That(s.T, Is.True); });

            // ROTR
            CompileInstruction(Sh4Assembler.ROTR(1));
            ExecuteAndAssert(
                () => _state.R[1] = 0x87654321,
                s => { Assert.That(s.R[1], Is.EqualTo(0xC3B2A190)); Assert.That(s.T, Is.True); });
        }

        [Test]
        public void TestShalAndShll() // São a mesma instrução
        {
            CompileInstruction(Sh4Assembler.SHAL(1));
            ExecuteAndAssert(
                () => _state.R[1] = 0x92345678,
                s => { Assert.That(s.R[1], Is.EqualTo(0x2468ACF0)); Assert.That(s.T, Is.True); });

            CompileInstruction(Sh4Assembler.SHLL(1));
             ExecuteAndAssert(
                () => _state.R[1] = 0x12345678,
                s => { Assert.That(s.R[1], Is.EqualTo(0x2468ACF0)); Assert.That(s.T, Is.False); });
        }
        
        [Test]
        public void TestSharAndShlr()
        {
            // SHAR (Arithmetic)
            CompileInstruction(Sh4Assembler.SHAR(1));
            ExecuteAndAssert(
                () => _state.R[1] = 0x92345679,
                s => { Assert.That(s.R[1], Is.EqualTo(0xC91A2B3C)); Assert.That(s.T, Is.True); });
            
            // SHLR (Logical)
            CompileInstruction(Sh4Assembler.SHLR(1));
            ExecuteAndAssert(
                () => _state.R[1] = 0x92345679,
                s => { Assert.That(s.R[1], Is.EqualTo(0x491A2B3C)); Assert.That(s.T, Is.True); });
        }

        [Test]
        public void TestFixedShifts()
        {
            // SHLL2, SHLL8, SHLL16
            CompileInstruction(Sh4Assembler.SHLL2(1));
            ExecuteAndAssert(() => _state.R[1] = 0x12345678, s => Assert.That(s.R[1], Is.EqualTo(0x48D159E0)));
            CompileInstruction(Sh4Assembler.SHLL8(1));
            ExecuteAndAssert(() => _state.R[1] = 0x12345678, s => Assert.That(s.R[1], Is.EqualTo(0x34567800)));
            CompileInstruction(Sh4Assembler.SHLL16(1));
            ExecuteAndAssert(() => _state.R[1] = 0x12345678, s => Assert.That(s.R[1], Is.EqualTo(0x56780000)));

            // SHLR2, SHLR8, SHLR16
            CompileInstruction(Sh4Assembler.SHLR2(1));
            ExecuteAndAssert(() => _state.R[1] = 0x12345678, s => Assert.That(s.R[1], Is.EqualTo(0x048D159E)));
            CompileInstruction(Sh4Assembler.SHLR8(1));
            ExecuteAndAssert(() => _state.R[1] = 0x12345678, s => Assert.That(s.R[1], Is.EqualTo(0x00123456)));
            CompileInstruction(Sh4Assembler.SHLR16(1));
            ExecuteAndAssert(() => _state.R[1] = 0x12345678, s => Assert.That(s.R[1], Is.EqualTo(0x00001234)));
        }

        [Test]
        public void TestShad() // Dynamic Arithmetic Shift
        {
            CompileInstruction(Sh4Assembler.SHAD(1, 2)); // m=1, n=2

            // Deslocamento para a esquerda (Rm é positivo)
            ExecuteAndAssert(
                () => { _state.R[1] = 5; _state.R[2] = 0x100; },
                s => Assert.That(s.R[2], Is.EqualTo(0x2000)));

            // Deslocamento aritmético para a direita (Rm é negativo)
            ExecuteAndAssert(
                () => { _state.R[1] = unchecked((uint)-5); _state.R[2] = 0xF0000000; },
                s => Assert.That(s.R[2], Is.EqualTo(0xFF800000)));

            // Caso especial: deslocamento para direita de 0 (Rm < 0 e Rm & 0x1F == 0)
            // Rn positivo -> 0
            ExecuteAndAssert(
                () => { _state.R[1] = 0x80000000; _state.R[2] = 0x1234; },
                s => Assert.That(s.R[2], Is.EqualTo(0)));
            
            // Rn negativo -> -1 e Rm = 0
            ExecuteAndAssert(
                () => { _state.R[1] = 0x80000000; _state.R[2] = 0; },
                s => Assert.That(s.R[2], Is.EqualTo(0)));
            // Rn negativo -> -1 e Rm = -0
            ExecuteAndAssert(
                () => { _state.R[1] = 0x80000000; _state.R[2] = 0x80000000; },
                s => Assert.That(s.R[2], Is.EqualTo(0xFFFFFFFF)));
        }

        [Test]
        public void TestShld() // Dynamic Logical Shift
        {
            CompileInstruction(Sh4Assembler.SHLD(1, 2)); // m=1, n=2

            // Deslocamento para a esquerda (Rm é positivo)
            ExecuteAndAssert(
                () => { _state.R[1] = 5; _state.R[2] = 0x100; },
                s => Assert.That(s.R[2], Is.EqualTo(0x2000)));

            // Deslocamento lógico para a direita (Rm é negativo)
            ExecuteAndAssert(
                () => { _state.R[1] = unchecked((uint)-5); _state.R[2] = 0xF0000000; },
                s => Assert.That(s.R[2], Is.EqualTo(0x07800000)));

            // Caso especial: deslocamento para direita de 0 (Rm < 0 e Rm & 0x1F == 0) -> 0
            ExecuteAndAssert(
                () => { _state.R[1] = 0x80000000; _state.R[2] = 0x1234; },
                s => Assert.That(s.R[2], Is.EqualTo(0)));
        }
    }
}