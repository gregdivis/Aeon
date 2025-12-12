using System.Numerics;
using System.Runtime.CompilerServices;

namespace Aeon.Emulator.Instructions;

internal static class Cmp
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Opcode("38/r rmb,rb|3A/r rb,rmb|3C al,ib|80/7 rmb,ib|82/7 rmb,ib|39/r rmw,rw|3B/r rw,rmw|3D ax,iw|81/7 rmw,iw|83/7 rmw,ibx", AddressSize = 16 | 32, OperandSize = 16 | 32)]
    public static void GenericCompare<TValue>(Processor p, TValue value1, TValue value2) where TValue : unmanaged, IBinaryInteger<TValue>
    {
        var result = value1 - value2;
        p.Flags.Update_Sub(value1, value2, result);
    }
}

internal static class Test
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Opcode("A8 al,ib|F6/0 rmb,ib|84/r rmb,rb|A9 ax,iw|F7/0 rmw,iw|85/r rmw,rw|F6/1 rmb,ib", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void GenericTest<TValue>(Processor p, TValue value1, TValue value2) where TValue : unmanaged, IBinaryInteger<TValue>
    {
        var result = value1 & value2;
        p.Flags.Update_Value(result);
        p.Flags.Carry = false;
        p.Flags.Overflow = false;
    }
}
