using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using runtime.core;

namespace runtime.ILCompiler;

using static OpCodes;

public struct IlExprBuilder
{
    private readonly ILGenerator _il;
    public readonly MethodBase InternalMethod;

    public const MethodAttributes InterfaceAttributes = MethodAttributes.Final | MethodAttributes.HideBySig |
                                                        MethodAttributes.NewSlot | MethodAttributes.Virtual;

    public static implicit operator MethodBuilder(IlExprBuilder b) => (MethodBuilder)b.InternalMethod;
    public static implicit operator ConstructorBuilder(IlExprBuilder b) => (ConstructorBuilder)b.InternalMethod;
    public static implicit operator IlExprBuilder(MethodBuilder mb) => new(mb);
    public static implicit operator IlExprBuilder(ConstructorBuilder cb) => new(cb);

    #region BuilderExtensions

    public readonly struct ILConversionBuilder
    {
        private readonly IlExprBuilder _eb;

        internal ILConversionBuilder(IlExprBuilder eb) => _eb = eb;

        public ILConversionBuilder Int(bool unsigned, int byteSize, bool overflow) {
            switch (byteSize)
            {
                case 1:
                    _eb._il.Emit(unsigned ? overflow?Conv_Ovf_U1:Conv_U1 : overflow?Conv_Ovf_I1:Conv_I1);
                    break;
                case 2:
                    _eb._il.Emit(unsigned ? overflow?Conv_Ovf_U2:Conv_U2 : overflow?Conv_Ovf_I2:Conv_I2);
                    break;
                case 4:
                    _eb._il.Emit(unsigned ? overflow?Conv_Ovf_U4:Conv_U4 : overflow?Conv_Ovf_I4:Conv_I4);
                    break;
                case 8:
                    _eb._il.Emit(unsigned ? overflow?Conv_Ovf_U8:Conv_U8 : overflow?Conv_Ovf_I8:Conv_I8);
                    break;
                default:
                    throw new NotSupportedException("Cannot Perform Int Conversion Conv_" +
                                                    (overflow ? "OverFlow_" : "") + (unsigned ? "U" : "I") + byteSize);
            }

            return this;
        }
        public ILConversionBuilder Float(int byteSize) {
            switch (byteSize) {
                case 4: _eb._il.Emit(Conv_R4);
                    break;
                case 8: _eb._il.Emit(Conv_R8);
                    break;
                default: throw new NotSupportedException($"Cannot Convert To Float{byteSize}");
            }
            return this;
        }

        public void CastClass(Type t) => _eb._il.Emit(Castclass, t);
    }

    public readonly struct ILFunctionExprBuilder
    {
        private readonly IlExprBuilder _eb;

        internal ILFunctionExprBuilder(IlExprBuilder eb) => _eb = eb;

        public ILFunctionExprBuilder Invoke(MethodInfo info, bool virtualCall = false) {
            _eb._il.EmitCall(virtualCall?Callvirt:Call, info, null);
            return this;
        }
        
        public ILFunctionExprBuilder Invoke(IlExprBuilder fb, bool requiresConstructor = false, bool requiresMethod = false) {
            switch (fb.InternalMethod)
            {
                case MethodBuilder mb when !requiresConstructor:
                    Invoke((MethodInfo)mb);
                    break;
                case ConstructorBuilder cb when !requiresMethod:
                    Invoke((ConstructorInfo)cb);
                    break;
                default:
                    throw new Exception("Unable To Invoke Unknown Internal Method!");
            }
            return this;
        }

        public ILFunctionExprBuilder Invoke(ConstructorInfo ci) {
            _eb.Create.Object(ci);
            return this;
        }

        public ILFunctionExprBuilder Println(string s) {
            _eb._il.EmitWriteLine(s);
            return this;
        }
    }

    public struct ILLoadExprBuilder
    {
        private readonly IlExprBuilder _eb;
        internal ILLoadExprBuilder(IlExprBuilder eb) => _eb = eb;

        public ILLoadExprBuilder Arg(int i) {
            _eb._il.Emit(Ldarg, _eb.InternalMethod.IsStatic ? i : i + 1);
            return this;
        }
        public ILLoadExprBuilder This(bool value = true)
        {
            _eb._il.Emit(Ldarg_0);

            if (!value)
                return this;

            //If the object is a value type, have to load the value 
            var dty = _eb.InternalMethod.DeclaringType;
            if (dty?.IsValueType == false)
                ValueObject(dty);
            
            return this;
        }

