using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Core;
using HyperSphere;
using runtime.core.internals;
using runtime.parse;
using runtime.stdlib;
using runtime.Utils;

namespace runtime.core.parse;

public class ExprParser : SpinorParser, IAntlrErrorListener<IToken>, IAntlrErrorListener<int> {
    private string _file;
    private bool _createLineNumberNodes = true;
    public static int DisplayLineCharWidth = 40;

    public ExprParser() : base(new CommonTokenStream(new SpinorLexer(null))) {
        base.RemoveErrorListeners();
        base.AddErrorListener(this);
        Lexer.RemoveErrorListeners();
        Lexer.AddErrorListener(this);
    }

    public Any Parse(string s, string file = null, bool createLineNumberNodes = true) =>
        Parse(new AntlrInputStream(s), file, createLineNumberNodes);

    public Any Parse(FileInfo file, bool createLineNumberNodes = true) =>
        Parse(new AntlrFileStream(file.FullName), file.FullName, createLineNumberNodes);

    public Any Parse(BaseInputCharStream stream, string file = null, bool createLineNumberNodes = true) {
        SetInput(stream);
        _file = file;
        _createLineNumberNodes = createLineNumberNodes;
        return Parse(ASTSymbols.Block, topExpr().exprBlock());
    }
    
    private Any Parse(PrimitiveExprContext ctx) {
        return ctx switch {
            null => Spinor.Nothing,
            BlockContext b => Parse(b),
            StructContext s => Parse(s),
            ModuleContext m => Parse(m),
            LiteralExprContext l => Parse(l.literal()),
            NameContext n => Parse(n),
            TupleExprContext t => Parse(t.tuple()),
            FunctionCallContext f => Parse(f),
            AbstractContext a => Parse(a),
            UsingContext u => Parse(u),
            ImportExprContext i => Parse(i),
            _ => throw new SpinorException("Unknown Expression " + ctx.GetText())
        };
    }
    private Expr Parse(FunctionCallContext ctx) {
        var funcName = Parse(ctx.ExprName());
        var t = ctx.tuple().expr();
        var list = new List<Any>(t.Length + 1);
        Expr e = new(ASTSymbols.Call, list);
        list.Add(funcName);
        list.AddRange(t.Select(Parse));
        return e;
    }

    private Any Parse(UsingContext ctx) => ParseUsingOrImportArgs(ASTSymbols.Using, ctx.SYSTEM()!=null, ctx.UsingName());

    private Any Parse(NameContext ctx) {
        if (ctx.elemOf == null)
            return Parse(ctx.ExprName());
        return new Expr(ASTSymbols.ElementOf, Parse(ctx.ExprName()), Parse(ctx.elemOf));
    }
    private Any ParseUsingOrImportArgs(Symbol head, bool isSystem, ITerminalNode[] names) {
        var list = new List<Any>(names.Length + 1);
        list.Add(isSystem);
        foreach(var n in names)
            list.Add(Parse(n));
        return new Expr(head, list);
    }

    private Any Parse(PrimitiveExprContext first, ITerminalNode[] bops, ExprContext[] terms) {
        var lastOp = (SpinorOperatorToken) bops[0].Symbol;
        var ex = ParseBinaryOrAssignment(lastOp, Parse(first), Parse(terms[0]));
        var headEx = ex;
        for (int i = 1, n = bops.Length; i < n; i++) {
            var op = (SpinorOperatorToken) bops[i].Symbol;
            var rhs = Parse(terms[i]);
            if (op.Operator.Chainable && op.OperatorType == lastOp.OperatorType)
                ex.Args.Add(rhs);
            else {
                ex = ParseBinaryOrAssignment(op, ex, rhs);
                lastOp = op;
            }
        }
        return headEx;
    }
    
    private Any Parse(ExprContext ctx) {
        var binaryOps = ctx.BinaryOrAssignableOp();
        return binaryOps.Length == 0 ? Parse(ctx.primitiveExpr()) : Parse(ctx.primitiveExpr(), binaryOps, ctx.expr());
    }

    private Expr ParseBinaryOrAssignment(SpinorOperatorToken opTok, Any lhs, Any rhs) =>
        opTok.OperatorKind == OperatorKind.Binary
            ? new(ASTSymbols.Call, opTok.Operator.Symbol, lhs, rhs)
            : new((Symbol) opTok.Text, lhs, rhs);

    private Any Parse(BlockContext ctx) => Parse(ctx.head.Type == BEGIN ? ASTSymbols.Block : ASTSymbols.Quote, ctx.exprBlock());

