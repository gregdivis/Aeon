using System.Numerics;
using System.Runtime.CompilerServices;

namespace Aeon.Emulator.Instructions.BitwiseLogic;

internal static class And
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Opcode("20/r rmb,rb|22/r rb,rmb|24 al,ib|80/4 rmb,ib|21/r rmw,rw|23/r rw,rmw|25 ax,iw|81/4 rmw,iw|83/4 rmw,ibx", AddressSize = 16 | 32, OperandSize = 16 | 32)]
    public static void GenericAnd<TValue>(Processor p, ref TValue dest, TValue src) where TValue : unmanaged, IBinaryInteger<TValue>
    {
        dest &= src;
        p.Flags.Update_Value(dest);
        p.Flags.Carry = false;
        p.Flags.Overflow = false;
    }
}
