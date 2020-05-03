using System.Runtime.CompilerServices;

namespace Aeon.Emulator.Instructions.ProtectedMode
{
    internal static class Ver
    {
        [Opcode("0F00/4 rm16", Name = "verr", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void VerifyRead(VirtualMachine vm, ushort segment)
        {
            if (segment != 0)
            {
                var descriptor = vm.PhysicalMemory.GetDescriptor(segment);
                if (!descriptor.IsSystemDescriptor)
                {
                    if (((SegmentDescriptor)descriptor).CanRead)
                    {
                        vm.Processor.Flags.Zero = true;
                        return;
                    }
                }
            }

            vm.Processor.Flags.Zero = false;
        }

        [Opcode("0F00/5 rm16", Name = "verw", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void VerifyWrite(VirtualMachine vm, ushort segment)
        {
            if (segment != 0)
            {
                var descriptor = vm.PhysicalMemory.GetDescriptor(segment);
                if (!descriptor.IsSystemDescriptor)
                {
                    if (((SegmentDescriptor)descriptor).CanWrite)
                    {
                        vm.Processor.Flags.Zero = true;
                        return;
                    }
                }
            }

            vm.Processor.Flags.Zero = false;
        }
    }
}
