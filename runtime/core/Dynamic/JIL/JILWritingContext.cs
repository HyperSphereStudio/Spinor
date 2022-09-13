using runtime.core.Abstract;

namespace runtime.core.Dynamic.JIL
{
    public interface JILWritingContext : JILReadingContext
    {
        public int LoadString(string s);
        public int LoadType(IJType t);
        public int LoadModule(IJModule m);
    }
}