global using SpinorTokenType = System.UInt16;

using System;
using System.Collections.Generic;
using Antlr4.Runtime;
using Core;

namespace runtime.core.Compilation;

[Flags]
public enum OperatorType : ushort {
    Suffix = 1 << 0,
    Infix = 1 << 1,
    Prefix = 1 << 2,
    LeftAssociative = 1 << 3,
    RightAssociative = 1 << 4,
    Unary = 1 << 5,
    Binary = 1 << 6,
    Comparison = 1 << 7,
    Assignable = 1 << 8,
    Commutative = 1 << 9,
    Chainable = Commutative
}

public class SpinorOperator {
    private static Dictionary<Symbol, SpinorOperator> _symbols2Operators;
    private static List<SpinorOperator> _token2Operators;
    private static List<List<SpinorOperator>> _operators;
    private const int OperatorBucketCount = 20;
    
    public readonly Symbol Symbol;
    public readonly byte Precedence;
    public readonly OperatorType Type;
    private readonly SpinorTokenType _tokenType;
    public SpinorTokenType TokenType => _tokenType;

    public SpinorOperator(Symbol symbol, byte precedence, OperatorType type, SpinorTokenType tokenType) {
        Symbol = symbol;
        Precedence = precedence;
        Type = type;
        _tokenType = tokenType;
    }

    public bool Suffix => Type.HasFlag(OperatorType.Suffix);
    public bool Prefix => Type.HasFlag(OperatorType.Prefix);
    public bool LeftAssociative => Type.HasFlag(OperatorType.LeftAssociative);
    public bool RightAssociative => Type.HasFlag(OperatorType.RightAssociative);
    public bool Infix => Type.HasFlag(OperatorType.Infix);
    public bool Binary => Type.HasFlag(OperatorType.Binary);
    public bool Unary => Type.HasFlag(OperatorType.Unary);
    public bool Commutative => Type.HasFlag(OperatorType.Commutative);
    public bool Assignable => Type.HasFlag(OperatorType.Assignable);
    public bool Chainable => Type.HasFlag(OperatorType.Chainable);
    
