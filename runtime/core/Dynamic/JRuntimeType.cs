using System;
using runtime.core.Abstract;

namespace runtime.core.Dynamic
{
    public class JRuntimeType : ImplType, IJType {
        protected readonly JRuntimeModule Mod;
        protected readonly JStructType Type;

        protected override bool IsByRefImpl() => Type == JStructType.AbstractType || Type == JStructType.MutableStruct;
        protected override bool IsPrimitiveImpl() => Type == JStructType.Primitive;
        protected override bool IsValueTypeImpl() => Type == JStructType.Struct;
        
        internal JRuntimeType(Type t, JRuntimeModule m, JStructType type) : base(t){
            Mod = m;
            Type = type;
        }

        public JStructType GetStructType() => Type;
        public IJModule GetModule() => Mod;
        public string GetName() => _t.FullName;

    }
}