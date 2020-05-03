namespace Aeon.Emulator.Instructions.FPU
{
    internal static class ConditionCodes
    {
        public const FPUStatus GreaterThan = FPUStatus.Clear;
        public const FPUStatus LessThan = FPUStatus.C0;
        public const FPUStatus Zero = FPUStatus.C3;
        public const FPUStatus Unordered = FPUStatus.C0 | FPUStatus.C2 | FPUStatus.C3;

        public const FPUStatus ConditionMask = FPUStatus.C0 | FPUStatus.C2 | FPUStatus.C3;
    }
}