    internal static void InitializeOperators() {
        _token2Operators = new();
        _operators = new(OperatorBucketCount);
        _symbols2Operators = new();
        
        for(var i = 0; i < OperatorBucketCount; i ++)
            _operators.Add(new List<SpinorOperator>(2));
        
        AddOperations(OperatorType.Binary | OperatorType.Infix | OperatorType.Assignable | OperatorType.RightAssociative, 0, "=");
     
        AddOperations(OperatorType.Binary | OperatorType.Infix | OperatorType.Comparison, 1,
            new[]{">", "<", ">=", "≥", "<=", "≤", "==", "===", "≡", "!=", "≠", "!==", "≢", "∈", "∉", 
                "∋", "∌", "⊆", "⊈", "⊂", "⊄", "⊊", "∝", "∊", "∍", "∥", "∦", "∷", "∺", "∻", "∽", "∾", "≁", "≃", 
                "≂", "≄", "≅", "≆", "≇", "≈", "≉", "≊", "≋", "≌", "≍", "≎", "≐", "≑", "≒", "≓", "≖", "≗", "≘", 
                "≙", "≚", "≛", "≜", "≝", "≞", "≟", "≣", "≦", "≧", "≨", "≩", "≪", "≫", "≬", "≭", "≮", "≯", "≰", "≱", 
                "≲", "≳", "≴", "≵", "≶", "≷", "≸", "≹", "≺", "≻", "≼", "≽", "≾", "≿", "⊀", "⊁", "⊃", "⊅", "⊇", "⊉", 
                "⊋", "⊏", "⊐", "⊑", "⊒", "⊜", "⊩", "⊬", "⊮", "⊰", "⊱", "⊲", "⊳", "⊴", "⊵", "⊶", "⊷", "⋍", "⋐", "⋑", 
                "⋕", "⋖", "⋗", "⋘", "⋙", "⋚", "⋛", "⋜", "⋝", "⋞", "⋟", "⋠", "⋡", "⋢", "⋣", "⋤", "⋥", "⋦", "⋧", "⋨", 
                "⋩", "⋪", "⋫", "⋬", "⋭", "⋲", "⋳", "⋴", "⋵", "⋶", "⋷", "⋸", "⋹", "⋺", "⋻", "⋼", "⋽", "⋾", "⋿", "⟈", 
                "⟉", "⟒", "⦷", "⧀", "⧁", "⧡", "⧣", "⧤", "⧥", "⩦", "⩧", "⩪", "⩫", "⩬", "⩭", "⩮", "⩯", "⩰", "⩱", "⩲", 
                "⩳", "⩵", "⩶", "⩷", "⩸", "⩹", "⩺", "⩻", "⩼", "⩽", "⩾", "⩿", "⪀", "⪁", "⪂", "⪃", "⪄", "⪅", "⪆", "⪇", "⪈", 
                "⪉", "⪊", "⪋", "⪌", "⪍", "⪎", "⪏", "⪐", "⪑", "⪒", "⪓", "⪔", "⪕", "⪖", "⪗", "⪘", "⪙", "⪚", "⪛", "⪜", "⪝", 
                "⪞", "⪟", "⪠", "⪡", "⪢", "⪣", "⪤", "⪥", "⪦", "⪧", "⪨", "⪩", "⪪", "⪫", "⪬", "⪭", "⪮", "⪯", "⪰", "⪱", "⪲", 
                "⪳", "⪴", "⪵", "⪶", "⪷", "⪸", "⪹", "⪺", "⪻", "⪼", "⪽", "⪾", "⪿", "⫀", "⫁", "⫂", "⫃", "⫄", "⫅", "⫆", "⫇", 
                "⫈", "⫉", "⫊", "⫋", "⫌", "⫍", "⫎", "⫏", "⫐", "⫑", "⫒", "⫓", "⫔", "⫕", "⫖", "⫗", "⫘", "⫙", "⫷", "⫸", 
                "⫹", "⫺", "⊢", "⊣", "⟂", "⫪", "⫫", "<:", ">:"});
        
        AddOperations(OperatorType.Binary | OperatorType.Infix | OperatorType.LeftAssociative | 
                      OperatorType.Assignable | OperatorType.Commutative | OperatorType.Chainable, 2,
            new[]{"+", "-", "−", "¦", "⊕", "⊖", "⊞", "⊟", "|++|", "∪", "∨", "⊔", "±", "∓", "∔", "∸", "≏", "⊎", "⊻", 
                "⊽", "⋎", "⋓", "⧺", "⧻", "⨈", "⨢", "⨣", "⨤", "⨥", "⨦", "⨧", "⨨", "⨩", "⨪", "⨫", "⨬", "⨭", "⨮", "⨹", "⨺", 
                "⩁", "⩂", "⩅", "⩊", "⩌", "⩏", "⩐", "⩒", "⩔", "⩖", "⩗", "⩛", "⩝", "⩡", "⩢", "⩣"});
        
        AddOperations(OperatorType.Binary | OperatorType.Infix | OperatorType.LeftAssociative | 
                      OperatorType.Assignable | OperatorType.Commutative | OperatorType.Chainable, 3,
            new[]{"*", "/", "⌿", "÷", "%", "&", "·", "·", "⋅", "∘", "×", "∩", "∧", "⊗", "⊘", "⊙", "⊚", "⊛", "⊠", "⊡", 
                    "⊓", "∗", "∙", "∤", "⅋", "≀", "⊼", "⋄", "⋆", "⋇", "⋉", "⋊", "⋋", "⋌", "⋏", "⋒", "⟑", "⦸", "⦼", "⦾", "⦿",
                    "⧶", "⧷", "⨇", "⨰", "⨱", "⨲", "⨳", "⨴", "⨵", "⨶", "⨷", "⨸", "⨻", "⨼", "⨽", "⩀", "⩃", "⩄", "⩋", "⩍", "⩎", 
                    "⩑", "⩓", "⩕", "⩘", "⩚", "⩜", "⩞", "⩟", "⩠", "⫛", "⊍", "▷", "⨝", "⟕", "⟖", "⟗", "⨟"});
        
        AddOperations(OperatorType.Binary | OperatorType.Infix | OperatorType.RightAssociative | OperatorType.Assignable, 4,
            new[]{"^", "↑", "↓", "⇵", "⟰", "⟱", "⤈", "⤉", "⤊", "⤋", "⤒", "⤓", "⥉", "⥌", "⥍", "⥏", "⥑", "⥔", "⥕", "⥘", "⥙", 
                "⥜", "⥝", "⥠", "⥡", "⥣", "⥥", "⥮", "⥯", "￪", "￬"});
        
        UpdateOperations();
    }

    public static void UpdateOperations() {
        foreach (var l in _operators) {
            l.Sort((x, y) => y.Symbol.String.Length.CompareTo(x.Symbol.String.Length));
            l.TrimExcess();
            if (l.Count == 0)
                throw new SpinorException("Cant Use This Bucket Count as it Contains Zero!");
        }
    }
    public static SpinorOperator GetOpFromToken(int ttype) => _token2Operators[ttype];
    public static void AddOperations(OperatorType type, byte precedence, params string[] operators) {
        foreach (var op in operators)
            AddOperator(new((Symbol) op, precedence, type, 0));
    }
    public static void AddOperator(SpinorOperator op) {
        SpinorOperator o = new(op.Symbol, op.Precedence, op.Type, (SpinorTokenType) _token2Operators.Count);
        _token2Operators.Add(o);
        _symbols2Operators.Add(o.Symbol, o);
        GetBucket(o.Symbol.String[0]).Add(o);
    }
    private static List<SpinorOperator> GetBucket(char c) => _operators[c % OperatorBucketCount];
    public static bool GetOp(Symbol s, out SpinorOperator sop) => _symbols2Operators.TryGetValue(s, out sop);
    public static unsafe bool GetOp(IIntStream s, out SpinorOperator sop) {
        var b = GetBucket((char) s.LA(1));
        var len = Math.Min(b[0].Symbol.String.Length, s.Size - s.Index);
        var charBuffer = stackalloc char[len];       //Max Symbol Length Since Sorted Largest to Smallest Length
        for (var i = 0; i <= len; i++)
            charBuffer[i] = (char) s.LA(i + 1);
        foreach (var so in b) {
            var sym = so.Symbol.String;
            for(int i = 0, n = sym.Length; i < n; i++)
                if (charBuffer[i] != sym[i])
                    goto NEXT;
            sop = so;
            return true;
            NEXT:;
        }
        sop = default;
        return false;
    }
}