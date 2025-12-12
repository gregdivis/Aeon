using System.Numerics;
using System.Runtime.CompilerServices;

namespace Aeon.Emulator.Instructions.BitShifting;

internal static class Shl
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Opcode("D0/4 rmb|D1/4 rmw", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void GenericShiftLeft1<TValue>(Processor p, ref TValue dest) where TValue : unmanaged, IBinaryInteger<TValue>
    {
        var value = dest;
        dest <<= 1;
        p.Flags.Update_Shl1(value, dest);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Opcode("D2/4 rmb,cl|C0/4 rmb,ib|D3/4 rmw,cl|C1/4 rmw,ib", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void GenericShiftLeft<TValue>(Processor p, ref TValue dest, byte count) where TValue : unmanaged, IBinaryInteger<TValue>
    {
        count &= 0x1F;
        if (count > 1)
        {
            var value = dest;
            dest <<= count;
            p.Flags.Update_Shl(value, TValue.CreateTruncating(count), dest);
        }
        else if (count == 1)
        {
            GenericShiftLeft1(p, ref dest);
        }
    }
}
