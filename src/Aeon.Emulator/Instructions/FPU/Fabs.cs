using System.Runtime.CompilerServices;

namespace Aeon.Emulator.Instructions.FPU;

internal static class Fabs
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Opcode("D9E1", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void Abs(Processor p)
    {
        ref var st0 = ref p.FPU.ST0_Ref;
        st0 = Math.Abs(st0);
    }
}
