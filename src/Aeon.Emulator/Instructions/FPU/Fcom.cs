using System.Runtime.CompilerServices;

namespace Aeon.Emulator.Instructions.FPU
{
    internal static class Fcom
    {
        [Opcode("D8/2 mf32", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Compare32(Processor p, float value)
        {
            Compare64(p, value);
        }

        [Opcode("DC/2 mf64|D8D0+ st", OperandSize = 16 | 32, AddressSize = 16 | 32)]
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
            {
                status = ConditionCodes.Unordered;
                p.FPU.StatusFlags |= FPUStatus.InvalidOperation;
            }

            p.FPU.StatusFlags |= status;
        }
    }

    internal static class Fcomp
    {
        [Opcode("D8/3 mf32", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ComparePop32(Processor p, float value)
        {
            Fcom.Compare32(p, value);
            p.FPU.Pop();
        }

        [Opcode("DC/3 mf64|D8D8+ st", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ComparePop64(Processor p, double value)
        {
            Fcom.Compare64(p, value);
            p.FPU.Pop();
        }
    }

    internal static class Fcompp
    {
        [Opcode("DED9", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CompareDoublePop64(Processor p)
        {
            Fcom.Compare64(p, p.FPU.GetRegisterValue(1));
            p.FPU.Pop();
            p.FPU.Pop();
        }
    }
}
