using System.Runtime.CompilerServices;
using Aeon.Emulator.Memory;

namespace Aeon.Emulator.Instructions.ProtectedMode;

internal static class Ver
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Opcode("0F00/4 rm16", Name = "verr", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void VerifyRead(VirtualMachine vm, ushort selector)
    {
        var flags = vm.Processor.Flags;

        if (selector == 0 || (selector & 0xFFF8) == 0)
        {
            flags.Zero = false;
            return;
        }

        uint index = (uint)selector >> 3;
        uint tableLimit = (selector & 4) != 0 ? vm.PhysicalMemory.LDTLimit : vm.PhysicalMemory.GDTLimit;

        if (index * 8u + 7u > tableLimit)
        {
            flags.Zero = false;  // Out of bounds
            return;
        }

        var desc = vm.PhysicalMemory.GetDescriptor(selector);

        if (desc.DescriptorType != DescriptorType.Segment || !((SegmentDescriptor)desc).IsPresent)
        {
            flags.Zero = false;
            return;
        }

        var segDesc = (SegmentDescriptor)desc;

        bool readable = !segDesc.IsCodeSegment || (segDesc.Attributes1 & SegmentDescriptor.ReadWrite) != 0;

        flags.Zero = readable;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Opcode("0F00/5 rm16", Name = "verw", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void VerifyWrite(VirtualMachine vm, ushort selector)
    {
        var flags = vm.Processor.Flags;

        if (selector == 0 || (selector & 0xFFF8) == 0)
        {
            flags.Zero = false;
            return;
        }

        uint index = (uint)selector >> 3;
        uint tableLimit = (selector & 4) != 0 ? vm.PhysicalMemory.LDTLimit : vm.PhysicalMemory.GDTLimit;

        if (index * 8u + 7u > tableLimit)
        {
            flags.Zero = false;  // Out of bounds
            return;
        }

        var desc = vm.PhysicalMemory.GetDescriptor(selector);

        if (desc.DescriptorType != DescriptorType.Segment || !((SegmentDescriptor)desc).IsPresent)
        {
            flags.Zero = false;
            return;
        }

        var segDesc = (SegmentDescriptor)desc;
        bool writable = segDesc.CanWrite;
        flags.Zero = writable;
    }
}
