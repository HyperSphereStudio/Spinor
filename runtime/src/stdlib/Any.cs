/*
   * Author : Johnathan Bizzano
   * Created : Monday, January 2, 2023
   * Last Modified : Monday, January 2, 2023
*/

using runtime.core;
using runtime.core.memory;
using runtime.core.type;

namespace Core;

public interface Any{
    public static AbstractType RuntimeType { get; internal set; }
    public SType Type { get; }
    protected internal virtual void Serialize(SpinorSerializer serializer) => serializer.WriteRef(this);
}

public interface IAny : Any {
    public new static abstract SType RuntimeType { get; }
}

public interface IAny<TConcrete> : IAny where TConcrete: IAny {
    SType Any.Type => TConcrete.RuntimeType;
}

public struct TBool : IAny {
    public readonly sbyte Value;
    public TBool(sbyte value) => Value = value;
    public static SType RuntimeType { get; }
    public SType Type { get; }
}
