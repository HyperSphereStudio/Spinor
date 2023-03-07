/*
   * Author : Johnathan Bizzano
   * Created : Monday, January 2, 2023
   * Last Modified : Monday, January 2, 2023
*/

parser grammar SpinorParser;

options { language=CSharp; superClass=SuperSpinorParser; tokenVocab=SpinorLexer; }

@header{using runtime.core.parse;}

topExpr: exprBlock EOF;
exprBlock: Termination* (expr[0] Termination*)* expr[0]?;

primaryExpr: 
    Name tuple                                                                              #functionCall
  | mutable=MUTABLE? STRUCT Name {SpinorState = SpinorState.Expression;} exprBlock END      #struct
  | (MODULE | bare=BAREMODULE) Name exprBlock END                                           #module
  | PRIMITIVE TYPE name=Name (EXTEND extends=Name)? integer END                             #primitive
  | (ABSTRACT|BUILTIN) TYPE name=Name (EXTEND extends=Name)? END                            #abstractOrBuiltin
  | head=(QUOTE|BEGIN) exprBlock END                                                        #block
  | tuple                                                                                   #tupleExpr
  | Name                                                                                    #nameExpr
  | literal                                                                                 #literalExpr
;

expr[int p]: primaryExpr ({OperatorPrecedence >= $p}? BinaryOrAssignableOp expr[NextOperatorPrecedence])*;

tuple: RPAR (expr[0] COMMA)* expr[0] LPAR;
integer: DIGIT+;  
float: DIGIT+ DOT DIGIT+;
                  
literal:
          float
      |   integer
; 
