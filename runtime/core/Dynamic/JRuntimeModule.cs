using System;
using System.Collections.Generic;
using System.Reflection;
using runtime.core.Abstract;
using runtime.core.Dynamic.JIL;

namespace runtime.core.Dynamic
{
    public class JRuntimeModule : ImplType, IJDyModule {
        
        protected readonly List<JGlobalRuntimeFieldInfo> InternalGlobalTable = new();
        protected readonly JILWriteableContext WritingContext;
        protected readonly IJModule ParentModule;
        protected HashSet<string> ExportedSymbols = new();
        protected readonly Dictionary<string, int> GlobalMap = new();
        protected JExecutionModule BaseModule;
        
        public JExecutionModule GetExecutionModule() => BaseModule;
        public IJModule GetParentModule() => ParentModule;

        internal JRuntimeModule(IJModule parentModule, string name) : base(null) {
            if (parentModule != null) BaseModule = parentModule.GetExecutionModule();
            ParentModule = parentModule;
            CreateGlobal<IJModule>(name, true);
        }

        public override Type GetNestedType(string name, BindingFlags bindingAttr) {
            if (GetGlobal(name, out JGlobalHandle<IJType> t, true))
                return (Type) (object) t;
            throw new JuliaException("Type \"" + name + "\" Does Not Exist In " + FullName);
        }

        internal bool TryGetGlobalImpl(string name, out JGlobalRuntimeFieldInfo v)
        {
            if (GlobalMap.TryGetValue(name, out int t))
            {
                v = InternalGlobalTable[t];
                return true;
            }
            v = default;
            return false;
        }

        public bool GetGlobal<T>(string name, out JGlobalHandle<T> handle, bool throwError)
        {
            if (TryGetGlobalImpl(name, out JGlobalRuntimeFieldInfo o)) {
                handle = o.GetHandle<T>();
                return true;
            }
            foreach (var uM in WritingContext.GetModuleList()) {
                if (uM.GetGlobal(name, out JGlobalHandle<T> v2, throwError)) {
                    handle = v2;
                    return true;
                }
            }
            if (throwError) throw new JuliaException("Cannot Find Global " + name);
            handle = default;
            return false;
        }
        
        public bool GetType(string name, out IJType v) {
            if (GetGlobal(name, out JGlobalHandle<IJType> val, true)) {
                v = val.Data;
                return true;
            }
            v = default;
            return false;
        }

        internal int CreateInternalGlobal<T>(string name, bool isConst) {
            if (TryGetGlobalImpl(name, out JGlobalRuntimeFieldInfo v) && v.IsConst)
                throw new JuliaException("Invalid Redefinition of Constant \"" + name + "\"");
            var i = JGlobalRuntimeFieldInfo.Create<T>(isConst);
            var idx = InternalGlobalTable.Count;
            InternalGlobalTable.Add(i);
            WritingContext.LoadString(name);
            return idx;
        }

        public JGlobalRuntimeFieldInfo CreateGlobal<T>(string name, bool isConst) {
            var idx = CreateInternalGlobal<T>(name, isConst);
            GlobalMap.Add(name, idx);
            return InternalGlobalTable[idx];
        }

        public bool IsUsingModule(IJModule mod) => WritingContext.UsingModule(mod);

        public void VisitUsingModules(Action<IJModule> moduleVisitor) {
            foreach (var v in WritingContext.GetModuleList())
                moduleVisitor(v);
        }

        public bool ContainsExportedSymbol(string name) => ExportedSymbols.Contains(name);

        public void VisitExportedSymbols(Action<string> exportedSymbolsVisitor)
        {
            foreach (var v in ExportedSymbols)
                exportedSymbolsVisitor(v);
        }
        
        public string GetString(int idx) => WritingContext.GetString(idx);
        public IJType GetType(int index) => WritingContext.GetType(index);
        public IJModule GetModule(int index) => WritingContext.GetModule(index);
        public IntPtr LoadDynamic(int index) => throw new NotImplementedException();
        public IJModule GetModule() => this;
        public IntPtr GetCurrentLocation() => throw new NotImplementedException();
        public int LoadString(string s) => WritingContext.LoadString(s);
        public int LoadType(IJType t) => WritingContext.LoadType(t);
        public int LoadModule(IJModule m) => WritingContext.LoadModule(m);

        public void VisitGlobals(Action<KeyValuePair<string, JGlobalRuntimeFieldInfo>> globalsVisitor) {
            foreach (var v in GlobalMap)
                globalsVisitor(new KeyValuePair<string, JGlobalRuntimeFieldInfo>(v.Key, InternalGlobalTable[v.Value]));
        }

        public bool GetGlobal(string name, out JGlobalRuntimeFieldInfo inf, bool throwError)
        {
            if (TryGetGlobalImpl(name, out JGlobalRuntimeFieldInfo v)) {
                inf = v;
                return true;
            }
            if (throwError) 
                throw new JuliaException("Invalid Redefinition of Constant \"" + name + "\"");
            inf = default;
            return false;
        }
    }
}