        public ILLoadExprBuilder ValueObject(Type t) {
            _eb._il.Emit(Ldobj, t);
            return this;
        }

        public ILLoadExprBuilder String(string str) {
            _eb._il.Emit(Ldstr, str);
            return this;
        }

        public ILLoadExprBuilder Bool(bool b) {
            _eb._il.Emit(b?Ldc_I4_1:Ldc_I4_0);
            return this;
        }

        public ILLoadExprBuilder Int64(Int64 v) {
            _eb._il.Emit(Ldc_I8, v);
            return this;
        }

        public ILLoadExprBuilder Int32(Int32 v) {
            _eb._il.Emit(Ldc_I4, v);
            return this;
        }

        public ILLoadExprBuilder Null() {
            _eb._il.Emit(Ldnull);
            return this;
        }
        
        public ILLoadExprBuilder Int16(Int16 v) {
            Int32(v);
            _eb.Convert.Int(false, 2, false);
            return this;
        }

        public ILLoadExprBuilder Int8(sbyte v) {
            Int32(v);
            _eb.Convert.Int(false, 1, false);
            return this;
        }

        public ILLoadExprBuilder UInt64(UInt64 v) {
            _eb._il.Emit(Ldc_I8, v);
            _eb.Convert.Int(true, 8, false);
            return this;
        }

        public ILLoadExprBuilder UInt32(UInt32 v) {
            _eb._il.Emit(Ldc_I4, v);
            _eb.Convert.Int(true, 4, false);
            return this;
        }

        public ILLoadExprBuilder UInt16(UInt16 v) {
            Int32(v);
            _eb.Convert.Int(true, 2, false);
            return this;
        }

        public ILLoadExprBuilder UInt8(byte v) {
            Int32(v);
            _eb.Convert.Int(true, 1, false);
            return this;
        }

        public ILLoadExprBuilder Float32(float v)
        {
            _eb._il.Emit(Ldc_R4, v);
            return this;
        }

        public ILLoadExprBuilder Float64(double v) {
            _eb._il.Emit(Ldc_R8, v);
            return this;
        }

        public ILLoadExprBuilder Token(Type t) {
            _eb._il.Emit(Ldtoken, t);
            return this;
        }

        public ILLoadExprBuilder Token(FieldInfo f) {
            _eb._il.Emit(Ldtoken, f);
            return this;
        }

        public ILLoadExprBuilder Token(MethodInfo m) {
            _eb._il.Emit(Ldtoken, m);
            return this;
        }

        public ILLoadExprBuilder Token(ConstructorInfo c) {
            _eb._il.Emit(Ldtoken, c);
            return this;
        }

        public ILLoadExprBuilder Type(Type t) {
            Token(t);
            _eb.Function.Invoke(Reflect.Type_GetRuntimeType);
            return this;
        }

        public ILLoadExprBuilder Field(FieldInfo f, bool loadAddr = false) {
            _eb._il.Emit(loadAddr? f.IsStatic?Ldsflda:Ldflda : f.IsStatic?Ldsfld:Ldfld, f);
            return this;
        }

        public ILLoadExprBuilder OptFloat(double v) => v > float.MaxValue ? Float32((float)v) : Float64(v);
        public ILLoadExprBuilder OptInt(Int64 v) {
            var less = v < 0;
            if (less) v *= -1;
            if (v < System.Int32.MaxValue)
                return Int32((Int32)v * (less ? -1 : 1));
            else
                return Int64(v * (less ? -1 : 1));
        }

        public ILLoadExprBuilder OptUInt(UInt64 v) => v < System.Int32.MaxValue ? UInt32((UInt32)v) : UInt64(v);
        public ILLoadExprBuilder Const(UInt64 v) => OptUInt(v);
        public ILLoadExprBuilder Const(Int64 v) => OptInt(v);
        public ILLoadExprBuilder Const(double v) => OptFloat(v);
        public ILLoadExprBuilder Const(string s) => String(s);
        public ILLoadExprBuilder Const(bool b) => Bool(b);
        public ILLoadExprBuilder Local(LocalBuilder lb) {
            _eb._il.Emit(Ldloc, lb);
            return this;
        }
        public void FromPointerInt(bool unsigned, int size) {
            switch (size) {
                case 1: _eb._il.Emit(unsigned?Ldind_U1:Ldind_I1);
                    break;
                case 2: _eb._il.Emit(unsigned?Ldind_U2:Ldind_I2);
                    break;
                case 4: _eb._il.Emit(unsigned?Ldind_U4:Ldind_I4);
                    break;
                case 8: _eb._il.Emit(Ldind_I8);
                    break;
                default: throw new NotSupportedException();
            }
        }

