using System.Numerics;
using System.Runtime.CompilerServices;

namespace Aeon.Emulator.Instructions.Arithmetic;

internal static class Dec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Opcode("FE/1 rmb|48+ rw|FF/1 rmw", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void GenericDecrement<TValue>(Processor p, ref TValue dest) where TValue : unmanaged, IBinaryInteger<TValue>
    {
        TValue original = dest;
        p.Flags.Update_Dec(original, --dest);
    }
}
