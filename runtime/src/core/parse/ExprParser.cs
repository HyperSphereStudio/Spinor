using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Core;
using HyperSphere;
using runtime.parse;

using static runtime.core.Spinor;

namespace runtime.core.Compilation;

public class ExprParser : SpinorParser, IAntlrErrorListener<IToken>, IAntlrErrorListener<int>
{
    private bool _init;
    private string _file;
    private bool _createLineNumberNodes = true;
    public static int DisplayLineCharWidth = 40;

    public ExprParser() : base(new CommonTokenStream(CreateSpinorLexer())) {}

    public Any Parse(string s, string file = null, bool createLineNumberNodes = true) =>
        Parse(new AntlrInputStream(s), file, createLineNumberNodes);

    public Any Parse(FileInfo file, bool createLineNumberNodes = true) =>
        Parse(new AntlrFileStream(file.FullName), file.FullName, createLineNumberNodes);

    public Any Parse(BaseInputCharStream stream, string file = null, bool createLineNumberNodes = true)
    {
        if (!_init)
        {
            RemoveErrorListeners();
            AddErrorListener(this);
            var l = Lexer;
            l.RemoveErrorListeners();
            l.AddErrorListener(this);
            _init = true;
        }

        SetInput(stream);
        _file = file;
        _createLineNumberNodes = createLineNumberNodes;
         //     Debug();
        var ctx = topExpr();
        return ctx.ChildCount == 0 ? null : Parse(ASTSymbols.Block, ctx.exprBlock(), true);
    }

    private Any Parse(PrimaryExprContext ctx)
    {
        return ctx switch
        {
            BlockContext b => Parse(b),
            StructContext s => Parse(s),
            ModuleContext m => Parse(m),
            LiteralExprContext l => Parse(l.literal()),
            NameExprContext n => Parse(n.Name()),
            TupleExprContext t => Parse(t.tuple()),
            FunctionCallContext f => Parse(f),
            PrimitiveContext p => Parse(p),
            AbstractOrBuiltinContext a => Parse(a),
            _ => throw new SpinorException("Unknown Expression " + ctx.GetText())
        };
    }

    private Expr Parse(FunctionCallContext ctx)
    {
        var t = ctx.tuple().expr();
        Expr e = new(ASTSymbols.Call, new List<Any>(1 + t.Length));
        e.Args.Add(Parse(ctx.Name()));
        foreach (var a in t)
            e.Args.Add(Parse(a));
        return e;
    }

    private Any Parse(ExprContext ctx)
    {
        var bops = ctx.BinaryOrAssignableOp();
        if (bops.Length == 0)
            return Parse(ctx.primaryExpr()); //Unwrap Normal Expr

        var exprs = ctx.expr();
        var op = (SpinorOperatorToken)bops[0].Symbol;
        var e = ParseBinaryOrAssignment(op, Parse(ctx.primaryExpr()), Parse(exprs[0]));

        var i = 1;
        if (!op.Operator.Chainable)
            goto UnChainable;

        Chainable:
        //BOPS will all be the same precedence. We can "chain" together chainable same precedence operators
        for (; i < bops.Length; i++)
        {
            var newOp = (SpinorOperatorToken)bops[i].Symbol;
            if (!op.Equals(newOp))
                goto UnChainable;
            e.Args.Add(Parse(exprs[i]));
        }

        UnChainable:
        for (; i < bops.Length; i++)
        {
            op = (SpinorOperatorToken)bops[i].Symbol;
            e = ParseBinaryOrAssignment(op, e, Parse(exprs[i]));
            if (op.Operator.Chainable)
                goto Chainable;
        }

        return e;
    }

    private Expr ParseBinaryOrAssignment(SpinorOperatorToken opTok, Any lhs, Any rhs) =>
        opTok.OperatorKind == OperatorKind.Binary
            ? new(ASTSymbols.Call, (Symbol)opTok.Text, lhs, rhs)
            : new((Symbol)opTok.Text, lhs, rhs);

    private Any Parse(BlockContext ctx) =>
        Parse(ctx.head.Type == BEGIN ? ASTSymbols.Block : ASTSymbols.Quote, ctx.exprBlock());

    private void CreateLineNode(Expr e, IToken terminationToken)
    {
        if (_createLineNumberNodes)
            e.Args.Add(new LineNumberNode(terminationToken.Line, _file));
    }

