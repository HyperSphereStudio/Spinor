/*
   * Author : Johnathan Bizzano
   * Created : Thursday, March 23, 2023
   * Last Modified : Thursday, March 23, 2023
*/

using runtime.core.internals;

namespace Core;

internal class Nothing {
   public override string ToString() => "nothing";
   public override bool Equals(object o) => o == null || ReferenceEquals(o, Spinor.Nothing.Value);
   public override int GetHashCode() => 0;
}