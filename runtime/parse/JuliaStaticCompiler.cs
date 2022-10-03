using System.Linq;
using Antlr4.Runtime.Tree;
using HyperSphere;
using runtime.core;
using runtime.core.JIL;
using runtime.ILCompiler;

namespace runtime.parse
{
    internal class ExprFrame {
        public IJType Type;
        public IJModule Module;
        public IJExpr Expr;

        internal ExprFrame(IJType type, IJModule module, IJExpr expr) {
            Module = module;
            Expr = expr;
            Type = type;
        }
    }

    internal class JuliaStaticCompiler {
        private JuliappParser _p;

        public JILModuleExpr Compile(JuliappParser p, IJModule evaluationModule) {
            _p = p;
            _p.Print();
            return Compile(_p.script, evaluationModule);
        }

        public IJType Compile(JuliaParser.TypeContext ctx, ExprFrame frame) {
            frame.Expr.GetName(Compile(ctx.Identifier()), out IJType result, true);
            return result;
        }

        public void CompileField(ITerminalNode nameID, JuliaParser.TypeContext type, ExprFrame frame, bool isConst, bool isGlobal) {
            IJField f = new JILFieldBuilder();
            f.Name = Compile(nameID);
            f.IsConst = isConst;
            f.IsGlobal = isGlobal;
            f.Type = Compile(type, frame);
            ((JILExprBuilder) frame.Expr).AddVariable(f);
        }

        public void Compile(JuliaParser.BlockExprContext ctx, ExprFrame frame) {
            if (ctx.blockVariableDeclaration() != null) {
                var bctx = ctx.blockVariableDeclaration();
                CompileField(bctx.blockArg().Identifier(), bctx.blockArg().type(), 
                    frame, bctx.Const() != null, bctx.Local() == null);
            } else if (ctx.functionCall() != null) 
                Compile(ctx.functionCall(), frame);
        }

        public void Compile(JuliaParser.ModuleContext ctx, ExprFrame frame) {
            var mod = new JILModuleBuilder();
            mod.Name = Compile(ctx.Identifier());
            mod.ParentModule = frame.Module;
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
            else if (ctx.implementedStructure() != null)
                typetype = ctx.implementedStructure().Mutable() != null ? JTypeType.Mutable : JTypeType.Struct;

            IJType tb = new JILTypeBuilder();
            tb.Type = typetype;
            tb.Name = name;
            frame.Type = tb;
            foreach (var item in ctx.structItem()) {
                if (item.function() != null) {
                    Compile(item.function(), frame);
                }else {
                    var sctx = item.structField();
                    CompileField(sctx.blockArg().Identifier(), sctx.blockArg().type(), frame, false, false);
                }
            }
            frame.Type = null;
        }
        public void Compile(JuliaParser.UsingModuleContext ctx, ExprFrame frame) {
            var ids = ctx.moduleRef().Identifier();
            var str = ctx.moduleRef().GetText().Substring(5);
            ModuleReference r = new();
            r.Name = str;
            ((JILExprBuilder) frame.Expr).AddReference(r);
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
        
        public JILModuleExpr Compile(JuliaParser.ScriptContext ctx, IJModule evaluationModule) {
            JILModuleExprBuilder eb = new JILModuleExprBuilder();
            ExprFrame frame = new(null, evaluationModule, eb);
            
            if (ctx.moduleExpr() != null)
                Compile(ctx.moduleExpr(), frame);
            else {
                foreach(var v in ctx.moduleExprStatement())
                    Compile(v.moduleExpr(), frame);
            }
            
            return eb.Create();
        }
    }
}