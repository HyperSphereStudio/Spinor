/*
   * Author : Johnathan Bizzano
   * Created : Monday, January 2, 2023
   * Last Modified : Monday, January 2, 2023
*/

using Antlr4.Runtime;
using runtime.core;
using runtime.core.Compilation;

namespace runtime.parse;

public enum OperatorKind : byte {
    Binary,
    Assignment
}

public class SpinorOperatorToken : IToken {
    public string Text {
        get {
            var sym = Operator.Symbol.String;
            if (OperatorKind == OperatorKind.Assignment && sym != "=")
                return sym + "=";
            return sym;
        }
    }
    
    public SpinorOperator Operator => SpinorOperator.GetOpFromToken(OperatorType);
    public ushort OperatorType { get; }  
    public OperatorKind OperatorKind { get; }
    public int Type { get; }
    public int Line { get; }
    public int Column { get; }
    public int Channel => 0;
    public int TokenIndex { get; }
    public int StartIndex => -1;
    public int StopIndex => -1;
    public ITokenSource TokenSource => null;
    public ICharStream InputStream => null;
    
    public SpinorOperatorToken(ushort operatorType, int type, int line, int column, OperatorKind opKind) {
        OperatorType = operatorType;
        OperatorKind = opKind;
        Type = type;
        TokenIndex = -1;
        Line = line;
        Column = column;
    }

    public override bool Equals(object o) => o is SpinorOperatorToken t && t.OperatorType == OperatorType;
}



