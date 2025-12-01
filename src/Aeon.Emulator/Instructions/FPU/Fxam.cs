namespace Aeon.Emulator.Instructions.FPU;

internal static class Fxam
{
    [Opcode("D9E5", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void Examine(VirtualMachine vm)
    {
        var fpu = vm.Processor.FPU;
        var flags = fpu.StatusFlags & ~(ConditionCodes.ConditionMask | FPUStatus.C1);
        double st0 = fpu.ST0;

        if (!fpu.IsRegisterUsed(0))
        {
            flags |= Empty;
        }
        else
        {
            if (st0 == 0)
                flags |= Zero;
            else if (double.IsInfinity(st0))
                flags |= Infinity;
            else if (double.IsNaN(st0))
                flags |= NaN;
            else
                flags |= Normal;
        }

        if (st0 < 0)
            flags |= FPUStatus.C1;

        fpu.StatusFlags = flags;
        vm.Processor.InstructionEpilog();
    }

    private const FPUStatus NaN = FPUStatus.C0;
    private const FPUStatus Normal = FPUStatus.C2;
    private const FPUStatus Infinity = FPUStatus.C0 | FPUStatus.C2;
    private const FPUStatus Zero = FPUStatus.C3;
    private const FPUStatus Empty = FPUStatus.C0 | FPUStatus.C3;
    private const FPUStatus Denormal = FPUStatus.C2 | FPUStatus.C3;
}
