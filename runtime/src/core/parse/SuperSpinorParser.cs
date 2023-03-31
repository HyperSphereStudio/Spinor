/*
   * Author : Johnathan Bizzano
   * Created : Monday, January 2, 2023
   * Last Modified : Monday, January 2, 2023
*/

using System;
using System.IO;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using HyperSphere;
using runtime.core.math;
using runtime.parse;

namespace runtime.core.parse;

public abstract class SuperSpinorParser : Parser {
    public SpinorLexer Lexer { get; }
    private SpinorOperator GetOp(int lt) => ((SpinorOperatorToken) TokenStream.LT(lt)).Operator;
    public bool TargetPrecedence(int i) {
        if (TokenStream.LT(i) is SpinorOperatorToken st)
            return st.Operator.Precedence >= i;
        return false;
    }

    protected int NextOperatorPrecedence {
        get {
            var op = GetOp(-1);
            if(op.RightAssociative || op.Prefix)
                return op.Precedence;
            return op.Precedence + 1;
        }
    }

    protected SuperSpinorParser(ITokenStream input, TextWriter output, TextWriter errorOutput) : base(input, output,
        errorOutput) {
        Lexer = (SpinorLexer) ((CommonTokenStream) input).TokenSource;
    }
    
    protected void SetInput(BaseInputCharStream stream) {
        Lexer.SetInputStream(stream);
        Lexer.Reset();
        Reset();
    }
    public static string PrintSyntaxTree(Parser parser, IParseTree root) {
        StringBuilder buf = new StringBuilder();
        Recursive(root, buf, 0, parser.RuleNames);
        return buf.ToString();
    }
    private static void Recursive(IParseTree aRoot, StringBuilder buf, int offset, string[] ruleNames) {
        for (var i = 0; i < offset; i++)
            buf.Append("  ");
        buf.Append(Trees.GetNodeText(aRoot, ruleNames)).Append("\n");
        if (aRoot is not ParserRuleContext prc) 
            return;

        if (prc.children == null) 
            return;
        
        foreach (IParseTree child in prc.children)
            Recursive(child, buf, offset + 1, ruleNames);
    }

    public static void PrintToken(IToken t) => Console.WriteLine(SpinorLexer.DefaultVocabulary.GetDisplayName(t.Type) + ":" + t.Text);
}