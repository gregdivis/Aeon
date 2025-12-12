using System.Numerics;
using System.Runtime.CompilerServices;

namespace Aeon.Emulator.Instructions.Arithmetic;

internal static class Sbb
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Opcode("1C al,ib|80/3 rmb,ib|18/r rmb,rb|1A/r rb,rmb|1D ax,iw|81/3 rmw,iw|83/3 rmw,ibx|19/r rmw,rw|1B/r rw,rmw", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void GenericByteCarrySub<TValue>(Processor p, ref TValue dest, TValue src) where TValue : unmanaged, IBinaryInteger<TValue>
    {
        uint c = p.Flags.Carry ? 1u : 0u;
        if (Unsafe.SizeOf<TValue>() < 4)
        {
            uint uResult = uint.CreateTruncating(dest) - uint.CreateTruncating(src) - c;
            p.Flags.Update_Sbb(dest, src, TValue.CreateTruncating(c), TValue.CreateTruncating(uResult));
            dest = TValue.CreateTruncating(uResult);
        }
        else
        {
            ulong uResult = ulong.CreateTruncating(dest) - ulong.CreateTruncating(src) - c;
            p.Flags.Update_Sbb(dest, src, TValue.CreateTruncating(c), TValue.CreateTruncating(uResult));
            dest = TValue.CreateTruncating(uResult);
        }
    }
}
