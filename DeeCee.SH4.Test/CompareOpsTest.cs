namespace DeeCee.SH4.Test;

public unsafe class CompareOpsTest
{
    private Sh4CpuState _state;
        
    [SetUp]
    public void Setup()
    {
        _state = new Sh4CpuState();
    }
    
    [Test]
    public void TestCMPSTR()
    {
        Sh4FrontEnd fe = new();
        fe.Compile(Sh4Assembler.CMPSTR(2, 3));

        fixed (Sh4CpuState* statePtr = &_state)
        {
            Sh4Interpreter interpreter = new(statePtr);
            
            Console.WriteLine(fe.Context.Block);
            _state.R[2] = 0xAABBCCDD;
            _state.R[3] = 0xAADDDDBB;
            _state.SR = 0;
            interpreter.Execute(fe.Context.Block);
            Assert.That(_state.T, Is.EqualTo(true));
            
            _state.R[2] = 0xAABBCCDD;
            _state.R[3] = 0xCCBBDDBB;
            _state.SR = 0;
            interpreter.Execute(fe.Context.Block);
            Assert.That(_state.T, Is.EqualTo(true));
            
            _state.R[2] = 0xAABBCCDD;
            _state.R[3] = 0xFFDDCCBB;
            _state.SR = 0;
            interpreter.Execute(fe.Context.Block);
            Assert.That(_state.T, Is.EqualTo(true));
            
            _state.R[2] = 0xAABBCCDD;
            _state.R[3] = 0xAADDDDDD;
            _state.SR = 0;
            interpreter.Execute(fe.Context.Block);
            Assert.That(_state.T, Is.EqualTo(true));
            
            
            _state.R[2] = 0xAABBCCDD;
            _state.R[3] = 0xFFDDEEBB;
            _state.SR = 0;
            interpreter.Execute(fe.Context.Block);
            Assert.That(_state.T, Is.EqualTo(false));
        }
    }
}