using System.Runtime.CompilerServices;

namespace Aeon.Emulator.Instructions.ProtectedMode
{
    internal static class StoreControl
    {
        [Opcode("0F01/4 rmw", Name = "smsw", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Store(Processor p, out ushort value)
        {
            value = (ushort)p.CR0;
        }

        [Opcode("0F20/0 rm32", Name = "stcr0", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MovFromCR0(Processor p, out uint value)
        {
            value = (uint)p.CR0;
        }

        [Opcode("0F20/2 rm32", Name = "stcr2", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MoveFromCR2(Processor p, out uint value)
        {
            value = p.CR2;
        }

        [Opcode("0F20/3 rm32", Name = "stcr3", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MovFromCR3(Processor p, out uint value)
        {
            value = p.CR3;
        }
    }
}
