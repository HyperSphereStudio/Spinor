using System;
using System.Text;

namespace runtime.core.JIL;

public class JILPrinter
{
    private int _indent = 0;
    private bool _hasIdented = true;

    public bool Dump;
    private readonly StringBuilder _sb = new();

    public JILPrinter(bool dump = true) => Dump = dump;

    public void IncIndent() => _indent++;
    public void DecIndent() => _indent--;

    public JILPrinter Print(string s)
    {
        if (!_hasIdented)
        {
            _sb.Capacity += _indent;
            for (int i = 0; i < _indent; i++)
                _sb.Append("\t");
            _hasIdented = true;
        }

        _sb.Append(s);
        return this;
    }

    public JILPrinter Println(string s = "")
    {
        Print(s);
        Print("\n");
        _hasIdented = false;
        return this;
    }

    public static unsafe void PrintCode(IJCodeContext ctx, byte[] code, JILPrinter io) {
        fixed (byte* ptr = code) {
            byte* mptr = ptr;
            var eptr = ptr + code.Length;
            while (mptr < eptr)
                PrintCode(ctx, ref mptr, io);
        }
    }

    private static unsafe void PrintCode(IJCodeContext ctx, ref byte* p, JILPrinter io)
    {
        switch (*p)
        {
            case JOp.LoadTypeOp:
                Reader.ReadLoadType(ref p, ctx, out var loadedType);
             
                if (io.Dump)
                    io.Println("#=Instantiate Type=#");
                PrintType(loadedType, io);
                return;
        }
    }

    public static unsafe void PrintBytes(byte* bytes, int length)
    {
        for (int i = 0; i < length; i++)
            Console.Write(bytes[i] + " ");
        Console.WriteLine();
    }

    private static unsafe string GetFieldTypeName(IJField f) {
        if (f is JILField jf)
            return f.Parent.GetNameField(jf.TypeRef).Name;
        return f.Type.Name;
    }
    
    public static void PrintType(IJType t, JILPrinter io)
    {
        if (t.Type == JTypeType.Struct)
            io.Print("struct ");
        else if (t.Type == JTypeType.Mutable)
            io.Print("mutable struct ");

        io.Println(t.Name).IncIndent();

        t.VisitFields(x => {
            if (x.IsConst)
                io.Print("const ");
                
            io.Println(x.Name);
            var tyName = GetFieldTypeName(x);
            
            if (tyName != "Any") {
                io.Print("::").Println(tyName);
            }

            return true;
        });
        
        t.VisitConstructors(x => {
            PrintMethod(t.Name, x, io);
            return true;
        });

        io.DecIndent();
        io.Println().Println("end");
    }

    public static void PrintMethod(string name, IJMethod m, JILPrinter io) {
        
    }

    public override string ToString() => _sb.ToString();
}