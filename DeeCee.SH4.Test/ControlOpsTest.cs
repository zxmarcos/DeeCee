namespace DeeCee.SH4.Test
{
    /// <summary>
    /// Testes completos para todas as instruções que movem dados para e de registradores de controle da CPU.
    /// </summary>
    public unsafe class ControlOpsTest
    {
        private Sh4CpuState _state;
        private Sh4FrontEnd _fe;
        private TestMemory _memory;

        [SetUp]
        public void Setup()
        {
            _state = new Sh4CpuState();
            _fe = new Sh4FrontEnd();
            _memory = new TestMemory(0x10000);
        }

        private void CompileInstruction(ushort instruction)
        {
            _fe.Context.Block.Clear();
            _fe.Compile(instruction);
        }

        private void ExecuteAndAssert(Action setup, Action<Sh4CpuState, TestMemory> assertion)
        {
            fixed (Sh4CpuState* statePtr = &_state)
            {
                var interpreter = new Sh4Interpreter(statePtr);
                interpreter.Memory = _memory;
                setup();
                interpreter.Execute(_fe.Context.Block);
                assertion(_state, _memory);
            }
        }

        // Testes para cada registrador de controle

        [Test]
        public void TestSrOps() // Status Register
        {
            const uint MASK_SR = 0x700083F3;
            byte regNum = 1;
            uint memAddr = 0x1000;

            CompileInstruction(Sh4Assembler.LDCSR(regNum));
            ExecuteAndAssert(() => _state.R[regNum] = 0xFFFFFFFF,
                (s, m) => Assert.That(s.SR, Is.EqualTo(0xFFFFFFFF & MASK_SR)));

            CompileInstruction(Sh4Assembler.STCSR(regNum));
            ExecuteAndAssert(() => _state.SR = 0x12345678, (s, m) => Assert.That(s.R[regNum], Is.EqualTo(0x12345678)));

            CompileInstruction(Sh4Assembler.LDCMSR(regNum));
            ExecuteAndAssert(() =>
            {
                _state.R[regNum] = memAddr;
                _memory.Write32(memAddr, 0xAAAAAAAA);
            }, (s, m) =>
            {
                Assert.That(s.SR, Is.EqualTo(0xAAAAAAAA & MASK_SR));
                Assert.That(s.R[regNum], Is.EqualTo(memAddr + 4));
            });

            CompileInstruction(Sh4Assembler.STCMSR(regNum));
            ExecuteAndAssert(() =>
            {
                _state.R[regNum] = memAddr + 4;
                _state.SR = 0xBBBBBBBB;
            }, (s, m) =>
            {
                Assert.That(m.Read32(memAddr), Is.EqualTo(0xBBBBBBBB));
                Assert.That(s.R[regNum], Is.EqualTo(memAddr));
            });
        }

        [Test]
        public void TestGbrOps() // Global Base Register
        {
            byte regNum = 2;
            uint memAddr = 0x2000;

            CompileInstruction(Sh4Assembler.LDCGBR(regNum));
            ExecuteAndAssert(() => _state.R[regNum] = 0xABCD, (s, m) => Assert.That(s.GBR, Is.EqualTo(0xABCD)));

            CompileInstruction(Sh4Assembler.STCGBR(regNum));
            ExecuteAndAssert(() => _state.GBR = 0xDCBA, (s, m) => Assert.That(s.R[regNum], Is.EqualTo(0xDCBA)));

            CompileInstruction(Sh4Assembler.LDCMGBR(regNum));
            ExecuteAndAssert(() =>
            {
                _state.R[regNum] = memAddr;
                _memory.Write32(memAddr, 0x11223344);
            }, (s, m) =>
            {
                Assert.That(s.GBR, Is.EqualTo(0x11223344));
                Assert.That(s.R[regNum], Is.EqualTo(memAddr + 4));
            });

            CompileInstruction(Sh4Assembler.STCMGBR(regNum));
            ExecuteAndAssert(() =>
            {
                _state.R[regNum] = memAddr + 4;
                _state.GBR = 0x55667788;
            }, (s, m) =>
            {
                Assert.That(m.Read32(memAddr), Is.EqualTo(0x55667788));
                Assert.That(s.R[regNum], Is.EqualTo(memAddr));
            });
        }

        [Test]
        public void TestVbrOps() // Vector Base Register
        {
            byte regNum = 3;
            uint memAddr = 0x3000;

            CompileInstruction(Sh4Assembler.LDCVBR(regNum));
            ExecuteAndAssert(() => _state.R[regNum] = 0x4567, (s, m) => Assert.That(s.VBR, Is.EqualTo(0x4567)));

            CompileInstruction(Sh4Assembler.STCVBR(regNum));
            ExecuteAndAssert(() => _state.VBR = 0x7654, (s, m) => Assert.That(s.R[regNum], Is.EqualTo(0x7654)));

            CompileInstruction(Sh4Assembler.LDCMVBR(regNum));
            ExecuteAndAssert(() =>
            {
                _state.R[regNum] = memAddr;
                _memory.Write32(memAddr, 0x12345678);
            }, (s, m) =>
            {
                Assert.That(s.VBR, Is.EqualTo(0x12345678));
                Assert.That(s.R[regNum], Is.EqualTo(memAddr + 4));
            });

            CompileInstruction(Sh4Assembler.STCMVBR(regNum));
            ExecuteAndAssert(() =>
            {
                _state.R[regNum] = memAddr + 4;
                _state.VBR = 0x87654321;
            }, (s, m) =>
            {
                Assert.That(m.Read32(memAddr), Is.EqualTo(0x87654321));
                Assert.That(s.R[regNum], Is.EqualTo(memAddr));
            });
        }

        [Test]
        public void TestSsrOps() // Saved Status Register
        {
            byte regNum = 4;
            uint memAddr = 0x4000;

            CompileInstruction(Sh4Assembler.LDCSSR(regNum));
            ExecuteAndAssert(() => _state.R[regNum] = 0xAAAA, (s, m) => Assert.That(s.SSR, Is.EqualTo(0xAAAA)));

            CompileInstruction(Sh4Assembler.STCSSR(regNum));
            ExecuteAndAssert(() => _state.SSR = 0xBBBB, (s, m) => Assert.That(s.R[regNum], Is.EqualTo(0xBBBB)));

            CompileInstruction(Sh4Assembler.LDCMSSR(regNum));
            ExecuteAndAssert(() =>
            {
                _state.R[regNum] = memAddr;
                _memory.Write32(memAddr, 0xCCCC);
            }, (s, m) =>
            {
                Assert.That(s.SSR, Is.EqualTo(0xCCCC));
                Assert.That(s.R[regNum], Is.EqualTo(memAddr + 4));
            });

            CompileInstruction(Sh4Assembler.STCMSSR(regNum));
            ExecuteAndAssert(() =>
            {
                _state.R[regNum] = memAddr + 4;
                _state.SSR = 0xDDDD;
            }, (s, m) =>
            {
                Assert.That(m.Read32(memAddr), Is.EqualTo(0xDDDD));
                Assert.That(s.R[regNum], Is.EqualTo(memAddr));
            });
        }

        [Test]
        public void TestSpcOps() // Saved Program Counter
        {
            byte regNum = 5;
            uint memAddr = 0x5000;

            CompileInstruction(Sh4Assembler.LDCSPC(regNum));
            ExecuteAndAssert(() => _state.R[regNum] = 0xEEEE, (s, m) => Assert.That(s.SPC, Is.EqualTo(0xEEEE)));

            CompileInstruction(Sh4Assembler.STCSPC(regNum));
            ExecuteAndAssert(() => _state.SPC = 0xFFFF, (s, m) => Assert.That(s.R[regNum], Is.EqualTo(0xFFFF)));

            CompileInstruction(Sh4Assembler.LDCMSPC(regNum));
            ExecuteAndAssert(() =>
            {
                _state.R[regNum] = memAddr;
                _memory.Write32(memAddr, 0x1010);
            }, (s, m) =>
            {
                Assert.That(s.SPC, Is.EqualTo(0x1010));
                Assert.That(s.R[regNum], Is.EqualTo(memAddr + 4));
            });

            CompileInstruction(Sh4Assembler.STCMSPC(regNum));
            ExecuteAndAssert(() =>
            {
                _state.R[regNum] = memAddr + 4;
                _state.SPC = 0x2020;
            }, (s, m) =>
            {
                Assert.That(m.Read32(memAddr), Is.EqualTo(0x2020));
                Assert.That(s.R[regNum], Is.EqualTo(memAddr));
            });
        }

        // [Test]
        // public void TestSgrOps() // Saved General Register (R15_BANK)
        // {
        //     uint regNum = 6;
        //     uint memAddr = 0x6000;
        //     
        //     CompileInstruction(Sh4Assembler.LDC_SGR(regNum));
        //     ExecuteAndAssert(() => _state.R[regNum] = 0x3030, (s, m) => Assert.That(s.SGR, Is.EqualTo(0x3030)));
        //
        //     CompileInstruction(Sh4Assembler.STC_SGR(regNum));
        //     ExecuteAndAssert(() => s.SGR = 0x4040, (s, m) => Assert.That(s.R[regNum], Is.EqualTo(0x4040)));
        //
        //     CompileInstruction(Sh4Assembler.LDCM_SGR(regNum));
        //     ExecuteAndAssert(() => { _state.R[regNum] = memAddr; _memory.Write32(memAddr, 0x5050); }, (s, m) => {
        //         Assert.That(s.SGR, Is.EqualTo(0x5050));
        //         Assert.That(s.R[regNum], Is.EqualTo(memAddr + 4));
        //     });
        //
        //     CompileInstruction(Sh4Assembler.STCM_SGR(regNum));
        //     ExecuteAndAssert(() => { _state.R[regNum] = memAddr + 4; s.SGR = 0x6060; }, (s, m) => {
        //         Assert.That(m.Read32(memAddr), Is.EqualTo(0x6060));
        //         Assert.That(s.R[regNum], Is.EqualTo(memAddr));
        //     });
        // }

        [Test]
        public void TestDbrOps() // Debug Base Register
        {
            byte regNum = 7;
            uint memAddr = 0x7000;

            CompileInstruction(Sh4Assembler.LDCDBR(regNum));
            ExecuteAndAssert(() => _state.R[regNum] = 0x7070, (s, m) => Assert.That(s.DBR, Is.EqualTo(0x7070)));

            CompileInstruction(Sh4Assembler.STCDBR(regNum));
            ExecuteAndAssert(() => _state.DBR = 0x8080, (s, m) => Assert.That(s.R[regNum], Is.EqualTo(0x8080)));

            CompileInstruction(Sh4Assembler.LDCMDBR(regNum));
            ExecuteAndAssert(() =>
            {
                _state.R[regNum] = memAddr;
                _memory.Write32(memAddr, 0x9090);
            }, (s, m) =>
            {
                Assert.That(s.DBR, Is.EqualTo(0x9090));
                Assert.That(s.R[regNum], Is.EqualTo(memAddr + 4));
            });

            CompileInstruction(Sh4Assembler.STCMDBR(regNum));
            ExecuteAndAssert(() =>
            {
                _state.R[regNum] = memAddr + 4;
                _state.DBR = 0xA1A1;
            }, (s, m) =>
            {
                Assert.That(m.Read32(memAddr), Is.EqualTo(0xA1A1));
                Assert.That(s.R[regNum], Is.EqualTo(memAddr));
            });
        }

        [Test]
        public void TestMacOps() // MACH and MACL Registers
        {
            byte regNumH = 8, regNumL = 9;
            uint memAddrH = 0x8000, memAddrL = 0x9000;

            CompileInstruction(Sh4Assembler.LDSMACH(regNumH));
            ExecuteAndAssert(() => _state.R[regNumH] = 0xAAAAAAAA,
                (s, m) => Assert.That(s.MACH, Is.EqualTo(0xAAAAAAAA)));
            CompileInstruction(Sh4Assembler.STSMACH(regNumH));
            ExecuteAndAssert(() => _state.MACH = 0xBBBBBBBB,
                (s, m) => Assert.That(s.R[regNumH], Is.EqualTo(0xBBBBBBBB)));

            CompileInstruction(Sh4Assembler.LDSMACL(regNumL));
            ExecuteAndAssert(() => _state.R[regNumL] = 0xCCCCCCCC,
                (s, m) => Assert.That(s.MACL, Is.EqualTo(0xCCCCCCCC)));
            CompileInstruction(Sh4Assembler.STSMACL(regNumL));
            ExecuteAndAssert(() => _state.MACL = 0xDDDDDDDD,
                (s, m) => Assert.That(s.R[regNumL], Is.EqualTo(0xDDDDDDDD)));

            CompileInstruction(Sh4Assembler.LDSMMACH(regNumH));
            ExecuteAndAssert(() =>
            {
                _state.R[regNumH] = memAddrH;
                _memory.Write32(memAddrH, 0x1234);
            }, (s, m) =>
            {
                Assert.That(s.MACH, Is.EqualTo(0x1234));
                Assert.That(s.R[regNumH], Is.EqualTo(memAddrH + 4));
            });

            CompileInstruction(Sh4Assembler.STSMMACL(regNumL));
            ExecuteAndAssert(() =>
            {
                _state.R[regNumL] = memAddrL + 4;
                _state.MACL = 0x5678;
            }, (s, m) =>
            {
                Assert.That(m.Read32(memAddrL), Is.EqualTo(0x5678));
                Assert.That(s.R[regNumL], Is.EqualTo(memAddrL));
            });
        }

        [Test]
        public void TestPrOps() // Procedure Register
        {
            byte regNum = 10;
            uint memAddr = 0xA000;

            CompileInstruction(Sh4Assembler.LDSPR(regNum));
            ExecuteAndAssert(() => _state.R[regNum] = 0xEEEEEEEE, (s, m) => Assert.That(s.PR, Is.EqualTo(0xEEEEEEEE)));

            CompileInstruction(Sh4Assembler.STSPR(regNum));
            ExecuteAndAssert(() => _state.PR = 0xFFFFFFFF, (s, m) => Assert.That(s.R[regNum], Is.EqualTo(0xFFFFFFFF)));

            CompileInstruction(Sh4Assembler.LDSMPR(regNum));
            ExecuteAndAssert(() =>
            {
                _state.R[regNum] = memAddr;
                _memory.Write32(memAddr, 0xABCDE);
            }, (s, m) =>
            {
                Assert.That(s.PR, Is.EqualTo(0xABCDE));
                Assert.That(s.R[regNum], Is.EqualTo(memAddr + 4));
            });

            CompileInstruction(Sh4Assembler.STSMPR(regNum));
            ExecuteAndAssert(() =>
            {
                _state.R[regNum] = memAddr + 4;
                _state.PR = 0xFEDCB;
            }, (s, m) =>
            {
                Assert.That(m.Read32(memAddr), Is.EqualTo(0xFEDCB));
                Assert.That(s.R[regNum], Is.EqualTo(memAddr));
            });
        }

        [Test]
        public void TestRbankOps() // Banked General Registers
        {
            byte bankRegIndex = 5; // Testa com R5_BANK
            byte generalRegNum = 3;
            uint memAddr = 0xB000;

            // LDC Rn, Rm_BANK
            CompileInstruction(Sh4Assembler.LDCRBANK(generalRegNum, bankRegIndex));
            ExecuteAndAssert(
                () =>
                {
                    _state.R[generalRegNum] = 0xABCDEFFF;
                    Console.WriteLine(_fe.Context.Block);
                },
                (s, m) =>
                {
                    // Verifica se o valor de R3 foi para RBank[5]
                    Assert.That(s.RBank[bankRegIndex], Is.EqualTo(0xABCDEFFF));
                });

            // STC Rm_BANK, Rn
            CompileInstruction(Sh4Assembler.STCRBANK(bankRegIndex, generalRegNum));
            ExecuteAndAssert(
                () => { _state.RBank[bankRegIndex] = 0x12345678; },
                (s, m) =>
                {
                    // Verifica se o valor de RBank[5] foi para R3
                    Assert.That(s.R[generalRegNum], Is.EqualTo(0x12345678));
                });

            // LDC.L @Rn+, Rm_BANK
            CompileInstruction(Sh4Assembler.LDCMRBANK(generalRegNum, bankRegIndex));
            ExecuteAndAssert(
                () =>
                {
                    _state.R[generalRegNum] = memAddr;
                    _memory.Write32(memAddr, 0xCAFEBABE);
                },
                (s, m) =>
                {
                    // Verifica se o valor da memória foi para RBank[5] e se R3 foi incrementado
                    Assert.That(s.RBank[bankRegIndex], Is.EqualTo(0xCAFEBABE));
                    Assert.That(s.R[generalRegNum], Is.EqualTo(memAddr + 4));
                });

            // STC.L Rm_BANK, @-Rn
            CompileInstruction(Sh4Assembler.STCMRBANK(bankRegIndex, generalRegNum));
            ExecuteAndAssert(
                () =>
                {
                    _state.R[generalRegNum] = memAddr + 4;
                    _state.RBank[bankRegIndex] = 0xDEADBEEF;
                },
                (s, m) =>
                {
                    // Verifica se o valor de RBank[5] foi para a memória e se R3 foi decrementado
                    Assert.That(m.Read32(memAddr), Is.EqualTo(0xDEADBEEF));
                    Assert.That(s.R[generalRegNum], Is.EqualTo(memAddr));
                });
        }
    }
}