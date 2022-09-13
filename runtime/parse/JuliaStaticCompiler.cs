using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Antlr4.Runtime.Tree;
using HyperSphere;
using runtime.core;
using runtime.core.Abstract;
using runtime.core.Dynamic;
using runtime.core.Dynamic.JIL;
using runtime.core.Static;
using runtime.ILCompiler;
using runtime.interop;

namespace runtime.parse
{
/*
    internal class ExprFrame
    {
        public JTypeBuilder Expr;
        public JRuntimeModule Module;
        public bool LastExpr;
        public Dictionary<string, LocalBuilder> Locals = new();

        internal ExprFrame(JTypeBuilder expr, JRuntimeModule module, bool isLastExpr) {
            LastExpr = isLastExpr;
            Module = module;
            Expr = expr;
        }
    }
    
    
    internal class JuliaStaticCompiler {
        private JuliappParser _p;
        private JExecutionModule _e;

        internal JuliaStaticCompiler(JExecutionModule e) => _e = e;

        public JFunction Compile(JuliappParser p) {
            _p = p;
            _p.Print();
            return Compile(_p.script);
        }

        public void Compile(JuliaParser.ModuleVariableDeclarationContext ctx, ExprFrame frame) {
            bool isConst = ctx.Const() != null;
            bool isGlobal = ctx.Local() == null;
            var name = Compile(ctx.blockArg().Identifier());
            Type type = ctx.blockArg().type() == null ? typeof(JObject) : FindType(Compile(ctx.blockArg().type().Identifier()), frame.Module);
            
            if (isGlobal)
                frame.Module.CreateGlobal(name, type, isConst);
            else
                frame.Locals.Add(name, frame.Expr.Create.Local(type));
        }

        public void Compile(JuliaParser.BlockVariableDeclarationContext ctx, ExprFrame frame) {
            
        }

        public void Compile(JuliaParser.BlockExprContext ctx, ExprFrame frame) {
            if (ctx.blockVariableDeclaration() != null) 
                Compile(ctx.blockVariableDeclaration(), frame);
            else if (ctx.functionCall() != null) 
                Compile(ctx.functionCall(), frame);
        }

        public void Compile(JuliaParser.StructFieldContext ctx, ILTypeBuilder b, ExprFrame frame) {
            var name = Compile(ctx.blockArg().Identifier());
            var ty = FindType(Compile(ctx.blockArg().type().Identifier()), b._m);
            b.CreateField(name, ty, ctx.Const() != null);
        }

        public void Compile(JuliaParser.ModuleContext ctx, ExprFrame frame) {
            var modName = Compile(ctx.Identifier());
            JModule m = new(frame.Module, modName);
            
            ILTypeBuilder modBuilder = frame.Module.CreateType(modName, JStructType.Module, null);
            var modExpr = modBuilder.TypeInitializer;
        //    Bootstrap.CreateModuleTypeHandle(modExpr, modBuilder, frame.Module, modName);
            
            if (ctx.moduleExpr() != null) {
                var arr = ctx.moduleExpr();
                ExprFrame frame2 = new(modExpr, m, true);
                for (int i = 0, n = arr.Length, e = n - 1; i < n; i++) {
                    frame2.LastExpr = i == e;
                    Compile(arr[i], frame2);
                }
            }
            
            m.InternalType = modBuilder.Create();
        }

        public void Compile(JuliaParser.FunctionContext ctx, ILTypeBuilder type, ExprFrame frame) {
         
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
            bool isLastExpr = frame.LastExpr;
            frame.LastExpr = false;
            Compile(ctx.typeName(), out string name, out JlType extendedType);
            if (frame.Module.ContainsName(name))
                 throw new JuliaException("Module Already Contains Name:" + name);
            JStructType type = JStructType.Invalid;
           
            if (ctx.abstractStructure() != null)
                type = JStructType.AbstractType;
            else if (ctx.implementedStructure() != null)
                type = ctx.implementedStructure().Mutable() != null ? JStructType.MutableStruct : JStructType.Struct;
           
            var b = frame.Module.CreateType(name, type, extendedType);
            Bootstrap.CreateTypeHandle(b.TypeInitializer, b, frame.Module, name);
            
            foreach (var item in ctx.structItem()) {
                if (item.function() != null) {
                    Compile(item.function(), b, frame);
                }else
                    Compile(item.structField(), b, frame);
            }
            
            var ty = b.Create();
            Bootstrap.CreateGlobalDefinitionCall(frame.Expr, frame.Module, name, ty, true);

            if (isLastExpr) {
                Bootstrap.CreateJTypeOf(frame.Expr, ty);
                frame.Expr.Return();
                var generator = new Lokad.ILPack.AssemblyGenerator();
                generator.GenerateAssembly(Julia.MAIN._M.Assembly, "test.dll");
            }
        }
        public void Compile(JuliaParser.UsingModuleContext ctx, ExprFrame frame) {
            var ids = ctx.moduleRef().Identifier();
            var str = ctx.moduleRef().GetText().Substring(5);
            frame.Module.UseModule(JModule.GetModule(str, ids.Select(x => x.GetText()).ToArray()));
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
            else if (ctx.moduleVariableDeclaration() != null)
                Compile(ctx.moduleVariableDeclaration(), frame);
        }
        
        public JFunction Compile(JuliaParser.ScriptContext ctx) {
            
            ExprFrame frame = new(expr, _e, true);
            
            if (ctx.moduleExpr() != null)
                Compile(ctx.moduleExpr(), frame);
            else {
                var arr = ctx.moduleExprStatement();
                for (int i = 0, n = arr.Length, e = n - 1; i < n; i++) {
                    frame.LastExpr = i == e;
                    Compile(arr[i].moduleExpr(), frame);
                }
            }
            
            return dynMet.CreateDelegate<JuliaExpression>();
        }

        public Type FindType(string name, JModule m) {
            name = name.Contains(".") ? name.Substring(name.LastIndexOf(".")) : name;
            if (m.GetType(name, out Type t))
                return t;
            
            Type g = Type.GetType(name);

            if (g != null)
                return g;
            
            throw new JuliaException("Unable To Find Type \"" + name + "\"");
        }
    }*/
}