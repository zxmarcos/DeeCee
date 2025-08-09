using System.Diagnostics;
using System.Reflection.Emit;
using System.Reflection.Metadata;

namespace DeeCee.SH4;

public class EmitterContext
{
    public BasicBlock Block { get; } = new();

    public Operand AllocateLocal(RegisterType registerType = RegisterType.Int32)
    {
        return new Operand(OperandKind.LocalVariable)
        {
            VarIndex = Block.LocalVariableCount++
        };
    }
    public Operand Add(Operand a, Operand b)
    {
        var result = AllocateLocal();
        Block.Add(new Instruction(a, b, result, Opcode.ADD));
        return result;
    }
    
    public Operand Sub(Operand a, Operand b)
    {
        var result = AllocateLocal();
        Block.Add(new Instruction(a, b, result, Opcode.SUB));
        return result;
    }

    public Operand And(Operand a, Operand b)
    {
        var result = AllocateLocal();
        Block.Add(new Instruction(a, b, result, Opcode.AND));
        return result;
    }
    
    public Operand Or(Operand a, Operand b)
    {
        var result = AllocateLocal();
        Block.Add(new Instruction(a, b, result, Opcode.OR));
        return result;
    }
    
    public Operand Xor(Operand a, Operand b)
    {
        var result = AllocateLocal();
        Block.Add(new Instruction(a, b, result, Opcode.XOR));
        return result;
    }
    
    public Operand Not(Operand a)
    {
        var result = AllocateLocal();
        Block.Add(new Instruction(a, null, result, Opcode.NOT));
        return result;
    }
    
    public Operand ShiftLeft(Operand a, Operand b)
    {
        var result = AllocateLocal();
        Block.Add(new Instruction(a, b, result, Opcode.SHL));
        return result;
    }
    
    public Operand ShiftRight(Operand a, Operand b)
    {
        var result = AllocateLocal();
        Block.Add(new Instruction(a, b, result, Opcode.SHR));
        return result;
    }
    
    public Operand ShiftRightArithmetic(Operand a, Operand b)
    {
        var result = AllocateLocal();
        Block.Add(new Instruction(a, b, result, Opcode.SAR));
        return result;
    }
    
    public Operand RotateLeft(Operand a, Operand b)
    {
        var result = AllocateLocal();
        Block.Add(new Instruction(a, b, result, Opcode.ROL));
        return result;
    }
    
    public Operand RotateRight(Operand a, Operand b)
    {
        var result = AllocateLocal();
        Block.Add(new Instruction(a, b, result, Opcode.ROR));
        return result;
    }
    
    public Operand CompareEqual(Operand a, Operand b)
    {
        var result = AllocateLocal();
        Block.Add(new Instruction(a, b, result, Opcode.CMP_EQ));
        return result;
    }
    
    public Operand CompareNotEqual(Operand a, Operand b)
    {
        var result = AllocateLocal();
        Block.Add(new Instruction(a, b, result, Opcode.CMP_NE));
        return result;
    }

    public Operand CompareGreater(Operand a, Operand b)
    {
        var result = AllocateLocal();
        Block.Add(new Instruction(a, b, result, Opcode.CMP_GT));
        return result;
    }
    
    public Operand CompareGreaterSigned(Operand a, Operand b)
    {
        var result = AllocateLocal();
        Block.Add(new Instruction(a, b, result, Opcode.CMP_GT_SIGN));
        return result;
    }
    
    public Operand CompareGreaterOrEqual(Operand a, Operand b)
    {
        var result = AllocateLocal();
        Block.Add(new Instruction(a, b, result, Opcode.CMP_GE));
        return result;
    }
    
    public Operand CompareGreaterOrEqualSigned(Operand a, Operand b)
    {
        var result = AllocateLocal();
        Block.Add(new Instruction(a, b, result, Opcode.CMP_GE_SIGN));
        return result;
    }
    
