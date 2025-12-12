using System.Runtime.CompilerServices;

namespace Aeon.Emulator.Instructions.FPU;

internal static class Fidivr
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Opcode("DE/7 m16", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void ReverseDivideInt16(Processor p, short value)
    {
        ref var st0 = ref p.FPU.ST0_Ref;
        st0 = value / st0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Opcode("DA/7 m32", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void ReverseDivideInt32(Processor p, int value)
    {
        ref var st0 = ref p.FPU.ST0_Ref;
        st0 = value / st0;
    }
}
