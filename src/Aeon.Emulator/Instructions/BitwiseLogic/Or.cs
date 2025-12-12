using System.Numerics;
using System.Runtime.CompilerServices;

namespace Aeon.Emulator.Instructions.BitwiseLogic;

internal static class Or
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Opcode("08/r rmb,rb|0A/r rb,rmb|0C al,ib|80/1 rmb,ib|09/r rmw,rw|0B/r rw,rmw|0D ax,iw|81/1 rmw,iw|83/1 rmw,ibx", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void GenericOr<TValue>(Processor p, ref TValue dest, TValue src) where TValue : unmanaged, IBinaryInteger<TValue>
    {
        dest |= src;
        p.Flags.Update_Value(dest);
        p.Flags.Carry = false;
        p.Flags.Overflow = false;
    }
}
