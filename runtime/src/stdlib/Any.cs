/*
   * Author : Johnathan Bizzano
   * Created : Monday, January 2, 2023
   * Last Modified : Monday, January 2, 2023
*/

using System.IO;
using runtime.core;
using runtime.core.Utils;

namespace Core;

public interface Any{
    public Type Type { get; }
    public bool Isa(Type t) => t.Isa(this);
    public bool EqualTo(object o);
    public int Hash();
    public string String();
    public void Print(TextWriter tw);
}

public abstract class SystemAny : Any{
    public abstract Type Type { get; }
    public bool EqualTo(object o) => Equals(o);
    public int Hash() => GetHashCode();
    public string String() => ToString();
    public void Print(TextWriter tw) => tw.Write(String());
}

public abstract class SystemPrimitiveAny : SystemAny {
    public object ObjectValue;
    public object Unbox() => ObjectValue;
    public abstract T Unbox<T>();
    public abstract System.Type UnderlyingType { get; }
    
    public static implicit operator bool(SystemPrimitiveAny a) => ((SystemPrimitiveAny<bool>) a).Value;
    public static implicit operator int(SystemPrimitiveAny a) => ((SystemPrimitiveAny<int>) a).Value;
    public static implicit operator long(SystemPrimitiveAny a) => ((SystemPrimitiveAny<long>) a).Value;
    public static implicit operator float(SystemPrimitiveAny a) => ((SystemPrimitiveAny<float>) a).Value;
    public static implicit operator double(SystemPrimitiveAny a) => ((SystemPrimitiveAny<double>) a).Value;
}

public class SystemPrimitiveAny<T> : SystemPrimitiveAny {
    public T Value;
    
    public override Type Type => Value == null ? Types.Nothing : SystemType.GetSystemType(Value.GetType());
    public override T1 Unbox<T1>() {
        if (Value is T1 t)
            return t;
        return (T1) ObjectValue;
    }

    public override System.Type UnderlyingType => typeof(T);
    public SystemPrimitiveAny(T v) => Value = v;
    public override string ToString() => Value.ToString();
    public override bool Equals(object o) => o is SystemPrimitiveAny s ? Value.Equals(s.ObjectValue) : Value.Equals(o);
    public override int GetHashCode() => Value.GetHashCode();
}