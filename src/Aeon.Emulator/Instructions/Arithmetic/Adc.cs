using System.Numerics;
using System.Runtime.CompilerServices;

namespace Aeon.Emulator.Instructions.Arithmetic;

internal static class Adc
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Opcode("10/r rmb,rb|12/r rb,rmb|14 al,ib|80/2 rmb,ib|11/r rmw,rw|13/r rw,rmw|15 ax,iw|81/2 rmw,iw|83/2 rmw,ibx", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void GenericAdd<TValue>(Processor p, ref TValue dest, TValue src) where TValue : unmanaged, IBinaryInteger<TValue>
    {
        uint c = p.Flags.Carry ? 1u : 0u;
        if (Unsafe.SizeOf<TValue>() < 4)
        {
            uint uResult = uint.CreateTruncating(dest) + uint.CreateTruncating(src) + c;
            p.Flags.Update_Adc(dest, src, TValue.CreateTruncating(c), TValue.CreateTruncating(uResult));
            dest = TValue.CreateTruncating(uResult);
        }
        else
        {
            ulong uResult = ulong.CreateTruncating(dest) + ulong.CreateTruncating(src) + c;
            p.Flags.Update_Adc(uint.CreateTruncating(dest), uint.CreateTruncating(src), c, (uint)uResult);
            dest = TValue.CreateTruncating(uResult);
        }
    }
}