    public Operand CompareLesser(Operand a, Operand b)
    {
        var result = AllocateLocal();
        Block.Add(new Instruction(a, b, result, Opcode.CMP_LT));
        return result;
    }

    public Operand SignExtend8(Operand a)
    {
        var result = AllocateLocal();
        Block.Add(new Instruction(a, null, result, Opcode.SIGN_EXT8));
        return result;
    }
    
    public Operand SignExtend16(Operand a)
    {
        var result = AllocateLocal();
        Block.Add(new Instruction(a, null, result, Opcode.SIGN_EXT16));
        return result;
    }
    
    public Operand ZeroExtend8(Operand a)
    {
        var result = AllocateLocal();
        Block.Add(new Instruction(a, null, result, Opcode.ZERO_EXT8));
        return result;
    }
    
    public Operand ZeroExtend16(Operand a)
    {
        var result = AllocateLocal();
        Block.Add(new Instruction(a, null, result, Opcode.ZERO_EXT16));
        return result;
    }
    
    

    public void If(Operand a, Action ifTrueBody, Action? ifFalseBody = null)
    {
        Debug.Assert(ifTrueBody != null);
        Operand skip = Label();
        Operand end = Label();
        if (ifFalseBody != null)
        {
            BranchIfFalse(a, skip);
            {
                ifTrueBody();
                Branch(end);
            }
            MarkLabel(skip);
            {
                ifFalseBody();
            }
            MarkLabel(end);
        }
        else
        {
            BranchIfFalse(a, skip);
            ifTrueBody();
            MarkLabel(skip);
        }
    }

    public Operand IsZero(Operand a)
    {
        return CompareEqual(a, Constant(0));
    }

    public void SetReg(byte regNum, Operand value)
    {
        Store(GetReg(regNum), value);
    }
    
    public void Store(Operand destiny, Operand value, MemoryWidth width = MemoryWidth.Dword)
    {
        Block.Add(new Instruction(value, null, destiny, Opcode.STORE));
    }
    
    public Operand Load(Operand source)
    {
        var result = AllocateLocal();
        Block.Add(new Instruction(source, null, result, Opcode.LOAD));
        return result;
    }

    public Operand Memory(Operand address, MemoryWidth width = MemoryWidth.Dword)
    {
        return new Operand(OperandKind.Memory)
        {
            Address = address,
            MemoryWidth = width
        };
    }
    
    public Operand Register(byte regNum, RegisterType registerType)
    {
        return new Operand(OperandKind.Register, regNum, registerType);
    }

    public Operand GetReg(byte regNum)
    {
        return new Operand(OperandKind.Register, regNum, RegisterType.Int32);
    }

    public Operand Constant(uint value)
    {
        return new Operand(ConstantType.UInt32)
        {
            UConst32 = value,
        };
    }
    
    public Operand Constant(int value)
    {
        return new Operand(ConstantType.Int32)
        {
            UConst32 = (UInt32) value,
        };
    }

    public void Copy(Operand src, Operand dst)
    {
        Block.Instructions.Add(new Instruction(src, null, dst, Opcode.COPY));
    }

    public void MarkLabel(Operand label)
    {
         Debug.Assert(label.Kind == OperandKind.Label);
         label.BlockOffset = Block.Instructions.Count;
    }

    public Operand Label()
    {
        return new Operand()
        {
            Kind = OperandKind.Label
        };
    }

    public void Branch(Operand label)
    {
        Block.Add(new Instruction(null, null, label, Opcode.BRANCH));
    }
    
    public void BranchIfTrue(Operand operand, Operand label)
    {
        Block.Add(new Instruction(operand, null, label, Opcode.BRANCH_TRUE));
    }
    
    public void BranchIfFalse(Operand operand, Operand label)
    {
        Block.Add(new Instruction(operand, null, label, Opcode.BRANCH_FALSE));
    }

    public void DebugOp(Operand operand, string? message)
    {
        Block.Add(new Instruction(operand, Operand.DebugMessage(message), null, Opcode.DEBUG));
    }
}