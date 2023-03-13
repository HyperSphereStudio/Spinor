/*
   * Author : Johnathan Bizzano
   * Created : Monday, January 2, 2023
   * Last Modified : Monday, January 2, 2023
*/

parser grammar SpinorParser;

options { language=CSharp; superClass=SuperSpinorParser; tokenVocab=SpinorLexer; }

@header{using runtime.core.parse;}

topExpr: exprBlock EOF;
exprBlock: Termination* (expr Termination*)*;

primitiveExpr: 
    Name tuple                                                                              #functionCall
  | mutable=MUTABLE? STRUCT Name exprBlock END                                              #struct
  | (MODULE | bare=BAREMODULE) Name exprBlock END                                           #module
  | PRIMITIVE TYPE name=Name (EXTEND extends=Name)? integer END                             #primitive
  | (ABSTRACT|BUILTIN) TYPE name=Name (EXTEND extends=Name)? END                            #abstractOrBuiltin
  | head=(QUOTE|BEGIN) exprBlock END                                                        #block
  | tuple                                                                                   #tupleExpr
  | Name                                                                                    #nameExpr
  | literal                                                                                 #literalExpr
;

binaryExpr[int p]: lhs=primitiveExpr {TargetPrecedence($p)}? BinaryOrAssignableOp binaryExpr[NextOperatorPrecedence]* rhs=primitiveExpr;
expr: binaryExpr[0] | primitiveExpr;

tuple: RPAR (expr COMMA)* expr LPAR;
integer: DIGIT+;  
float: DIGIT+ DOT DIGIT+;
                  
literal:
          float
      |   integer
; 