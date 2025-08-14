using NUnit.Framework;
using System;

namespace DeeCee.SH4.Test
{
    /// <summary>
    /// Testes para instruções aritméticas (adição, subtração, negação).
    /// </summary>
    public unsafe class ArithmeticOpsTest
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
                var interpreter = new Interpreter.Interpreter(statePtr);
                setup();
                interpreter.Execute(_fe.Context.Block);
                assertion(_state);
            }
        }

        [Test]
        public void TestAddAndAddI()
        {
            // ADD Rm, Rn
            CompileInstruction(Sh4Assembler.ADD(1, 2));
            ExecuteAndAssert(
                () =>
                {
                    _state.R[1] = 10;
                    _state.R[2] = 20;
                },
                s => Assert.That(s.R[2], Is.EqualTo(30)));

            // ADDI #imm, Rn
            CompileInstruction(Sh4Assembler.ADDI(1, -10)); // Adiciona -10 a R[1]
            ExecuteAndAssert(
                () => _state.R[1] = 50,
                s => Assert.That(s.R[1], Is.EqualTo(40)));
        }

        [Test]
        public void TestAddC() // Add with Carry
        {
            CompileInstruction(Sh4Assembler.ADDC(1, 2));

            // T=0, sem carry out
            ExecuteAndAssert(
                () =>
                {
                    _state.R[1] = 100;
                    _state.R[2] = 200;
                    _state.T = false;
                },
                s =>
                {
                    Assert.That(s.R[2], Is.EqualTo(300));
                    Assert.That(s.T, Is.False);
                });

            // T=1, sem carry out
            ExecuteAndAssert(
                () =>
                {
                    _state.R[1] = 100;
                    _state.R[2] = 200;
                    _state.T = true;
                },
                s =>
                {
                    Assert.That(s.R[2], Is.EqualTo(301));
                    Assert.That(s.T, Is.False);
                });

            // T=0, com carry out
            ExecuteAndAssert(
                () =>
                {
                    _state.R[1] = 0xFFFFFFF0;
                    _state.R[2] = 0x20;
                    _state.T = false;
                },
                s =>
                {
                    Assert.That(s.R[2], Is.EqualTo(0x10));
                    Assert.That(s.T, Is.True);
                });

            // T=1, com carry out
            ExecuteAndAssert(
                () =>
                {
                    _state.R[1] = 0xFFFFFFFF;
                    _state.R[2] = 1;
                    _state.T = true;
                },
                s =>
                {
                    Assert.That(s.R[2], Is.EqualTo(1));
                    Assert.That(s.T, Is.True);
                });
        }

        [Test]
        public void TestAddV() // Add with signed Overflow check
        {
            CompileInstruction(Sh4Assembler.ADDV(1, 2));

            // Sem overflow: pos + pos = pos
            ExecuteAndAssert(
                () =>
                {
                    _state.R[1] = 100;
                    _state.R[2] = 200;
                },
                s =>
                {
                    Assert.That(s.R[2], Is.EqualTo(300));
                    Assert.That(s.T, Is.False);
                });

            // Com overflow: pos + pos = neg
            ExecuteAndAssert(
                () =>
                {
                    _state.R[1] = 0x70000000;
                    _state.R[2] = 0x70000000;
                },
                s =>
                {
                    Assert.That(s.R[2], Is.EqualTo(0xE0000000));
                    Assert.That(s.T, Is.True);
                });

            // Com overflow: neg + neg = pos
            ExecuteAndAssert(
                () =>
                {
                    _state.R[1] = 0x90000000;
                    _state.R[2] = 0x90000000;
                },
                s =>
                {
                    Assert.That(s.R[2], Is.EqualTo(0x20000000));
                    Assert.That(s.T, Is.True);
                });
        }

        [Test]
        public void TestSubAndSubC()
        {
            // SUB
            CompileInstruction(Sh4Assembler.SUB(1, 2));
            ExecuteAndAssert(
                () =>
                {
                    _state.R[1] = 10;
                    _state.R[2] = 30;
                },
                s => Assert.That(s.R[2], Is.EqualTo(20)));

            // SUBC (T=1, sem borrow)
            CompileInstruction(Sh4Assembler.SUBC(1, 2));
            ExecuteAndAssert(
                () =>
                {
                    _state.R[1] = 10;
                    _state.R[2] = 30;
                    _state.T = true;
                },
                s =>
                {
                    Assert.That(s.R[2], Is.EqualTo(19));
                    Assert.That(s.T, Is.False);
                });

            // SUBC (T=0, com borrow)
            ExecuteAndAssert(
                () =>
                {
                    _state.R[1] = 30;
                    _state.R[2] = 10;
                    _state.T = false;
                },
                s =>
                {
                    Assert.That(s.R[2], Is.EqualTo(unchecked((uint)-20)));
                    Assert.That(s.T, Is.True);
                });
        }

        [Test]
        public void TestSubV() // Subtract with signed Overflow check
        {
            CompileInstruction(Sh4Assembler.SUBV(1, 2));

            // Sem overflow: pos - neg = pos
            ExecuteAndAssert(
                () =>
                {
                    _state.R[1] = unchecked((uint)-100);
                    _state.R[2] = 200;
                },
                s =>
                {
                    Assert.That(s.R[2], Is.EqualTo(300));
                    Assert.That(s.T, Is.False);
                });

            // Com overflow: pos - neg = neg
            ExecuteAndAssert(
                () =>
                {
                    _state.R[1] = unchecked((uint)-2);
                    _state.R[2] = 0x7FFFFFFF;
                },
                s =>
                {
                    Assert.That(s.R[2], Is.EqualTo(0x80000001));
                    Assert.That(s.T, Is.True);
                });

            // Com overflow: neg - pos = pos
            ExecuteAndAssert(
                () =>
                {
                    _state.R[1] = 2;
                    _state.R[2] = 0x80000000;
                },
                s =>
                {
                    Assert.That(s.R[2], Is.EqualTo(0x7FFFFFFE));
                    Assert.That(s.T, Is.True);
                });
        }

        [Test]
        public void TestNegAndNegC()
        {
            // NEG
            CompileInstruction(Sh4Assembler.NEG(1, 2));
            ExecuteAndAssert(
                () => _state.R[1] = 50,
                s => Assert.That(s.R[2], Is.EqualTo(unchecked((uint)-50))));

            // NEGC (T=0)
            CompileInstruction(Sh4Assembler.NEGC(1, 2));
            ExecuteAndAssert(
                () =>
                {
                    _state.R[1] = 1;
                    _state.T = false;
                },
                s =>
                {
                    Assert.That(s.R[2], Is.EqualTo(~0U));
                    Assert.That(s.T, Is.True);
                });

            CompileInstruction(Sh4Assembler.NEGC(1, 2));
            ExecuteAndAssert(
                () =>
                {
                    _state.R[1] = 0;
                    _state.T = true;
                },
                s =>
                {
                    Assert.That(s.R[2], Is.EqualTo(~0U));
                    Assert.That(s.T, Is.True);
                });

            // NEGC com R[m]=0 (caso de borda para T)
            ExecuteAndAssert(
                () =>
                {
                    _state.R[1] = 0;
                    _state.T = false;
                },
                s =>
                {
                    Assert.That(s.R[2], Is.EqualTo(0));
                    Assert.That(s.T, Is.True);
                });
        }

        [Test]
        public void TestClrMac()
        {
            // CLRMAC
            CompileInstruction(Sh4Assembler.CLRMAC());
            ExecuteAndAssert(
                () =>
                {
                    _state.MACH = 1;
                    _state.MACL = 2;
                },
                s =>
                {
                    Assert.That(s.MACH, Is.EqualTo(0));
                    Assert.That(s.MACL, Is.EqualTo(0));
                });
        }

        [Test]
        public void TestDt()
        {
            // DT R0
            CompileInstruction(Sh4Assembler.DT(0));
            ExecuteAndAssert(
                () =>
                {
                    _state.R[0] = 1;
                    _state.T = false;
                },
                s =>
                {
                    Assert.That(s.R[0], Is.EqualTo(0));
                    Assert.That(s.T, Is.True);
                });

            ExecuteAndAssert(
                () =>
                {
                    _state.R[0] = 2;
                    _state.T = false;
                },
                s =>
                {
                    Assert.That(s.R[0], Is.EqualTo(1));
                    Assert.That(s.T, Is.False);
                });
        }
        
        [Test]
        public void TestMuluAndMuls()
        {
            CompileInstruction(Sh4Assembler.MULU(1,2));
            ExecuteAndAssert(
                () =>
                {
                    _state.R[1] = 0x0001_0000;
                    _state.R[2] = 0x0001_0000;
                },
                s =>
                {
                    Assert.That(s.MACL, Is.EqualTo(0));
                });
            ExecuteAndAssert(
                () =>
                {
                    _state.R[1] = 0x0001_0002;
                    _state.R[2] = 0x0001_FFFF;
                },
                s =>
                {
                    Assert.That(s.MACL, Is.EqualTo(0xFFFF*2));
                });
        }

    }
}