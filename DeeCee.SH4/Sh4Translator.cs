using System.Diagnostics;
using DeeCee.SH4.Translate;

namespace DeeCee.SH4;

public class Sh4Translator
{
    private IMemory _memory;
    private const uint MaxInstructionPerBlock = 1000;
    private SH4Dasm _dasm = new();

    public Sh4Translator(IMemory memory)
    {
        _memory = memory;
    }
    
    public Dictionary<uint, BasicBlock> Blocks { get; } = new();
    

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

            if (instr == null)
            {
                Console.WriteLine($"NULL Instr {opcode.Value}");
                break;
            }
            
            Console.WriteLine($"{pc:X8} {opcode.Value:X4} {_dasm.Disassemble(opcode.Value).FullInstruction}");
            
            
            if (instr.IsBranch())
            {
                if (instr.IsDelayed())
                {
                    var delaySlot = new Sh4Opcode(_memory.Read16(pc + 2));
                    var delayInstr = Sh4OpcodeTable.GetInstruction(delaySlot.Value);
                    Debug.Assert(!delayInstr.IsBranch());

                    delayInstr.Emit(ctx);
                }
                
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