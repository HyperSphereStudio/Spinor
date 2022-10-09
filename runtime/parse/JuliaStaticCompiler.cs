using System;
using Antlr4.Runtime.Tree;
using HyperSphere;
using runtime.core;
using runtime.core.JIL;
using runtime.core.Runtime;

namespace runtime.parse
{
    internal class ExprFrame {
        public JILTypeBuilder Type;
        public JILModuleBuilder Module;
        public JILExprBuilder Expr;

        internal ExprFrame(JILTypeBuilder type, JILModuleBuilder module, JILExprBuilder expr) {
            Module = module;
            Expr = expr;
            Type = type;
        }
    }

    internal struct JuliaStaticCompiler {
        private JuliappParser _p;

        public JRuntimeExpr Compile(JuliappParser p, JRuntimeModule evaluationModule) {
            _p = p;
            _p.Print();
            return Compile(_p.script, evaluationModule);
        }
        
        public JILFieldBuilder CompileField(ITerminalNode nameID, JuliaParser.TypeContext type, ExprFrame frame, bool isConst, bool isGlobal) {
            var f = new JILFieldBuilder(Compile(nameID), frame.Expr);
            f.IsConst = isConst;
            f.IsGlobal = isGlobal;

            var ty = type != null ? Compile(type.Identifier()) : "Any";
            f.TypeRef = (frame.Expr as IJExpr).GetNameRef(ty);

            return f;
        }

        public void Compile(JuliaParser.BlockExprContext ctx, ExprFrame frame) {
            if (ctx.blockVariableDeclaration() != null) {
                var bctx = ctx.blockVariableDeclaration();
                CompileField(bctx.blockArg().Identifier(), bctx.blockArg().type(), 
                    frame, bctx.Const() != null, bctx.Local() == null);
            } else if (ctx.functionCall() != null) 
                Compile(ctx.functionCall(), frame);
        }

        public void Compile(JuliaParser.ModuleContext ctx, ExprFrame frame)
        {
            var mod = frame.Module.DefineModule(Compile(ctx.Identifier()));
            if (ctx.moduleExpr() != null) {
                ExprFrame frame2 = new(null, mod, mod);
                foreach(var v in ctx.moduleExpr())
                    Compile(v, frame2);
            }
            mod.Create();
        }

        public void Compile(JuliaParser.FunctionContext ctx, ExprFrame frame) {
            
        }

        public string Compile(ITerminalNode n) => n.GetText();

        public void Compile(JuliaParser.TypeNameContext ctx, out string TypeName, out IJType ExtendedTypeName) {
            TypeName = ctx.Identifier().GetText();
            if (ctx.type() != null) {
                ExtendedTypeName = null;
            }
            else ExtendedTypeName = null;
        }
        
        public void Compile(JuliaParser.StructureContext ctx, ExprFrame frame)
        {
            string name = Compile(ctx.typeName().Identifier());
            JTypeType typetype = JTypeType.None;

            if (ctx.abstractStructure() != null)
                typetype = JTypeType.Abstract;
            else if (ctx.compositeStructure() != null)
                typetype = ctx.compositeStructure().Mutable() != null ? JTypeType.Mutable : JTypeType.Struct;

            var tb = frame.Module.DefineType(name);
            tb.Type = typetype;
            frame.Type = tb;
            foreach (var item in ctx.structItem()) {
                if (item.function() != null) {
                    Compile(item.function(), frame);
                }else {
                    var sctx = item.structField();
                    tb.AddField(CompileField(sctx.blockArg().Identifier(), sctx.blockArg().type(), frame, false, false));
                }
            }

            frame.Expr.Code.InstantiateType(tb);
            
            frame.Type = null;
        }
        
        public void Compile(JuliaParser.UsingModuleContext ctx, ExprFrame frame) {
            var ids = ctx.moduleRef().Identifier();
            var str = ctx.moduleRef().GetText().Substring(5);
            frame.Module.AddName(str, true, false);
        }
        
        public object Compile(JuliaParser.FunctionCallContext ctx, ExprFrame frame) => null;
        public void Compile(JuliaParser.ModuleExprContext ctx, ExprFrame frame) {
            if (ctx.usingModule() != null)
                Compile(ctx.usingModule(), frame);
            else if (ctx.module() != null)
                Compile(ctx.module(), frame);
            else if (ctx.blockExpr() != null)
                Compile(ctx.blockExpr(), frame);
            else if (ctx.structure() != null)
                Compile(ctx.structure(), frame);
            else if (ctx.moduleVariableDeclaration() != null) {
                var mvd = ctx.moduleVariableDeclaration();
                CompileField(mvd.blockArg().Identifier(), mvd.blockArg().type(), frame, mvd.Const() != null, mvd.Local() == null);
            }
        }
        
        public JRuntimeExpr Compile(JuliaParser.ScriptContext ctx, JRuntimeModule evaluationModule) {
            JILBuilder eb = new(evaluationModule);
            ExprFrame frame = new(null, eb, eb);
            
            if (ctx.moduleExpr() != null)
                Compile(ctx.moduleExpr(), frame);
            else {
                foreach(var v in ctx.moduleExprStatement())
                    Compile(v.moduleExpr(), frame);
            }

            return eb.CreateExpression();
        }
    }
}