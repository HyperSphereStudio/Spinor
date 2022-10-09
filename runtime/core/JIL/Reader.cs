namespace runtime.core.JIL;

public class Reader
{
    private static unsafe byte* ReadD(ref byte* p)
    {
        var b = p + 1;
        p += JOp.OpCodeTable[*p].OperandsSize;
        return b;
    }

    private static unsafe T ReadData<T>(ref byte* ptr) where T : unmanaged
    {
        var t = (T*) ptr;
        var v = *t++;
        ptr = (byte*) t;
        return v;
    }

    private static unsafe T ReadData<T>(byte* ptr) where T : unmanaged => *(T*)ptr;

    public static unsafe void ReadLoadType(ref byte* p, IJCodeContext ctx, out IJType loadedType) {
        var d = ReadData<int>(ReadD(ref p));
        loadedType = ctx.GetCtxType(d);
        if (loadedType == null)
            throw new InternalJuliaException("Type Not Loaded!");
    }
}