/*
   * Author : Johnathan Bizzano
   * Created : Monday, January 2, 2023
   * Last Modified : Monday, January 2, 2023
*/

using System;
using System.IO;
using Antlr4.Runtime;
using Core;
using HyperSphere;
using runtime.core.math;
using runtime.parse;
using runtime.stdlib;


namespace runtime.core.parse;

public abstract class SuperSpinorLexer : Lexer, ITokenFactory{
    private int _lastTokenType = Eof;
    public bool StoreLast = false;

    public SuperSpinorLexer(ICharStream input, TextWriter output, TextWriter errorOutput) : base(input, output,
        errorOutput) {
        
    }
    public override ITokenFactory TokenFactory => this;
    public new void Type(int i) => base.Type = i;
    public bool LastTokenMatch(int t) => _lastTokenType == t;
    public bool LastTokenMatch(int t1, int t2) => _lastTokenType == t1 || _lastTokenType == t2;
    public bool LastTokenMatch(int t1, int t2, int t3) => _lastTokenType == t1 || _lastTokenType == t2 || _lastTokenType == t3;
    public void SeekDelta(int delta) => InputStream.Seek(InputStream.Index + delta);

    private void InsertToken(IToken t, int consumeLength = 0) {
        if(consumeLength != 0)
            InputStream.Seek(InputStream.Index + consumeLength - 1);
        Emit(t);
    }

    public override void Emit(IToken t) {
        base.Emit(t);

        if (StoreLast)
            _lastTokenType = t.Type;
        else 
            StoreLast = true;
    }

    public bool IsBinaryOrAssignableOp() {
                                 //Last Token Cant Be An Binary Operator
        if (LastTokenMatch(SpinorLexer.BinaryOrAssignableOp))
            return false;
        
        if (!SpinorOperator.GetOp(InputStream, out var v) || !v.Binary) 
            return false;
        
        var sym = v.Symbol.String;
        if (v.Assignable && (InputStream.LA(sym.Length + 1) == '=' || v.Symbol == ASTSymbols.Assign))
            InsertToken(new SpinorOperatorToken(v.TokenType, Line, Column, OperatorKind.Assignment), sym.Length + 1);
        else 
            InsertToken(new SpinorOperatorToken(v.TokenType, Line, Column, OperatorKind.Binary), sym.Length);
        
        return true;
    }

    public IToken Create(Tuple<ITokenSource, ICharStream> source, int type, string text, int channel, int start,
        int stop, int line, int charPositionInLine) {
        
        if(type == SpinorLexer.ExprTermination)
            return TerminationToken.Instance;
        
        return text != null ? new CommonToken(type, text) : new CommonToken(source, type, channel, start, stop);
    }
    
    public IToken Create(int type, string text) => Create(default, type, text, 0, -1, -1, -1, -1);
}


//Save Memory By Not Instantiating Termination Tokens
internal class TerminationToken : IToken {
    internal static readonly TerminationToken Instance = new();
    public string Text => ";";
    public int Type => SpinorLexer.ExprTermination;
    public int Line => -1;
    public int Column => -1;
    public int Channel => SpinorLexer.DefaultTokenChannel;
    public int TokenIndex => -1;
    public int StartIndex => -1;
    public int StopIndex => -1;
    public ITokenSource TokenSource => null;
    public ICharStream InputStream => null;
}
