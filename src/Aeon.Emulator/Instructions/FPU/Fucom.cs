using System.Runtime.CompilerServices;

namespace Aeon.Emulator.Instructions.FPU
{
    internal static class Fucom
    {
        [Opcode("DDE0+ st", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Compare64(Processor p, double value)
        {
            p.FPU.StatusFlags &= ~ConditionCodes.ConditionMask;
            double st0 = p.FPU.ST0;

            FPUStatus status;
            if (st0 > value)
                status = ConditionCodes.GreaterThan;
            else if (st0 < value)
                status = ConditionCodes.LessThan;
            else if (st0 == value)
                status = ConditionCodes.Zero;
            else
                status = ConditionCodes.Unordered;

            p.FPU.StatusFlags |= status;
        }
    }

    internal static class Fucomp
    {
        [Opcode("DDE8+ st", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ComparePop64(Processor p, double value)
        {
            Fucom.Compare64(p, value);
            p.FPU.Pop();
        }
    }

    internal static class Fucompp
    {
        [Opcode("DAE9", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CompareDoublePop64(Processor p)
        {
            Fucom.Compare64(p, p.FPU.GetRegisterRef(1));
            p.FPU.Pop();
            p.FPU.Pop();
        }
    }
}
