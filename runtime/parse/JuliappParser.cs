using System;
using System.IO;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using HyperSphere;
using runtime.core;

namespace runtime.parse
{
    public class JuliaParserErrorListener<Symbol> : IAntlrErrorListener<Symbol>
    {
        private bool EncounteredError;
        private int err_count = 0;

        public void SyntaxError(TextWriter output, IRecognizer recognizer, Symbol offendingSymbol, int line, int charPositionInLine,
            string msg, RecognitionException e) {
            if (err_count++ == 3) 
                Finish();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine("Parsing Error @ Line:" + line + ":" + charPositionInLine + "\t" + msg);
            Console.ResetColor();
        }

        public void Finish() {
            if (err_count > 0) 
                throw new JuliaException("Encountered " + err_count + " Parser Failures!");
        }
    }

    public class JuliappParser : BaseObject
    {
        private readonly BaseInputCharStream _stream;
        private readonly JuliaLexer _lexer;
        private readonly JuliaParser _parser;
        private readonly CommonTokenStream _token_stream;
        public readonly JuliaParser.ScriptContext script;

        public JuliappParser(string s) : this(new AntlrInputStream(s)){}
        public JuliappParser(FileInfo file) : this(new AntlrFileStream(file.Name)){}

        public JuliappParser(BaseInputCharStream stream) {
            _stream = stream;
            _lexer = new JuliaLexer(stream);
            _token_stream = new CommonTokenStream(_lexer);
            _parser = new JuliaParser(_token_stream);
            var listener = new JuliaParserErrorListener<IToken>();
            _parser.RemoveErrorListeners();
            _parser.AddErrorListener(listener);
            script = _parser.script();
            listener.Finish();
        }

        public override string ToString() => PrintSyntaxTree(_parser, script);
        
        public static string PrintSyntaxTree(Parser parser, IParseTree root) {
            StringBuilder buf = new StringBuilder();
            Recursive(root, buf, 0,  parser.RuleNames);
            return buf.ToString();
        }

        private static void Recursive(IParseTree aRoot, StringBuilder buf, int offset, string[] ruleNames) {
            for (int i = 0; i < offset; i++) buf.Append("  ");
            buf.Append(Trees.GetNodeText(aRoot, ruleNames)).Append("\n");
            if (aRoot is ParserRuleContext) {
                ParserRuleContext prc = (ParserRuleContext) aRoot;
                if (prc.children != null) {
                    foreach (IParseTree child in prc.children) {
                        Recursive(child, buf, offset + 1, ruleNames);
                    }
                }
            }
        }
    }
}