/*
   * Author : Johnathan Bizzano
   * Created : Monday, January 2, 2023
   * Last Modified : Monday, January 2, 2023
*/

global using SerializationPointer = System.Int32;

using System;
using System.IO;
using Core;
using runtime.core.parse;
using runtime.stdlib;
using runtime.Utils;

namespace runtime.core.Compilation;

public class SerializedExprContext {
    public readonly InternContainer<string> StringPool = new();
    public readonly InternContainer<Symbol> SymbolPool = new();
    
    public int InternString(string s) => StringPool.Load(s);
    public int InternSymbol(Symbol s) => SymbolPool.Load(s);
    public string LoadString(int i) => StringPool.Get(i);
    public Symbol LoadSymbol(int i) => SymbolPool.Get(i);
}

public class ExprSerializer : IExprWalker, IImplSpinorWalker<Any, Expr> {
    private MemoryStream _ms;
    private readonly SerializedExprContext _cp = new();
    
    public ExprSerializer(){}
    public byte[] Serialize(Any expr, out SerializedExprContext ctx) {
        _ms = new();
        ((ISpinorExprWalker<Any, Expr>) this).Walk(expr);
        ctx = _cp;
        return _ms.ToArray();
    }
    
    public void WalkExpr(Expr e, bool innerExpr) {
        WriteType(ExprType.Expr);
        WriteSymbol(e.Head);
        WriteAnyArray(new(e, 0));
    }
    
    public void WriteAnyArray(ExprArgWalker<Any, Expr> items) {
        var startExprP = _ms.Position;
        WriteMemory((ushort) items.Length);

        //Store the position indexes of the arguments
        var argTable = _ms.Position;
        var tblSize = sizeof(SerializationPointer) * items.Length;
        _ms.SetLength(_ms.Length + tblSize);
        _ms.Position += tblSize;
        
        int index = 0;
        foreach (var b in items) {
            var bp = _ms.Position;
            ((ISpinorExprWalker<Any, Expr>) this).Walk(b, false);
            var cp = _ms.Position;
            _ms.Position = argTable + sizeof(SerializationPointer) * index++;
            WritePtr((SerializationPointer) (bp - startExprP));
            _ms.Position = cp;
        }
    }
    public void WritePtr(SerializationPointer ptr) => WriteMemory(ptr);
    public void WalkLineNumberNode(int line, string file) {
        WriteType(ExprType.LineNumberNode);
        WriteMemory(line);
        WriteString(file);
    }

    public void WalkSymbol(Symbol s) {
        WriteType(ExprType.Symbol);
        WriteSymbol(s);
    }
    public void WalkBool(bool b) => WalkMemory(b, ExprType.Bool);
    public void WalkInteger(long l) => WalkMemory(l, ExprType.Float);
    public void WalkFloat(double d) => WalkMemory(d, ExprType.Float);
    public void WriteType(ExprType e) => WriteMemory(e);
    public void WriteSymbol(Symbol s) => WriteMemory(_cp.InternSymbol(s));
    public unsafe void WriteMemory<T>(T t) where T : unmanaged => _ms.Write(new ReadOnlySpan<byte>(&t, sizeof(T)));
    public void WalkMemory<T>(T t, ExprType e) where T : unmanaged{
        WriteType(e);
        WriteMemory(t);
    }
    public void WriteString(string s) => WriteMemory(_cp.InternString(s));
}