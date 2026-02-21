using System.Diagnostics;
using System.Runtime.CompilerServices;
using DeeCee.SH4.Translate;

namespace DeeCee.SH4.Interpreter;

public unsafe class Interpreter
{
    private readonly Sh4CpuState* _state;
    private InterpValue[] _vars;
    public IMemory Memory { get; set; }
    
    public Interpreter(Sh4CpuState* state)
    {
        this._state = state;
    }

    InterpValue GetReg(byte regNum)
    {
        return regNum switch
        {
            < 16 => InterpValue.FromUInt64(_state->R[regNum]),
            < (byte)Sh4EmitterContext.RegConstants.RnBank => InterpValue.FromUInt64(_state->RBank[regNum - 16]),
            <= (byte)Sh4EmitterContext.RegConstants.FR15_Bank1 and >= (byte)Sh4EmitterContext.RegConstants.FR0_Bank0 =>
                InterpValue.FromFloat(_state->FR.F[(int)(regNum - Sh4EmitterContext.RegConstants.FR0_Bank0)]),
            _ => (Sh4EmitterContext.RegConstants)regNum switch
            {
                Sh4EmitterContext.RegConstants.PC => InterpValue.FromUInt64(_state->PC),
                Sh4EmitterContext.RegConstants.SR => InterpValue.FromUInt64(_state->SR),
                Sh4EmitterContext.RegConstants.GBR => InterpValue.FromUInt64(_state->GBR),
                Sh4EmitterContext.RegConstants.PR => InterpValue.FromUInt64(_state->PR),
                Sh4EmitterContext.RegConstants.SSR => InterpValue.FromUInt64(_state->SSR),
                Sh4EmitterContext.RegConstants.SPC => InterpValue.FromUInt64(_state->SPC),
                Sh4EmitterContext.RegConstants.VBR => InterpValue.FromUInt64(_state->VBR),
                Sh4EmitterContext.RegConstants.SGR => InterpValue.FromUInt64(_state->SGR),
                Sh4EmitterContext.RegConstants.DBR => InterpValue.FromUInt64(_state->DBR),
                Sh4EmitterContext.RegConstants.MACH => InterpValue.FromUInt64(_state->MACH),
                Sh4EmitterContext.RegConstants.MACL => InterpValue.FromUInt64(_state->MACL),
                Sh4EmitterContext.RegConstants.FPSCR => InterpValue.FromUInt64(_state->FPSCR),
                _ => throw new ArgumentOutOfRangeException(nameof(regNum), regNum, null)
            }
        };
    }
    
    void SetReg(byte regNum, InterpValue value)
    {
        if (!value.IsInteger())
            throw new ArgumentException("O valor deve ser um inteiro");
        
        UInt32 val = value.AsUInt32();
        if (regNum < 16)
        {
            _state->R[regNum] = val;
            return;
        }
        if (regNum < (byte)Sh4EmitterContext.RegConstants.RnBank)
        {
            _state->RBank[regNum - 16] = val;
            return;
        }

        if (regNum is <= (byte)Sh4EmitterContext.RegConstants.FR15_Bank1 and >= (byte)Sh4EmitterContext.RegConstants.FR0_Bank0)
        {
            _state->FR.F[(int)(regNum - Sh4EmitterContext.RegConstants.FR0_Bank0)] = val;
            return;
        }
        
        switch((Sh4EmitterContext.RegConstants)regNum)
        {
            case Sh4EmitterContext.RegConstants.PC: _state->PC = val;
                return;
            case Sh4EmitterContext.RegConstants.SR: _state->SR = val;
                return;
            case Sh4EmitterContext.RegConstants.GBR: _state->GBR = val;
                return;
            case Sh4EmitterContext.RegConstants.PR: _state->PR = val;
                return;
            case Sh4EmitterContext.RegConstants.SSR: _state->SSR = val;
                return;
            case Sh4EmitterContext.RegConstants.SPC: _state->SPC = val;
                return;
            case Sh4EmitterContext.RegConstants.VBR: _state->VBR = val;
                return;
            case Sh4EmitterContext.RegConstants.SGR: _state->SGR = val;
                return;
            case Sh4EmitterContext.RegConstants.DBR: _state->DBR = val;
                return;
            case Sh4EmitterContext.RegConstants.MACH: _state->MACH = val;
                return;
            case Sh4EmitterContext.RegConstants.MACL: _state->MACL = val;
                return;
            case Sh4EmitterContext.RegConstants.FPSCR: _state->FPSCR = val;
                return;
            default:
                throw new ArgumentOutOfRangeException(nameof(regNum), regNum, null);
        };
    }
    
