using System;
using System.Text;

namespace runtime.core
{
    public class JuliaException : Exception
    {
        public JuliaException(string message) => Message = message;
        public JuliaException() => Message = "";
        
        public JuliaException(params object[] messages) {
            StringBuilder sb = new StringBuilder();
            foreach(var v in messages)
                sb.Append(v);
            Message = sb.ToString();
        }

        public override string Message { get; }
    }
}