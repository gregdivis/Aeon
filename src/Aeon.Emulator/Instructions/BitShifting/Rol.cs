using System.Numerics;
using System.Runtime.CompilerServices;

namespace Aeon.Emulator.Instructions.BitShifting;

internal static class Rol
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Opcode("D0/0 rmb|D1/0 rmw", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void RotateLeft1<TValue>(Processor p, ref TValue dest) where TValue : unmanaged, IBinaryInteger<TValue>
    {
        dest = TValue.RotateLeft(dest, 1);
        p.Flags.Update_Rol1(dest);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Opcode("D2/0 rmb,cl|C0/0 rmb,ib|D3/0 rmw,cl|C1/0 rmw,ib", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void RotateLeft<TValue>(Processor p, ref TValue dest, byte count) where TValue : unmanaged, IBinaryInteger<TValue>
    {
        int actualCount = count & 0x1F;
        if (actualCount > 1)
        {
            dest = TValue.RotateLeft(dest, actualCount);
            p.Flags.Update_Rol(dest);
        }
        else if (actualCount == 1)
        {
            RotateLeft1(p, ref dest);
        }
    }
}
