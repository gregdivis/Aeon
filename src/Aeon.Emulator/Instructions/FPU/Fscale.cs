using System.Runtime.CompilerServices;

namespace Aeon.Emulator.Instructions.FPU;

internal static class Fscale
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Opcode("D9FD", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void Scale(VirtualMachine vm)
    {
        var fpu = vm.Processor.FPU;
        ref var st0 = ref fpu.ST0_Ref;
        st0 = Math.ScaleB(st0, (int)fpu.GetRegisterRef(1));
    }
}
