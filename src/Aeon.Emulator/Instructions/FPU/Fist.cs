using System.Runtime.CompilerServices;

namespace Aeon.Emulator.Instructions.FPU;

internal static class Fist
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Opcode("DF/2 m16", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void StoreInt16(Processor p, out short dest)
    {
        var res = p.FPU.Round(p.FPU.ST0);
        if (res <= short.MinValue)
            dest = short.MinValue;
        else if (res >= short.MaxValue)
            dest = short.MaxValue;
        else
            dest = (short)res;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Opcode("DB/2 m32", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void StoreInt32(Processor p, out int dest)
    {
        var res = p.FPU.Round(p.FPU.ST0);
        if (res <= int.MinValue)
            dest = int.MinValue;
        else if (res >= int.MaxValue)
            dest = int.MaxValue;
        else
            dest = (int)res;
    }
}

internal static class Fistp
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Opcode("DF/3 m16", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void StoreInt16(Processor p, out short dest)
    {
        Fist.StoreInt16(p, out dest);
        p.FPU.Pop();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Opcode("DB/3 m32", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void StoreInt32(Processor p, out int dest)
    {
        Fist.StoreInt32(p, out dest);
        p.FPU.Pop();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Opcode("DF/7 m64", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void StoreInt64(Processor p, out long dest)
    {
        dest = (long)p.FPU.Round(p.FPU.ST0);
        p.FPU.Pop();
    }
}
