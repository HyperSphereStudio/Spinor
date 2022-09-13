using System;
using runtime.core.Abstract;
using runtime.ILCompiler;

namespace runtime.core.Dynamic
{
    public class JTypeBuilder : JRuntimeType{
        private readonly string _name;
        private readonly Type ExtendedType;
        public readonly ILTypeBuilder Builder;
        
        public override string Name => _name;
        public override Type BaseType => ExtendedType;

        internal JTypeBuilder(ILTypeBuilder b, JRuntimeModule m, JStructType type) : base(b.InternalBuilder, m, type) => Builder = b;
        
        public JRuntimeType Create() => new(Builder.Create(), Mod, Type);
    }
}