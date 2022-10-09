
using runtime.core;
/**This Class Was Machine Generated. Do not Modify*/
public enum MemoryType : byte{
    None,
    //CONSTANTS//
    Signed,
    Unsigned,
    Floating,
    Pointer,
    Block,
    Array,
    String,
    //END CONSTANTS//
    
    Type,
    Module,
    Function,
    
    //VARIADICS//
    Variable,
    //END VARIADICS//
}

public enum OperationType : byte{
    None,
    Push,
    Pop,
    PushVariadic,
    PopVariadic,
    Load,
    Invoke
}

public readonly struct JOp {
    public static readonly JOp[] OpCodeTable = new JOp[22];
    public readonly byte OpCode;
    public readonly sbyte StackChange;
    public readonly byte OperandsSize;
    public readonly MemoryType Memory;
    public readonly OperationType Behaviour;

    public bool IsPush => Behaviour == OperationType.Push;
    public bool IsPop => Behaviour == OperationType.Pop;

    private unsafe JOp(byte opCode, sbyte stackChange, byte operandsSize, MemoryType memory, OperationType behaviour) {
        OpCode = opCode;
        StackChange = stackChange;
        OperandsSize = operandsSize;
        Memory = memory;
        Behaviour = behaviour;
        OpCodeTable[OpCode] = this;
    }

    public byte GetRuntimeOpCode() {
        if (Behaviour == OperationType.Push) {
            if (Memory == MemoryType.Signed) {
                switch (OperandsSize) {
                    case 1: return PushInt8.OpCode;
                    case 2: return PushInt16.OpCode;
                    case 4: return PushInt32.OpCode;
                    case 8: return PushInt64.OpCode;
                }
            }else if (Memory == MemoryType.Unsigned) {
                switch (OperandsSize) {
                    case 1: return PushUInt8.OpCode;
                    case 2: return PushUInt16.OpCode;
                    case 4: return PushUInt32.OpCode;
                    case 8: return PushUInt64.OpCode;
                }   
            }else if (Memory == MemoryType.Floating)
            {
                if (OperandsSize == 4) return PushFloat32.OpCode;
                if (OperandsSize == 8) return PushFloat64.OpCode;
            }else if (Memory == MemoryType.String)
                return PushString.OpCode;
            else if (Memory == MemoryType.Array)
                return PushArray.OpCode;
        }else if (Behaviour == OperationType.Pop) {
            if (Memory == MemoryType.Array) {
                return PopArray.OpCode;
            }
            switch (OperandsSize) {
                case 1: return Pop8.OpCode;
                case 2: return Pop16.OpCode;
                case 4: return Pop32.OpCode;
                case 8: return Pop64.OpCode;
            }
        }else if (Behaviour == OperationType.Load) {
            switch (Memory) {
                case MemoryType.Type: return LoadType.OpCode;
                case MemoryType.Module: return LoadModule.OpCode;
                case MemoryType.Function: return LoadMethod.OpCode;
            }
        }else if (Behaviour == OperationType.Invoke) {
            return InvokeFunction.OpCode;
        }

        throw new JuliaException("Unable To Determine OpCode!");
    }

	public const byte PushInt8Op = 0;
	public const byte PushInt16Op = 1;
	public const byte PushInt32Op = 2;
	public const byte PushInt64Op = 3;
	public const byte PushUInt8Op = 4;
	public const byte PushUInt16Op = 5;
	public const byte PushUInt32Op = 6;
	public const byte PushUInt64Op = 7;
	public const byte PushFloat32Op = 8;
	public const byte PushFloat64Op = 9;
	public const byte PushArrayOp = 10;
	public const byte PushStringOp = 11;
	public const byte Pop8Op = 12;
	public const byte Pop16Op = 13;
	public const byte Pop32Op = 14;
	public const byte Pop64Op = 15;
	public const byte PopArrayOp = 16;
	public const byte PopStringOp = 15;
	public const byte LoadTypeOp = 17;
	public const byte LoadModuleOp = 18;
	public const byte LoadMethodOp = 19;
	public const byte InvokeFunctionOp = 20;


	public static readonly JOp PushInt8 = new(PushInt8Op, 1, 2, MemoryType.Signed, OperationType.Push);
	public static readonly JOp PushInt16 = new(PushInt16Op, 2, 3, MemoryType.Signed, OperationType.Push);
	public static readonly JOp PushInt32 = new(PushInt32Op, 4, 5, MemoryType.Signed, OperationType.Push);
	public static readonly JOp PushInt64 = new(PushInt64Op, 8, 9, MemoryType.Signed, OperationType.Push);
	public static readonly JOp PushUInt8 = new(PushUInt8Op, 1, 2, MemoryType.Unsigned, OperationType.Push);
	public static readonly JOp PushUInt16 = new(PushUInt16Op, 2, 3, MemoryType.Unsigned, OperationType.Push);
	public static readonly JOp PushUInt32 = new(PushUInt32Op, 4, 5, MemoryType.Unsigned, OperationType.Push);
	public static readonly JOp PushUInt64 = new(PushUInt64Op, 8, 9, MemoryType.Unsigned, OperationType.Push);
	public static readonly JOp PushFloat32 = new(PushFloat32Op, 4, 5, MemoryType.Floating, OperationType.Push);
	public static readonly JOp PushFloat64 = new(PushFloat64Op, 8, 9, MemoryType.Floating, OperationType.Push);
	public static readonly JOp PushArray = new(PushArrayOp, 12, 13, MemoryType.Array, OperationType.Push);
	public static readonly JOp PushString = new(PushStringOp, 8, 5, MemoryType.String, OperationType.Push);
	public static readonly JOp Pop8 = new(Pop8Op, -1, 2, MemoryType.Block, OperationType.Pop);
	public static readonly JOp Pop16 = new(Pop16Op, -1, 2, MemoryType.Block, OperationType.Pop);
	public static readonly JOp Pop32 = new(Pop32Op, -1, 2, MemoryType.Block, OperationType.Pop);
	public static readonly JOp Pop64 = new(Pop64Op, -1, 2, MemoryType.Block, OperationType.Pop);
	public static readonly JOp PopArray = new(PopArrayOp, -12, 13, MemoryType.Array, OperationType.Pop);
	public static readonly JOp PopString = new(PopStringOp, -1, 2, MemoryType.Block, OperationType.Pop);
	public static readonly JOp LoadType = new(LoadTypeOp, 8, 5, MemoryType.Type, OperationType.Load);
	public static readonly JOp LoadModule = new(LoadModuleOp, 8, 5, MemoryType.Module, OperationType.Load);
	public static readonly JOp LoadMethod = new(LoadMethodOp, 8, 5, MemoryType.Function, OperationType.Load);
	public static readonly JOp InvokeFunction = new(InvokeFunctionOp, 0, 9, MemoryType.Function, OperationType.Invoke);


	public static readonly string[] OpCodeNames = {"PushInt8", "PushInt16", "PushInt32", "PushInt64", "PushUInt8", "PushUInt16", "PushUInt32", "PushUInt64", "PushFloat32", "PushFloat64", "PushArray", "PushString", "Pop8", "Pop16", "Pop32", "Pop64", "PopArray", "PopString", "LoadType", "LoadModule", "LoadMethod", "InvokeFunction"};
	public string Name => OpCodeNames[OpCode];
}
