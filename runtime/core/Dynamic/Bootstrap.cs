using System;
using System.Collections.Generic;
using System.Reflection;
using runtime.core.Abstract;
using runtime.ILCompiler;

namespace runtime.core.Dynamic
{
    public class Bootstrap
    {
        public static readonly string JModuleHandleName = "__JMODULE__";
        public static readonly string JTypeHandleName = "__JTYPE__";
        
        internal const FieldAttributes BootStrapFlags = FieldAttributes.InitOnly | FieldAttributes.Static | FieldAttributes.Public;
        
        internal static readonly MethodInfo
            InstantiateGlobalCreator = typeof(Bootstrap).GetMethod("Instantiate", BindingFlags.NonPublic | BindingFlags.Static),
            GetExecutionModule = SharpReflect.GetMethod<JExecutionModule, string>("GetExecutionModule"),
            GetDictionaryIndexForJModules = typeof(Dictionary<string, Type>).GetMethod("get_Item");
        internal static readonly FieldInfo JModulesTypesField = SharpReflect.GetField<IJModule>("Types");
        
        internal static JGlobalHandle<T> Instantiate<T>(JRuntimeModule m, string name, bool isConst) => m.CreateGlobal<T>(name, isConst).GetHandle<T>();
        
      

        internal static FieldInfo CreateExecutionModuleTypeHandle(IlExprBuilder b, ILTypeBuilder modBuilder, string name) {
            var modHandle = modBuilder.CreateFieldImpl(JModuleHandleName, typeof(JExecutionModule), BootStrapFlags);
            b.Load.String(name);
            b.Function.Invoke(GetExecutionModule);
            b.Store.Field(modHandle);
            return modHandle;
        }
        
        
    }
}