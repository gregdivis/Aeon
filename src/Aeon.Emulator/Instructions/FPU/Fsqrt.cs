using System;
using System.Runtime.CompilerServices;

namespace Aeon.Emulator.Instructions.FPU
{
    internal static class Fsqrt
    {
        [Opcode("D9FA", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SquareRoot(VirtualMachine vm)
        {
            ref var st0 = ref vm.Processor.FPU.ST0_Ref;
            st0 = Math.Sqrt(st0);
        }
    }
}
