/*
   * Author : Johnathan Bizzano
   * Created : Monday, January 2, 2023
   * Last Modified : Monday, January 2, 2023
*/

using Antlr4.Runtime;
using HyperSphere;
using runtime.core;
using runtime.core.math;

namespace runtime.parse;

public enum OperatorKind : byte {
    Binary,
    Assignment
}

public record SpinorOperatorToken(ushort OperatorType, int Line, int Column, OperatorKind OperatorKind) : IToken{
    public SpinorOperator Operator => SpinorOperator.GetOpFromToken(OperatorType);
    public string Text {
        get {
            var sym = Operator.Symbol.String;
            if (OperatorKind == OperatorKind.Assignment && sym != "=")
                return sym + "=";
            return sym;
        }
    }
    public int Type => SpinorLexer.BinaryOrAssignableOp;
    public int Channel => SpinorLexer.DefaultTokenChannel;
    public int TokenIndex => -1;
    public int StartIndex => -1;
    public int StopIndex => -1;
    public ITokenSource TokenSource => null;
    public ICharStream InputStream => null;
}



