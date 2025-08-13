using System.Runtime.InteropServices;
using DeeCee.SH4;
using DeeCee.SH4.Interpreter;

namespace DeeCee.Core;

public unsafe class Dreamcast : IDisposable
{
    private Sh4CpuState _sh4State;
    private GCHandle _sh4StateHandle;
    private readonly Sh4FrontEnd _sh4FrontEnd;
    private readonly Interpreter _sh4Interpreter;
    private readonly Memory _memory;

    private readonly MemoryBlock _rom;
    private readonly MemoryBlock _flashRom;
    private readonly MemoryBlock _ram;

    private SH4Dasm _sh4Dasm;
    public Dreamcast()
    {
        _rom = new MemoryBlock(1024 * 1024 * 2); // 2MB
        _flashRom = new MemoryBlock(1024 * 256); // 256Kb
        _ram = new MemoryBlock(1024 * 1024 * 16); // 16MB
        
        _memory = new Memory();
        _memory.MapMemory(_rom.Ptr,      0x0000_0000, 0x001F_FFFF, Memory.MapType.Read);
        //_memory.MapMemory(_flashRom.Ptr, 0x0020_0000, 0x0023_FFFF, Memory.MapType.Read);
        //_memory.MapMemory(_ram.Ptr,      0x0C00_0000, 0x0CFF_FFFF, Memory.MapType.ReadWrite);
        
        
        _rom.LoadFrom(@"D:\dev\DeeCee\Data\dc_boot.bin");
        _flashRom.LoadFrom(@"D:\dev\DeeCee\Data\dc_flash.bin");
        
        _sh4State = new Sh4CpuState();
        _sh4StateHandle = GCHandle.Alloc(_sh4State, GCHandleType.Pinned);
        
        _sh4Dasm = new SH4Dasm();
        _sh4FrontEnd = new Sh4FrontEnd();
        _sh4Interpreter = new Interpreter((Sh4CpuState*)_sh4StateHandle.AddrOfPinnedObject());
        _sh4Interpreter.Memory = _memory;

        _sh4State.PC = 0x00000000;
    }


    public void Run(int instructionCount)
    {
        for (int i = 0; i < instructionCount; i++)
        {
            _sh4FrontEnd.Context.Block.Clear();
            ushort opcode = _memory.Read16(_sh4State.PC);
            var lastPc = _sh4State.PC;
            Console.WriteLine($"{_sh4State.PC:X8} {opcode:X4} {_sh4Dasm.Disassemble(opcode).FullInstruction}");
            try
            {
                _sh4FrontEnd.Compile(opcode);
                _sh4Interpreter.Execute(_sh4FrontEnd.Context.Block);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            if (_sh4State.PC == lastPc)
            {
                _sh4State.PC += 2;
            }
        }
    }

    public void Dispose()
    {
        _sh4StateHandle.Free();
        _rom?.Dispose();
        _flashRom?.Dispose();
        _ram?.Dispose();
    }
}