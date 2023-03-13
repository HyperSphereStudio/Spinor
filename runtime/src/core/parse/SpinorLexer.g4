/*
   * Author : Johnathan Bizzano
   * Created : Monday, January 2, 2023
   * Last Modified : Monday, January 2, 2023
*/

lexer grammar SpinorLexer;

options { language=CSharp; superClass=SuperSpinorLexer; }
@header{using runtime.core.parse;}

QUOTE: 'quote';
BEGIN: 'begin';
DIGIT: [0-9];
COND: '?';
DOT: '.';
END: 'end' {BinaryOpPossible = true;};
BAREMODULE: 'baremodule';
MODULE: 'module';
MUTABLE: 'mutable';
STRUCT: 'struct';
RPAR: '(';
LPAR: ')';
COMMA: ',';
TYPE: 'type';
ABSTRACT: 'abstract' {BinaryOpPossible = false;};
PRIMITIVE: 'primitive' {BinaryOpPossible = false;};
BUILTIN: 'abstractbuiltin' {BinaryOpPossible = false;};
EXTEND: '<:';

BinaryOrAssignableOp: {IsBinaryOrAssignableOp()}? .;

fragment IdentifierPrefixCharacter: [a-zA-Z_!];
fragment IdentifierSuffixCharacter: [a-zA-Z0-9_!];

Name: IdentifierPrefixCharacter IdentifierSuffixCharacter*;  
Termination: (NewLine | ';')+ | EOF;

NewLine: ('\r' '\n'? | '\n');
Whitespace: [ \t]+ -> skip;
BlockComment: '#=' .*? '=#' -> skip;
LineComment: '#' ~[\r\n]* -> skip;   