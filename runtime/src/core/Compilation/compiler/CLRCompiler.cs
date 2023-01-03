/*
   * Author : Johnathan Bizzano
   * Created : Monday, January 2, 2023
   * Last Modified : Monday, January 2, 2023
*/

using Core;
using runtime.stdlib;

namespace runtime.core.Compilation.compiler;

public class CLRCompiler<TAny, TExpr> : ExprCompiler<TAny, TExpr>  
                                        where TExpr : IExpr<TAny> {
    public CLRCompiler(SpinorExprWalker<TAny, TExpr> walker) : base(walker) {}


    protected override void Reset() {}
    
}

public class SerializedCLRCompiler : CLRCompiler<SAny, SExpr> {
    public SerializedCLRCompiler() : base(new ExprDeserializer()) {}
}

public class CLRCompiler : CLRCompiler<Any, Expr> {
    public CLRCompiler() : base(new ImplementedExprWalker()) {}
}