        public void FromPointerFloat(int size) {
            switch (size) {
                case 4: _eb._il.Emit(Ldind_R4);
                    break;
                case 8: _eb._il.Emit(Ldind_R8);
                    break;
                default: throw new NotSupportedException();
            }
        }
        public void FromPointerRef() => _eb._il.Emit(Ldind_Ref);
    }

    public struct ILStoreExprBuilder
    {
        private readonly IlExprBuilder _eb;
        internal ILStoreExprBuilder(IlExprBuilder eb) => _eb = eb;

        public ILStoreExprBuilder Field(FieldInfo f) {
            _eb._il.Emit(f.IsStatic ? Stsfld : Stfld, f);
            return this;
        }

        public ILStoreExprBuilder Local(LocalBuilder lb) {
            _eb._il.Emit(Stloc, lb);
            return this;
        }

        public void ToPointerInt(int size) {
            switch (size) {
                case 1: _eb._il.Emit(Stind_I1); break;
                case 2: _eb._il.Emit(Stind_I2); break;
                case 4: _eb._il.Emit(Stind_I4); break;
                case 8: _eb._il.Emit(Stind_I8); break;
                default: throw new NotSupportedException();
            }
        }
        public void ToPointerFloat(int size) {
            switch (size) {
                case 4: _eb._il.Emit(Stind_R4);
                    break;
                case 8: _eb._il.Emit(Stind_R8);
                    break;
                default: throw new NotSupportedException();
            }
        }
        public void ToPointerRef() => _eb._il.Emit(Stind_Ref);
    }

    public struct ILCreateExprBuilder
    {
        private readonly IlExprBuilder _eb;
        internal ILCreateExprBuilder(IlExprBuilder eb) => _eb = eb;
        public LocalBuilder Local(Type t, bool pinned = false) => _eb._il.DeclareLocal(t, pinned);

        public LocalBuilder LocalAndStore(Type t) {
            var lb = Local(t);
            _eb.Store.Local(lb);
            return lb;
        }

        public void Object(ConstructorInfo ci) => _eb._il.Emit(Newobj, ci);
        public void Object(IlExprBuilder cb) => Object((ConstructorInfo) cb);
    }

    public struct ILArrayExprBuilder
    {
        private readonly IlExprBuilder _eb;
        internal ILArrayExprBuilder(IlExprBuilder eb) => _eb = eb;

        public ILArrayExprBuilder LoadElement(int i, Type t) {
            _eb.Load.Const(i);
            return LoadElement(t);
        }

        public ILArrayExprBuilder LoadElement(Type t) {
            _eb._il.Emit(Ldelem, t);
            return this;
        }

        public ILArrayExprBuilder StoreElement(Type t) {
            _eb._il.Emit(Stelem, t);
            return this;
        }

        public ILArrayExprBuilder LoadElementArrayAddress() {
            _eb._il.Emit(Ldelema);
            return this;
        }

        public ILArrayExprBuilder LoadElementArrayAddress(int i) {
            _eb.Load.Const(i);
            return LoadElementArrayAddress();
        }
        public ILArrayExprBuilder StoreElement(int i, Type t) {
            _eb.Load.Const(i);
            return StoreElement(t);
        }

        public ILArrayExprBuilder Create1d(Type elType, bool unInit=false, bool pinned=false) {
            if (unInit) {
                _eb.Load.Bool(pinned);
                _eb.Function.Invoke(Reflect.GC_AllocateUnitialized_1.MakeGenericMethod(elType));
            }else _eb._il.Emit(Newarr, elType);
            return this;
        }

        public ILArrayExprBuilder Create1d(Type elType, int size, bool unInit=false, bool pinned=false) {
            if (size == 0) {
                _eb.Function.Invoke(Reflect.Array_Empty.MakeGenericMethod(elType));
            }else{
                _eb.Load.Const(size);
                Create1d(elType, unInit, pinned);
            }
            return this;
        }

