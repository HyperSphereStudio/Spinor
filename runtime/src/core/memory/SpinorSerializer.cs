/*
   * Author : Johnathan Bizzano
   * Created : Wednesday, February 22, 2023
   * Last Modified : Wednesday, February 22, 2023
*/

using System;
using System.Collections.Generic;
using System.IO;
using Core;
using runtime.Utils;

namespace runtime.core.memory;

internal record struct SerializedTypeInformation(int TypeIndex);

public class SpinorSerializer {
   private readonly MemoryStream _stream = new();

   private readonly Dictionary<SType, SerializedTypeInformation> _typeMap = new() {
       { Symbol.RuntimeType, new SerializedTypeInformation(1) }
   };

   private readonly InternContainer<Any> _refVals = new();

   public void Serialize(Any a) {
       Write(RegisterType(a.Type));
       a.Serialize(this);
   }
   
   public void WriteRef(Any a) => Write(_refVals.GetIndex(a));

   public unsafe void Write<T>(T v) where T : unmanaged => _stream.Write(new ReadOnlySpan<byte>(&v, sizeof(T)));

   public int RegisterType(SType t) {
       if (_typeMap.TryGetValue(t, out var i)) 
           return i.TypeIndex;
       _typeMap.Add(t, i);
       i = new(_typeMap.Count);
       return i.TypeIndex;
   }
}