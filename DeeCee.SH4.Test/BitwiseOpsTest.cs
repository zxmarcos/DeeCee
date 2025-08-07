namespace DeeCee.SH4.Test;

public unsafe class BitwiseOpsTest
{
    private Sh4CpuState _state;
    private TestMemory _memory;
        
    [SetUp]
    public void Setup()
    {
        _state = new Sh4CpuState();
        _memory = new TestMemory(1024);
    }
    
    [Test]
    public void TestAND()
    {
        Sh4FrontEnd fe = new();
        fe.Compile(Sh4Assembler.AND(1, 2));
        var a = 0xAACCDDFCU;
        var b = 0x12345678U;
        
        fixed (Sh4CpuState* statePtr = &_state)
        {
            Sh4Interpreter interpreter = new(statePtr);

            Console.WriteLine(fe.Context.Block);
            _state.R[1] = a;
            _state.R[2] = b;
            _state.SR = 0;
            interpreter.Execute(fe.Context.Block);
            Assert.That(_state.R[2], Is.EqualTo(a & b));
        }
    }
    
    [Test]
    public void TestANDI()
    {
        Sh4FrontEnd fe = new();
        fe.Compile(Sh4Assembler.ANDI(-2));

        fixed (Sh4CpuState* statePtr = &_state)
        {
            Sh4Interpreter interpreter = new(statePtr);

            Console.WriteLine(fe.Context.Block);
            _state.R[0] = 129;
            _state.SR = 0;
            interpreter.Execute(fe.Context.Block);
            Assert.That(_state.R[0], Is.EqualTo(129 & -2));
        }
    }
    
    [Test]
    public void TestANDB()
    {
        Sh4FrontEnd fe = new();
        byte a = 0xFC;
        var b = 0xFFFFFF1A;
        fe.Compile(Sh4Assembler.ANDB(a));

        fixed (Sh4CpuState* statePtr = &_state)
        {
            Sh4Interpreter interpreter = new(statePtr);
            interpreter.Memory = _memory;
            
            _memory.Write32(0x0, b);

            Console.WriteLine(fe.Context.Block);
            _state.R[0] = 0;
            _state.GBR = 0x0;
            interpreter.Execute(fe.Context.Block);
            Assert.That(_memory.Read8(0x0), Is.EqualTo(a & b));
        }
    }
    
    
    [Test]
    public void TestOR()
    {
        Sh4FrontEnd fe = new();
        fe.Compile(Sh4Assembler.OR(1, 2));
        var a = 0x55C7DDFCU;
        var b = 0x12745678U;
        
        fixed (Sh4CpuState* statePtr = &_state)
        {
            Sh4Interpreter interpreter = new(statePtr);

            Console.WriteLine(fe.Context.Block);
            _state.R[1] = a;
            _state.R[2] = b;
            _state.SR = 0;
            interpreter.Execute(fe.Context.Block);
            Assert.That(_state.R[2], Is.EqualTo(a | b));
        }
    }
    
    [Test]
    public void TestORI()
    {
        Sh4FrontEnd fe = new();
        fe.Compile(Sh4Assembler.ORI(55));

        fixed (Sh4CpuState* statePtr = &_state)
        {
            Sh4Interpreter interpreter = new(statePtr);

            Console.WriteLine(fe.Context.Block);
            _state.R[0] = 126;
            _state.SR = 0;
            interpreter.Execute(fe.Context.Block);
            Assert.That(_state.R[0], Is.EqualTo(126 | 55));
        }
    }
    
    [Test]
    public void TestORB()
    {
        Sh4FrontEnd fe = new();
        byte a = 0xFC;
        var b = 0xFF00FF1A;
        fe.Compile(Sh4Assembler.ORB(a));

        fixed (Sh4CpuState* statePtr = &_state)
        {
            Sh4Interpreter interpreter = new(statePtr);
            interpreter.Memory = _memory;
            
            _memory.Write32(0x0, b);

            Console.WriteLine(fe.Context.Block);
            _state.R[0] = 0;
            _state.GBR = 0x0;
            interpreter.Execute(fe.Context.Block);
            Assert.That(_memory.Read8(0x0), Is.EqualTo((a | b) & 0xFF));
        }
    }
    
