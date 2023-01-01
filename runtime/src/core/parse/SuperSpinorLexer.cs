using System;
using System.IO;
using Antlr4.Runtime;
using HyperSphere;
using runtime.core.expr;

namespace runtime.parse;

public abstract class SuperSpinorLexer : Lexer {
    
    public void EmitToken(IToken t, int consumeLength = 0) {
        Token = t;
        if(consumeLength != 0)
            InputStream.Seek(InputStream.Index + consumeLength - 1);
    }

    public bool IsBinaryOrAssignableOp() {
        if (!SpinorOperator.GetOp(InputStream, out var v) || !v.Binary) 
            return false;
        var sym = v.Symbol.String;
        if (v.Assignable && (InputStream.LA(sym.Length + 1) == '=' || sym == "="))
            EmitToken(new SpinorOperatorToken(v.TokenType, SpinorLexer.BinaryOrAssignableOp, Line, Column, OperatorKind.Assignment), sym.Length + 1);
        else 
            EmitToken(new SpinorOperatorToken(v.TokenType, SpinorLexer.BinaryOrAssignableOp, Line, Column, OperatorKind.Binary), sym.Length);
        
        return true;
    }

    public SuperSpinorLexer(ICharStream input) : base(input) {}
    public SuperSpinorLexer(ICharStream input, TextWriter output, TextWriter errorOutput) : base(input, output, errorOutput){}

    internal void Initialize() => TokenFactory = new SuperSpinorTokenFactory();
}

internal class SuperSpinorTokenFactory : ITokenFactory {
    public SuperSpinorTokenFactory(){}
    public IToken Create(Tuple<ITokenSource, ICharStream> source, int type, string text, int channel, int start, int stop, int line, int charPositionInLine) => text != null ? new CommonToken(type, text) : new CommonToken(source, type, channel, start, stop);

    public IToken Create(int type, string text) => Create(default, type, text, 0, -1, -1, -1, -1);
}
