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
    public SpinorException(string message, Exception innerException) : base(message, innerException){}
    public SpinorException(){}
    public SpinorException(string format, params object[] values) : base(string.Format(format, values)){}

    public void Print() {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Error.WriteLine(Message);
        Console.Error.WriteLine(GetBaseException().ToString());
        Console.ResetColor();
    }
}