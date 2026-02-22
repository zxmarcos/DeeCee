using System.Collections.Generic;
using System.Linq;

namespace DeeCee.SH4.JIT;

public enum LocationType
{
    Register,
    Stack
}

public struct Location
{
    public LocationType Type;
    public X64Registers Register;
    public int StackOffset;
}

public class RegisterAllocator
{
    private readonly LivenessAnalysis _liveness;

    // Use callee-saved registers for allocation.
    private readonly List<X64Registers> _availableRegisters = new()
    {
        X64Registers.RBX,
        X64Registers.R12,
        X64Registers.R13,
        X64Registers.R14
        // R15 reserved for State Pointer
    };

    public Dictionary<int, Location> Mapping { get; } = new();
    public int StackSize { get; private set; }
    public List<X64Registers> UsedRegisters { get; } = new();

    public RegisterAllocator(LivenessAnalysis liveness, int initialStackOffset = 0)
    {
        _liveness = liveness;
        StackSize = initialStackOffset;
    }

    public void Allocate()
    {
        _liveness.Analyze();

        var intervals = _liveness.Ranges.Values.OrderBy(x => x.Start).ToList();
        var active = new List<LivenessAnalysis.Range>();
        // We sort registers to have deterministic allocation
        var freeRegisters = new List<X64Registers>(_availableRegisters);

        foreach (var interval in intervals)
        {
            // Expire old intervals
            // Iterate backwards to remove safely
            for (int i = active.Count - 1; i >= 0; i--)
            {
                if (active[i].End < interval.Start)
                {
                    var id = active[i].Id;
                    if (Mapping.TryGetValue(id, out var loc) && loc.Type == LocationType.Register)
                    {
                        freeRegisters.Add(loc.Register);
                        // Sort again to keep deterministic preference (optional but good for debugging)
                        freeRegisters.Sort();
                    }
                    active.RemoveAt(i);
                }
            }

            // Allocate
            if (freeRegisters.Count > 0)
            {
                var reg = freeRegisters[0];
                freeRegisters.RemoveAt(0);
                Mapping[interval.Id] = new Location { Type = LocationType.Register, Register = reg };
                if (!UsedRegisters.Contains(reg)) UsedRegisters.Add(reg);
                active.Add(interval);
            }
            else
            {
                // Spill to stack
                // Stack grows down, offsets are negative from RBP
                StackSize += 8;
                Mapping[interval.Id] = new Location { Type = LocationType.Stack, StackOffset = -StackSize };
                // Spilled intervals don't occupy registers, so we don't add to active for register tracking purposes.
            }
        }
    }

    public Location? GetLocation(int id)
    {
        if (Mapping.TryGetValue(id, out var loc)) return loc;
        return null;
    }
}
