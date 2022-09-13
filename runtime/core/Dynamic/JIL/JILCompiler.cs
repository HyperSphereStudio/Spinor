using runtime.core.Abstract;
using runtime.core.Static;
using runtime.ILCompiler;

namespace runtime.core.Dynamic.JIL
{
    public delegate object JuliaExpression();
    
    public struct JILCompiler {
        public const string JModuleHandleName = "__JMODULE__";
        public const string JTypeHandleName = "__JTYPE__";

        public readonly JFunction Function;
        public readonly IlExprBuilder Builder;
        

        public JILCompiler(JFunction function, IlExprBuilder builder) {
            Builder = builder;
            Function = function;
            
        }
        
        private void CreateJModuleOf(JRuntimeModule m) => Builder.Load.FieldValue(m.GetField(JModuleHandleName));
        private void CreateJTypeOf(JRuntimeType ty) => Builder.Load.FieldValue(ty.GetField(JTypeHandleName));

        public JuliaExpression Compile() {
            
            return null;
        }
    }
}