using System.Numerics;
using System.Runtime.CompilerServices;

namespace Aeon.Emulator.Instructions.BitwiseLogic;

internal static class Not
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Opcode("F6/2 rmb|F7/2 rmw", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void GenericNot<TValue>(VirtualMachine vm, ref TValue dest) where TValue : unmanaged, IBinaryInteger<TValue> => dest = ~dest;
}
