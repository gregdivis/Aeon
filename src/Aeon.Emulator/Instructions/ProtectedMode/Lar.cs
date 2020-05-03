using System.Runtime.CompilerServices;

namespace Aeon.Emulator.Instructions.ProtectedMode
{
    internal static class Lar
    {
        [Opcode("0F02/r rw,rmw", AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LoadAccessRights(VirtualMachine vm, out ushort dest, ushort selector)
        {
            var descriptor = vm.PhysicalMemory.GetDescriptor(selector);
            dest = (ushort)(descriptor.AccessRights << 8);

            if (selector != 0)
                vm.Processor.Flags.Zero = true;
            else
                vm.Processor.Flags.Zero = false;
        }

        [Alternate("LoadAccessRights", AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LoadAccessRights(VirtualMachine vm, out uint dest, uint selector)
        {
            var descriptor = vm.PhysicalMemory.GetDescriptor(selector & 0xFFFFu);
            dest = ((uint)descriptor.AccessRights) << 8;

            if (selector != 0)
                vm.Processor.Flags.Zero = true;
            else
                vm.Processor.Flags.Zero = false;
        }
    }
}
