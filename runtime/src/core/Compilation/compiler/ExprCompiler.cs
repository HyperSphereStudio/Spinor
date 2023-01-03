/*
   * Author : Johnathan Bizzano
   * Created : Monday, January 2, 2023
   * Last Modified : Monday, January 2, 2023
*/

using System;
using System.Collections.Generic;
using Core;
using runtime.stdlib;

namespace runtime.core.Compilation.compiler;

public struct GlobalEvaluationClosure {
    public List<object> UnInitializedValues;
}

public class Local {
    public Core.Type Type;
    
}

public class InterpreterFrame{
    public Module Module;
    public Dictionary<Symbol, Local> Locals = new(0);

    public InterpreterFrame(Module module) {
        Module = module;
    }
}

public abstract class ExprCompiler<TAny, TExpr> : SpinorExprWalker<TAny, TExpr>
                          where TExpr : IExpr<TAny> {
    private readonly SpinorExprWalker<TAny, TExpr> _walker;
    private readonly List<object> _unInitializedValues = new();
    private TopModule _topExecutingModule;
    private InterpreterFrame _frame;
    private object lastExpr;

    public ExprCompiler(SpinorExprWalker<TAny, TExpr> walker) => _walker = walker;

    protected abstract void Reset();

    public Action Compile(TAny a, Module m) {
        Reset();
        _topExecutingModule = m.TopModule;
        _frame = new InterpreterFrame(m);
        lock (_topExecutingModule.Lock) {
            Walk(a);
        }
        GlobalEvaluationClosure closure = new() {
            UnInitializedValues = _unInitializedValues
        };
        return null;
    }

    public override void WalkCall(Symbol function, ExprArgWalker<TAny, TExpr> args) {
        
    }

    public override void WalkOperatorCall(bool innerExpr, SpinorOperator op, ExprArgWalker<TAny, TExpr> args) {
        
    }

    public override void WalkAssignment(SpinorOperator op, TAny lhs, TAny rhs) {
        var name = ToSymbol(lhs);
        if (!_frame.Locals.TryGetValue(name, out var v)) {
            v = new Local();
            _frame.Locals.Add(name, v);
        }
        Walk(rhs);
        
    }

    public abstract void WriteInitializeModule(Module m);
    
    public override void WalkModule(bool isBare, Symbol name, TExpr block) {
        var m = new Module(name, _frame.Module, !isBare);
        var cf = _frame;
        _frame = new InterpreterFrame(m);
        _unInitializedValues.Add(m);
        WriteInitializeModule(m);
        foreach (var ex in new ExprArgWalker<TAny, TExpr>(block)) {
            Walk(ex, false);
        }
        _frame = cf;
    }
    
    
    public override void WalkStruct(bool isMutable, Symbol name, TExpr block) {
        
    }

    public override void WalkBlock(Symbol head, ExprArgWalker<TAny, TExpr> items) {
        
    }

    public override void WalkTuple(ExprArgWalker<TAny, TExpr> items) {
        
    }

    public override void WalkLineNumberNode(int line, string file) {
    }
    
    public override void WalkSymbol(Symbol s){}

    public override void WalkBool(bool b) {
        
    }

    public override void WalkInteger(long l) {
       
    }

    public override void WalkFloat(double d) {
        
    }
    
    public override TExpr ToExpr(TAny a) => _walker.ToExpr(a);
    public override bool ToBool(TAny a) => _walker.ToBool(a);
    public override void ToLineNumberNode(TAny a, out int line, out string file) => _walker.ToLineNumberNode(a, out line, out file);
    public override Symbol ToSymbol(TAny a) => _walker.ToSymbol(a);
    public override long ToInteger(TAny a) => _walker.ToInteger(a);
    public override double ToFloat(TAny a) => _walker.ToFloat(a);
    public override ExprType GetType(TAny a) => _walker.GetType(a);
}