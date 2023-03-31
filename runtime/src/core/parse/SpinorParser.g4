/*
   * Author : Johnathan Bizzano
   * Created : Monday, January 2, 2023
   * Last Modified : Monday, January 2, 2023
*/

parser grammar SpinorParser;

options { language=CSharp; superClass=SuperSpinorParser; tokenVocab=SpinorLexer; }

@header{using runtime.core.parse;}

topExpr: exprBlock EOF;
exprBlock: ExprTermination* (expr[0] ExprTermination*)*;

primitiveExpr: 
    ExprName tuple                                                                    #functionCall
  
  | (Module|BareModule) ModuleName exprBlock ExprEnd                                  #module
  
  | MUTABLE? Struct TypeName (TypeExtend ext=primitiveExpr)? exprBlock ExprEnd        #struct
  | ABSTRACT Type TypeName (TypeExtend ext=primitiveExpr)? ExprEnd                    #abstract
  
  | head=(QUOTE|BEGIN) exprBlock ExprEnd                                              #block
  
  | tuple                                                                             #tupleExpr
  | ExprName (ELEMENTOF elemOf=primitiveExpr)?                                        #name
  
  | SYSTEM? Using (UsingName UsingDot)* UsingName                                     #using
  | SYSTEM? Import ((ImportName ImportDot)* ImportName) importName?                   #importExpr                                 
  
  | literal                                                                           #literalExpr
;
                           
expr[int p]: 
    primitiveExpr 
    //Binary Operation Portion
    ({TargetPrecedence($p)}? BinaryOrAssignableOp expr[NextOperatorPrecedence])*;
    
            //(1, 2, 3...)                (1,)
tuple: RPAR ((expr[0] COMMA)* expr[0])? LPAR;

string: String stringPart* StringEnd;
stringPart:
  StringText                                          #StrText
  | StringInterpName                                  #StrNameInterp
  | StringInterpExprBegin expr[0] LPAR                #StrExpr
;
                  
literal:
       ExprFloatingPoint     #floatingPoint
    |  ExprInteger           #integer
    |  ExprSymbol            #symbol
    |  string                #str
; 

importName: ImportAs ImportName;