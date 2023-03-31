/*
   * Author : Johnathan Bizzano
   * Created : Monday, January 2, 2023
   * Last Modified : Monday, January 2, 2023
*/

lexer grammar SpinorLexer;

options { language=CSharp; superClass=SuperSpinorLexer; }
@header{using runtime.core.parse;}

fragment NamePrefixChar: [a-zA-Z_];
fragment NameSuffixChar: [a-zA-Z0-9_!];
fragment Name: NamePrefixChar NameSuffixChar*;
fragment Digit: [0-9];
fragment NewLine: ('\r' '\n'? | '\n');
fragment Termination: (NewLine | ';')+;
fragment WhiteSpace: [ \t]+;
fragment BlockComment: '#=' .*? '=#';
fragment LineComment: '#' ~[\r\n]*;
fragment FloatingPoint: Digit+ '.' Digit+;
fragment Integer: Digit+;
fragment Symbol: ':' Name;
fragment End: 'end';

QUOTE: 'quote';
BEGIN: 'begin';
COND: '?';
DOT: '.';
COMMA: ',';
MUTABLE: 'mutable';
ABSTRACT: 'abstract';
PRIMITIVE: 'primitive';
SYSTEM: 'system';
ELEMENTOF: '::';

BinaryOrAssignableOp: {IsBinaryOrAssignableOp()}? .;

//Mode Initializers
RPAR: '(' -> pushMode(DEFAULT_MODE);

Type: {LastTokenMatch(ABSTRACT)}? 'type' -> pushMode(TYPE_MODE);
String: '"' -> pushMode(STRING_MODE);
Struct: 'struct' -> pushMode(TYPE_MODE);
Module: 'module' -> pushMode(MODULE_MODE);
BareModule: 'baremodule' -> pushMode(MODULE_MODE);

//Modes that cannot change contexts internally dont require pushing
Using: 'using' -> mode(USING_MODE);
Import: 'import' -> mode(IMPORT_MODE);

//Mode Destroyers
LPAR: ')' -> popMode;
ExprEnd: End -> popMode;

//These Must Be Last to make sure previous definitions are matched first
ExprFloatingPoint: FloatingPoint;  
ExprInteger: Integer;
ExprSymbol: Symbol;
ExprName: Name;  
ExprTermination: Termination;
ExprWhitespace: WhiteSpace -> skip;
ExprBlockComment: BlockComment -> skip;
ExprLineComment: LineComment -> skip; 

mode STRING_MODE;
StringInterpExprBegin: '$(' -> pushMode(DEFAULT_MODE);
StringInterpName: '$' Name;
StringText: ~('$'|'\n'|'"')+;
StringEnd: '"' -> popMode;

mode USING_MODE;
UsingName: Name;
UsingDot: '.';
UsingWhitespace: WhiteSpace -> skip;
UsingEnd: Termination {
    Type(ExprTermination); //Convert to ExprTermination
    Mode(DEFAULT_MODE);
}; 

mode IMPORT_MODE;
ImportAs: 'as';
ImportDot: '.';
ImportName: Name;
ImportWhitespace: WhiteSpace -> skip;
ImportEnd: Termination {
    Type(ExprTermination); //Convert to ExprTermination
    Mode(DEFAULT_MODE);
};

mode TYPE_MODE;
TypeWhitespace: WhiteSpace -> skip;
TypeTermination: Termination -> skip;
TypeName: Name -> mode(MAYBE_EXTENDED_TYPE_MODE);

mode MAYBE_EXTENDED_TYPE_MODE;
MaybeTypeExtWhitespace: WhiteSpace -> skip;
MaybeTypeExtTermination: Termination -> skip;
TypeExtend: '<:' -> mode(DEFAULT_MODE);
Escape: . {
    SeekDelta(-1); //Ignore Token
    Skip();
    Mode(DEFAULT_MODE); 
};

mode MODULE_MODE;
ModuleName: Name -> mode(DEFAULT_MODE);
ModuleTermination: Termination -> skip;
ModuleWhitespace: WhiteSpace -> skip;