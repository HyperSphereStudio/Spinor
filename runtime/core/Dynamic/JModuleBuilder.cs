using System;
using System.Collections.Generic;
using System.Reflection;
using runtime.core.Abstract;
using runtime.ILCompiler;

namespace runtime.core.Dynamic
{
    public class JModuleBuilder : JRuntimeModule {
        protected readonly Dictionary<string, JTypeBuilder> CompilingTypes = new(); 
        
        public JModuleBuilder(JRuntimeModule parentModule, string name) : base(parentModule, name) {}
        
        public JTypeBuilder CreateType(string name, JStructType type, IJType extendedType) {
            Type parentType = typeof(object);
            TypeAttributes attribs = TypeAttributes.Public;
            if (type == JStructType.Struct) {
                if (extendedType != null && extendedType.GetStructType() == JStructType.AbstractType)
                    attribs |= TypeAttributes.Abstract;
                else
                {
                    attribs |= TypeAttributes.SequentialLayout;
                    parentType = typeof(object);
                }
            }
            else if (type == JStructType.MutableStruct || type == JStructType.__EModule__)
                attribs |= TypeAttributes.Class;
            else if (type == JStructType.AbstractType)
                attribs |= TypeAttributes.Abstract;

            var ty = new JTypeBuilder(new ILTypeBuilder(BaseModule._M.DefineType(FullName + "." + name, attribs, parentType)), this, type);
            CompilingTypes.Add(name, ty);
            return ty;
        }
        
        
    }
}