using System.Diagnostics;
using DeeCee.SH4.Translate;

namespace DeeCee.SH4;

public class Sh4Translator
{
    private IMemory _memory;
    private const uint MaxInstructionPerBlock = 1000;
    public SH4Dasm Dasm { get; } = new();

    public List<(uint Start, uint End)> Breakpoints { get; } = new();

    public Sh4Translator(IMemory memory)
    {
        _memory = memory;
    }
    
    public Dictionary<uint, BasicBlock> Blocks { get; } = new();
    
    
    public void AddBreakpoint(uint start, uint end)
    {
        Breakpoints.Add((start, end));
    }
    
    public void RemoveBreakpoint(uint start, uint end)
    {
        Breakpoints.RemoveAll(r => r.Start == start && r.End == end);
    }
    
    public void ClearBreakpoints()
    {
        Breakpoints.Clear();
    }

    public void AddBreakpoint(uint pc)
    {
        AddBreakpoint(pc, pc);
    }
    
    private bool IsAtBreakpoint(uint pc) => Breakpoints.Any(r => pc >= r.Start && pc <= r.End);


    public BasicBlock GetBlock(uint pc, bool singleStep = false)
    {
        var blockPc = pc;
        if (Blocks.TryGetValue(pc, out var block))
        {
            return block;
        }

        Sh4EmitterContext ctx = new();
        int count = 0;
        do
        {
            var opcode = new Sh4Opcode(_memory.Read16(pc));
            // Console.WriteLine($"OpCode: {opcode.Value:X4} at {pc:X8}");
            var instr = Sh4OpcodeTable.GetInstruction(opcode.Value);
            ctx.Op = opcode;
            if (IsAtBreakpoint(pc) && !singleStep)
            {
                Console.WriteLine($"BREAKPOINT {pc:X8}");
                break;
            }

            if (instr == null)
            {
                Console.WriteLine($"NULL Instr {opcode.Value}");
                break;
            }
            
            Console.WriteLine($"{pc:X8} {opcode.Value:X4} {Dasm.DisassembleWithAddresses([opcode.Value], pc)[0].FullInstruction}");
            
            
            if (instr.IsBranch())
            {
                if (instr.IsDelayed())
                {
                    Console.WriteLine($"DELAYED {pc:X8}");
                    var delaySlot = new Sh4Opcode(_memory.Read16(pc + 2));
                    var delayInstr = Sh4OpcodeTable.GetInstruction(delaySlot.Value);
                    Debug.Assert(!delayInstr.IsBranch());
                    
                    pc += 2;
                    Console.WriteLine($"{pc:X8} {opcode.Value:X4} {Dasm.Disassemble(delaySlot.Value).FullInstruction} *DELAY_SLOT");
                    ctx.Op = delaySlot;
                    delayInstr.Emit(ctx);
                    ctx.NextInstruction();
                    ctx.Op = opcode;
                }
                else
                {
                    Console.WriteLine($"NO_DELAYED {pc:X8}");
                }
                instr.Emit(ctx);
                // O próximo PC é computado diretamente na função.
                pc += 2;
                break;
            }
            
            instr.Emit(ctx);
            ctx.NextInstruction();
            pc += 2;

            count++;
        } while (!singleStep && count < MaxInstructionPerBlock);

        if (!singleStep)
        {
            Blocks.Add(blockPc, ctx.Block);
        }
        return ctx.Block;
    }
}