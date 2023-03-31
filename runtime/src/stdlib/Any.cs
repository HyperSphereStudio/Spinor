/*
   * Author : Johnathan Bizzano
   * Created : Monday, January 2, 2023
   * Last Modified : Monday, January 2, 2023
*/

using System;
using System.IO;
using runtime.core.internals;
using runtime.core.memory;
using runtime.core.type;

namespace runtime.stdlib;

public struct Any : IAny{
    public readonly IAny Value;
    public static SType RuntimeType { get; internal set; }

    public Any(ISystemAny value) => Value = value ?? Spinor.Nothing.Value;
    public Any(ISpinorAny value) => Value = value ?? Spinor.Nothing.Value;
    public bool IsSystemType => Value is ISystemAny;
    public bool IsSpinorType => Value is ISpinorAny;
    public bool IsNothing => ReferenceEquals(Value, Spinor.Nothing.Value) || Value == null;
    public SType Type => Value.Type;
    
    public static implicit operator Any(bool a) => Box(a);
    public static implicit operator Any(sbyte a) => Box(a);
    public static implicit operator Any(int a) => Box(a);
    public static implicit operator Any(short a) => Box(a);
    public static implicit operator Any(ushort a) => Box(a);
    public static implicit operator Any(uint a) => Box(a);
    public static implicit operator Any(byte a) => Box(a);
    public static implicit operator Any(long a) => Box(a);
    public static implicit operator Any(ulong a) => Box(a);
    public static implicit operator Any(Decimal a) => Box(a);
    public static implicit operator Any(Half a) => Box(a);
    public static implicit operator Any(double a) => Box(a);
    public static implicit operator Any(float a) => Box(a);
    public static explicit operator Any(string s) => Box(s);
    public static explicit operator bool(Any a) => System<bool>.Unbox(a);
    public static explicit operator sbyte(Any a) => System<sbyte>.Unbox(a);
    public static explicit operator int(Any a) => System<int>.Unbox(a);
    public static explicit operator short(Any a) => System<short>.Unbox(a);
    public static explicit operator ushort(Any a) => System<ushort>.Unbox(a);
    public static explicit operator uint(Any a) => System<uint>.Unbox(a);
    public static explicit operator byte(Any a) => System<byte>.Unbox(a);
    public static explicit operator long(Any a) => System<long>.Unbox(a);
    public static explicit operator ulong(Any a) => System<ulong>.Unbox(a);
    public static explicit operator Decimal(Any a) => System<Decimal>.Unbox(a);
    public static explicit operator Half(Any a) => System<Half>.Unbox(a);
    public static explicit operator double(Any a) => System<double>.Unbox(a);
    public static explicit operator float(Any a) => System<float>.Unbox(a);
    public static explicit operator string(Any a) => System<string>.Unbox(a);
    public static Any Box<T>(T t) => System<T>.Box(t);
    public override string ToString() => Value.ToString();
    public override bool Equals(object o) => Value.Equals(o);
    public override int GetHashCode() => Value.GetHashCode();
    public new Type GetType() => Value.GetType();
    public void Print(TextWriter tw) => Value.Print(tw);
    public T Cast<T>() where T: IAny => (T) (IsNothing ? default : Value);
}

public interface IAny {
    public static abstract SType RuntimeType { get; }
    public SType Type { get; }
    public new Type GetType() => Type.UnderlyingType;
    protected internal virtual void Serialize(SpinorSerializer serializer) => serializer.WriteRef(0);

    public virtual void Print(TextWriter tw) {
        tw.Write(Type.Name);
        tw.Write("(");
        foreach(var f in Type.Layout.Fields) {
            tw.Write(f.Name);
            if (f.FieldType != Any.RuntimeType) {
                tw.Write("::");
                tw.Write(f.FieldType.Name);
            }
            tw.WriteLine();
        }
        tw.Write(")");
    }
    public string String() {
        StringWriter sw = new();
        Print(sw);
        return sw.ToString();
    }
}

public interface ISpinorAny : IAny {}

public interface ISystemAny : IAny{
    public object ObjectValue { get; }
    void IAny.Print(TextWriter tw) => tw.Write(ToString());
}