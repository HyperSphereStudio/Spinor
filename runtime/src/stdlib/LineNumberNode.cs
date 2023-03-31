/*
   * Author : Johnathan Bizzano
   * Created : Monday, January 2, 2023
   * Last Modified : Monday, January 2, 2023
*/

using System;
using System.IO;
using runtime.core.internals;

namespace runtime.stdlib;

public sealed record LineNumberNode(int Line, string File) : ISpinorAny {
   public static SType RuntimeType { get; } = 
      Spinor.Core.InitializeStructType(SpinorTypeAttributes.Class, "LineNumberNode", 
         Any.RuntimeType, 1, null, 
         new TypeLayout(0, typeof(LineNumberNode), new SpinorField[2], null, null));
   
   public SType Type => RuntimeType;
   public override string ToString() => "#= " + File + ":" + Line + " =#";
   void IAny.Print(TextWriter tw) {
      tw.Write("#= ");
      tw.Write(File);
      tw.Write(":");
      tw.Write(Line);
      tw.Write(" =#");
   }
   
   public static implicit operator Any(LineNumberNode s) => new(s);
   public static implicit operator LineNumberNode(Any a) => a.Cast<LineNumberNode>();
}