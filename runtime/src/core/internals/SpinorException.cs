/*
   * Author : Johnathan Bizzano
   * Created : Monday, January 2, 2023
   * Last Modified : Monday, January 2, 2023
*/

using System;
using System.IO;
using System.Text;

namespace runtime.core;

public class SpinorException : Exception {
    protected readonly string _message;
    public SpinorException(string message) => _message = message;
    public SpinorException() => _message = "";

    public SpinorException(string format, params object[] values) => _message = string.Format(format, values);

    public SpinorException(params object[] messages) {
        StringBuilder sb = new StringBuilder();
        foreach(var v in messages)
            sb.Append(v);
        _message = sb.ToString();
    }

    public override string Message => _message;

    public void Print() {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Error.WriteLine(_message);
        Console.ResetColor();
    }
}

public class InternalSpinorException : SpinorException {
    public InternalSpinorException(string message) : base(message){}
    public InternalSpinorException() : base(){}
        
    public InternalSpinorException(params object[] messages) : base(messages){}
        
    public override string Message => "Internal Spinor Exception. Please report this exception!\n" + _message;
    
    public void Print() {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Error.WriteLine(_message);
        Console.ResetColor();
    }
}