using System;
using System.Reflection;
using runtime.ILCompiler;

namespace runtime.core
{
    public class JGlobalRuntimeFieldInfo {
        public readonly bool IsConst;
        private readonly dynamic _specializedGlobalInfo;
        
        public object Value {
            get => _specializedGlobalInfo.Data;
            set => _specializedGlobalInfo.Data = value;
        }
        
        public FieldInfo FieldInfo => _specializedGlobalInfo.FieldInfo;

        public JGlobalHandle<T> GetHandle<T>() => (JGlobalHandle<T>) _specializedGlobalInfo;

        internal JGlobalRuntimeFieldInfo(object o, bool isConst) {
            _specializedGlobalInfo = o;
            IsConst = isConst;
        }
        internal void WriteOptimizeGetCode(IlExprBuilder b) => _specializedGlobalInfo.WriteOptimizeGetCode(b);
        internal void WriteOptimizeSetCode(IlExprBuilder b) => _specializedGlobalInfo.WriteOptimizeSetCode(b);
        internal static JGlobalRuntimeFieldInfo Create<T>(bool isConst) => new (new JGlobalHandle<T>(), isConst);
    }
    
    public class JGlobalHandle<T> {
        public T Data;
        public Type FieldType => typeof(T);
        public FieldInfo FieldInfo => GetType().GetField("Data");
        internal void WriteOptimizeGetCode(IlExprBuilder b) => b.Load.FieldValue(FieldInfo);
        internal void WriteOptimizeSetCode(IlExprBuilder b) => b.Store.Field(FieldInfo);
    }
}