/*
   * Author : Johnathan Bizzano
   * Created : Friday, January 6, 2023
   * Last Modified : Friday, January 6, 2023
*/

using System.Collections.Immutable;
using System.Reflection.Emit;
using Core;

namespace runtime.core;

public abstract class SpinorContext {
   public abstract int PointerSize { get; }
   protected internal abstract void Initialize();
   protected abstract void Destroy();
   ~SpinorContext() => Destroy();
}

public abstract class SpinorRuntimeContext : SpinorContext{
   public ModuleBuilder ModuleBuilder { get; }
   public void Save(string path) => new Lokad.ILPack.AssemblyGenerator().GenerateAssembly(ModuleBuilder.Assembly, path);
   
   internal SpinorRuntimeContext() {
      var asm = AssemblyBuilder.DefineDynamicAssembly(new("SPINOR_RUNTIME"), AssemblyBuilderAccess.Run);
      ModuleBuilder = asm.DefineDynamicModule("SPINOR_RUNTIME_MODULE");
   }
   
   
}

public abstract class SpinorCompiledContext : SpinorContext {}