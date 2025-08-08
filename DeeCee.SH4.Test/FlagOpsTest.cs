using NUnit.Framework;
using System;

namespace DeeCee.SH4.Test;

/// <summary>
/// Testes para instruções que manipulam diretamente as flags S e T.
/// </summary>
public unsafe class FlagOpsTest
{
    private Sh4CpuState _state;
    private Sh4FrontEnd _fe;

    [SetUp]
    public void Setup()
    {
        _state = new Sh4CpuState();
        _fe = new Sh4FrontEnd();
    }

    /// <summary>
    /// Limpa o bloco de contexto, compila uma única instrução e a prepara para execução.
    /// </summary>
    private void CompileInstruction(ushort instruction)
    {
        _fe.Context.Block.Clear();
        _fe.Compile(instruction);
    }

    /// <summary>
    /// Define um estado inicial para as flags, executa o bloco compilado e
    /// permite que uma ação de asserção verifique o resultado.
    /// </summary>
    private void ExecuteAndAssert(Action setupState, Action<Sh4CpuState> assertAction)
    {
        fixed (Sh4CpuState* statePtr = &_state)
        {
            var interpreter = new Sh4Interpreter(statePtr);
                
            // Define o estado inicial antes da execução
            setupState();
                
            interpreter.Execute(_fe.Context.Block);
                
            // Verifica o estado final
            assertAction(_state);
        }
    }

    [Test]
    public void TestSETT()
    {
        CompileInstruction(Sh4Assembler.SETT());

        // Testa a definição de T quando T=0
        ExecuteAndAssert(
            () => _state.T = false, 
            state => Assert.That(state.T, Is.True, "T deveria ser definido como verdadeiro")
        );

        // Testa a definição de T quando T=1 (deve permanecer 1)
        ExecuteAndAssert(
            () => _state.T = true, 
            state => Assert.That(state.T, Is.True, "T deveria permanecer verdadeiro")
        );
    }

    [Test]
    public void TestCLRT()
    {
        CompileInstruction(Sh4Assembler.CLRT());

        // Testa a limpeza de T quando T=1
        ExecuteAndAssert(
            () => _state.T = true,
            state => Assert.That(state.T, Is.False, "T deveria ser definido como falso")
        );

        // Testa a limpeza de T quando T=0 (deve permanecer 0)
        ExecuteAndAssert(
            () => _state.T = false,
            state => Assert.That(state.T, Is.False, "T deveria permanecer falso")
        );
    }

    [Test]
    public void TestSETS()
    {
        CompileInstruction(Sh4Assembler.SETS());
            
        // Testa a definição de S quando S=0
        ExecuteAndAssert(
            () => _state.S = false,
            state => Assert.That(state.S, Is.True, "S deveria ser definido como verdadeiro")
        );

        // Testa a definição de S quando S=1 (deve permanecer 1)
        ExecuteAndAssert(
            () => _state.S = true,
            state => Assert.That(state.S, Is.True, "S deveria permanecer verdadeiro")
        );
    }

    [Test]
    public void TestCLRS()
    {
        CompileInstruction(Sh4Assembler.CLRS());

        // Testa a limpeza de S quando S=1
        ExecuteAndAssert(
            () => _state.S = true,
            state => Assert.That(state.S, Is.False, "S deveria ser definido como falso")
        );

        // Testa a limpeza de S quando S=0 (deve permanecer 0)
        ExecuteAndAssert(
            () => _state.S = false,
            state => Assert.That(state.S, Is.False, "S deveria permanecer falso")
        );
    }
}