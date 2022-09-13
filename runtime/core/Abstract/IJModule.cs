using System;
using System.Collections.Generic;
using runtime.core.Dynamic;

namespace runtime.core.Abstract
{
    public interface IJModule
    {
        public JExecutionModule GetExecutionModule();
        public IJModule GetParentModule();
        
        public bool IsUsingModule(IJModule name);
        public void VisitUsingModules(Action<IJModule> moduleVisitor);
        
        public bool ContainsExportedSymbol(string name);
        public void VisitExportedSymbols(Action<string> exportedSymbolsVisitor);

        public bool GetGlobal<T>(string name, out JGlobalHandle<T> handle, bool throwError = false);
        public bool GetGlobal(string name, out JGlobalRuntimeFieldInfo inf, bool throwError = false);
        public void VisitGlobals(Action<KeyValuePair<string, JGlobalRuntimeFieldInfo>> globalsVisitor);
    }
}