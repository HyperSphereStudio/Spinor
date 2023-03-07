/*
   * Author : Johnathan Bizzano
   * Created : Monday, January 2, 2023
   * Last Modified : Monday, January 2, 2023
*/

using System.IO;

namespace Core;

public sealed record LineNumberNode(int Line, string File) : IAny {
   public static SType RuntimeType { get; set; }
   public SType Type => RuntimeType;

   public override string ToString() => "#= " + File + ":" + Line + " =#";
   public void Print(TextWriter tw) {
      tw.Write("#= ");
      tw.Write(File);
      tw.Write(":");
      tw.Write(Line);
      tw.Write(" =#");
   }

   
}