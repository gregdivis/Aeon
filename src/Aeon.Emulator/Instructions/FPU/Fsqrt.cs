using System.Runtime.CompilerServices;

namespace Aeon.Emulator.Instructions.FPU;

internal static class Fsqrt
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Opcode("D9FA", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void SquareRoot(VirtualMachine vm)
    {
        ref var st0 = ref vm.Processor.FPU.ST0_Ref;
        st0 = Math.Sqrt(st0);
    }
}
