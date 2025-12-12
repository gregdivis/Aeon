using System.Numerics;
using System.Runtime.CompilerServices;

namespace Aeon.Emulator.Instructions.BitwiseLogic;

internal static class Xor
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Opcode("34 al,ib|80/6 rmb,ib|30/r rmb,rb|32/r rb,rmb|35 ax,iw|81/6 rmw,iw|83/6 rmw,ibx|31/r rmw,rw|33/r rw,rmw", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void GenericXor<TValue>(Processor p, ref TValue dest, TValue src) where TValue : unmanaged, IBinaryInteger<TValue>
    {
        dest ^= src;
        p.Flags.Update_Value(dest);
        p.Flags.Carry = false;
        p.Flags.Overflow = false;
    }
}
