parser grammar SpinorParser;

options { language=CSharp; superClass=SuperSpinorParser; tokenVocab=SpinorLexer;}

@header{using runtime.parse;}

topExpr: exprBlock EOF;
exprBlock: Termination* (expr[0] Termination*)* expr[0]?;

primaryExpr: 
    RPAR (expr[0] COMMA)+ expr[0] LPAR                              #tuple
  | RPAR expr[0] LPAR                                               #parenthetical
  | mutable=MUTABLE? STRUCT ID exprBlock END                        #struct
  | (MODULE | bare=BAREMODULE) ID exprBlock END                     #module
  | head=(QUOTE|BEGIN) exprBlock END                                #block
  | ID                                                              #name
  | literal                                                         #literalExpr
;

expr[int p]: primaryExpr ({OperatorPrecedence >= $p}? BinaryOrAssignableOp expr[NextOperatorPrecedence])*;
                  
literal:
          DIGIT+ DOT DIGIT+    #float
      |   DIGIT+               #integer
; 
