using System.Numerics;
using System.Runtime.CompilerServices;

namespace Aeon.Emulator.Instructions.Arithmetic;

internal static class Neg
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Opcode("F6/3 rmb|F7/3 rmw", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void GenericNegate<TValue>(Processor p, ref TValue dest) where TValue : unmanaged, IBinaryInteger<TValue>
    {
        TValue sResult = -dest;
        p.Flags.Update_Sub(TValue.Zero, dest, sResult);
        dest = sResult;
    }
}
