namespace Aeon.Emulator.Instructions.FPU;

internal static class F2xm1
{
    [Opcode("D9F0", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void Compute2xMinus1(Processor p)
    {
        ref var st0 = ref p.FPU.ST0_Ref;
        st0 = Math.Pow(2.0, st0) - 1.0;
    }
}
