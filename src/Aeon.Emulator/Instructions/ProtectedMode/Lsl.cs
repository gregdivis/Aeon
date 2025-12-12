using System.Runtime.CompilerServices;

namespace Aeon.Emulator.Instructions.ProtectedMode;

internal static class Lsl
{
    [Opcode("0F03/r rw,rmw", AddressSize = 16 | 32)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LoadSegmentLimit16(VirtualMachine vm, out ushort dest, ushort selector)
    {
        var descriptor = vm.PhysicalMemory.GetDescriptor(selector);
        //if(!descriptor.IsSystemDescriptor)
        {
            dest = (ushort)((SegmentDescriptor)descriptor).ByteLimit;
            vm.Processor.Flags.Zero = true;
        }
        //else
        //{
        //    dest = 0;
        //    vm.Processor.Flags &= ~EFlags.Zero;
        //}
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Alternate(nameof(LoadSegmentLimit16), AddressSize = 16 | 32)]
    public static void LoadSegmentLimit32(VirtualMachine vm, out uint dest, uint selector)
    {
        var descriptor = vm.PhysicalMemory.GetDescriptor(selector & 0xFFFFu);
        //if(!descriptor.IsSystemDescriptor)
        {
            dest = ((SegmentDescriptor)descriptor).ByteLimit;
            vm.Processor.Flags.Zero = true;
        }
        //else
        //{
        //    dest = 0;
        //    vm.Processor.Flags &= ~EFlags.Zero;
        //}
    }
}
