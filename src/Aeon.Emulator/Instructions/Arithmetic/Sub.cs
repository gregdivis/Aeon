using System.Numerics;
using System.Runtime.CompilerServices;

namespace Aeon.Emulator.Instructions.Arithmetic;

internal static class Sub
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Opcode("2C al,ib|80/5 rmb,ib|28/r rmb,rb|2A/r rb,rmb|2D ax,iw|81/5 rmw,iw|83/5 rmw,ibx|29/r rmw,rw|2B/r rw,rmw", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void Subtract<TValue>(Processor p, ref TValue dest, TValue src) where TValue : unmanaged, IBinaryInteger<TValue>
    {
        TValue uResult = TValue.CreateTruncating(uint.CreateTruncating(dest) - uint.CreateTruncating(src));
        p.Flags.Update_Sub(dest, src, uResult);
        dest = uResult;
    }
}
