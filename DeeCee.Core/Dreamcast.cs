using System.Runtime.InteropServices;
using DeeCee.SH4;
using DeeCee.SH4.Interpreter;

namespace DeeCee.Core;

public unsafe class Dreamcast : IDisposable
{
    private Sh4CpuState* _sh4State;
    private GCHandle _sh4StateHandle;
    private readonly Sh4Translator _sh4Translator;
    private readonly Interpreter _sh4Interpreter;
    private readonly Memory _memory;

    private MemoryBlock Rom { get; }
    private MemoryBlock FlashRom { get; }
    private MemoryBlock Ram { get; }

    private SH4Dasm _sh4Dasm;
    public Dreamcast()
    {
        Rom = new MemoryBlock(1024 * 1024 * 2); // 2MB
        FlashRom = new MemoryBlock(1024 * 256); // 256Kb
        Ram = new MemoryBlock(1024 * 1024 * 16); // 16MB
        
        _memory = new Memory();
        for (uint i = 0; i < 8; i++)
        {
            uint baseAddr = 0x2000_0000u * i;
            _memory.MapMemory(Rom.Ptr,      baseAddr + 0x0000_0000, baseAddr + 0x001F_FFFF, Memory.MapType.ReadWrite);
            _memory.MapMemory(FlashRom.Ptr, baseAddr + 0x0020_0000, baseAddr + 0x0023_FFFF, Memory.MapType.ReadWrite);
            _memory.MapMemory(Ram.Ptr,      baseAddr + 0x0C00_0000, baseAddr + 0x0CFF_FFFF, Memory.MapType.ReadWrite);
        }
   
        Rom.LoadFrom(@"D:\dev\DeeCee\Data\dc_boot.bin");
        FlashRom.LoadFrom(@"D:\dev\DeeCee\Data\dc_flash.bin");
        

        _sh4StateHandle = GCHandle.Alloc(new Sh4CpuState(), GCHandleType.Pinned);
        _sh4State = (Sh4CpuState*)_sh4StateHandle.AddrOfPinnedObject();
        
        _sh4Dasm = new SH4Dasm();
        _sh4Translator = new Sh4Translator(_memory);
        _sh4Interpreter = new Interpreter(_sh4State)
        {
            Memory = _memory
        };

        _sh4State->PC = 0xA0000000;
    }


    public void Run(int instructionCount)
    {
        for (int i = 0; i < instructionCount; i++)
        {
            Console.WriteLine($"------------------> Next PC {_sh4State->PC:X8}\n");
            var block = _sh4Translator.GetBlock(_sh4State->PC, false);
            
            try
            {
                _sh4Interpreter.Execute(block);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }

    public void Dispose()
    {
        _sh4StateHandle.Free();
        Rom?.Dispose();
        FlashRom?.Dispose();
        Ram?.Dispose();
    }
}