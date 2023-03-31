/*
   * Author : Johnathan Bizzano
   * Created : Thursday, March 16, 2023
   * Last Modified : Thursday, March 16, 2023
*/

using System;
using System.Linq;
using System.Reflection;
using System.Text;
using Core;
using runtime.core.internals;
using runtime.ILCompiler;
using runtime.stdlib;

namespace runtime.core.function;

public readonly record struct MethodSignature(SType[] Args) {
    public readonly ushort Specificity = (ushort) Args.Aggregate(0, (i, type) => type.Specificity + i);

    public bool Match(MethodSignature sig) {
        for (var i = 0; i < Args.Length; i++) {
            if (!sig.Args[i].IsAssignableTo(Args[i]))
                return false;
        }
        return true;
    }

    public override string ToString() => "(" + string.Join(", ", Args.GetEnumerator()) + ")";
}

public record Method(MethodSignature Signature, SType ReturnType, MethodInfo InternalMethod){
    public Any Invoke(params Any[] values) {
        var calling = new object[values.Length];
        for (var i = 0; i < values.Length; i++)
            calling[i] = UnboxObjectForMethodCall(Signature.Args[i], values[i]);
        return Spinor.Box(InternalMethod.Invoke(null, calling));
    } 
    
   public static object UnboxObjectForMethodCall(SType methodCallParameter, Any a) => methodCallParameter == Any.RuntimeType ? a.Value : a;
   public object DirectInvoke(params object[] values) => InternalMethod.Invoke(null, values);
   public void Call(IlExprBuilder exb) => exb.Function.Invoke(InternalMethod);
}