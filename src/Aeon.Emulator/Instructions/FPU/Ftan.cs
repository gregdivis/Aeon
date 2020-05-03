using System;
using System.Runtime.CompilerServices;

namespace Aeon.Emulator.Instructions.FPU
{
    internal static class Ftan
    {
        [Opcode("D9F2", Name = "fptan", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PartialTangent(VirtualMachine vm)
        {
            var fpu = vm.Processor.FPU;
            ref var st0 = ref fpu.ST0_Ref;
            st0 = Math.Tan(st0);
            fpu.Push(1.0);
        }

        [Opcode("D9F3", Name = "fpatan", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PartialArctangent(VirtualMachine vm)
        {
            var fpu = vm.Processor.FPU;
            ref var st1 = ref fpu.GetRegisterRef(1);
            st1 = Math.Atan2(st1, fpu.ST0_Ref);
            fpu.Pop();
        }
    }
}
