/*
   * Author : Johnathan Bizzano
   * Created : Sunday, March 26, 2023
   * Last Modified : Sunday, March 26, 2023
*/

using System;
using runtime.core.internals;
using runtime.stdlib;

namespace runtime.core.type;
using static SpinorTypeAttributes;

public class VarArg : SType, ISpinorAny {
   public new static SType RuntimeType { get; } = 
      Spinor.Core.InitializeStructType(Class|Concrete, "VarArg", 
         SType.RuntimeType, 1, null,
         new TypeLayout(0, typeof(SystemTuple), null, null, null));
   
   public VarArg(Symbol name, SType super, Module module, 
      SpinorTypeAttributes attributes, int specificity, TypeLayout layout) : 
      base(name, super, module, attributes, specificity, layout) {}
}