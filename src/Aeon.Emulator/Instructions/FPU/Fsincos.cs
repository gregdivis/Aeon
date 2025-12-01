namespace Aeon.Emulator.Instructions.FPU;

internal static class Fsincos
{
    [Opcode("D9FB", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void SineCosine(VirtualMachine vm)
    {
        ref var st0 = ref vm.Processor.FPU.ST0_Ref;

        var (sin, cos) = Math.SinCos(st0);

        st0 = sin;
        vm.Processor.FPU.Push(cos);
    }
}
