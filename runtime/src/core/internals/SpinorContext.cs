/*
   * Author : Johnathan Bizzano
   * Created : Friday, January 6, 2023
   * Last Modified : Friday, January 6, 2023
*/

using System;
using System.Collections.Immutable;
using System.Reflection;
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
   public void Save() {
      Spinor.Root.Initialize();
      var asm = new Lokad.ILPack.AssemblyGenerator();
      asm.GenerateAssembly(ModuleBuilder.Assembly, new[]{ModuleBuilder.Assembly}, $"{ModuleBuilder.Assembly.GetName().Name}.dll");
   }
   
   internal SpinorRuntimeContext(string name, string version) {
      var asmName = new AssemblyName($"{name}, Version={version}");
      var asm = AssemblyBuilder.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.Run);
      ModuleBuilder = asm.DefineDynamicModule("SPINOR_RUNTIME_MODULE");
   }
}

public abstract class SpinorCompiledContext : SpinorContext {}