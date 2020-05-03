using System;
using System.Runtime.CompilerServices;

namespace Aeon.Emulator.Instructions.FPU
{
    internal static class Fsin
    {
        [Opcode("D9FE", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Sine(VirtualMachine vm)
        {
            ref var st0 = ref vm.Processor.FPU.ST0_Ref;
            st0 = Math.Sin(st0);
        }
    }
}
