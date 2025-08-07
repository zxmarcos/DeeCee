using System.Runtime.CompilerServices;
using System.Text;

namespace DeeCee.SH4;

public class BasicBlock
{
    public List<Instruction> Instructions { get; } = new List<Instruction>();
    public int LocalVariableCount { get; set; } = 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(Instruction instruction)
    {
        Instructions.Add(instruction);
    }

    public override string ToString()
    {
        int bpc = 0;
        StringBuilder sb = new StringBuilder();
        foreach (var i in Instructions)
        {
            sb.AppendLine($"{bpc}: {i}");
            bpc++;
        }
        return sb.ToString();
    }

    public void Clear()
    {
        LocalVariableCount = 0;
        Instructions.Clear();
    }
}