using System.Diagnostics;
using DeeCee.SH4.Translate;

namespace DeeCee.SH4;

public unsafe class Sh4Interpreter
{
    private readonly Sh4CpuState *_state;
    private UInt32[] _vars;
    public IMemory Memory { get; set; }
    
    public Sh4Interpreter(Sh4CpuState *state)
    {
        this._state = state;
    }
    
    /// <summary>
    /// Obtém o valor de um registrador específico.
    /// </summary>
    UInt32 GetReg(byte regNum)
    {
        // A expressão switch é uma forma moderna e concisa para o switch-case.
        return regNum switch
        {
            (byte)Sh4EmitterContext.RegConstants.PC   => _state->PC,
            (byte)Sh4EmitterContext.RegConstants.PR   => _state->PR,
            (byte)Sh4EmitterContext.RegConstants.GBR  => _state->GBR,
            (byte)Sh4EmitterContext.RegConstants.SR   => _state->SR,
            (byte)Sh4EmitterContext.RegConstants.SSR  => _state->SSR,
            (byte)Sh4EmitterContext.RegConstants.SPC  => _state->SPC,
            (byte)Sh4EmitterContext.RegConstants.VBR  => _state->VBR,
            (byte)Sh4EmitterContext.RegConstants.SGR  => _state->SGR,
            (byte)Sh4EmitterContext.RegConstants.DBR  => _state->DBR,
            (byte)Sh4EmitterContext.RegConstants.MACH => _state->MACH,
            (byte)Sh4EmitterContext.RegConstants.MACL => _state->MACL,
            // Caso padrão: para registradores de propósito geral (R0-R15)
            _ => _state->R[regNum]
        };
    }

    /// <summary>
    /// Define o valor de um registrador específico.
    /// </summary>
    void SetReg(byte regNum, UInt32 value)
    {
        switch (regNum)
        {
            case (byte)Sh4EmitterContext.RegConstants.PC:   _state->PC = value;   break;
            case (byte)Sh4EmitterContext.RegConstants.PR:   _state->PR = value;   break;
            case (byte)Sh4EmitterContext.RegConstants.GBR:  _state->GBR = value;  break;
            case (byte)Sh4EmitterContext.RegConstants.SR:   _state->SR = value;   break;
            case (byte)Sh4EmitterContext.RegConstants.SSR:  _state->SSR = value;  break;
            case (byte)Sh4EmitterContext.RegConstants.SPC:  _state->SPC = value;  break;
            case (byte)Sh4EmitterContext.RegConstants.VBR:  _state->VBR = value;  break;
            case (byte)Sh4EmitterContext.RegConstants.SGR:  _state->SGR = value;  break;
            case (byte)Sh4EmitterContext.RegConstants.DBR:  _state->DBR = value;  break;
            case (byte)Sh4EmitterContext.RegConstants.MACH: _state->MACH = value; break;
            case (byte)Sh4EmitterContext.RegConstants.MACL: _state->MACL = value; break;
            // Caso padrão: para registradores de propósito geral (R0-R15)
            default: _state->R[regNum] = value; break;
        };
    }

    UInt32 GetValue(Operand operand)
    {
        if (operand.Kind == OperandKind.Register)
        {
            return GetReg(operand.RegNum);
        }
        else if (operand.Kind == OperandKind.Constant)
        {
            return operand.UConst32;
        }
        else if (operand.Kind == OperandKind.LocalVariable)
        {
            return _vars[operand.VarIndex];
        }
        else if (operand.Kind == OperandKind.Label)
        {
            return (UInt32) operand.BlockOffset;
        }

        return 0;
    }

    void SetValue(Operand operand, UInt32 value)
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
        _vars = new UInt32[block.LocalVariableCount];
        
