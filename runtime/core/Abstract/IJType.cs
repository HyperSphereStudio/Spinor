using System;

namespace runtime.core.Abstract
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class JuliaTypeAttribute : Attribute{}
    
    public enum JStructType : byte {
        MutableStruct,
        Struct,
        AbstractType,
        Primitive,
        __EModule__, //Do not use. For Internal Usage,
        Invalid
    }
    
    public interface IJType
    {
        public JStructType GetStructType();
        public IJModule GetModule();
        public string GetName();
    }
}