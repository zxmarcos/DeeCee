namespace DeeCee.SH4.Test;

public class TestMemoryTest
{
    private TestMemory _memory;
        
    [SetUp]
    public void Setup()
    {
        _memory = new TestMemory(1024);
    }
    
    [Test]
    public void TestMemory()
    {
        _memory.Write32(0x0, 0x11223344);
        Assert.That(_memory.Read32(0x0), Is.EqualTo(0x11223344));
        
        _memory.Write32(0x0, 0x55667788);
        Assert.That(_memory.Read32(0x0), Is.EqualTo(0x55667788));
        
        _memory.Write8(0x0, 0xFF);
        Assert.That(_memory.Read32(0x0), Is.EqualTo(0x556677FF));
        
        _memory.Write16(0x1, 0xAABB);
        Assert.That(_memory.Read32(0x0), Is.EqualTo(0x55AABBFF));;
    }
    
}