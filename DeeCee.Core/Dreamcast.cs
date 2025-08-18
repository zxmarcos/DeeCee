using System.Runtime.InteropServices;
using DeeCee.SH4;
using DeeCee.SH4.Interpreter;

namespace DeeCee.Core;

public unsafe class Dreamcast : IDisposable
{
    public Sh4CpuState* Sh4State { get; }
    private GCHandle _sh4StateHandle;
    private readonly Sh4Translator _sh4Translator;
    private readonly Interpreter _sh4Interpreter;
    public Memory Mem { get; }

    private MemoryBlock Rom { get; }
    private MemoryBlock FlashRom { get; }
    private MemoryBlock Ram { get; }

    public SH4Dasm Sh4Dasm { get; }

    public Dreamcast()
    {
        Rom = new MemoryBlock(1024 * 1024 * 2); // 2MB
        FlashRom = new MemoryBlock(1024 * 256); // 256Kb
        Ram = new MemoryBlock(1024 * 1024 * 16); // 16MB
        
        Mem = new Memory();
        for (uint i = 0; i < 8; i++)
        {
            uint baseAddr = 0x2000_0000u * i;
            Mem.MapMemory(Rom.Ptr,      baseAddr + 0x0000_0000, baseAddr + 0x001F_FFFF, Memory.MapType.ReadWrite);
            Mem.MapMemory(FlashRom.Ptr, baseAddr + 0x0020_0000, baseAddr + 0x0023_FFFF, Memory.MapType.ReadWrite);
            Mem.MapMemory(Ram.Ptr,      baseAddr + 0x0C00_0000, baseAddr + 0x0CFF_FFFF, Memory.MapType.ReadWrite);
        }
   
        Rom.LoadFrom(@"D:\dev\DeeCee\Data\dc_boot.bin");
        FlashRom.LoadFrom(@"D:\dev\DeeCee\Data\dc_flash.bin");
        

        _sh4StateHandle = GCHandle.Alloc(new Sh4CpuState(), GCHandleType.Pinned);
        Sh4State = (Sh4CpuState*)_sh4StateHandle.AddrOfPinnedObject();
        
        Sh4Dasm = new SH4Dasm();
        _sh4Translator = new Sh4Translator(Mem);
        _sh4Interpreter = new Interpreter(Sh4State)
        {
            Memory = Mem
        };

        Sh4State->PC = 0xA0000000;
    }


    public void Run(int instructionCount)
    {
        for (int i = 0; i < instructionCount; i++)
        {
            Console.WriteLine($"------------------> Next PC {Sh4State->PC:X8}\n");
            var block = _sh4Translator.GetBlock(Sh4State->PC, false);
            
            try
            {
                // Console.WriteLine($"IR_BEGIN: \n{block}\nIR_END\n");
                _sh4Interpreter.Execute(block);
                // Console.WriteLine(*_sh4State);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }

    public void Step()
    {
        var block = _sh4Translator.GetBlock(Sh4State->PC, true);
        try
        {
            _sh4Interpreter.Execute(block);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
    public void Debug()
    {
        Console.WriteLine("Modo debug: comandos - s (step), r (registradores), q (sair)");

        bool showIr = false;
        while (true)
        {
            Console.Write("(debug) > ");
            var cmd = Console.ReadLine();
            if (cmd == null) continue;
            cmd = cmd.Trim().ToLowerInvariant();

            if (cmd == "q")
            {
                break;
            }
            else if (cmd == "i")
            {
                showIr = !showIr;
                if (showIr)
                {
                    Console.WriteLine("IR Emit enabled");
                }
                else
                {
                    Console.WriteLine("IR Emit disabled");   
                }
            }
            else if (cmd == "r")
            {
                try
                {
                    Console.WriteLine(*Sh4State);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            else if (cmd == "s")
            {
                Console.WriteLine($"------------------> Next PC {Sh4State->PC:X8}\n");
                var block = _sh4Translator.GetBlock(Sh4State->PC, true);
                try
                {
                    if (showIr)
                    {
                        Console.WriteLine($"IR_BEGIN:\n{block}IR_END\n");
                    }
                    _sh4Interpreter.Execute(block);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            else
            {
                Console.WriteLine("Comando inválido. Use: s (step), r (registradores), q (sair).");
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