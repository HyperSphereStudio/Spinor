grammar Julia;

script: (EOF | ((moduleExpr | moduleExprStatement+) EOF));

moduleExpr: Termination? (structure | moduleVariableDeclaration | module | usingModule | blockExpr);
moduleVariableDeclaration: Const? Local? blockArg blockVariableInstatiation?;
module: Module Identifier moduleExpr* endExpr;
moduleExprStatement: moduleExpr Termination;
usingModule: Using moduleRef;
moduleRef: (Identifier Dot)* Identifier;
moduleIdentifier: moduleRef Dot Identifier;
symbolIdentifier: moduleIdentifier | Identifier;

abstractStructure: AbstractType;
compositeStructure: Mutable? Struct;
structure: (abstractStructure | compositeStructure) typeName structItem* endExpr;
structField: Const? blockArg;
structItem: Termination? (function | structField) Termination;

blockExpr: functionCall | blockVariableDeclaration;
blockExprStatement: blockExpr Termination;
blockVariableInstatiation: Assignment blockExpr;
blockArg: Identifier (ValueType type)?;
blockVariableDeclaration: Const? (Global | Local)? blockArg blockVariableInstatiation?;

function: shortFunction | longFunction;
functionHeader:(moduleRef | Identifier) namedTuple;
functionItem: blockExprStatement Termination;
functionCall: symbolIdentifier tuple;
shortFunction: functionHeader Assign functionItem;
longFunction: Function functionHeader Termination functionItem* endExpr;

tuple: RightParen tupleList LeftParen;
namedTuple: RightParen ((blockArg Comma)* blockArg)? LeftParen;
typetuple: RightParen tupleList Comma LeftParen;
tupleList: (blockExpr Comma)* Comma blockExpr;

typeName: Identifier (Extend type)?;
parameterizedType: Identifier (RightBrace type LeftBrace)+;
type: Identifier | parameterizedType;
endExpr: Termination? End;

Symbol: Colon IdentifierSuffixCharacter*;

AbstractType: 'abstract' 'type';
Using: 'using';
Extend: '<:';
Global: 'global';
Const: 'const';
Local: 'local';
Mutable: 'mutable';
Function: 'function';
Module: 'module';
Struct: 'struct';
For: 'for';
Goto: 'goto';
If: 'if';
Else: 'else';
ElseIf: 'elseif';
Return: 'return';
Continue: 'continue';
Do: 'do';
End: 'end';

LeftParen: '(';
RightParen: ')';
LeftBracket: '[';
RightBracket: ']';
LeftBrace: '{';
RightBrace: '}';

//Operators
And : '&';
Or : '|';
Caret : '^';
Not : '!';
Tilde : '~';
Plus : '+';
Minus : '-';
Star : '*';
Div : '/';
Mod : '%';
LeftShift : '<<';
RightShift : '>>';

Operator: And | Or | Caret | Not | Tilde | Plus | Minus | Div | Mod | LeftShift | RightShift;

//Comparison Operators
Less : '<';
LessEqual : '<=';
Greater : '>';
GreaterEqual : '>=';

ComparisonOperator: Less | LessEqual | Greater | GreaterEqual;

//Shortcut Operators
ShortAnd : '&&';
ShortOr : '||';

ShortcutOperator: ShortAnd | ShortOr;

Question: '?';
Colon: ':';
ValueType: '::';
Semi: ';';
Comma: ',';
Assign: '=';
Arrow: '->';
Dot: '.';
Splat: '...';

Termination: (NewLine | Semi)+;

AugmentedAssignment: Operator Assignment;
Assignment: Assign | AugmentedAssignment;
Constant: IntegerConstant | DecimalConstant;
Identifier: IdentifierPrefixCharacter IdentifierSuffixCharacter*;

IntegerConstant: Digit+;
DecimalConstant: Digit+ Dot Digit+;

fragment Digit: [0-9];
fragment IdentifierPrefixCharacter: [a-zA-Z_!];
fragment IdentifierSuffixCharacter: [a-zA-Z0-9_!];

NewLine: ('\r' '\n'? | '\n');
Whitespace: [ \t]+ -> skip;
BlockComment: '#=' .*? '=#' -> skip;
LineComment: '#' ~[\r\n]* -> skip;