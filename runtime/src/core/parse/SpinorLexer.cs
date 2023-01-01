//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.10.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from C:\Users\JohnB\Desktop\HyperProjects\Spinor\runtime\src\core\parse\SpinorLexer.g4 by ANTLR 4.10.1

// Unreachable code detected
#pragma warning disable 0162
// The variable '...' is assigned but its value is never used
#pragma warning disable 0219
// Missing XML comment for publicly visible type or member '...'
#pragma warning disable 1591
// Ambiguous reference in cref attribute
#pragma warning disable 419

namespace HyperSphere {
using runtime.parse;
using System;
using System.IO;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;
using DFA = Antlr4.Runtime.Dfa.DFA;

[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.10.1")]
[System.CLSCompliant(false)]
public partial class SpinorLexer : SuperSpinorLexer {
	protected static DFA[] decisionToDFA;
	protected static PredictionContextCache sharedContextCache = new PredictionContextCache();
	public const int
		BinaryOrAssignableOp=1, QUOTE=2, BEGIN=3, DIGIT=4, COND=5, DOT=6, END=7, 
		BAREMODULE=8, MODULE=9, MUTABLE=10, STRUCT=11, RPAR=12, LPAR=13, COMMA=14, 
		ID=15, Termination=16, NewLine=17, Whitespace=18, BlockComment=19, LineComment=20;
	public static string[] channelNames = {
		"DEFAULT_TOKEN_CHANNEL", "HIDDEN"
	};

	public static string[] modeNames = {
		"DEFAULT_MODE"
	};

	public static readonly string[] ruleNames = {
		"BinaryOrAssignableOp", "QUOTE", "BEGIN", "DIGIT", "COND", "DOT", "END", 
		"BAREMODULE", "MODULE", "MUTABLE", "STRUCT", "RPAR", "LPAR", "COMMA", 
		"IdentifierPrefixCharacter", "IdentifierSuffixCharacter", "ID", "Termination", 
		"NewLine", "Whitespace", "BlockComment", "LineComment"
	};


	public SpinorLexer(ICharStream input)
	: this(input, Console.Out, Console.Error) { }

	public SpinorLexer(ICharStream input, TextWriter output, TextWriter errorOutput)
	: base(input, output, errorOutput)
	{
		Interpreter = new LexerATNSimulator(this, _ATN, decisionToDFA, sharedContextCache);
	}

	private static readonly string[] _LiteralNames = {
		null, null, "'quote'", "'begin'", null, "'?'", "'.'", "'end'", "'baremodule'", 
		"'module'", "'mutable'", "'struct'", "'('", "')'", "','"
	};
	private static readonly string[] _SymbolicNames = {
		null, "BinaryOrAssignableOp", "QUOTE", "BEGIN", "DIGIT", "COND", "DOT", 
		"END", "BAREMODULE", "MODULE", "MUTABLE", "STRUCT", "RPAR", "LPAR", "COMMA", 
		"ID", "Termination", "NewLine", "Whitespace", "BlockComment", "LineComment"
	};
	public static readonly IVocabulary DefaultVocabulary = new Vocabulary(_LiteralNames, _SymbolicNames);

	[NotNull]
	public override IVocabulary Vocabulary
	{
		get
		{
			return DefaultVocabulary;
		}
	}

	public override string GrammarFileName { get { return "SpinorLexer.g4"; } }

	public override string[] RuleNames { get { return ruleNames; } }

	public override string[] ChannelNames { get { return channelNames; } }

	public override string[] ModeNames { get { return modeNames; } }

	public override int[] SerializedAtn { get { return _serializedATN; } }

	static SpinorLexer() {
		decisionToDFA = new DFA[_ATN.NumberOfDecisions];
		for (int i = 0; i < _ATN.NumberOfDecisions; i++) {
			decisionToDFA[i] = new DFA(_ATN.GetDecisionState(i), i);
		}
	}
	public override bool Sempred(RuleContext _localctx, int ruleIndex, int predIndex) {
		switch (ruleIndex) {
		case 0 : return BinaryOrAssignableOp_sempred(_localctx, predIndex);
		}
		return true;
	}
	private bool BinaryOrAssignableOp_sempred(RuleContext _localctx, int predIndex) {
		switch (predIndex) {
		case 0: return IsBinaryOrAssignableOp();
		}
		return true;
	}

	private static int[] _serializedATN = {
		4,0,20,166,6,-1,2,0,7,0,2,1,7,1,2,2,7,2,2,3,7,3,2,4,7,4,2,5,7,5,2,6,7,
		6,2,7,7,7,2,8,7,8,2,9,7,9,2,10,7,10,2,11,7,11,2,12,7,12,2,13,7,13,2,14,
		7,14,2,15,7,15,2,16,7,16,2,17,7,17,2,18,7,18,2,19,7,19,2,20,7,20,2,21,
		7,21,1,0,1,0,1,0,1,1,1,1,1,1,1,1,1,1,1,1,1,2,1,2,1,2,1,2,1,2,1,2,1,3,1,
		3,1,4,1,4,1,5,1,5,1,6,1,6,1,6,1,6,1,7,1,7,1,7,1,7,1,7,1,7,1,7,1,7,1,7,
		1,7,1,7,1,8,1,8,1,8,1,8,1,8,1,8,1,8,1,9,1,9,1,9,1,9,1,9,1,9,1,9,1,9,1,
		10,1,10,1,10,1,10,1,10,1,10,1,10,1,11,1,11,1,12,1,12,1,13,1,13,1,14,1,
		14,1,15,1,15,1,16,1,16,5,16,116,8,16,10,16,12,16,119,9,16,1,17,1,17,4,
		17,123,8,17,11,17,12,17,124,1,17,3,17,128,8,17,1,18,1,18,3,18,132,8,18,
		1,18,3,18,135,8,18,1,19,4,19,138,8,19,11,19,12,19,139,1,19,1,19,1,20,1,
		20,1,20,1,20,5,20,148,8,20,10,20,12,20,151,9,20,1,20,1,20,1,20,1,20,1,
		20,1,21,1,21,5,21,160,8,21,10,21,12,21,163,9,21,1,21,1,21,1,149,0,22,1,
		1,3,2,5,3,7,4,9,5,11,6,13,7,15,8,17,9,19,10,21,11,23,12,25,13,27,14,29,
		0,31,0,33,15,35,16,37,17,39,18,41,19,43,20,1,0,5,1,0,48,57,4,0,33,33,65,
		90,95,95,97,122,5,0,33,33,48,57,65,90,95,95,97,122,2,0,9,9,32,32,2,0,10,
		10,13,13,172,0,1,1,0,0,0,0,3,1,0,0,0,0,5,1,0,0,0,0,7,1,0,0,0,0,9,1,0,0,
		0,0,11,1,0,0,0,0,13,1,0,0,0,0,15,1,0,0,0,0,17,1,0,0,0,0,19,1,0,0,0,0,21,
		1,0,0,0,0,23,1,0,0,0,0,25,1,0,0,0,0,27,1,0,0,0,0,33,1,0,0,0,0,35,1,0,0,
		0,0,37,1,0,0,0,0,39,1,0,0,0,0,41,1,0,0,0,0,43,1,0,0,0,1,45,1,0,0,0,3,48,
		1,0,0,0,5,54,1,0,0,0,7,60,1,0,0,0,9,62,1,0,0,0,11,64,1,0,0,0,13,66,1,0,
		0,0,15,70,1,0,0,0,17,81,1,0,0,0,19,88,1,0,0,0,21,96,1,0,0,0,23,103,1,0,
		0,0,25,105,1,0,0,0,27,107,1,0,0,0,29,109,1,0,0,0,31,111,1,0,0,0,33,113,
		1,0,0,0,35,127,1,0,0,0,37,134,1,0,0,0,39,137,1,0,0,0,41,143,1,0,0,0,43,
		157,1,0,0,0,45,46,4,0,0,0,46,47,9,0,0,0,47,2,1,0,0,0,48,49,5,113,0,0,49,
		50,5,117,0,0,50,51,5,111,0,0,51,52,5,116,0,0,52,53,5,101,0,0,53,4,1,0,
		0,0,54,55,5,98,0,0,55,56,5,101,0,0,56,57,5,103,0,0,57,58,5,105,0,0,58,
		59,5,110,0,0,59,6,1,0,0,0,60,61,7,0,0,0,61,8,1,0,0,0,62,63,5,63,0,0,63,
		10,1,0,0,0,64,65,5,46,0,0,65,12,1,0,0,0,66,67,5,101,0,0,67,68,5,110,0,
		0,68,69,5,100,0,0,69,14,1,0,0,0,70,71,5,98,0,0,71,72,5,97,0,0,72,73,5,
		114,0,0,73,74,5,101,0,0,74,75,5,109,0,0,75,76,5,111,0,0,76,77,5,100,0,
		0,77,78,5,117,0,0,78,79,5,108,0,0,79,80,5,101,0,0,80,16,1,0,0,0,81,82,
		5,109,0,0,82,83,5,111,0,0,83,84,5,100,0,0,84,85,5,117,0,0,85,86,5,108,
		0,0,86,87,5,101,0,0,87,18,1,0,0,0,88,89,5,109,0,0,89,90,5,117,0,0,90,91,
		5,116,0,0,91,92,5,97,0,0,92,93,5,98,0,0,93,94,5,108,0,0,94,95,5,101,0,
		0,95,20,1,0,0,0,96,97,5,115,0,0,97,98,5,116,0,0,98,99,5,114,0,0,99,100,
		5,117,0,0,100,101,5,99,0,0,101,102,5,116,0,0,102,22,1,0,0,0,103,104,5,
		40,0,0,104,24,1,0,0,0,105,106,5,41,0,0,106,26,1,0,0,0,107,108,5,44,0,0,
		108,28,1,0,0,0,109,110,7,1,0,0,110,30,1,0,0,0,111,112,7,2,0,0,112,32,1,
		0,0,0,113,117,3,29,14,0,114,116,3,31,15,0,115,114,1,0,0,0,116,119,1,0,
		0,0,117,115,1,0,0,0,117,118,1,0,0,0,118,34,1,0,0,0,119,117,1,0,0,0,120,
		123,3,37,18,0,121,123,5,59,0,0,122,120,1,0,0,0,122,121,1,0,0,0,123,124,
		1,0,0,0,124,122,1,0,0,0,124,125,1,0,0,0,125,128,1,0,0,0,126,128,5,0,0,
		1,127,122,1,0,0,0,127,126,1,0,0,0,128,36,1,0,0,0,129,131,5,13,0,0,130,
		132,5,10,0,0,131,130,1,0,0,0,131,132,1,0,0,0,132,135,1,0,0,0,133,135,5,
		10,0,0,134,129,1,0,0,0,134,133,1,0,0,0,135,38,1,0,0,0,136,138,7,3,0,0,
		137,136,1,0,0,0,138,139,1,0,0,0,139,137,1,0,0,0,139,140,1,0,0,0,140,141,
		1,0,0,0,141,142,6,19,0,0,142,40,1,0,0,0,143,144,5,35,0,0,144,145,5,61,
		0,0,145,149,1,0,0,0,146,148,9,0,0,0,147,146,1,0,0,0,148,151,1,0,0,0,149,
		150,1,0,0,0,149,147,1,0,0,0,150,152,1,0,0,0,151,149,1,0,0,0,152,153,5,
		61,0,0,153,154,5,35,0,0,154,155,1,0,0,0,155,156,6,20,0,0,156,42,1,0,0,
		0,157,161,5,35,0,0,158,160,8,4,0,0,159,158,1,0,0,0,160,163,1,0,0,0,161,
		159,1,0,0,0,161,162,1,0,0,0,162,164,1,0,0,0,163,161,1,0,0,0,164,165,6,
		21,0,0,165,44,1,0,0,0,10,0,117,122,124,127,131,134,139,149,161,1,6,0,0
	};

	public static readonly ATN _ATN =
		new ATNDeserializer().Deserialize(_serializedATN);


}
} // namespace HyperSphere
