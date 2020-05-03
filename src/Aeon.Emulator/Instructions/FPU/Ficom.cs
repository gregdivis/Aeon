using System.Runtime.CompilerServices;

namespace Aeon.Emulator.Instructions.FPU
{
    internal static class Ficom
    {
        [Opcode("DE/2 m16", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CompareInt16(Processor p, short value)
        {
            Fcom.Compare64(p, value);
        }

        [Opcode("DA/2 m32", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CompareInt16(Processor p, int value)
        {
            Fcom.Compare64(p, value);
        }
    }

    internal static class Ficomp
    {
        [Opcode("DE/3 m16", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CompareInt16(Processor p, short value)
        {
            Fcomp.ComparePop64(p, value);
        }

        [Opcode("DA/3 m32", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CompareInt16(Processor p, int value)
        {
            Fcomp.ComparePop64(p, value);
        }
    }
}
