using System;
using System.Runtime.CompilerServices;
using runtime.core.Abstract;

namespace runtime.core.Dynamic.JIL
{
    public interface JILReadingContext {
        public string GetString(int index);
        public IJType GetType(int index);
        public IJModule GetModule(int index);
        
        public IntPtr LoadDynamic(int index);

        public T LoadDynamic<T>(int index) {
            unsafe {
                return Unsafe.Read<T>((void*) LoadDynamic(index));
            }
        }
        
        public IJModule GetModule();
        public IntPtr GetCurrentLocation();
    }
}