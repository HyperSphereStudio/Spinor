﻿using System.Reflection;

namespace RuntimeGenerator.JOp
{
    public enum MemoryType : byte
    {
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

    public enum OperationType : byte
    {
        None,
        Push,
        Pop,
        PushVariadic,
        PopVariadic,
        Load,
        Invoke
    }

    public readonly struct JOp
    {
        private static byte _internalCounter = 0;
        public readonly byte OpCode = _internalCounter++;
        public readonly sbyte StackChange;
        public readonly byte OperandsSize;
        public readonly MemoryType Memory;
        public readonly OperationType Behaviour;

        public bool IsPush => Behaviour == OperationType.Push;
        public bool IsPop => Behaviour == OperationType.Pop;


        public unsafe JOp(sbyte stackChange, byte operandsSize, MemoryType memory, OperationType behaviour) {
            StackChange = stackChange;
            OperandsSize = (byte)(operandsSize + sizeof(byte));
            Memory = memory;
            Behaviour = behaviour;
        }

        public static readonly JOp PushInt8 = new(1, 1, MemoryType.Signed, OperationType.Push);
        public static readonly JOp PushInt16 = new(2, 2, MemoryType.Signed, OperationType.Push);
        public static readonly JOp PushInt32 = new(4, 4, MemoryType.Signed, OperationType.Push);
        public static readonly JOp PushInt64 = new(8, 8, MemoryType.Signed, OperationType.Push);
        public static readonly JOp PushUInt8 = new(1, 1, MemoryType.Unsigned, OperationType.Push);
        public static readonly JOp PushUInt16 = new(2, 2, MemoryType.Unsigned, OperationType.Push);
        public static readonly JOp PushUInt32 = new(4, 4, MemoryType.Unsigned, OperationType.Push);
        public static readonly JOp PushUInt64 = new(8, 8, MemoryType.Unsigned, OperationType.Push);
        public static readonly JOp PushFloat32 = new(4, 4, MemoryType.Floating, OperationType.Push);
        public static readonly JOp PushFloat64 = new(8, 8, MemoryType.Floating, OperationType.Push);
        public static readonly JOp PushArray = new(12, 12, MemoryType.Array, OperationType.Push);
        public static readonly JOp PushString = new(8, 4, MemoryType.String, OperationType.Push);

        public static readonly JOp Pop8 = new(-1, 1, MemoryType.Block, OperationType.Pop);
        public static readonly JOp Pop16 = new(-1, 1, MemoryType.Block, OperationType.Pop);
        public static readonly JOp Pop32 = new(-1, 1, MemoryType.Block, OperationType.Pop);
        public static readonly JOp Pop64 = new(-1, 1, MemoryType.Block, OperationType.Pop);
        public static readonly JOp PopArray = new(-12, 12, MemoryType.Array, OperationType.Pop);
        public static readonly JOp PopString = Pop64;

        public static readonly JOp LoadType = new(8, 4, MemoryType.Type, OperationType.Load);
        public static readonly JOp LoadModule = new(8, 4, MemoryType.Module, OperationType.Load);
        public static readonly JOp LoadMethod = new(8, 4, MemoryType.Function, OperationType.Load);


        //Variadic Stack Size
        public static readonly JOp InvokeFunction = new(0, 8, MemoryType.Function, OperationType.Invoke);
    }

    public class JOPCodeGenerator
    {
        public static void GenerateOpCodeTable(string rootPath)
        {
            var dir = Path.Join(rootPath, "runtime/core/JIL/generated/JILOPCode.cs");
            TextWriter fs = new StreamWriter(new FileStream(dir, FileMode.Create));
            string s = @"
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
    public static readonly JOp[] OpCodeTable = new JOp[%LREPLACE%];
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

        throw new JuliaException(""Unable To Determine OpCode!"");
    }";
            var fi = typeof(JOp).GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(x => x.FieldType == typeof(JOp)).ToArray();

            fs.Write(s.Replace("%LREPLACE%", fi.Length.ToString()));
            fs.Write("\n\n");

            List<string> names = new();
            foreach (var f in fi) {
                var p = (JOp)f.GetValue(null);
                fs.WriteLine("\tpublic const byte " + f.Name + "Op" + " = " + p.OpCode + ";");
                names.Add(f.Name);
            }

            fs.Write("\n\n");

            foreach (var f in fi) {
                var p = (JOp)f.GetValue(null);
                object[] param = {
                    f.Name + "Op", p.StackChange, p.OperandsSize, "MemoryType." + p.Memory,
                    "OperationType." + p.Behaviour
                };
                fs.WriteLine("\tpublic static readonly JOp " + f.Name + " = new(" + String.Join(", ", param) + ");");
            }

            fs.Write("\n\n\tpublic static readonly string[] OpCodeNames = {");
            bool isFirst = true;
            foreach (var name in names)
            {
                if (!isFirst) fs.Write(", ");
                else isFirst = false;
                fs.Write("\"" + name + "\"");
            }

            fs.WriteLine("};\n\tpublic string Name => OpCodeNames[OpCode];\n}");
            fs.Close();
        }
    }
}