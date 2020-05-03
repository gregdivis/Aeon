using System;
using System.Runtime.CompilerServices;

namespace Aeon.Emulator.Instructions.FPU
{
    internal static class Fsincos
    {
        [Opcode("D9FB", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SineCosine(VirtualMachine vm)
        {
            ref var st0 = ref vm.Processor.FPU.ST0_Ref;

            double sine = Math.Sin(st0);
            double cosine = Math.Cos(st0);

            st0 = sine;
            vm.Processor.FPU.Push(cosine);
        }
    }
}
