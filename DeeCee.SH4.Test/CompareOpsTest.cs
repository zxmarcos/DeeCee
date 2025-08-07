using NUnit.Framework;
using System;

namespace DeeCee.SH4.Test
{
    public unsafe class CompareOpsTest
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
            Console.WriteLine("Limpando e compilando: " + instruction.ToString("X4") + "");
            _fe.Context.Block.Clear();
            _fe.Compile(instruction);
        }
        
        private void ExecuteTest(Action setupState, bool expectedT)
        {
            fixed (Sh4CpuState* statePtr = &_state)
            {
                var interpreter = new Sh4Interpreter(statePtr);
                _state.SR = 0;
                setupState();
                interpreter.Execute(_fe.Context.Block);
                Assert.That(_state.T, Is.EqualTo(expectedT), $"Falha no teste para T={expectedT}");
            }
        }

        [Test]
        public void TestCMPEQI()
        {
            // Compara R0 com um valor imediato positivo
            CompileInstruction(Sh4Assembler.CMPEQI(42));
            ExecuteTest(() => _state.R[0] = 42, expectedT: true);
            ExecuteTest(() => _state.R[0] = 43, expectedT: false);

            // Compara R0 com um valor imediato negativo
            CompileInstruction(Sh4Assembler.CMPEQI(-10));
            ExecuteTest(() => _state.R[0] = unchecked((uint)-10), expectedT: true);
            ExecuteTest(() => _state.R[0] = 10, expectedT: false);
        }

        [Test]
        public void TestCMPEQ()
        {
            // Assembly: CMPEQ R1, R2 -> m=1, n=2
            // Operação: Rn == Rm -> R[2] == R[1]. A ordem não afeta o resultado.
            CompileInstruction(Sh4Assembler.CMPEQ(1, 2));

            ExecuteTest(() => { _state.R[1] = 123; _state.R[2] = 123; }, expectedT: true);
            ExecuteTest(() => { _state.R[1] = 123; _state.R[2] = 456; }, expectedT: false);
            ExecuteTest(() => { _state.R[1] = unchecked((uint)-5); _state.R[2] = unchecked((uint)-5); }, expectedT: true);
        }

        [Test]
        public void TestCMPGE()
        {
            // Assembly: CMPGE R3, R4 -> m=3, n=4
            // Operação com sinal: se Rn >= Rm -> se R[4] >= R[3]
            CompileInstruction(Sh4Assembler.CMPGE(3, 4));

            // Teste n > m (10 >= 5)
            ExecuteTest(() => { _state.R[4] = 10; _state.R[3] = 5; }, expectedT: true);

            // Teste n == m (5 >= 5)
            ExecuteTest(() => { _state.R[4] = 5; _state.R[3] = 5; }, expectedT: true);

            // Teste n < m (4 >= 5)
            ExecuteTest(() => { _state.R[4] = 4; _state.R[3] = 5; }, expectedT: false);

            // Teste n > m com negativos (-5 >= -10)
            ExecuteTest(() => { _state.R[4] = unchecked((uint)-5); _state.R[3] = unchecked((uint)-10); }, expectedT: true);

            // Teste n > m com positivo e negativo (5 >= -5)
            ExecuteTest(() => { _state.R[4] = 5; _state.R[3] = unchecked((uint)-5); }, expectedT: true);
        }

        [Test]
        public void TestCMPGT()
        {
            // Assembly: CMPGT R5, R6 -> m=5, n=6
            // Operação com sinal: se Rn > Rm -> se R[6] > R[5]
            CompileInstruction(Sh4Assembler.CMPGT(5, 6));

            // Teste n > m (10 > 5)
            ExecuteTest(() => { _state.R[6] = 10; _state.R[5] = 5; }, expectedT: true);

            // Teste n == m (5 > 5)
            ExecuteTest(() => { _state.R[6] = 5; _state.R[5] = 5; }, expectedT: false);

            // Teste n < m (4 > 5)
            ExecuteTest(() => { _state.R[6] = 4; _state.R[5] = 5; }, expectedT: false);

            // Teste n > m com negativos (-5 > -10)
            ExecuteTest(() => { _state.R[6] = unchecked((uint)-5); _state.R[5] = unchecked((uint)-10); }, expectedT: true);
        }

        [Test]
        public void TestCMPHI()
        {
            // Assembly: CMPHI R7, R8 -> m=7, n=8
            // Operação sem sinal: se Rn > Rm -> se R[8] > R[7]
            CompileInstruction(Sh4Assembler.CMPHI(7, 8));

            // Teste n > m (unsigned), 0xFFFFFFFF > 1
            ExecuteTest(() => { _state.R[8] = 0xFFFFFFFF; _state.R[7] = 1; }, expectedT: true);

            // Teste n > m, 10 > 5
            ExecuteTest(() => { _state.R[8] = 10; _state.R[7] = 5; }, expectedT: true);

            // Teste n == m, 5 > 5
            ExecuteTest(() => { _state.R[8] = 5; _state.R[7] = 5; }, expectedT: false);
            
            // Teste n < m, 5 > 10
            ExecuteTest(() => { _state.R[8] = 5; _state.R[7] = 10; }, expectedT: false);
        }

        [Test]
        public void TestCMPHS()
        {
            // Assembly: CMPHS R9, R10 -> m=9, n=10
            // Operação sem sinal: se Rn >= Rm -> se R[10] >= R[9]
            CompileInstruction(Sh4Assembler.CMPHS(9, 10));

            // Teste n > m (unsigned), 0xFFFFFFFF >= 1
            ExecuteTest(() => { _state.R[10] = 0xFFFFFFFF; _state.R[9] = 1; }, expectedT: true);

            // Teste n > m, 10 >= 5
            ExecuteTest(() => { _state.R[10] = 10; _state.R[9] = 5; }, expectedT: true);

            // Teste n == m, 5 >= 5
            ExecuteTest(() => { _state.R[10] = 5; _state.R[9] = 5; }, expectedT: true);

            // Teste n < m, 5 >= 10
            ExecuteTest(() => { _state.R[10] = 5; _state.R[9] = 10; }, expectedT: false);
        }
        
        [Test]
        public void TestCMPPL()
        {
            // Operação com sinal: se Rn > 0
            CompileInstruction(Sh4Assembler.CMPPL(11));

            ExecuteTest(() => _state.R[11] = 100, expectedT: true);
            ExecuteTest(() => _state.R[11] = 0, expectedT: false);
            ExecuteTest(() => _state.R[11] = unchecked((uint)-100), expectedT: false);
        }

        [Test]
        public void TestCMPPZ()
        {
            // Operação com sinal: se Rn >= 0
            CompileInstruction(Sh4Assembler.CMPPZ(12));

            ExecuteTest(() => _state.R[12] = 100, expectedT: true);
            ExecuteTest(() => _state.R[12] = 0, expectedT: true);
            ExecuteTest(() => _state.R[12] = unchecked((uint)-90), expectedT: false);
        }
        
        [Test]
        public void TestCMPSTR()
        {
            // Assembly: CMPSTR R2, R3 -> m=2, n=3
            // Operação: XOR(Rn, Rm) -> XOR(R[3], R[2]). A ordem não afeta o resultado.
            CompileInstruction(Sh4Assembler.CMPSTR(2, 3));
    
            ExecuteTest(() => { _state.R[2] = 0xAABBCCDD; _state.R[3] = 0xAADDDDBB; }, expectedT: true);
            ExecuteTest(() => { _state.R[2] = 0xAABBCCDD; _state.R[3] = 0xCCBBDDBB; }, expectedT: true);
            ExecuteTest(() => { _state.R[2] = 0xAABBCCDD; _state.R[3] = 0xFFDDCCBB; }, expectedT: true);
            ExecuteTest(() => { _state.R[2] = 0xAABBCCDD; _state.R[3] = 0xAADDDDDD; }, expectedT: true);
            ExecuteTest(() => { _state.R[2] = 0xAABBCCDD; _state.R[3] = 0xFFDDEEBB; }, expectedT: false);
        }
    }
}