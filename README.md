# juliapp
.NET Runtime for Julia++
VERY EXPERIMENTAL, MOSTLY NOT FUNCTIONAL AT THE MOMENT!
Designed to implement julia into .NET. Will translate julia into JIL (Julia Intermediate) which will then lower to IL or Machine Code on type specialization

Here are some of the resources that my project is composed of:
Runtime, Code Generation & Execution Platform: .NET 6 (Statically Typed Language)
Common Intermediate Language (CIL). JRM lowered to CIL on compilation.
Parser, Lexer & AST Walker: ANTLR 
Base Language: Julia (Dynamically Typed Language) 

Language Project:
Julia++ Runtime: Required to run Julia++ Code. Contains the parser, JIL compiler and other tools that dynamically link together the program during runtime. 

Julia++ Grammer. The grammar defines the structure of the language and when compiled to code with ANTLR, will generate a parser, lexer and AST walker that will be used by the Julia++ Static Compiler to Julia Intermediate Language (JIL) I am currently working on making it Julia Compatible before adding my own extensions.

Code Model. Used to represent the Julia++ in a compact form

Julia++ Intermediate Language (JIL). Stack-based dynamic code form that contains name references rather than static typing. The name references point to a predetermined location that is analogous to a global variable that can be rewritten through the lifespan of the program. JIL cannot be directly executed as it is meant to be read-only and compact.

Julia++ Runtime Model. Converts JIL to JRM. The JRM can be directly interpreted (JIL Interpreter) or lowered to CIL (JIL Compiler). 

ILCompiler. Used by the JIL Compiler to generate IL code.

Testing. Where the Julia++ Runtime is tested to make sure it is valid.

Julia Standard Library (JSL). During Runtime Compilation, the JSL will be compiled to JIL and compacted into a jstl.cjpp file that contains the code model of the JSL and precompiles various methods to speed up runtime execution.

Julia++ Standard Library (PSTL). Extension of the JSL for the Julia++ Language

This is my current project structure. All of them still require a lot of work but this is my road map. I will complete the projects in the following order: 
JIL, JRM, JIL Interpreter (Currently Working On). Create JRM of basic program(s) that is able to be successfully interpreted

JIL Compiler.  Create JRM of basic program(s) that is able to be successfully compiled.
Julia Grammer. Make it Julia v1.9 Compatible and be able to lower to JIL.

JIL Dynamic Compiler & Julia Expr Lib. (Julia contains macros that essentially insert code during parsing, this will be an extension of the JIL Static Compiler).

JIL serializer to .cjpp files

Julia STL Modification. The JSL will need to be modified in order to make it work in the new environment

Survey Math, Physics & CS Faculty about features they would like to see in a language

Julia++ Grammer. Extend the Julia Grammer to custom features

Julia++ Standard Library. Creation of the Julia++ STL. 

Let me know if you have any questions or advice, this is a very complex project and this is just my current plan for tackling this complex problem!



