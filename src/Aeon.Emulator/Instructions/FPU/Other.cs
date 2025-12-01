namespace Aeon.Emulator.Instructions.FPU;

internal static class Other
{
    [Opcode("D9FC", Name = "frndint", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void Frndint(Processor p)
    {
        ref var st0 = ref p.FPU.ST0_Ref;
        st0 = p.FPU.Round(st0);
    }

    [Opcode("D9E4", Name = "ftst", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void Ftst(VirtualMachine vm)
    {
        vm.Processor.FPU.StatusFlags &= ~ConditionCodes.ConditionMask;
        double st0 = vm.Processor.FPU.ST0;

        FPUStatus status;
        if (st0 > 0.0)
            status = ConditionCodes.GreaterThan;
        else if (st0 < 0.0)
            status = ConditionCodes.LessThan;
        else if (st0 == 0.0)
            status = ConditionCodes.Zero;
        else
        {
            status = ConditionCodes.Unordered;
            vm.Processor.FPU.StatusFlags |= FPUStatus.InvalidOperation;
        }

        vm.Processor.FPU.StatusFlags |= status;
    }
}
