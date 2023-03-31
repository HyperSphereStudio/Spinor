/*
   * Author : Johnathan Bizzano
   * Created : Friday, March 17, 2023
   * Last Modified : Friday, March 17, 2023
*/

using System;
using System.Reflection;
using runtime.core.internals;
using runtime.stdlib;
using runtime.Utils;
using Module = runtime.stdlib.Module;

namespace runtime.core.type;
using static SpinorTypeAttributes;

public class System : SType, ISpinorAny {
   public new static SType RuntimeType { get; } = 
      Spinor.Core.InitializeStructType(Class|Concrete, "System", 
         SType.RuntimeType, 2, null, new TypeLayout(0, typeof(System), null, null, null));
   
   private MethodInfo _getRuntimeTypeMethod;
   public override MethodInfo GetRuntimeTypeMethod => _getRuntimeTypeMethod;
   
   private System(Symbol name, 
                    SType super,
                    Module module, 
                    SpinorTypeAttributes attributes, 
                    int specificity, TypeLayout layout) : 
         base(name, super, module, attributes, specificity, layout){}

   internal static SType CreateSystemType<T>() {
      var name = (Symbol) typeof(T).Name;
      SpinorTypeAttributes at = 0;
      
      if (typeof(T).IsClass)
         at |= Class;
      
      if (typeof(T).IsUnmanaged())
         at |= Unmanaged;

      if (typeof(T).IsValueType || (at.HasFlag(Class) && !typeof(T).IsAbstract))
         at |= Concrete;
      
      var f = typeof(T).GetFields(BindingFlags.Public|BindingFlags.Instance);
      var fields = new SpinorField[f.Length];
      var p = typeof(T).GetProperties();
      var properties = new SpinorProperty[p.Length];
      
      var sys = new System(name, null, Spinor.Root, at, 1, 
         new TypeLayout(0, typeof(T), fields, properties, null));
      
      //Delay Field Initialization to prevent stack overflow
      for (var i = 0; i < fields.Length; i++)
         fields[i] = new(f[i]);

      for (var i = 0; i < properties.Length; i++)
         properties[i] = new(p[i]);

      sys._getRuntimeTypeMethod = typeof(System<T>).GetMethod("get_RuntimeType");
      
      return sys;
   }
}

public static class System<T> {
   public static SType RuntimeType { get; } = System.CreateSystemType<T>();
   public static Func<T, Any> Box { get; private set; } = t => new SystemValue<T>(t);
   public static T Unbox(Any a) => ((SystemValue<T>) a.Value).Value;
}

public readonly struct SystemValue<T> : ISystemAny {
   public static SType RuntimeType => System<T>.RuntimeType;
   public SType Type => System<T>.RuntimeType;
   public readonly T Value;

   internal SystemValue(T value) => Value = value;
   public object ObjectValue => Value;
   Type IAny.GetType() => typeof(T);
   public override string ToString() => Value.ToString();
   public override int GetHashCode() => Value.GetHashCode();
   public override bool Equals(object o) => Value.Equals(o);

   public static implicit operator Any(SystemValue<T> v) => new(v);
   public static implicit operator SystemValue<T>(Any v) => (SystemValue<T>) v.Value;
   public static implicit operator T(SystemValue<T> v) => v.Value;
   public static implicit operator SystemValue<T>(T v) => new(v);
}