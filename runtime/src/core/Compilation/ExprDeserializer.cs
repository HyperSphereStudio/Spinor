/*
   * Author : Johnathan Bizzano
   * Created : Monday, January 2, 2023
   * Last Modified : Monday, January 2, 2023
*/

using Core;
using runtime.stdlib;

namespace runtime.core.Compilation;

public readonly unsafe struct Ptr {
   private readonly byte* _value;
   public Ptr(byte* value) => _value = value;
   public Ptr(Ptr value, int offset) => _value = value._value + offset;
   public T Load<T>(int offset = 0) where T : unmanaged => *(T*)(_value + offset);
}
public readonly struct SAny {
   internal readonly Ptr Ptr;
   private readonly ExprDeserializer _deserializer;

   public SAny(Ptr ptr, ExprDeserializer deserializer) {
      Ptr = ptr;
      _deserializer = deserializer;
   }

   public ExprType Type => Ptr.Load<ExprType>();
   public Ptr Unbox => new(Ptr, sizeof(ExprType));
}
public readonly struct SAnyArray {
   internal readonly Ptr Ptr;
   internal readonly ExprDeserializer Deserializer;

   public SAnyArray(Ptr ptr, ExprDeserializer deserializer) {
      Ptr = ptr;
      Deserializer = deserializer;
   }
   public int Length => Ptr.Load<ushort>();
   public Ptr PositionTableStart => new(Ptr, sizeof(ushort));
   public SAny this[int i] => new(new(Ptr, PositionTableStart.Load<SerializationPointer>(i * sizeof(SerializationPointer))), Deserializer);
}
public readonly struct SExpr : IExpr<SAny> {
   private readonly SAnyArray _a;

   public SExpr(Ptr ptr, ExprDeserializer deserializer) => _a = new(new(ptr, sizeof(int)), deserializer);
   public Symbol Head => _a.Deserializer.ReadSymbol( new(_a.Ptr, -sizeof(int)));
   public int ArgCount => _a.Length;
   internal Ptr Ptr => _a.Ptr;
   public SAny this[int i] => _a[i];
}

public unsafe class ExprDeserializer : SpinorExprWalker<SAny, SExpr> {
   private byte* _memoryPtr;
   internal IExprLargeConstantPool Cp;
   
   public void Deserialize(byte[] memory, IExprLargeConstantPool cp) {
      Cp = cp;
      fixed (byte* ptr = memory) {
         _memoryPtr = ptr;
         Walk(new(new(_memoryPtr), this));
      }
   }

   public override ExprType GetType(SAny a) => a.Type;
   public override void ToLineNumberNode(SAny a, out int line, out string file) {
      Ptr p = a.Unbox;
      line = ReadMemory<int>(p);
      file = ReadString(new(p, sizeof(int)));
   }
   public T ReadMemory<T>(SAny a) where T:unmanaged => ReadMemory<T>(a.Unbox);
   public T ReadMemory<T>(Ptr p) where T:unmanaged => p.Load<T>();
   public Symbol ReadSymbol(Ptr p) => Cp.LoadSymbol(p.Load<int>());
   public string ReadString(Ptr p) => Cp.LoadString(p.Load<int>());
   public override Symbol ToSymbol(SAny a) => ReadSymbol(a.Unbox);
   public override long ToInteger(SAny a) => ReadMemory<long>(a);
   public override double ToFloat(SAny a) => ReadMemory<double>(a);
   public override bool ToBool(SAny a) => ReadMemory<bool>(a);
   public override SExpr ToExpr(SAny a) => new(a.Unbox, this);

   public override void WalkOperatorCall(bool innerExpr, SpinorOperator op, ExprArgWalker<SAny, SExpr> args) => throw new System.NotImplementedException();
   public override void WalkAssignment(SpinorOperator op, SAny lhs, SAny rhs) => throw new System.NotImplementedException();
   public override void WalkModule(bool isBare, Symbol name, SExpr block) => throw new System.NotImplementedException();
   public override void WalkSymbol(Symbol s)  => throw new System.NotImplementedException();
   public override void WalkBool(bool b) => throw new System.NotImplementedException();
   public override void WalkInteger(long l) => throw new System.NotImplementedException();
   public override void WalkFloat(double d)  => throw new System.NotImplementedException();
   public override void WalkStruct(bool isMutable, Symbol name, SExpr block) => throw new System.NotImplementedException();
   public override void WalkBlock(Symbol head, ExprArgWalker<SAny, SExpr> items) => throw new System.NotImplementedException();
   public override void WalkCall(Symbol function, ExprArgWalker<SAny, SExpr> args) => throw new System.NotImplementedException();
   public override void WalkLineNumberNode(int line, string file) => throw new System.NotImplementedException();
   public override void WalkTuple(ExprArgWalker<SAny, SExpr> items) => throw new System.NotImplementedException();
}