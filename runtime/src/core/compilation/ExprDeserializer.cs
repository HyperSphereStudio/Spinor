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

public unsafe class ExprDeserializer : IImplSpinorWalker<SAny, SExpr> {
   private byte* _memoryPtr;
   internal SerializedExprContext Cp;
   
   public void Deserialize(byte[] memory, SerializedExprContext cp) {
      Cp = cp;
      fixed (byte* ptr = memory) {
         _memoryPtr = ptr;
         ((ISpinorExprWalker<SAny, SExpr>) this).Walk(new(new(_memoryPtr), this));
      }
   }
   
   public ExprType GetType(SAny a) => a.Type;
   public void ToLineNumberNode(SAny a, out int line, out string file) {
      Ptr p = a.Unbox;
      line = ReadMemory<int>(p);
      file = ReadString(new(p, sizeof(int)));
   }
   public T ReadMemory<T>(SAny a) where T:unmanaged => ReadMemory<T>(a.Unbox);
   public T ReadMemory<T>(Ptr p) where T:unmanaged => p.Load<T>();
   public Symbol ReadSymbol(Ptr p) => Cp.LoadSymbol(p.Load<int>());
   public string ReadString(Ptr p) => Cp.LoadString(p.Load<int>());
   public Symbol ToSymbol(SAny a) => ReadSymbol(a.Unbox);
   public long ToInteger(SAny a) => ReadMemory<long>(a);
   public double ToFloat(SAny a) => ReadMemory<double>(a);
   public bool ToBool(SAny a) => ReadMemory<bool>(a);
   public SExpr ToExpr(SAny a) => new(a.Unbox, this);
}