        int ipc = 0;
        while (ipc < block.Instructions.Count)
        {
            var instruction = block.Instructions[ipc];
            ++ipc;
            switch (instruction.Opcode)
            {
                case Opcode.ADD:
                {
                    var a = GetValue(instruction.A);
                    var b = GetValue(instruction.B);
                    SetValue(instruction.Destiny, a + b);
                    break;
                }
                case Opcode.SUB:
                {
                    var a = GetValue(instruction.A);
                    var b = GetValue(instruction.B);
                    SetValue(instruction.Destiny, a - b);
                    break;
                }
                case Opcode.STORE:
                {
                    Debug.Assert(instruction.Destiny != null, "instruction.Destiny != null");
                    Debug.Assert(instruction.A != null, "instruction.A != null");

                    switch (instruction.Destiny.Kind)
                    {
                        case OperandKind.Memory:
                            Console.WriteLine($"StoreMemory {instruction.Destiny.Address}");
                            var address = GetValue(instruction.Destiny.Address);
                            switch (instruction.Destiny.MemoryWidth)
                            {
                                case MemoryWidth.Byte:
                                    Memory.Write8(address, (byte) GetValue(instruction.A));
                                    break;
                                case MemoryWidth.Word:
                                    Memory.Write16(address, (UInt16) GetValue(instruction.A));
                                    break;
                                case MemoryWidth.Dword:
                                    Memory.Write32(address, GetValue(instruction.A));
                                    break;
                                case MemoryWidth.Qword:
                                    Memory.Write64(address, GetValue(instruction.A));
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
                    break;
                }
                case Opcode.LOAD:
                {
                    Debug.Assert(instruction.Destiny != null, "instruction.Destiny != null");
                    var address = GetValue(instruction.A.Address);
                    Console.WriteLine($"LoadMemory {instruction.Destiny.Address} {address}");
                    switch (instruction.A.MemoryWidth)
                    {
                        case MemoryWidth.Byte:
                            SetValue(instruction.Destiny, Memory.Read8(address));
                            break;
                        case MemoryWidth.Word:
                            SetValue(instruction.Destiny, Memory.Read16(address));
                            break;
                        case MemoryWidth.Dword:
                            SetValue(instruction.Destiny, Memory.Read32(address));
                            break;
                        case MemoryWidth.Qword:
                            // TODO: ???
                            SetValue(instruction.Destiny, (uint) Memory.Read64(address));
                            break;
                        case null:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    break;
                }
                case Opcode.AND:
                {
                    var a = GetValue(instruction.A);
                    var b = GetValue(instruction.B);
                    SetValue(instruction.Destiny, a & b);
                    break;
                }
                case Opcode.OR:
                {
                    var a = GetValue(instruction.A);
                    var b = GetValue(instruction.B);
                    SetValue(instruction.Destiny, a | b);
                    break;
                }
                case Opcode.XOR:
                {
                    var a = GetValue(instruction.A);
                    var b = GetValue(instruction.B);
                    SetValue(instruction.Destiny, a ^ b);
                    break;
                }
                case Opcode.NOT:
                {
                    var a = GetValue(instruction.A);
                    SetValue(instruction.Destiny, ~a);
                    break;
                }
                

                case Opcode.COPY:
                {
                    var a = GetValue(instruction.A);
                    SetValue(instruction.Destiny, a);
                    break;
                }
                case Opcode.CMP_EQ:
                {
                    var a = GetValue(instruction.A);
                    var b = GetValue(instruction.B);
                    SetValue(instruction.Destiny, a == b ? 1U : 0U);
                    break;
                }
                case Opcode.CMP_NE:
                {
                    var a = GetValue(instruction.A);
                    var b = GetValue(instruction.B);
                    SetValue(instruction.Destiny, a != b ? 1U : 0U);
                    break;
                }
                case Opcode.CMP_LT:
                {
                    var a = GetValue(instruction.A);
                    var b = GetValue(instruction.B);
                    SetValue(instruction.Destiny, a < b ? 1U : 0U);
                    break;
                }
                case Opcode.CMP_GT:
                {
                    var a = GetValue(instruction.A);
                    var b = GetValue(instruction.B);
                    SetValue(instruction.Destiny, a > b ? 1U : 0U);
                    break;
                }
                case Opcode.CMP_GT_SIGN:
                {
                    var a = GetValue(instruction.A);
                    var b = GetValue(instruction.B);
                    SetValue(instruction.Destiny, (Int32)a > (Int32)b ? 1U : 0U);
                    break;
                }
                case Opcode.CMP_GE:
                {
                    var a = GetValue(instruction.A);
                    var b = GetValue(instruction.B);
                    SetValue(instruction.Destiny, a >= b ? 1U : 0U);
                    break;
                }
                case Opcode.CMP_GE_SIGN:
                {
                    var a = GetValue(instruction.A);
                    var b = GetValue(instruction.B);
                    SetValue(instruction.Destiny, (Int32)a >= (Int32)b ? 1U : 0U);
                    break;
                }
                case Opcode.BRANCH:
                {
                    ipc = (int) GetValue(instruction.Destiny);
                    break;
                }
                case Opcode.BRANCH_TRUE:
                {
                    var a = GetValue(instruction.A);
                    if (a != 0)
                    {
                        ipc = (int) GetValue(instruction.Destiny);
                    }

                    break;
                }
                case Opcode.BRANCH_FALSE:
                {
                    var a = GetValue(instruction.A);
                    if (a == 0)
                    {
                        ipc = (int) GetValue(instruction.Destiny);
                    }

                    break;
                }

                case Opcode.SHL:
                {
                    var a = GetValue(instruction.A);
                    var b = (byte)GetValue(instruction.B);
                    SetValue(instruction.Destiny, a << b);
                    break;
                }
                case Opcode.SHR:
                {
                    var a = GetValue(instruction.A);
                    var b = (byte)GetValue(instruction.B);
                    SetValue(instruction.Destiny, a >> b);
                    break;
                }
                case Opcode.SAR:
                {
                    var a = GetValue(instruction.A);
                    var b = (byte)GetValue(instruction.B);
                    SetValue(instruction.Destiny, (UInt32)((Int32)a >> b));
                    break;
                }
                case Opcode.ROL:
                {
                    var a = GetValue(instruction.A);
                    var b = (byte)GetValue(instruction.B);
                    SetValue(instruction.Destiny, (a << b) | (a >> (32 - b)));
                    break;
                }
                case Opcode.ROR:
                {
                    var a = GetValue(instruction.A);
                    var b = (byte)GetValue(instruction.B);
                    SetValue(instruction.Destiny, (a >> b) | (a << (32 - b)));;
                    break;
                }
                case Opcode.DEBUG:
                {
                    Console.WriteLine($"DEBUG {instruction.A} {instruction.B}");
                    break;
                }

                case Opcode.SIGN_EXT8:
                {
                    var a = GetValue(instruction.A);
                    SetValue(instruction.Destiny, (UInt32)(sbyte)(a & 0xFF));
                    break;
                }
                case Opcode.SIGN_EXT16:
                {
                    var a = GetValue(instruction.A);
                    SetValue(instruction.Destiny, (UInt32)(short)(a & 0xFFFF));
                    break;
                }
                case Opcode.ZERO_EXT8:
                {
                    var a = GetValue(instruction.A);
                    SetValue(instruction.Destiny, (byte)(a & 0xFF));
                    break;
                }
                case Opcode.ZERO_EXT16:
                {
                    var a = GetValue(instruction.A);
                    SetValue(instruction.Destiny, (UInt16)(a & 0xFFFF));
                    break;
                }

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}