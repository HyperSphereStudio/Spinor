using System;
using runtime.core.Abstract;

namespace runtime.core.Dynamic.JIL
{
    public struct JILString{
        private readonly int _tableIndex;
        public JILString(JILWritingContext ctx, string s) => _tableIndex = ctx.LoadString(s);
        public string Value(JILReadingContext ctx) => ctx.GetString(_tableIndex);
    }

    public struct JILSVec<T> where T: unmanaged{
        private int _tableIndex;
        public int Length;

        public unsafe T this[JILReadingContext ctx, int v] {
            get => ((T*)ctx.LoadDynamic(_tableIndex))[v];
            set => ((T*)ctx.LoadDynamic(_tableIndex))[v] = value;
        }
    }

    public struct JILTypeRef{
        private readonly int _tableIndex;
        public JILTypeRef(JILWritingContext ctx, IJType t) => _tableIndex = ctx.LoadType(t);
        public IJType Value(JILReadingContext ctx) => ctx.GetType(_tableIndex);
    }
    
    public struct JILModuleRef{
        private readonly int _tableIndex;
        public JILModuleRef(JILWritingContext ctx, IJType t) => _tableIndex = ctx.LoadType(t);
        public IJType Value(JILReadingContext ctx) => ctx.GetType(_tableIndex);
    }
    
    public struct JILExpression{
        private readonly int _tableIndex;
        public JILExpression(JILWritingContext ctx, int offset) => _tableIndex = offset;
        public IntPtr Value(JILReadingContext ctx) => ctx.LoadDynamic(_tableIndex);
        public unsafe JOp GetExpressionType(JILReadingContext ctx) => *(JOp*) Value(ctx);
    }
    
    public struct JILVar{
        public JILString Name;
        public JILTypeRef Type;
        public JVarFlags Flags;

        public bool IsConst => (Flags & JVarFlags.Const) != 0;
        public bool IsGlobal => (Flags & JVarFlags.Global) != 0;
        public bool IsLocal => !IsGlobal;
    }
    
    public struct JILStruct{
        public JILString Name;
        public JStructType Type;
        public JILSVec<JILVar> Fields;
    }

    public struct JILUsing{
        public JILString Name;
    }

    public struct JILInvoke{
        public JILString Function;
        public JILSVec<JILExpression> Arguments;
    }

    public struct JILBlock{
        public JILSVec<JILVar> Variables;
        public JILSVec<JILExpression> Statements;
    }

    public struct JILFunction{
        public JILString Name;
        public JILBlock Block;
        public JILTypeRef ReturnType;
        public JILSVec<JILTypeRef> ParameterTypes;
    }

    public struct JILModule{
        public JILString Name;
        public bool IsBare;
        public JILModuleRef ParentModule;
        public JILSVec<JILExpression> Expressions;
    }

    public struct JILReturn{
        public JILExpression ReturnExpr;
    }

    public struct JILAssign {
        public JILVar Var;
        public JILExpression Expr;
    }

    public struct JILTypeOf { public JILTypeRef Type; }
    public struct JILModuleOf { public JILModuleRef Module; }
    
}