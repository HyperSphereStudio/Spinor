using System;
using System.Collections.Generic;
using runtime.core.Abstract;
using runtime.Utils;

namespace runtime.core.Dynamic.JIL
{
    public class JILWriteableContext : JILWritingContext{
        private readonly InternedCollection<string> Strings = new();
        private readonly InternedCollection<IJType> Types = new();
        private readonly InternedCollection<IJModule> Modules = new();
        
        public string GetString(int index) => Strings.Get(index);
        public IJType GetType(int index) => Types.Get(index);
        public IJModule GetModule(int index) => Modules.Get(index);

        public IntPtr LoadDynamic(int index) => throw new NotImplementedException();
        public IJModule GetModule() => throw new NotImplementedException();
        public IntPtr GetCurrentLocation() => throw new NotImplementedException();
        public int LoadString(string s) => Strings.Load(s);
        public int LoadType(IJType t) => Types.Load(t);
        public int LoadModule(IJModule m) => Modules.Load(m);

        internal bool UsingModule(IJModule m) => Modules.Contains(m);
        internal List<IJModule> GetModuleList() => Modules.DataList;
    }
}