    InterpValue GetValue(Operand operand)
    {
        if (operand.Kind == OperandKind.Register)
        {
            return GetReg(operand.RegNum);
        }

        if (operand.Kind == OperandKind.Constant)
        {
            return InterpValue.FromUInt64(operand.UConst32);
        }

        if (operand.Kind == OperandKind.LocalVariable)
        {
            return _vars[operand.VarIndex];
        }

        if (operand.Kind == OperandKind.Label)
        {
            return InterpValue.FromInt64(operand.BlockOffset);
        }

        return InterpValue.FromUInt64(0);
    }

    void SetValue(Operand operand, InterpValue value)
    {
        if (operand.Kind == OperandKind.Register)
        {
            SetReg(operand.RegNum, value);
        }
        else if (operand.Kind == OperandKind.LocalVariable)
        {
            _vars[operand.VarIndex] = value;
        }
    }

    public void Execute(BasicBlock block)
    {
        _vars = new InterpValue[block.LocalVariableCount];
        
        int ipc = 0;
        while (ipc < block.Instructions.Count)
        {
            var instruction = block.Instructions[ipc];
            ++ipc;
            switch (instruction.Opcode)
            {
                case Opcode.DEBUG:
                    break;
                case Opcode.COPY:
                    OpCopy(instruction);
                    break;
                case Opcode.ADD:
                    OpAdd(instruction);
                    break;
                case Opcode.SUB:
                    OpSub(instruction);
                    break;
                case Opcode.MUL:
                    OpMul(instruction);
                    break;
                case Opcode.MULS:
                    OpMuls(instruction);
                    break;
                case Opcode.STORE:
                    OpStore(instruction);
                    break;
                case Opcode.LOAD:
                    OpLoad(instruction);
                    break;
                case Opcode.AND:
                    OpAnd(instruction);
                    break;
                case Opcode.OR:
                    OpOr(instruction);
                    break;
                case Opcode.XOR:
                    OpXor(instruction);
                    break;
                case Opcode.NOT:
                    OpNot(instruction);
                    break;
                case Opcode.SHL:
                    OpShl(instruction);
                    break;
                case Opcode.SHR:
                    OpShr(instruction);
                    break;
                case Opcode.SAR:
                    OpSar(instruction);
                    break;
                case Opcode.ROL:
                    OpRol(instruction);
                    break;
                case Opcode.ROR:
                    OpRor(instruction);
                    break;
                case Opcode.SIGN_EXT8:
                    OpSext8(instruction);
                    break;
                case Opcode.SIGN_EXT16:
                    OpSext16(instruction);
                    break;
                case Opcode.ZERO_EXT8:
                    OpZext8(instruction);
                    break;
                case Opcode.ZERO_EXT16:
                    OpZext16(instruction);
                    break;
                case Opcode.CMP_EQ:
                    OpCmpEq(instruction);
                    break;
                case Opcode.CMP_NE:
                    OpCmpNe(instruction);
                    break;
                case Opcode.CMP_LT:
                    OpCmpLt(instruction);
                    break;
                case Opcode.CMP_GT:
                    OpCmpGt(instruction);
                    break;
                case Opcode.CMP_GT_SIGN:
                    OpCmpGtSign(instruction);
                    break;
                case Opcode.CMP_GE:
                    OpCmpGe(instruction);
                    break;
                case Opcode.CMP_GE_SIGN:
                    OpCmpGeSign(instruction);
                    break;
                case Opcode.BRANCH:
                {
                    var (branch, offset) = OpBranch(instruction);
                    if (branch)
                    {
                        ipc = offset;
                    }

                    break;
                }
                case Opcode.BRANCH_TRUE:
                {
                    var (branch, offset) = OpBranchTrue(instruction);
                    if (branch)
                    {
                        ipc = offset;
                    }

                    break;
                }
                case Opcode.BRANCH_FALSE:
                {
                    var (branch, offset) = OpBranchFalse(instruction);
                    if (branch)
                    {
                        ipc = offset;
                    }

                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    private void OpMul(Instruction instruction)
    {
        var a = GetValue(instruction.A);
        var b = GetValue(instruction.B);
        Debug.Assert(b.IsInteger() && a.IsInteger());
        
        SetValue(instruction.Destiny, InterpValue.FromUInt64(a.AsUInt32() * b.AsUInt32()));
    }
    
    private void OpMuls(Instruction instruction)
    {
        var a = GetValue(instruction.A);
        var b = GetValue(instruction.B);
        Debug.Assert(b.IsInteger() && a.IsInteger());
        
        SetValue(instruction.Destiny, InterpValue.FromUInt64((UInt32)(a.AsInt32() * b.AsInt32())));
    }

    private void OpSext8(Instruction instruction)
    {
        var a = GetValue(instruction.A);
        Debug.Assert(a.IsInteger());
        SetValue(instruction.Destiny, InterpValue.FromUInt64((UInt64)(sbyte)(a.AsUInt32() & 0xFF)));
    }
    
    private void OpSext16(Instruction instruction)
    {
        var a = GetValue(instruction.A);
        Debug.Assert(a.IsInteger());
        SetValue(instruction.Destiny, InterpValue.FromUInt64((UInt64)(short)(a.AsUInt32() & 0xFFFF)));
    }
    
    private void OpZext8(Instruction instruction)
    {
        var a = GetValue(instruction.A);
        Debug.Assert(a.IsInteger());
        SetValue(instruction.Destiny, InterpValue.FromUInt64((byte)(a.AsUInt32() & 0xFF)));
    }
    
    private void OpZext16(Instruction instruction)
    {
        var a = GetValue(instruction.A);
        Debug.Assert(a.IsInteger());
        SetValue(instruction.Destiny, InterpValue.FromUInt64(a.AsUInt32() & 0xFFFF));
    }

    void OpAdd(Instruction instruction)
    {
        var a = GetValue(instruction.A);
        var b = GetValue(instruction.B);
        Debug.Assert(b.IsInteger() && a.IsInteger());
        
        SetValue(instruction.Destiny, InterpValue.FromUInt64(a.AsUInt32() + b.AsUInt32()));
    }
    
    void OpSub(Instruction instruction)
    {
        var a = GetValue(instruction.A);
        var b = GetValue(instruction.B);
        Debug.Assert(b.IsInteger() && a.IsInteger());
        
        SetValue(instruction.Destiny, InterpValue.FromUInt64(a.AsUInt32() - b.AsUInt32()));
    }
    
    void OpAnd(Instruction instruction)
    {
        var a = GetValue(instruction.A);
        var b = GetValue(instruction.B);
        Debug.Assert(b.IsInteger() && a.IsInteger());
        
        SetValue(instruction.Destiny, InterpValue.FromUInt64(a.AsUInt32() & b.AsUInt32()));
    }
    
    void OpOr(Instruction instruction)
    {
        var a = GetValue(instruction.A);
        var b = GetValue(instruction.B);
        Debug.Assert(b.IsInteger() && a.IsInteger());
        
        SetValue(instruction.Destiny, InterpValue.FromUInt64(a.AsUInt32() | b.AsUInt32()));
    }
    
    void OpXor(Instruction instruction)
    {
        var a = GetValue(instruction.A);
        var b = GetValue(instruction.B);
        Debug.Assert(b.IsInteger() && a.IsInteger());
        
        SetValue(instruction.Destiny, InterpValue.FromUInt64(a.AsUInt32() ^ b.AsUInt32()));
    }
    
    void OpNot(Instruction instruction)
    {
        var a = GetValue(instruction.A);
        Debug.Assert(a.IsInteger());
        
        SetValue(instruction.Destiny, InterpValue.FromUInt64(~a.AsUInt32()));
    }
    
    void OpShl(Instruction instruction)
    {
        var a = GetValue(instruction.A);
        var b = GetValue(instruction.B);
        Debug.Assert(b.IsInteger() && a.IsInteger());
        
        SetValue(instruction.Destiny, InterpValue.FromUInt64(a.AsUInt32() << (byte)b.AsUInt32()));
    }
    
    void OpShr(Instruction instruction)
    {
        var a = GetValue(instruction.A);
        var b = GetValue(instruction.B);
        Debug.Assert(b.IsInteger() && a.IsInteger());
        
        SetValue(instruction.Destiny, InterpValue.FromUInt64(a.AsUInt32() >> (byte)b.AsUInt32()));
    }
    
    void OpSar(Instruction instruction)
    {
        var a = GetValue(instruction.A);
        var b = GetValue(instruction.B);
        Debug.Assert(b.IsInteger() && a.IsInteger());
        
        SetValue(instruction.Destiny, InterpValue.FromUInt64((UInt32)(a.AsInt32() >> (byte)b.AsUInt32())));
    }
    
    void OpRol(Instruction instruction)
    {
        var a = GetValue(instruction.A);
        var b = GetValue(instruction.B);
        Debug.Assert(b.IsInteger() && a.IsInteger());
        
        SetValue(instruction.Destiny, InterpValue.FromUInt64((a.AsUInt32() << (byte)b.AsUInt32()) | (a.AsUInt32() >> (32 - (byte)b.AsUInt32()))));
    }
    
    void OpRor(Instruction instruction)
    {
        var a = GetValue(instruction.A);
        var b = GetValue(instruction.B);
        Debug.Assert(b.IsInteger() && a.IsInteger());
        
        SetValue(instruction.Destiny, InterpValue.FromUInt64((a.AsUInt32() >> (byte)b.AsUInt32()) | (a.AsUInt32() << (32 - (byte)b.AsUInt32()))));
    }
    
    void OpCmpEq(Instruction instruction)
    {
        var a = GetValue(instruction.A);
        var b = GetValue(instruction.B);
        Debug.Assert(b.IsInteger() && a.IsInteger());
        
        SetValue(instruction.Destiny, InterpValue.FromUInt64(a.AsUInt32() == b.AsUInt32() ? 1U : 0));;
    }
    
    void OpCmpNe(Instruction instruction)
    {
        var a = GetValue(instruction.A);
        var b = GetValue(instruction.B);
        Debug.Assert(b.IsInteger() && a.IsInteger());
        SetValue(instruction.Destiny, InterpValue.FromUInt64(a.AsUInt32() != b.AsUInt32() ? 1U : 0));;
    }
    
    void OpCmpGe(Instruction instruction)
    {
        var a = GetValue(instruction.A);
        var b = GetValue(instruction.B);
        Debug.Assert(b.IsInteger() && a.IsInteger());
        
        SetValue(instruction.Destiny, InterpValue.FromUInt64(a.AsUInt32() >= b.AsUInt32() ? 1U : 0));;
    }
    
    void OpCmpLt(Instruction instruction)
    {
        var a = GetValue(instruction.A);
        var b = GetValue(instruction.B);
        Debug.Assert(b.IsInteger() && a.IsInteger());
        
        SetValue(instruction.Destiny, InterpValue.FromUInt64(a.AsUInt32() < b.AsUInt32() ? 1U : 0));;
    }
    
    void OpCmpGt(Instruction instruction)
    {
        var a = GetValue(instruction.A);
        var b = GetValue(instruction.B);
        Debug.Assert(b.IsInteger() && a.IsInteger());
        
        SetValue(instruction.Destiny, InterpValue.FromUInt64(a.AsUInt32() > b.AsUInt32() ? 1U : 0));;
    }
    
    void OpCmpGtSign(Instruction instruction)
    {
        var a = GetValue(instruction.A);
        var b = GetValue(instruction.B);
        Debug.Assert(b.IsInteger() && a.IsInteger());
        
        SetValue(instruction.Destiny, InterpValue.FromUInt64(a.AsInt32() > b.AsInt32() ? 1U : 0));;
    }
    
    void OpCmpGeSign(Instruction instruction)
    {
        var a = GetValue(instruction.A);
        var b = GetValue(instruction.B);
        Debug.Assert(b.IsInteger() && a.IsInteger());

        SetValue(instruction.Destiny, InterpValue.FromUInt64(a.AsInt32() >= b.AsInt32() ? 1U : 0));;
    }

    void OpCopy(Instruction instruction)
    {
        var a = GetValue(instruction.A);
        SetValue(instruction.Destiny, a);
    }
    
    (bool,int) OpBranchTrue(Instruction instruction)
    {
        var a = GetValue(instruction.A);
        Debug.Assert(a.IsInteger());

        if (a.AsUInt32() == 0)
        {
            return (false, 0);
        }

        return (true, GetValue(instruction.Destiny!).AsInt32());
    }
    
    (bool,int) OpBranchFalse(Instruction instruction)
    {
        var a = GetValue(instruction.A!);
        Debug.Assert(a.IsInteger());

        if (a.AsUInt32() != 0)
        {
            return (false, 0);
        }

        return (true, GetValue(instruction.Destiny!).AsInt32());
    }
    
    (bool,int) OpBranch(Instruction instruction)
    {
        return (true, GetValue(instruction.Destiny!).AsInt32());
    }
    
    void OpLoad(Instruction instruction)
    {
        Debug.Assert(instruction.Destiny != null, "instruction.Destiny != null");
        var address = GetValue(instruction.A.Address).AsUInt32();
        switch (instruction.A.MemoryWidth)
        {
            case MemoryWidth.Byte:
                SetValue(instruction.Destiny, InterpValue.FromUInt64(Memory.Read8(address)));
                break;
            case MemoryWidth.Word:
                SetValue(instruction.Destiny, InterpValue.FromUInt64(Memory.Read16(address)));
                break;
            case MemoryWidth.Dword:
                SetValue(instruction.Destiny, InterpValue.FromUInt64(Memory.Read32(address)));
                break;
            case MemoryWidth.Qword:
                // TODO: ???
                SetValue(instruction.Destiny, InterpValue.FromUInt64(Memory.Read64(address)));
                break;
            case null:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    void OpStore(Instruction instruction)
    {
        Debug.Assert(instruction.Destiny != null, "instruction.Destiny != null");
        Debug.Assert(instruction.A != null, "instruction.A != null");

        switch (instruction.Destiny.Kind)
        {
            case OperandKind.Memory:
                var address = GetValue(instruction.Destiny.Address).AsUInt32();
                switch (instruction.Destiny.MemoryWidth)
                {
                    case MemoryWidth.Byte:
                        Memory.Write8(address, (byte)GetValue(instruction.A).AsUInt32());
                        break;
                    case MemoryWidth.Word:
                        Memory.Write16(address, (UInt16)GetValue(instruction.A).AsUInt32());
                        break;
                    case MemoryWidth.Dword:
                        Memory.Write32(address, GetValue(instruction.A).AsUInt32());
                        break;
                    case MemoryWidth.Qword:
                        Memory.Write64(address, GetValue(instruction.A).AsUInt64());
                        break;
                    case null:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                break;
            case OperandKind.LocalVariable:
                _vars[instruction.Destiny.VarIndex] = GetValue(instruction.A);
                break;
            default:
            {
                var value = GetValue(instruction.A);
                SetReg(instruction.Destiny.RegNum, value);
                break;
            }
        }
    }
}