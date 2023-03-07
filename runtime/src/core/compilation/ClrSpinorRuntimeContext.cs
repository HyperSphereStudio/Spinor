/*
   * Author : Johnathan Bizzano
   * Created : Friday, January 6, 2023
   * Last Modified : Friday, January 6, 2023
*/

namespace runtime.core.CLR;

public class ClrSpinorRuntimeContext : SpinorRuntimeContext {
   
   public override unsafe int PointerSize => sizeof(void*);
   protected internal override void Initialize() {}
   protected override void Destroy(){}

   public ClrSpinorRuntimeContext(string name, string version) : base(name, version) {
      
   }
}