/*
   * Author : Johnathan Bizzano
   * Created : Monday, January 2, 2023
   * Last Modified : Monday, January 2, 2023
*/

parser grammar SpinorParser;

options { language=CSharp; superClass=SuperSpinorParser; tokenVocab=SpinorLexer;}

@header{using runtime.parse;}

topExpr: exprBlock EOF;
exprBlock: Termination* (expr[0] Termination*)* expr[0]?;

primaryExpr: 
    ID tuple                                                        #functionCall
  | mutable=MUTABLE? STRUCT ID exprBlock END                        #struct
  | (MODULE | bare=BAREMODULE) ID exprBlock END                     #module
  | head=(QUOTE|BEGIN) exprBlock END                                #block
  | tuple                                                           #tupleExpr
  | ID                                                              #name
  | literal                                                         #literalExpr
;

expr[int p]: primaryExpr ({OperatorPrecedence >= $p}? BinaryOrAssignableOp expr[NextOperatorPrecedence])*;

tuple: RPAR (expr[0] COMMA)* expr[0] LPAR;
                  
literal:
          DIGIT+ DOT DIGIT+    #float
      |   DIGIT+               #integer
; 
