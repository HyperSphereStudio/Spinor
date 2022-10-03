using System;
using System.Text;

namespace runtime.core
{
    public class JuliaException : Exception
    {
        protected readonly string _message;
        public JuliaException(string message) => _message = message;
        public JuliaException() => _message = "";
        
        public JuliaException(params object[] messages) {
            StringBuilder sb = new StringBuilder();
            foreach(var v in messages)
                sb.Append(v);
            _message = sb.ToString();
        }

        public override string Message => _message;
    }

    public class InternalJuliaException : JuliaException
    {
        public InternalJuliaException(string message) : base(message){}
        public InternalJuliaException() : base(){}
        
        public InternalJuliaException(params object[] messages) : base(messages){}
        
        public override string Message => "Internal Julia Exception. Please report this exception!\n" + _message;
    }
}