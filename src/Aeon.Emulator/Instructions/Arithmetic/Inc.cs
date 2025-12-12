using System.Numerics;
using System.Runtime.CompilerServices;

namespace Aeon.Emulator.Instructions.Arithmetic;

internal static class Inc
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Opcode("FE/0 rmb|40+ rw|FF/0 rmw", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void GenericIncrement<TValue>(Processor p, ref TValue dest) where TValue : unmanaged, IBinaryInteger<TValue>
    {
        TValue original = dest;
        p.Flags.Update_Inc(original, ++dest);
    }
}
