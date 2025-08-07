namespace DeeCee.SH4.Test;

public unsafe class Tests
{
    private Sh4CpuState _state;
        
    [SetUp]
    public void Setup()
    {
        _state = new Sh4CpuState();
    }
    
    [Test]
    public void TestADD()
    {
        Sh4FrontEnd fe = new();
        fe.Compile(Sh4Assembler.ADD(1, 2));

        fixed (Sh4CpuState* statePtr = &_state)
        {
            Sh4Interpreter interpreter = new(statePtr);

            Console.WriteLine(fe.Context.Block);
            _state.R[1] = 1;
            _state.R[2] = 2;
            _state.SR = 0;
            interpreter.Execute(fe.Context.Block);
            Assert.That(_state.R[2], Is.EqualTo(3));
        }
    }

    [Test]
    public void TestADDC()
    {
        
        Sh4FrontEnd cpu = new();

        fixed (Sh4CpuState* statePtr = &_state)
        {
            Sh4Interpreter interpreter = new(statePtr);
            
            cpu.Compile(Sh4Assembler.ADDC(3, 1));
            
            Console.WriteLine(cpu.Context.Block);
            _state.R[1] = 1;
            _state.R[3] = 0xFFFFFFFF;
            _state.SR = 0;
            interpreter.Execute(cpu.Context.Block);
            Assert.That(_state.R[1], Is.EqualTo(0x0));
            Assert.That(_state.T, Is.EqualTo(true));
            
            cpu.Context.Block.Clear();
            cpu.Compile(Sh4Assembler.ADDC(2, 0));
            Console.WriteLine(cpu.Context.Block);
            _state.R[0] = 0;
            _state.R[2] = 0;
            _state.SR = 1;
            interpreter.Execute(cpu.Context.Block);
            Assert.That(_state.R[0], Is.EqualTo(0x1));
            Assert.That(_state.T, Is.EqualTo(false));
        }
    }
    
    [Test]
    public void TestADDV()
    {
        Sh4FrontEnd cpu = new();

        fixed (Sh4CpuState* statePtr = &_state)
        {
            Sh4Interpreter interpreter = new(statePtr);
            
            cpu.Compile(Sh4Assembler.ADDV(0, 1));
            
            Console.WriteLine(cpu.Context.Block);
            _state.R[0] = 1;
            _state.R[1] = 0x7FFFFFFE;
            _state.SR = 0;
            interpreter.Execute(cpu.Context.Block);
            Assert.That(_state.R[1], Is.EqualTo(0x7FFFFFFF));
            Assert.That(_state.T, Is.EqualTo(false));
            
            cpu.Context.Block.Clear();
            cpu.Compile(Sh4Assembler.ADDV(0, 1));
            Console.WriteLine(cpu.Context.Block);
            _state.R[0] = 2;
            _state.R[1] = 0x7FFFFFFE;
            _state.SR = 0;
            interpreter.Execute(cpu.Context.Block);
            Assert.That(_state.R[1], Is.EqualTo(0x80000000));
            Assert.That(_state.T, Is.EqualTo(true));
        }
    }
    
    [Test]
    public void TestADDI()
    {
        Sh4FrontEnd fe = new();
        fe.Compile(Sh4Assembler.ADDI(1, -4));

        fixed (Sh4CpuState* statePtr = &_state)
        {
            Sh4Interpreter interpreter = new(statePtr);

            Console.WriteLine(fe.Context.Block);
            _state.R[1] = 5;
            _state.SR = 0;
            interpreter.Execute(fe.Context.Block);
            Assert.That(_state.R[1], Is.EqualTo(1));
        }
    }
}