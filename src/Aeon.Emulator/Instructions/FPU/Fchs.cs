using System.Runtime.CompilerServices;

namespace Aeon.Emulator.Instructions.FPU;

internal static class Fchs
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Opcode("D9E0", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void ChangeSign(Processor p)
    {
        ref var st0 = ref p.FPU.ST0_Ref;
        st0 = -st0;
    }
}
