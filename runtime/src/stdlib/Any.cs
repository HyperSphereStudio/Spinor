/*
   * Author : Johnathan Bizzano
   * Created : Monday, January 2, 2023
   * Last Modified : Monday, January 2, 2023
*/

using runtime.core.memory;
using runtime.core.type;

namespace Core;

public interface Any{
    public static AbstractType RuntimeType { get; internal set; }
    public SType Type { get; }
    protected internal virtual void Serialize(SpinorSerializer serializer) => serializer.WriteRef(this);
}

public interface IAny : Any{
    public new static abstract SType RuntimeType { get; }
}