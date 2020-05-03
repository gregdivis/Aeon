using System.Runtime.CompilerServices;

namespace Aeon.Emulator.Instructions.ProtectedMode
{
    internal static class StoreDescriptors
    {
        [Opcode("0F01/1 m64", Name = "sidt", AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void StoreIDT(PhysicalMemory m, ref ulong address)
        {
            address &= 0xFFFF000000000000u;
            address |= m.IDTLimit;
            address |= (ulong)((m.IDTAddress << 16) & 0x00FFFFFFu);
        }
        [Alternate(nameof(StoreIDT), AddressSize = 16 | 32, OperandSize = 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void StoreIDT32(PhysicalMemory m, ref ulong address)
        {
            address &= 0xFFFF000000000000u;
            address |= m.IDTLimit;
            address |= (ulong)m.IDTAddress << 16;
        }

        [Opcode("0F01/0 m64", Name = "sgdt", AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void StoreGDT(PhysicalMemory m, ref ulong address)
        {
            address &= 0xFFFF000000000000u;
            address |= m.GDTLimit;
            address |= (ulong)((m.GDTAddress << 16) & 0x00FFFFFFu);
        }
        [Alternate(nameof(StoreGDT), OperandSize = 32, AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void StoreGDT32(PhysicalMemory m, ref ulong address)
        {
            address &= 0xFFFF000000000000u;
            address |= m.GDTLimit;
            address |= (ulong)m.GDTAddress << 16;
        }

        [Opcode("0F00/0 rmw", Name = "sldt", AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void StoreLDT(PhysicalMemory m, out ushort selector)
        {
            selector = m.LDTSelector;
        }
        [Alternate(nameof(StoreLDT), AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void StoreLDT32(PhysicalMemory m, out uint selector)
        {
            selector = m.LDTSelector;
        }

        [Opcode("0F00/1 rmw", Name = "str", AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void StoreTaskRegister(PhysicalMemory m, out ushort selector)
        {
            selector = m.TaskSelector;
        }
        [Alternate(nameof(StoreTaskRegister), AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void StoreTaskRegister32(PhysicalMemory m, out uint selector)
        {
            selector = m.TaskSelector;
        }
    }
}
