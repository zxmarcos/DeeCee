using System.Text;

namespace DeeCee.SH4.JIT;

public class LivenessAnalysis
{
    private const int BaseLocalId = 100;
    private readonly BasicBlock _block;

    private readonly Dictionary<int, HashSet<int>> _liveOut = new();
    private readonly Dictionary<int, HashSet<int>> _liveIn = new();

    public record Range
    {
        public int Id;
        public int Start;
        public int End;
    }

    private Dictionary<int, Range> Ranges { get; } = new();

    public LivenessAnalysis(BasicBlock block)
    {
        _block = block;
    }

    public int GetId(Operand? operand)
    {
        if (operand == null)
            return -1;
        if (operand.Kind == OperandKind.Register)
        {
            return operand.RegNum;
        }

        if (operand.Kind == OperandKind.LocalVariable)
        {
            return BaseLocalId + operand.VarIndex;
        }

        return -1;
    }

    private void AddIfValid(int id, HashSet<int> set)
    {
        if (id >= 0) set.Add(id);
    }

    private string GetName(int x)
    {
        if (x < BaseLocalId)
        {
            return $"R{x}";
        }

        return $"L{x - BaseLocalId}";
    }

    public void Analyze()
    {
        for (int i = _block.Instructions.Count - 1; i >= 0; i--)
        {
            var instruction = _block.Instructions[i];
            var @in = new HashSet<int>();
            var @out = new HashSet<int>();

            bool hasNextInstruction = (i + 1) < _block.Instructions.Count;

            if (hasNextInstruction)
            {
                foreach (var x in _liveIn[i + 1])
                {
                    @out.Add(x);
                }
            }

            AddIfValid(GetId(instruction.A), @in);
            AddIfValid(GetId(instruction.B), @in);

            var dId = GetId(instruction.Destiny);
            if (dId != -1)
            {
                @out.Remove(GetId(instruction.Destiny));
            }

            foreach (var x in @out)
            {
                @in.Add(x);
            }

            _liveIn[i] = @in;
            _liveOut[i] = @out;
        }

        ComputeRanges();
    }


    private void ComputeRanges()
    {
        for (int i = 0; i < _block.Instructions.Count; i++)
        {
            var @in = _liveIn[i];
            foreach (var x in @in)
            {
                if (!Ranges.TryGetValue(x, out var range))
                {
                    Ranges[x] = new Range
                    {
                        Id = x,
                        Start = i,
                        End = i
                    };
                }
                else
                {
                    range.End = i;
                }
            }
        }
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Liveness Analysis:");
        for (int i = 0; i < _block.Instructions.Count; i++)
        {
            List<string> liveInNames = new List<string>();
            List<string> liveOutNames = new List<string>();

            sb.Append($"{i}: [");
            foreach (var x in _liveIn[i])
            {
                liveInNames.Add(GetName(x));
            }

            sb.Append(string.Join(", ", liveInNames));
            sb.Append("] ");
            sb.Append("[");
            foreach (var x in _liveOut[i])
            {
                liveOutNames.Add(GetName(x));
            }

            sb.Append(string.Join(", ", liveOutNames));
            sb.Append("] ");
            sb.AppendLine();
        }

        sb.AppendLine("\nRanges:");
        
        var intervals = Ranges.Values.OrderBy(x => x.Id).ThenBy(x => x.Start).ToList();
        foreach (var x in intervals)
        {
            sb.AppendLine($"{GetName(x.Id)}: [{x.Start}, {x.End}]");
        }

        return sb.ToString();
    }
}