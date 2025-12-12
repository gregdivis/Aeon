using System.Numerics;
using System.Runtime.CompilerServices;

namespace Aeon.Emulator.Instructions.BitShifting;

internal static class Shr
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Opcode("D0/5 rmb|D1/5 rmw", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void GenericShiftRight1<TValue>(Processor p, ref TValue dest) where TValue : unmanaged, IBinaryInteger<TValue>
    {
        var value = dest;
        dest >>= 1;
        p.Flags.Update_Shr1(value, dest);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Opcode("D2/5 rmb,cl|C0/5 rmb,ib|D3/5 rmw,cl|C1/5 rmw,ib", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void GenericShiftRight<TValue>(Processor p, ref TValue dest, byte count) where TValue : unmanaged, IBinaryInteger<TValue>
    {
        count &= 0x1F;
        if (count > 1)
        {
            var value = dest;
            dest >>>= count;
            p.Flags.Update_Shr(value, TValue.CreateTruncating(count), dest);
        }
        else if (count == 1)
        {
            GenericShiftRight1(p, ref dest);
        }
    }
}