    private Any Parse(TupleContext ctx)
    {
        var ex = ctx.expr();
        return ex.Length == 1
            ? Parse(ex[0])
            : //Singleton
            new Expr(ASTSymbols.Tuple, ctx.expr().Select(Parse).ToList());
    }

    private Any Parse(Symbol head, ExprBlockContext ctx, bool canCollapse = false)
    {
        var exs = ctx.expr();
        if (canCollapse && exs.Length == 1)
            return Parse(exs[0]);

        var ex = new Expr(head, new List<Any>(ctx.ChildCount));
        foreach (var e in ctx.children) {
            if (e is ITerminalNode it && it.Symbol.Type == Termination)
                CreateLineNode(ex, it.Symbol);
            else
                ex.Args.Add(Parse((ExprContext)e));
        }

        return ex;
    }

    private Symbol Parse(ITerminalNode symbolNode) => (Symbol) symbolNode.GetText();
    private Symbol Parse(IToken symbolNode) => (Symbol) symbolNode.Text;

    private Expr Parse(PrimitiveContext ctx) {
        Any e = Parse(ctx.name);
        if (ctx.extends != null)
            e = new Expr(ASTSymbols.Extends, e, Parse(ctx.extends));
        return new Expr(ASTSymbols.Primitive, e, Parse(ctx.integer()));
    }

    private Expr Parse(AbstractOrBuiltinContext ctx) {
        Any e = Parse(ctx.name);
        if (ctx.extends != null)
            e = new Expr(ASTSymbols.Extends, e, Parse(ctx.extends));
        return new Expr( ctx.ABSTRACT() != null ? ASTSymbols.Abstract : ASTSymbols.Builtin, e);
    }
    
    private Expr Parse(ModuleContext m) => new(ASTSymbols.Module,
        Box(m.bare == null),
        Parse(m.Name()),
        Parse(ASTSymbols.Block, m.exprBlock()));

    private Expr Parse(StructContext s) => new(
        ASTSymbols.Struct,
        Box(s.mutable != null),
        Parse(s.Name()),
        Parse(ASTSymbols.Block, s.exprBlock()));

    private Any Parse(LiteralContext l) {
        return l.GetChild(0) switch {
            FloatContext f => Parse(f),
            IntegerContext i => Parse(i),
            _ => throw new SpinorException("Unknown Literal " + l)
        };
    }
    
    private Any Parse(IntegerContext ic) => Box(long.Parse(ic.GetText()));
    private Any Parse(FloatContext fc) => Box(double.Parse(fc.GetText()));

    private void ParserException(string msg, int line, int charPosInLine) {
        var ts = TokenStream;
        if (charPosInLine == -1)
            charPosInLine = ts.TokenSource.Column;
        var tc = ts.TokenSource.InputStream;

        //Trim Front To Single Line
        var start = tc.Index;
        for (var i = 1; i < start; i++) {
            var c = tc.LA(-i);
            if (c != '\n' && c != '\r') 
                continue;
            start -= i;
            break;
        }
        
        //Trim Rear To Single Line
        var end = tc.Index;
        for (var i = 1; i < start; i++) {
            var c = tc.LA(i);
            if (c != '\n' && c != '\r') 
                continue;
            end += i;
            break;
        }
        
        end = Math.Min(end, start + DisplayLineCharWidth);
        Interval textInterval = new Interval(start + 1, end - 1);
        string lineWindow = tc.GetText(textInterval);
        string errorLink = _file == null ? "" : new Uri(Path.GetFullPath(_file)).ToString();
        
        //Create Arrows of Offending Token Location
        string errorBars = new string(' ', charPosInLine) + '^';

        throw new SpinorException("Spinor Parsing Error:\t{0}\n:> {1} {2}:{3}\n\n{4}\n\t{5}", 
            msg, errorLink, line, charPosInLine, lineWindow, errorBars);
    }
    public void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line,
        int charPositionInLine, string msg, RecognitionException e) => ParserException(msg, line, charPositionInLine);

    public void SyntaxError(TextWriter output, IRecognizer recognizer, int offendingSymbol, int line,
        int charPositionInLine, string msg, RecognitionException e) => ParserException(msg, line, charPositionInLine);

    private void Debug()
    {
        PrintToken(Lexer.Token);
        foreach (var t in Lexer.GetAllTokens())
            PrintToken(t);
        Lexer.Reset();
        PrintSyntaxTree(this, topExpr()).PrintLn();
        Reset();
    }

    private static SpinorLexer CreateSpinorLexer()
    {
        var sl = new SpinorLexer(null);
        sl.Initialize();
        return sl;
    }
}