using System;
using System.Runtime.InteropServices;
using runtime.core.Abstract;

namespace runtime.core.Dynamic.JIL
{
    public unsafe class JILReadonlyContext : JILReadingContext
    {
        private readonly string[] Strings;
        private readonly IJType[] Types;
        private readonly IJModule[] Modules;
        private readonly byte* MemoryChunk;
        private readonly GCHandle DynamicChunkHandle;

        public JILReadonlyContext(string[] strings, IJType[] types, IJModule[] modules, byte[] dynamicChunkHandle) {
            Strings = strings;
            Types = types;
            Modules = modules;
            DynamicChunkHandle = GCHandle.Alloc(dynamicChunkHandle, GCHandleType.Pinned);
            fixed (byte* chunk = dynamicChunkHandle) MemoryChunk = chunk;
        }
        
        public string GetString(int index) => Strings[index];
        public IJType GetType(int index) => Types[index];
        public IJModule GetModule(int index) => Modules[index];

        public IntPtr LoadDynamic(int index) => new(MemoryChunk + index);
        
        public IJModule GetModule() => throw new NotImplementedException();
        public IntPtr GetCurrentLocation() => throw new NotImplementedException();
    }
}