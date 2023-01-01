lexer grammar SpinorLexer;

options { language=CSharp; superClass=SuperSpinorLexer; }
@header{using runtime.parse;}

BinaryOrAssignableOp: {IsBinaryOrAssignableOp()}? .;

QUOTE: 'quote';
BEGIN: 'begin';
DIGIT: [0-9];
COND: '?';
DOT: '.';
END: 'end';
BAREMODULE: 'baremodule';
MODULE: 'module';
MUTABLE: 'mutable';
STRUCT: 'struct';
RPAR: '(';
LPAR: ')';
COMMA: ',';

fragment IdentifierPrefixCharacter: [a-zA-Z_!];
fragment IdentifierSuffixCharacter: [a-zA-Z0-9_!];

ID: IdentifierPrefixCharacter IdentifierSuffixCharacter*;  
Termination: (NewLine | ';')+ | EOF;

NewLine: ('\r' '\n'? | '\n');
Whitespace: [ \t]+ -> skip;
BlockComment: '#=' .*? '=#' -> skip;
LineComment: '#' ~[\r\n]* -> skip;   

