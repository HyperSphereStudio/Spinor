/*
   * Author : Johnathan Bizzano
   * Created : Monday, January 2, 2023
   * Last Modified : Monday, January 2, 2023
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Core;
using runtime.core.Compilation;
using runtime.ILCompiler;
using runtime.stdlib;
using Module = Core.Module;

namespace runtime.core.compilation;

#region Slots
public interface ISlot {
    public Symbol Name { get; }
    public SType SType { get; }
}

public readonly record struct SlotAllocation(Symbol Name, SType SType) : ISlot {
    public System.Type UnderlyingType => SType.UnderlyingType;
}

public sealed record Local(Symbol Name, SType SType, LocalBuilder Builder) : ISlot {
    public System.Type UnderlyingType => Builder.LocalType;

    public void Print(TextWriter tw) {
        tw.Write("loc:");
        tw.Write(Name);
        tw.Write("::");
        tw.Write(SType);
    }
}

public sealed record Parameter(Symbol Name, SType SType, Type UnderlyingType, int Index) : ISlot {
    public void Print(TextWriter tw) {
        tw.Write("param:");
        tw.Write(Name);
        tw.Write("::");
        tw.Write(SType);
    }
}

#endregion
#region Frames
public readonly record struct LocalFrame() {
    private readonly Dictionary<Symbol, Local> _locals = new();
    public bool GetLocal(Symbol name, out Local v) => _locals.TryGetValue(name, out v);
    public void SetLocal(Symbol name, Local v) => _locals.Add(name, v);
}

public record MethodFrame(Module Module, SType ReturnSType, IlExprBuilder Expr, params SlotAllocation[] ParameterTypes) {
    public readonly Stack<LocalFrame> LocalFrames = new();
    public SType LastSType { get; internal set; }
    public LocalFrame Frame => LocalFrames.Peek();
    
    public LocalFrame PushLocalFrame() {
        var lf = new LocalFrame();
        LocalFrames.Push(lf);
        return lf;
    }
    
    public LocalFrame PopLocalFrame() => LocalFrames.Pop();

    public bool FindLocalName(Symbol name, out Local p) {
        foreach (var lk in LocalFrames) {
            if (!lk.GetLocal(name, out var l))
                continue;
            p = l;
            return true;
        }
        p = default;
        return false;
    }       
}

public class CompilerFrames{
    public readonly Stack<MethodFrame> Frames = new();
    public LocalFrame LocalFrame => Frame.Frame;
    public MethodFrame Frame { get; private set; }
    public Module Module => Frame.Module;

    public void PushFrame(MethodFrame f) => Frames.Push(f);
    public MethodFrame PopFrame() => Frames.Pop();
    public LocalFrame PushLocalFrame() => Frame.PushLocalFrame();
    protected LocalFrame PopLocalFrame() => Frame.PopLocalFrame();
}
#endregion

public class InstanceExprCompiler<TAny, TExpr> : ISpinorExprWalker<TAny, TExpr> where TExpr : IExpr<TAny> {
    private readonly ISpinorExprWalker<TAny, TExpr> _exprReader;
    private readonly CompilerFrames _compilerFrame = new();
    
    protected SpinorRuntimeContext RuntimeContext { get; private set; }
    protected Module Module => _compilerFrame.Module;
    protected MethodFrame MethodFrame => _compilerFrame.Frame;
    protected LocalFrame LocalFrame => _compilerFrame.LocalFrame;
    public IlExprBuilder Expr => MethodFrame.Expr;
    private ISpinorExprWalker<TAny, TExpr> This => this;
    public SType LastSType => MethodFrame.LastSType;
    
    internal InstanceExprCompiler(ISpinorExprWalker<TAny, TExpr> exprReader) => _exprReader = exprReader;
    
    #region Impl
    protected void Push(bool b) => Expr.Load.Bool(b);
    protected void Push(long l) => Expr.Load.Int64(l);
    protected void Push(int i) => Expr.Load.Int32(i);
    protected void Push(double d) => Expr.Load.Float64(d);
    protected void Push(Local local) => Expr.Load.Local(local.Builder);

    protected void Push(FieldInfo fi) => Expr.Load.FieldValue(fi);
    protected void LoadArrayElement(ISlot ty) => Expr.Array.LoadElement(ty.SType.UnderlyingType);
    protected void Invoke(MethodInfo m) => Expr.Function.Invoke(m);
    protected Local CreateLocal(SlotAllocation local, bool isFixed) => new (local.Name, local.SType, Expr.Create.Local(local.UnderlyingType, isFixed));
    protected void Push(Parameter local) => Expr.Load.Arg(local.Index);
    protected void Store(Local local) => Expr.Store.Local(local.Builder);
    protected void Store(FieldInfo fi) => Expr.Store.Field(fi);

    public void WalkLineNumberNode(int line, string file) {}
    #endregion
    
    protected void PushConstObject(Any a) {
        
    }

    protected SlotAllocation CreateParam(Symbol name, SType ty) => new(name, ty);

    protected virtual void Push(object o) {
        switch (o) {
            case Local l:
                Push(l);
                break;
            case Parameter p:
                Push(p);
                break;
            case TAny a:
                This.Walk(a, false);
                break;
            default:
                throw new SpinorException("Unable to Push {0}", o);
        }
    }
    
    public DynamicMethod CompileTopLevelMethod(TAny a, Module m, out Type returnType, params SType[] types) {
        //Itll take in a pointer as last arg that will be passed by the interpreter as final value due to unknown return type (interpreter knows the type on invocation though).
        Type[] tys = new Type[types.Length + 1];
        for (int i = 0; i < tys.Length; i++)
            tys[i] = types[i].UnderlyingType;
        tys[^1] = typeof(IntPtr);
        
        var mt = new DynamicMethod("", typeof(void), tys);
        _compilerFrame.PushFrame(new MethodFrame(m, null, new IlExprBuilder(mt), types.Select(x => new SlotAllocation(null, x)).ToArray()));
        This.Walk(a);

        returnType = MethodFrame.LastSType.UnderlyingType;
        Expr.Store.ToPointer(returnType);
        Expr.ReturnVoid();
        
        return mt;
    }

    public SType CompileExpr(TAny ex) {
        This.Walk(ex);
        return LastSType;
    }

    public void WalkCall(Symbol function, ExprArgWalker<TAny, TExpr> args) {
        if (MethodFrame.FindLocalName(function, out var l)) {
            
        }
    }

    public void WalkOperatorCall(bool innerExpr, SpinorOperator op, ExprArgWalker<TAny, TExpr> args) {
        
    }

    public void WalkAssignment(SpinorOperator op, TAny lhs, TAny rhs) {
        var name = ToSymbol(lhs);
        var ty = CompileExpr(rhs);
        if (!MethodFrame.FindLocalName(name, out var v)) {
            var l = CreateLocal(new(name, ty), false);
            LocalFrame.SetLocal(name, l);
            Store(l);
        }else Store(v);
    }

    public void WalkModule(bool isBare, Symbol name, TExpr block) => throw new SpinorException("Module not allowed in Compilation!");
    public void WalkStruct(bool isMutable, Symbol name, TExpr block) => throw new SpinorException("Struct not allowed in Compilation!");
    public void WalkAbstractType(Symbol name, Symbol extends, bool isBuiltin) => throw new SpinorException("Abstract Type Not allowed in Compilation!");
    public void WalkPrimitiveType(Symbol name, int bits, Symbol extends) => throw new SpinorException("Primitive Type not allowed in Compilation!");

    public void WalkBlock(Symbol head, ExprArgWalker<TAny, TExpr> items) {
    
    }

    public void WalkTuple(ExprArgWalker<TAny, TExpr> items) {
        
    }
    
    public void WalkSymbol(Symbol s) => PushConstObject(s);
    public void WalkBool(bool b) => Push(b);
    public void WalkInteger(long l) => Push(l);
    public void WalkFloat(double d) => Push(d);

    #region Reading
    public TExpr ToExpr(TAny a) => _exprReader.ToExpr(a);
    public bool ToBool(TAny a) => _exprReader.ToBool(a);
    public void ToLineNumberNode(TAny a, out int line, out string file) => _exprReader.ToLineNumberNode(a, out line, out file);
    public Symbol ToSymbol(TAny a) => _exprReader.ToSymbol(a);
    public long ToInteger(TAny a) => _exprReader.ToInteger(a);
    public double ToFloat(TAny a) => _exprReader.ToFloat(a);
    public ExprType GetType(TAny a) => _exprReader.GetType(a);
    #endregion
}