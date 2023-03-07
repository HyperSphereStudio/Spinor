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
using runtime.core.Compilation;
using runtime.parse;

namespace runtime.core.parse;

public abstract class SuperSpinorParser : Parser {
    public SpinorLexer Lexer => (SpinorLexer) ((CommonTokenStream) TokenStream).TokenSource;
    private SpinorOperator GetOp(int lt) => ((SpinorOperatorToken) TokenStream.LT(lt)).Operator;

    public SpinorState SpinorState {
        get => Lexer.SpinorState;
        set => Lexer.SpinorState = value;
    }
    
    protected int OperatorPrecedence => GetOp(1).Precedence;

    protected int NextOperatorPrecedence {
        get {
            var op = GetOp(-1);
            if(op.RightAssociative || op.Prefix)
                return op.Precedence;
            return op.Precedence + 1;
        }
    }
    
    protected SuperSpinorParser(ITokenStream input) : base(input) {}
    protected SuperSpinorParser(ITokenStream input, TextWriter output, TextWriter errorOutput): base(input, output, errorOutput) {}
    protected void SetInput(BaseInputCharStream stream) {
        var lex = Lexer;
        lex.SetInputStream(stream);
        lex.Reset();
        Reset();
    }
    public static string PrintSyntaxTree(Parser parser, IParseTree root) {
        StringBuilder buf = new StringBuilder();
        Recursive(root, buf, 0, parser.RuleNames);
        return buf.ToString();
    }
    private static void Recursive(IParseTree aRoot, StringBuilder buf, int offset, string[] ruleNames) {
        for (int i = 0; i < offset; i++) buf.Append("  ");
        buf.Append(Trees.GetNodeText(aRoot, ruleNames)).Append("\n");
        if (aRoot is ParserRuleContext)
        {
            ParserRuleContext prc = (ParserRuleContext)aRoot;
            if (prc.children != null)
            {
                foreach (IParseTree child in prc.children)
                {
                    Recursive(child, buf, offset + 1, ruleNames);
                }
            }
        }
    }

    public static void PrintToken(IToken t) => Console.WriteLine(SpinorLexer.DefaultVocabulary.GetDisplayName(t.Type) + ":" + t.Text);
}