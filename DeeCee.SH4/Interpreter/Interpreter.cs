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
        if (regNum < 16)
            return InterpValue.UInt32(_state->R[regNum]);
        if (regNum < (byte)Sh4EmitterContext.RegConstants.RnBank)
        {
            return InterpValue.UInt32(_state->RBank[regNum - 16]);
        }

        return (Sh4EmitterContext.RegConstants)regNum switch
        {
            Sh4EmitterContext.RegConstants.PC => InterpValue.UInt32(_state->PC),
            Sh4EmitterContext.RegConstants.SR => InterpValue.UInt32(_state->SR),
            Sh4EmitterContext.RegConstants.GBR => InterpValue.UInt32(_state->GBR),
            Sh4EmitterContext.RegConstants.PR => InterpValue.UInt32(_state->PR),
            Sh4EmitterContext.RegConstants.SSR => InterpValue.UInt32(_state->SSR),
            Sh4EmitterContext.RegConstants.SPC => InterpValue.UInt32(_state->SPC),
            Sh4EmitterContext.RegConstants.VBR => InterpValue.UInt32(_state->VBR),
            Sh4EmitterContext.RegConstants.SGR => InterpValue.UInt32(_state->SGR),
            Sh4EmitterContext.RegConstants.DBR => InterpValue.UInt32(_state->DBR),
            Sh4EmitterContext.RegConstants.MACH => InterpValue.UInt32(_state->MACH),
            Sh4EmitterContext.RegConstants.MACL => InterpValue.UInt32(_state->MACL),
            _ => throw new ArgumentOutOfRangeException(nameof(regNum), regNum, null)
        };
    }
    
    void SetReg(byte regNum, InterpValue value)
    {
        if (value.Type != InterpValue.ValueType.UInt32 && value.Type != InterpValue.ValueType.Int32)
            throw new ArgumentException("O valor deve ser um UInt32");
        
        UInt32 val = (UInt32)value.Value!;
        if (regNum < 16)
        {
            _state->R[regNum] = val;
            return;
        }
        if (regNum < (byte)Sh4EmitterContext.RegConstants.RnBank)
        {
            _state->RBank[regNum - 16] = val;
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
            return InterpValue.UInt32(operand.UConst32);
        }

        if (operand.Kind == OperandKind.LocalVariable)
        {
            return _vars[operand.VarIndex];
        }

        if (operand.Kind == OperandKind.Label)
        {
            return InterpValue.Int32(operand.BlockOffset);
        }

        return InterpValue.UInt32(0);
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

    private void OpSext8(Instruction instruction)
    {
        var a = GetValue(instruction.A);
        Debug.Assert(a.IsInteger());
        SetValue(instruction.Destiny, InterpValue.UInt32((UInt32)(sbyte)((UInt32)a.Value & 0xFF)));
    }
    
    private void OpSext16(Instruction instruction)
    {
        var a = GetValue(instruction.A);
        Debug.Assert(a.IsInteger());
        SetValue(instruction.Destiny, InterpValue.UInt32((UInt32)(short)((UInt32)a.Value & 0xFFFF)));
    }
    
    private void OpZext8(Instruction instruction)
    {
        var a = GetValue(instruction.A);
        Debug.Assert(a.IsInteger());
        SetValue(instruction.Destiny, InterpValue.UInt32((byte)((UInt32)a.Value & 0xFF)));
    }
    
    private void OpZext16(Instruction instruction)
    {
        var a = GetValue(instruction.A);
        Debug.Assert(a.IsInteger());
        SetValue(instruction.Destiny, InterpValue.UInt32((UInt32)a.Value & 0xFFFF));
    }

    void OpAdd(Instruction instruction)
    {
        var a = GetValue(instruction.A);
        var b = GetValue(instruction.B);
        Debug.Assert(b.IsInteger() && a.IsInteger());
        
        SetValue(instruction.Destiny, InterpValue.UInt32((UInt32)a.Value + (UInt32)b.Value));
    }
    
    void OpSub(Instruction instruction)
    {
        var a = GetValue(instruction.A);
        var b = GetValue(instruction.B);
        Debug.Assert(b.IsInteger() && a.IsInteger());
        
        SetValue(instruction.Destiny, InterpValue.UInt32((UInt32)a.Value - (UInt32)b.Value));
    }
    
    void OpAnd(Instruction instruction)
    {
        var a = GetValue(instruction.A);
        var b = GetValue(instruction.B);
        Debug.Assert(b.IsInteger() && a.IsInteger());
        
        SetValue(instruction.Destiny, InterpValue.UInt32((UInt32)a.Value & (UInt32)b.Value));
    }
    
    void OpOr(Instruction instruction)
    {
        var a = GetValue(instruction.A);
        var b = GetValue(instruction.B);
        Debug.Assert(b.IsInteger() && a.IsInteger());
        
        SetValue(instruction.Destiny, InterpValue.UInt32((UInt32)a.Value | (UInt32)b.Value));
    }
    
    void OpXor(Instruction instruction)
    {
        var a = GetValue(instruction.A);
        var b = GetValue(instruction.B);
        Debug.Assert(b.IsInteger() && a.IsInteger());
        
        SetValue(instruction.Destiny, InterpValue.UInt32((UInt32)a.Value ^ (UInt32)b.Value));
    }
    
    void OpNot(Instruction instruction)
    {
        var a = GetValue(instruction.A);
        Debug.Assert(a.IsInteger());
        
        SetValue(instruction.Destiny, InterpValue.UInt32(~(UInt32)a.Value));
    }
    
    void OpShl(Instruction instruction)
    {
        var a = GetValue(instruction.A);
        var b = GetValue(instruction.B);
        Debug.Assert(b.IsInteger() && a.IsInteger());
        
        SetValue(instruction.Destiny, InterpValue.UInt32((UInt32)a.Value << (byte)b.Value));
    }
    
    void OpShr(Instruction instruction)
    {
        var a = GetValue(instruction.A);
        var b = GetValue(instruction.B);
        Debug.Assert(b.IsInteger() && a.IsInteger());
        
        SetValue(instruction.Destiny, InterpValue.UInt32((UInt32)a.Value >> (byte)b.Value));
    }
    
    void OpSar(Instruction instruction)
    {
        var a = GetValue(instruction.A);
        var b = GetValue(instruction.B);
        Debug.Assert(b.IsInteger() && a.IsInteger());
        
        SetValue(instruction.Destiny, InterpValue.UInt32((UInt32)((Int32)a.Value) >> (byte)b.Value));
    }
    
    void OpRol(Instruction instruction)
    {
        var a = GetValue(instruction.A);
        var b = GetValue(instruction.B);
        Debug.Assert(b.IsInteger() && a.IsInteger());
        
        SetValue(instruction.Destiny, InterpValue.UInt32(((UInt32)a.Value << (byte)b.Value) | ((UInt32)a.Value >> (32 - (byte)b.Value))));
    }
    
    void OpRor(Instruction instruction)
    {
        var a = GetValue(instruction.A);
        var b = GetValue(instruction.B);
        Debug.Assert(b.IsInteger() && a.IsInteger());
        
        SetValue(instruction.Destiny, InterpValue.UInt32(((UInt32)a.Value >> (byte)b.Value) | ((UInt32)a.Value << (32 - (byte)b.Value))));
    }
    
    void OpCmpEq(Instruction instruction)
    {
        var a = GetValue(instruction.A);
        var b = GetValue(instruction.B);
        Debug.Assert(b.IsInteger() && a.IsInteger());
        
        SetValue(instruction.Destiny, InterpValue.UInt32((UInt32)a.Value == (UInt32)b.Value ? 1U : 0));;
    }
    
    void OpCmpNe(Instruction instruction)
    {
        var a = GetValue(instruction.A);
        var b = GetValue(instruction.B);
        Debug.Assert(b.IsInteger() && a.IsInteger());
        SetValue(instruction.Destiny, InterpValue.UInt32((UInt32)a.Value != (UInt32)b.Value ? 1U : 0));;
    }
    
    void OpCmpGe(Instruction instruction)
    {
        var a = GetValue(instruction.A);
        var b = GetValue(instruction.B);
        Debug.Assert(b.IsInteger() && a.IsInteger());
        
        SetValue(instruction.Destiny, InterpValue.UInt32((UInt32)a.Value >= (UInt32)b.Value ? 1U : 0));;
    }
    
    void OpCmpLt(Instruction instruction)
    {
        var a = GetValue(instruction.A);
        var b = GetValue(instruction.B);
        Debug.Assert(b.IsInteger() && a.IsInteger());
        
        SetValue(instruction.Destiny, InterpValue.UInt32((UInt32)a.Value < (UInt32)b.Value ? 1U : 0));;
    }
    
    void OpCmpGt(Instruction instruction)
    {
        var a = GetValue(instruction.A);
        var b = GetValue(instruction.B);
        Debug.Assert(b.IsInteger() && a.IsInteger());
        
        SetValue(instruction.Destiny, InterpValue.UInt32((UInt32)a.Value > (UInt32)b.Value ? 1U : 0));;
    }
    
    void OpCmpGtSign(Instruction instruction)
    {
        var a = GetValue(instruction.A);
        var b = GetValue(instruction.B);
        Debug.Assert(b.IsInteger() && a.IsInteger());
        
        var av = (Int32)(uint)a.Value;
        var bv = (Int32)(uint)b.Value;
        
        SetValue(instruction.Destiny, InterpValue.UInt32(av > bv ? 1U : 0));;
    }
    
    void OpCmpGeSign(Instruction instruction)
    {
        var a = GetValue(instruction.A);
        var b = GetValue(instruction.B);
        Debug.Assert(b.IsInteger() && a.IsInteger());
        
        var av = (Int32)(uint)a.Value;
        var bv = (Int32)(uint)b.Value;
        SetValue(instruction.Destiny, InterpValue.UInt32(av >= bv ? 1U : 0));;
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

        if ((UInt32)a.Value == 0)
        {
            return (false, 0);
        }

        return (true, (int)GetValue(instruction.Destiny).Value);
    }
    
    (bool,int) OpBranchFalse(Instruction instruction)
    {
        var a = GetValue(instruction.A);
        Debug.Assert(a.IsInteger());

        if ((UInt32)a.Value != 0)
        {
            return (false, 0);
        }

        return (true, (int)GetValue(instruction.Destiny).Value);
    }
    
    (bool,int) OpBranch(Instruction instruction)
    {
        return (true, (int)GetValue(instruction.Destiny).Value);
    }
    
    void OpLoad(Instruction instruction)
    {
        Debug.Assert(instruction.Destiny != null, "instruction.Destiny != null");
        var address = (UInt32)GetValue(instruction.A.Address).Value;
        // Console.WriteLine($"LoadMemory {instruction.Destiny.Address} {address}");
        switch (instruction.A.MemoryWidth)
        {
            case MemoryWidth.Byte:
                SetValue(instruction.Destiny, InterpValue.UInt32(Memory.Read8(address)));
                break;
            case MemoryWidth.Word:
                SetValue(instruction.Destiny, InterpValue.UInt32(Memory.Read16(address)));
                break;
            case MemoryWidth.Dword:
                SetValue(instruction.Destiny, InterpValue.UInt32(Memory.Read32(address)));
                break;
            case MemoryWidth.Qword:
                // TODO: ???
                SetValue(instruction.Destiny, InterpValue.UInt64(Memory.Read64(address)));
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
                Console.WriteLine($"StoreMemory {instruction.Destiny.Address}");
                var address = (UInt32)GetValue(instruction.Destiny.Address).Value;
                switch (instruction.Destiny.MemoryWidth)
                {
                    case MemoryWidth.Byte:
                        Memory.Write8(address, (byte)GetValue(instruction.A).Value);
                        break;
                    case MemoryWidth.Word:
                        Memory.Write16(address, (UInt16)GetValue(instruction.A).Value);
                        break;
                    case MemoryWidth.Dword:
                        Memory.Write32(address, (UInt32)GetValue(instruction.A).Value);
                        break;
                    case MemoryWidth.Qword:
                        Memory.Write64(address, (UInt64)GetValue(instruction.A).Value);
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