using System.Numerics;
using System.Runtime.CompilerServices;

namespace Aeon.Emulator.Instructions.Arithmetic;

internal static class Add
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Opcode("00/r rmb,rb|02/r rb,rmb|04 al,ib|80/0 rmb,ib|01/r rmw,rw|03/r rw,rmw|05 ax,iw|81/0 rmw,iw|83/0 rmw,ibx", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void GenericAdd<TValue>(Processor p, ref TValue dest, TValue src) where TValue : unmanaged, IBinaryInteger<TValue>
    {
        TValue uResult = dest + src;
        p.Flags.Update_Add(dest, src, uResult);
        dest = uResult;
    }
}