    [Test]
    public void TestXOR()
    {
        Sh4FrontEnd fe = new();
        fe.Compile(Sh4Assembler.XOR(1, 2));
        var a = 0x55C7DDFCU;
        var b = 0x12745678U;
        
        fixed (Sh4CpuState* statePtr = &_state)
        {
            Sh4Interpreter interpreter = new(statePtr);

            Console.WriteLine(fe.Context.Block);
            _state.R[1] = a;
            _state.R[2] = b;
            _state.SR = 0;
            interpreter.Execute(fe.Context.Block);
            Assert.That(_state.R[2], Is.EqualTo(a ^ b));
        }
    }
    
    [Test]
    public void TestXORI()
    {
        Sh4FrontEnd fe = new();
        fe.Compile(Sh4Assembler.XORI(33));

        fixed (Sh4CpuState* statePtr = &_state)
        {
            Sh4Interpreter interpreter = new(statePtr);

            Console.WriteLine(fe.Context.Block);
            _state.R[0] = 219;
            _state.SR = 0;
            interpreter.Execute(fe.Context.Block);
            Assert.That(_state.R[0], Is.EqualTo(219 ^ 33));
        }
    }
    
    [Test]
    public void TestXORB()
    {
        Sh4FrontEnd fe = new();
        byte a = 0x1C;
        var b = 0xFF00F64E;
        fe.Compile(Sh4Assembler.XORB(a));

        fixed (Sh4CpuState* statePtr = &_state)
        {
            Sh4Interpreter interpreter = new(statePtr);
            interpreter.Memory = _memory;
            
            _memory.Write32(0x0, b);

            Console.WriteLine(fe.Context.Block);
            _state.R[0] = 0;
            _state.GBR = 0x0;
            interpreter.Execute(fe.Context.Block);
            Assert.That(_memory.Read8(0x0), Is.EqualTo((a ^ b) & 0xFF));
        }
    }
    
    [Test]
    public void TestNOT()
    {
        Sh4FrontEnd fe = new();
        fe.Compile(Sh4Assembler.NOT(1, 0));

        fixed (Sh4CpuState* statePtr = &_state)
        {
            Sh4Interpreter interpreter = new(statePtr);

            Console.WriteLine(fe.Context.Block);
            _state.R[0] = 0;
            _state.R[1] = 12;
            _state.SR = 0;
            interpreter.Execute(fe.Context.Block);
            Assert.That(_state.R[0], Is.EqualTo(~12U));
        }
    }
    
    [Test]
    public void TestTST()
    {
        Sh4FrontEnd fe = new();
        
        
        fixed (Sh4CpuState* statePtr = &_state)
        {
            Sh4Interpreter interpreter = new(statePtr);
            
            fe.Compile(Sh4Assembler.TST(0, 0));
            Console.WriteLine(fe.Context.Block);
            _state.R[0] = 0;
            _state.SR = 0;
            interpreter.Execute(fe.Context.Block);
            Assert.That(_state.T, Is.EqualTo(true));
            
            fe.Context.Block.Clear();
            fe.Compile(Sh4Assembler.TST(0, 0));
            Console.WriteLine(fe.Context.Block);
            _state.R[0] = 1;
            _state.SR = 0;
            interpreter.Execute(fe.Context.Block);
            Assert.That(_state.T, Is.EqualTo(false));
        }
    }
    
    [Test]
    public void TestTSTI()
    {
        Sh4FrontEnd fe = new();
        fe.Compile(Sh4Assembler.TSTI((sbyte)-128u));

        fixed (Sh4CpuState* statePtr = &_state)
        {
            Sh4Interpreter interpreter = new(statePtr);

            Console.WriteLine(fe.Context.Block);
            _state.R[0] = 0xFFFFFF7F;
            _state.SR = 0;
            interpreter.Execute(fe.Context.Block);
            Assert.That(_state.T, Is.EqualTo(true));
        }
    }
    
    [Test]
    public void TestTSTB()
    {
        Sh4FrontEnd fe = new();
        fe.Compile(Sh4Assembler.TSTB(0xA5));

        fixed (Sh4CpuState* statePtr = &_state)
        {
            Sh4Interpreter interpreter = new(statePtr);
            interpreter.Memory = _memory;
            
            _memory.Write8(0x0, 0xA5);

            Console.WriteLine(fe.Context.Block);
            _state.GBR = 0x0;
            interpreter.Execute(fe.Context.Block);
            Assert.That(_state.T, Is.EqualTo(false));
        }
    }
    
    
}