using System.Diagnostics;
using System.Runtime.InteropServices;
using DeeCee.SH4;
using DeeCee.SH4.Interpreter;
using DeeCee.SH4.JIT;

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
        
        // Control Registers
        Mem.MapRead32Handler(1, 0xFC00_0000, 0xFFFF_FFFF); 
        Mem.SetRead32Handler(1, (addr) =>
        {
            if (addr == 0xFF80_0028)
            {
                Console.WriteLine("Read RFCR");
                return (uint)Random.Shared.Next(0, 0x20);
            }

            return 0u;
        });
        Mem.MapRead16Handler(1, 0xFC00_0000, 0xFFFF_FFFF); 
        Mem.SetRead16Handler(1, (addr) =>
        {
            if (addr == 0xFF80_0028)
            {
                Console.WriteLine("Read RFCR");
                return (ushort)Random.Shared.Next(0, 0x20);
            }

            return 0;
        });
   
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

        Sh4State->Reset();
        
        _sh4Translator.AddBreakpoint(0x0000B860);

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

    private string DasmAt(uint pc)
    {
        return Sh4Dasm.DisassembleWithAddresses([_sh4Interpreter.Memory.Read16(pc)], pc)[0].FullInstruction;
    }
    
    public void Debug()
    {
        Console.WriteLine("Modo debug: comandos - s (step), r (registradores), q (sair)");

        bool showIr = false;
        String lastCmd = "";
        while (true)
        {
            Utils.SaveConsole();
            Utils.SetConsoleColor(ConsoleColor.Red);
            Console.Write($"{Sh4State->PC:X8} ");;
            Console.Write("(debug) [{0}] >", DasmAt(Sh4State->PC).PadRight(16));
            Utils.RestoreConsole();
            
            if (lastCmd != "")
            {
                Console.Write($"[{lastCmd}] ");
            }
            var cmd = Console.ReadLine();
            if (cmd == null) continue;
            cmd = cmd.Trim().ToLowerInvariant();

            if (cmd == "")
            {
                cmd = lastCmd;
            }
            lastCmd = cmd;
            
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
            else if (cmd == "b")
            {
                Sh4State->PC = Sh4State->PC - 2;
                Console.WriteLine($"PC set to {Sh4State->PC:X8}");
            }
            else if (cmd == "break")
            {
                Debugger.Break();
            }
            else if (cmd.StartsWith("d"))
            {
      
                var n = cmd.Substring(1);
                var res = uint.TryParse(n, out var ni);
                if (!res)
                    ni = 1;
                
                for (int i = 0; i < ni; i++)
                {
                    var pc = (uint)(Sh4State->PC + i * 2);
                    var opcode = new Sh4Opcode(_sh4Interpreter.Memory.Read16(pc));
                    Console.WriteLine($"{pc:X8} {opcode.Value:X4} {_sh4Translator.Dasm.DisassembleWithAddresses([opcode.Value], pc)[0].FullInstruction}");

                }
            }
            else if (cmd == "s")
            {
                var block = _sh4Translator.GetBlock(Sh4State->PC, true);
                try
                {
                    if (showIr)
                    {
                        Console.WriteLine($"IR_BEGIN:\n{block}IR_END\n");

                        var livenessAnalyzer = new LivenessAnalysis(block);
                        livenessAnalyzer.Analyze();
                        Console.WriteLine(livenessAnalyzer);
                    }
                    _sh4Interpreter.Execute(block);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            else if (cmd == "x")
            {
                Utils.SaveConsole();
                Utils.SetConsoleColor(ConsoleColor.Blue, ConsoleColor.White);
                var block = _sh4Translator.GetBlock(Sh4State->PC, false);
                Utils.RestoreConsole();
                
                try
                {
                    if (showIr)
                    {
                        Console.WriteLine($"IR_BEGIN:\n{block}IR_END\n");
                        if (false)
                        {
                            
                            var livenessAnalyzer = new LivenessAnalysis(block);
                            livenessAnalyzer.Analyze();
                            Console.WriteLine(livenessAnalyzer);
                        }
                    }
                    _sh4Interpreter.Execute(block);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            else if (cmd == "resume")
            {
                Console.WriteLine("Executando continuamente. Digite qualquer comando e pressione Enter para interromper.");
                var cts = new System.Threading.CancellationTokenSource();
                var token = cts.Token;

                var execTask = System.Threading.Tasks.Task.Run(() =>
                {
                    uint lastPc = 0;
                    while (!token.IsCancellationRequested)
                    {
                        try
                        {
                            var block = _sh4Translator.GetBlock(Sh4State->PC, false);
                            // Evite logar IR em modo contínuo para não poluir a saída
                            _sh4Interpreter.Execute(block);
                            if (Sh4State->PC != lastPc)
                            {
                                Console.WriteLine("* PC: $" + Sh4State->PC.ToString("X8") + "");
                            }
                            lastPc = Sh4State->PC;
                        }
                        catch (NotImplementedException e)
                        {
                            Utils.SaveConsole();
                            Utils.SetConsoleColor(ConsoleColor.Red);
                            Console.WriteLine(e);
                            Utils.RestoreConsole();
                            break;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    }
                }, token);

                // Espera por um novo comando do usuário; ao recebê-lo, cancela a execução contínua.
                var nextCmd = Console.ReadLine();
                cts.Cancel();

                try { execTask.Wait(); } catch { /* ignorar exceções de cancelamento */ }

                // Se o usuário digitou algo, armazena como último comando
                if (!string.IsNullOrWhiteSpace(nextCmd))
                {
                    lastCmd = nextCmd.Trim().ToLowerInvariant();
                    Console.WriteLine($"Execução interrompida. Próximo comando recebido: '{lastCmd}'.");
                }
                else
                {
                    Console.WriteLine("Execução interrompida.");
                }
            }

            else if (cmd.StartsWith("h"))
            {
                // Formatos aceitos:
                // h <endereco>
                // Ex.: h 0x2000
                var parts = cmd.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 2)
                {
                    Console.WriteLine("Uso: h <endereco>. Ex.: h 0x2000");
                    continue;
                }

                uint addr;
                var s = parts[1].Trim();
                if (s.StartsWith("0x", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (!uint.TryParse(s.AsSpan(2), System.Globalization.NumberStyles.HexNumber, null, out addr))
                    {
                        Console.WriteLine("Endereço inválido.");
                        continue;
                    }
                }
                else if (!uint.TryParse(s, out addr))
                {
                    Console.WriteLine("Endereço inválido.");
                    continue;
                }

                try
                {
                    var dump = HexDump(addr, 8, 16);
                    Console.WriteLine(dump);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Erro ao ler memória: {e.Message}");
                }
            }

            else if (cmd.StartsWith("r8 ") || cmd.StartsWith("r16 ") || cmd.StartsWith("r32 "))
            {
                var parts = cmd.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 2 || !TryParseUint(parts[1], out var addr))
                {
                    Console.WriteLine("Uso: r8|r16|r32 <endereco>. Ex.: r16 0x2000");
                    continue;
                }

                try
                {
                    if (cmd.StartsWith("r8 "))
                    {
                        var v = Mem.Read8(addr);
                        Console.WriteLine($"[R8 ] [{addr:X8}] -> 0x{v:X2}");
                    }
                    else if (cmd.StartsWith("r16 "))
                    {
                        var v = Mem.Read16(addr);
                        Console.WriteLine($"[R16] [{addr:X8}] -> 0x{v:X4}");
                    }
                    else
                    {
                        var v = Mem.Read32(addr);
                        Console.WriteLine($"[R32] [{addr:X8}] -> 0x{v:X8}");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Erro ao ler memória: {e.Message}");
                }
            }
            else if (cmd.StartsWith("w8 ") || cmd.StartsWith("w16 ") || cmd.StartsWith("w32 "))
            {
                var parts = cmd.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 2)
                {
                    Console.WriteLine("Uso: w8|w16|w32 <endereco>,<valor>. Ex.: w16 0x2000,0x1000");
                    continue;
                }

                var av = parts[1].Split(',', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (av.Length != 2 || !TryParseUint(av[0], out var addr) || !TryParseUint(av[1], out var value))
                {
                    Console.WriteLine("Parâmetros inválidos. Ex.: w32 0x2000,0xDEADBEEF");
                    continue;
                }

                try
                {
                    if (cmd.StartsWith("w8 "))
                    {
                        Mem.Write8(addr, (byte)(value & 0xFF));
                        var r = Mem.Read8(addr);
                        Console.WriteLine($"[W8 ] [{addr:X8}] <= 0x{value & 0xFF:X2} (lido: 0x{r:X2})");
                    }
                    else if (cmd.StartsWith("w16 "))
                    {
                        Mem.Write16(addr, (ushort)(value & 0xFFFF));
                        var r = Mem.Read16(addr);
                        Console.WriteLine($"[W16] [{addr:X8}] <= 0x{value & 0xFFFF:X4} (lido: 0x{r:X4})");
                    }
                    else
                    {
                        Mem.Write32(addr, value);
                        var r = Mem.Read32(addr);
                        Console.WriteLine($"[W32] [{addr:X8}] <= 0x{value:X8} (lido: 0x{r:X8})");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Erro ao escrever memória: {e.Message}");
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
    
    private string HexDump(uint startAddr, int lines, int bytesPerLine)
    {
        var sb = new System.Text.StringBuilder(lines * (10 + bytesPerLine * 3 + 2 + bytesPerLine));
        uint addr = startAddr;

        for (int line = 0; line < lines; line++)
        {
            // Endereço no início da linha
            sb.Append(addr.ToString("X8"));
            sb.Append("  ");

            // Bytes em hex
            for (int i = 0; i < bytesPerLine; i++)
            {
                byte b;
                try
                {
                    b = Mem.Read8(addr + (uint)i);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    b = 0;
                }

                sb.Append(b.ToString("X2"));
                if (i != bytesPerLine - 1) sb.Append(' ');
                if (i == (bytesPerLine / 2) - 1) sb.Append("  "); // espaço extra no meio
            }

            sb.Append("  ");

            // ASCII
            for (int i = 0; i < bytesPerLine; i++)
            {
                byte b;
                try
                {
                    b = Mem.Read8(addr + (uint)i);
                }
                catch
                {
                    b = 0;
                }

                char c = (b >= 32 && b <= 126) ? (char)b : '.';
                sb.Append(c);
            }

            sb.AppendLine();
            addr += (uint)bytesPerLine;
        }

        return sb.ToString();
    }


    private static bool TryParseUint(string s, out uint value)
    {
        s = s.Trim();
        if (s.StartsWith("0x", StringComparison.InvariantCultureIgnoreCase))
        {
            return uint.TryParse(s.AsSpan(2), System.Globalization.NumberStyles.HexNumber, null, out value);
        }
        return uint.TryParse(s, out value);
    }
}