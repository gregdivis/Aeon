using System.Runtime.CompilerServices;

namespace Aeon.Emulator.Instructions.FPU
{
    internal static class Fld
    {
        [Opcode("D9/0 mf32", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LoadReal32(Processor p, float value)
        {
            p.FPU.Push(value);
        }

        [Opcode("DD/0 mf64|D9C0+ st", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LoadReal64(Processor p, double value)
        {
            p.FPU.Push(value);
        }

        [Opcode("DB/5 mf80", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LoadReal80(Processor p, Real10 value)
        {
            p.FPU.Push((double)value);
        }
    }
}