    private void CreateLineNode(Expr e, IToken terminationToken) {
        if (_createLineNumberNodes)
            e.Args.Add(new LineNumberNode(terminationToken.Line, _file));
    }

    private Any Parse(TupleContext ctx) {
        var ex = ctx.expr();
        return ex.Length == 1
            ? Parse(ex[0])
            : //Singleton
            new Expr(ASTSymbols.Tuple, ctx.expr().Select(Parse).ToList());
    }

    private Any Parse(Symbol head, ExprBlockContext ctx) {
        var ex = new Expr(head, new List<Any>(ctx.ChildCount));
        if (ctx.ChildCount == 0)
            return ex;
        foreach (var e in ctx.children) {
            if (e is ITerminalNode it && it.Symbol.Type == ExprTermination)
                CreateLineNode(ex, it.Symbol);
            else
                ex.Args.Add(Parse((ExprContext) e));
        }
        return ex;
    }

    private Symbol Parse(ITerminalNode symbolNode) => symbolNode == null ? null : (Symbol) symbolNode.GetText();

    /**Expr(:abstract, name, extension?)**/
    private Expr Parse(AbstractContext ctx) => new(ASTSymbols.Abstract, Parse(ctx.TypeName()), Parse(ctx.ext));

    /**Expr(:module, isNotBare, name, block)**/
    private Expr Parse(ModuleContext m) => new(ASTSymbols.Module,
        m.BareModule() == null,
        Parse(m.ModuleName()),
        Parse(ASTSymbols.Block, m.exprBlock()));

    /**Expr(:struct, isMutable, name, extension?, block)**/
    private Expr Parse(StructContext s) => new(ASTSymbols.Struct,
        s.MUTABLE() != null,
        Parse(s.TypeName()),
        Parse(s.ext),
        Parse(ASTSymbols.Block, s.exprBlock()));

    private Any Parse(LiteralContext l) {
        return l switch {
            FloatingPointContext f => Parse(f),
            IntegerContext i => Parse(i),
            SymbolContext s => Parse(s),
            StrContext s => Parse(s.@string()),
            _ => throw new SpinorException("Unknown Literal " + l)
        };
    }
    private Any Parse(StringContext s) {
        var parts = s.stringPart();
        // if (parts.Length == 1 && parts[0] is StrTextContext strCtx)
         //   return strCtx.GetText();
        var x = new Expr(ASTSymbols.String);
        return default;
    }
    private Any Parse(IntegerContext ic) => long.Parse(ic.GetText());
    private Any Parse(FloatingPointContext fc) => double.Parse(fc.GetText());
    private Symbol Parse(SymbolContext s) => Parse(s.ExprSymbol());

    #region Debugging
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
        var textInterval = new Interval(start + 1, end - 1);
        var lineWindow = tc.GetText(textInterval);
        var errorLink = _file == null ? "" : new Uri(Path.GetFullPath(_file)).ToString();
        
        //Create Arrows of Offending Token Location
        var errorBars = new string(' ', charPosInLine) + '^';

        throw new SpinorException("Spinor Parsing Error:\t{0}\n:> {1} {2}:{3}\n\n{4}\n\t{5}", 
            msg, errorLink, line, charPosInLine, lineWindow, errorBars);
    }
    public void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line,
        int charPositionInLine, string msg, RecognitionException e) => ParserException(msg, line, charPositionInLine);

    public void SyntaxError(TextWriter output, IRecognizer recognizer, int offendingSymbol, int line,
        int charPositionInLine, string msg, RecognitionException e) => ParserException(msg, line, charPositionInLine);

    private void Debug() {
        DebugLexer();
        DebugParser();
    }

    private void DebugLexer() {
        try {
            PrintToken(Lexer.Token);
            for (var t = Lexer.NextToken(); t.Type != -1; t = Lexer.NextToken())
                PrintToken(t);
            Lexer.Reset();
        }
        catch (Exception) {
            Console.Error.WriteLine("\n\n\n##LEXER CRASH##");
            Console.Write("Lexer Modes:");
            Lexer.ModeStack.Select(x => Lexer.ModeNames[x]).ToArray().PrintLn(Console.Out);
            Console.WriteLine($"Current:{Lexer.ModeNames[Lexer.CurrentMode]}");
            throw;
        }
    }

    private void DebugParser() {
        PrintSyntaxTree(this, topExpr()).PrintLn();
        Reset();
    }
    
    #endregion
}