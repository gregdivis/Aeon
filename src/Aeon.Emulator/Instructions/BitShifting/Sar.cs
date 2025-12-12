using System.Numerics;
using System.Runtime.CompilerServices;

namespace Aeon.Emulator.Instructions.BitShifting;

internal static class Sar
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Opcode("D0/7 rmb|D1/7 rmw", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void ArithmeticShiftRight1Generic<TValue>(Processor p, ref TValue dest) where TValue : unmanaged, IBinaryInteger<TValue>, ISignedNumber<TValue>
    {
        TValue value = dest;
        dest >>= 1;
        p.Flags.Update_Sar1(value, TValue.CreateTruncating(dest));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Opcode("D2/7 rmb,cl|C0/7 rmb,ib|D3/7 rmw,cl|C1/7 rmw,ib", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void ArithmeticShiftRightGeneric<TValue>(Processor p, ref TValue dest, byte count) where TValue : unmanaged, IBinaryInteger<TValue>, ISignedNumber<TValue>
    {
        count &= 0x1F;
        if (count > 0)
        {
            TValue value = TValue.CreateTruncating(int.CreateTruncating(dest) >> count);
            dest = value;
            p.Flags.Update_Sar(value, TValue.CreateTruncating(count), dest);
        }
        else if (count == 1)
        {
            ArithmeticShiftRight1Generic(p, ref dest);
        }
    }
}
