using System.Runtime.CompilerServices;

namespace Aeon.Emulator.Instructions.ProtectedMode;

internal static class LoadDescriptors
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Opcode("0F01/2 m64", Name = "lgdt", AddressSize = 16 | 32)]
    public static void LoadGlobalDescriptorTable(PhysicalMemory m, ulong address)
    {
        uint baseAddress = (uint)(address >> 16) & 0x00FFFFFFu;
        m.GDTAddress = baseAddress;
        m.GDTLimit = (uint)(address & 0xFFFFu);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Alternate(nameof(LoadGlobalDescriptorTable), AddressSize = 16 | 32, OperandSize = 32)]
    public static void LoadGlobalDescriptorTable32(PhysicalMemory m, ulong address)
    {
        uint baseAddress = (uint)(address >> 16);
        m.GDTAddress = baseAddress;
        m.GDTLimit = (uint)(address & 0xFFFFu);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Opcode("0F01/3 m64", Name = "lidt", AddressSize = 16 | 32)]
    public static void LoadInterruptDescriptorTable(PhysicalMemory m, ulong address)
    {
        uint baseAddress = (uint)(address >> 16) & 0x00FFFFFFu;
        m.IDTAddress = baseAddress;
        m.IDTLimit = (uint)(address & 0xFFFFu);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Alternate(nameof(LoadInterruptDescriptorTable), AddressSize = 16 | 32, OperandSize = 32)]
    public static void LoadInterruptDescriptorTable32(PhysicalMemory m, ulong address)
    {
        uint baseAddress = (uint)(address >> 16);
        m.IDTAddress = baseAddress;
        m.IDTLimit = (uint)(address & 0xFFFFu);
    }

    [Opcode("0F00/2 rmw", Name = "lldt")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LoadLocalDescriptorTable(PhysicalMemory m, ushort selector)
    {
        m.UpdateLocalDescriptor(selector);
    }
    [Alternate(nameof(LoadLocalDescriptorTable))]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LoadLocalDescriptorTable32(PhysicalMemory m, uint selector)
    {
        m.UpdateLocalDescriptor((ushort)selector);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Opcode("0F00/3 rmw", Name = "ltr", AddressSize = 16 | 32)]
    public static void LoadTaskRegister(PhysicalMemory m, ushort selector)
    {
        m.TaskSelector = selector;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Alternate(nameof(LoadTaskRegister), AddressSize = 16 | 32)]
    public static void LoadTaskRegister32(PhysicalMemory m, uint selector)
    {
        m.TaskSelector = (ushort)selector;
    }
}
