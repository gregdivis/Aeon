using System.Numerics;
using System.Runtime.CompilerServices;

namespace Aeon.Emulator.Instructions.BitShifting;

internal static class Ror
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Opcode("D0/1 rmb|D1/1 rmw", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void RotateRight1<TValue>(Processor p, ref TValue dest) where TValue : unmanaged, IBinaryInteger<TValue>
    {
        dest = TValue.RotateRight(dest, 1);
        p.Flags.Update_Ror1(dest);
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
    [Opcode("D2/1 rmb,cl|C0/1 rmb,ib|D3/1 rmw,cl|C1/1 rmw,ib", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void RotateRight<TValue>(Processor p, ref TValue dest, byte count) where TValue : unmanaged, IBinaryInteger<TValue>
    {
        int actualCount = count & 0x1F;
        if (actualCount > 1)
        {
            dest = TValue.RotateRight(dest, actualCount);
            p.Flags.Update_Ror(dest);
        }
        else if (actualCount == 1)
        {
            RotateRight1(p, ref dest);
        }
    }
}