        public LocalBuilder Serialize1D<T, TE>(T[] e, Action<T> perElementSerializer) => Serialize1D<T, TE>(e, e.Length, perElementSerializer);
        public LocalBuilder Serialize1D<T, TE>(IList<T> e, Action<T> perElementSerializer) => Serialize1D<T, TE>(e, e.Count, perElementSerializer);
        public LocalBuilder Serialize1D<T, TE>(IEnumerable<T> e, int length, Action<T> perElementSerializer) {
            Create1d(typeof(TE), length, true);
            var array = _eb.Create.LocalAndStore(typeof(TE[]));
            var index = 0;
            foreach (var n in e) {
                _eb.Load.Local(array);
                _eb.Load.Int32(index++);
                perElementSerializer(n);
                StoreElement(typeof(TE));
            }
            return array;
        }
    }

    public struct ILUnsafeExprBuilder
    {
        private readonly IlExprBuilder _eb;
        internal ILUnsafeExprBuilder(IlExprBuilder eb) => _eb = eb;

        public void StackAlloc() => _eb._il.Emit(Localloc);

        public void StackAlloc(Type t, int size)
        {
            _eb.Load.UInt32((uint)(Marshal.SizeOf(t) * size));
            StackAlloc();
        }

        public void CopyBlock(int numBytes) {
            _eb.Load.Int32(numBytes);
            CopyBlock();
        }

        public void CopyBlock() => _eb._il.Emit(Cpblk);
    }

    public struct ILMathBuilder {
        private readonly IlExprBuilder _eb;
        internal ILMathBuilder(IlExprBuilder eb) => _eb = eb;

        public ILMathBuilder Add(bool check = false) {
            _eb._il.Emit(check?Add_Ovf:OpCodes.Add);
            return this;
        }

        public ILMathBuilder Sub(bool check = false) {
            _eb._il.Emit(check?Sub_Ovf:OpCodes.Sub);
            return this;
        }
    }

    public struct ILConditionalBuilder {
        private readonly IlExprBuilder _eb;
        internal ILConditionalBuilder(IlExprBuilder eb) => _eb = eb;

        public ILConditionalBuilder IfTrue(Label l, bool ternary = false) {
            _eb._il.Emit(ternary?Brtrue_S:Brtrue, l);
            return this;
        }

        public ILConditionalBuilder IfFalse(Label l, bool ternary = false)
        {
            _eb._il.Emit(ternary?Brfalse_S:Brfalse, l);
            return this;
        }

        public ILConditionalBuilder Goto(Label l, bool ternary = false) {
            _eb._il.Emit(ternary?Br_S:Br, l);
            return this;
        }
        
        public Label CreateLabel() => _eb._il.DefineLabel();

        public ILConditionalBuilder StartLabel(Label l) {
            _eb._il.MarkLabel(l);
            return this;
        }
    }
    
    #endregion

    #region BuilderExtensionImpl

    public ILFunctionExprBuilder Function => new(this);
    public ILLoadExprBuilder Load => new(this);
    public ILStoreExprBuilder Store => new(this);
    public ILCreateExprBuilder Create => new(this);
    public ILArrayExprBuilder Array => new(this);
    public ILUnsafeExprBuilder Unsafe => new(this);
    public ILConversionBuilder Convert => new(this);
    public ILMathBuilder Math => new(this);
    public ILConditionalBuilder Condition => new(this);

    #endregion

    private IlExprBuilder(ILGenerator il, MethodBase internalMethod)
    {
        _il = il;
        InternalMethod = internalMethod;
    }

    public IlExprBuilder(MethodBuilder mb) : this(mb.GetILGenerator(), mb)
    {
    }

    public IlExprBuilder(ConstructorBuilder cb) : this(cb.GetILGenerator(), cb)
    {
    }

    public IlExprBuilder(DynamicMethod cb) : this(cb.GetILGenerator(), cb)
    {
    }

    #region HelperFunctions

    public void Emit(OpCode code) => _il.Emit(code);

    public void Return() => Emit(Ret);
    public void Duplicate() => Emit(Dup);
    public void Box(Type t) => _il.Emit(OpCodes.Box, t);
    public void Unbox(Type t, bool any = false) => _il.Emit(any ? Unbox_Any : OpCodes.Unbox, t);

    public void DefineArg(int index, string name, ParameterAttributes attributes = ParameterAttributes.None)
    {
        index++;
        switch (InternalMethod)
        {
            case MethodBuilder mb:
                mb.DefineParameter(index, attributes, name);
                break;
            case ConstructorBuilder cb:
                cb.DefineParameter(index, attributes, name);
                break;
        }
    }

    public void ReturnVoid() {
        _il.Emit(Nop);
        Return();
    }

    #endregion
}