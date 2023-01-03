/*
   * Author : Johnathan Bizzano
   * Created : Monday, January 2, 2023
   * Last Modified : Monday, January 2, 2023
*/

global using SerializationPointer = System.Int32;

using System;
using System.IO;
using Core;
using runtime.stdlib;

namespace runtime.core.Compilation;

public interface IExprLargeConstantPool {
    public int InternString(string s);
    public int InternSymbol(Symbol s);
    public string LoadString(int i);
    public Symbol LoadSymbol(int i);
}

public class ExprSerializer : ExprWalker {
    private MemoryStream _ms;
    private IExprLargeConstantPool _cp;
    
    public ExprSerializer(){}
    public byte[] Serialize(IExprLargeConstantPool cp, Any expr) {
        _ms = new();
        _cp = cp;
        Walk(expr);
        return _ms.ToArray();
    }
    
    public override void WalkCall(Symbol function, ExprArgWalker<Any, Expr> args) {}
    public override void WalkOperatorCall(bool innerExpr, SpinorOperator op, ExprArgWalker<Any, Expr> args) {}
    public override void WalkAssignment(SpinorOperator op, Any lhs, Any rhs) {}
    public override void WalkModule(bool isBare, Symbol name, Expr block){}
    public override void WalkStruct(bool isMutable, Symbol name, Expr block){}
    public override void WalkBlock(Symbol head, ExprArgWalker<Any, Expr> items) {}
    public override void WalkTuple(ExprArgWalker<Any, Expr> items) {}
    public override void WalkExpr(Expr e, bool innerExpr) {
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
            Walk(b, false);
            var cp = _ms.Position;
            _ms.Position = argTable + sizeof(SerializationPointer) * index++;
            WritePtr((SerializationPointer) (bp - startExprP));
            _ms.Position = cp;
        }
    }
    public void WritePtr(SerializationPointer ptr) => WriteMemory(ptr);
    public override void WalkLineNumberNode(int line, string file) {
        WriteType(ExprType.LineNumberNode);
        WriteMemory(line);
        WriteString(file);
    }

    public override void WalkSymbol(Symbol s) {
        WriteType(ExprType.Symbol);
        WriteSymbol(s);
    }
    public override void WalkBool(bool b) => WalkMemory(b, ExprType.Bool);
    public override void WalkInteger(long l) => WalkMemory(l, ExprType.Float);
    public override void WalkFloat(double d) => WalkMemory(d, ExprType.Float);
    public void WriteType(ExprType e) => WriteMemory(e);
    public void WriteSymbol(Symbol s) => WriteMemory(_cp.InternSymbol(s));
    public unsafe void WriteMemory<T>(T t) where T : unmanaged => _ms.Write(new ReadOnlySpan<byte>(&t, sizeof(T)));
    public void WalkMemory<T>(T t, ExprType e) where T : unmanaged{
        WriteType(e);
        WriteMemory(t);
    }
    public void WriteString(string s) => WriteMemory(_cp.InternString(s));
}