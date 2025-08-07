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
            Assert.That(_memory.Read32(0x0), Is.EqualTo(a & b));
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
            Assert.That(_memory.Read32(0x0), Is.EqualTo(a | b));
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
            Assert.That(_memory.Read32(0x0), Is.EqualTo(a ^ b));
        }
    }
}