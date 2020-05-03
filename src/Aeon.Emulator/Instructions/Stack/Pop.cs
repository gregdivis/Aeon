using Aeon.Emulator.RuntimeExceptions;
using System.Runtime.CompilerServices;

namespace Aeon.Emulator.Instructions.Stack
{
    internal static class Pop
    {
        [Opcode("8F/0 rmw|58+ rw", AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void PopWord(VirtualMachine vm, out ushort dest)
        {
            dest = vm.PopFromStack();
        }
        [Alternate(nameof(PopWord), AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void PopDWord(VirtualMachine vm, out uint dest)
        {
            dest = vm.PopFromStack32();
        }

        [Opcode("9D", Name = "popf", AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void PopFlags(VirtualMachine vm)
        {
            var p = vm.Processor;
            uint flags = (uint)(p.Flags.Value & ~ModifiableFlags16);
            flags |= vm.PopFromStack() & (uint)ModifiableFlags16;

            bool throwTrap = false;
            if (((EFlags)flags & EFlags.Trap) != 0 && (p.Flags.Value & EFlags.Trap) == 0)
                throwTrap = true;

            p.Flags.Value = (EFlags)flags;

            p.InstructionEpilog();

            if (throwTrap)
                throw new EnableInstructionTrapException();
        }
        [Alternate(nameof(PopFlags), AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void PopFlags32(VirtualMachine vm)
        {
            var p = vm.Processor;
            uint flags = (uint)(p.Flags.Value & ~ModifiableFlags32);
            flags |= vm.PopFromStack32() & (uint)ModifiableFlags32;

            bool throwTrap = false;
            if (((EFlags)flags & EFlags.Trap) != 0 && (p.Flags.Value & EFlags.Trap) == 0)
                throwTrap = true;

            p.Flags.Value = (EFlags)flags;

            p.InstructionEpilog();

            if (throwTrap)
                throw new EnableInstructionTrapException();
        }

        private const EFlags ModifiableFlags32 = EFlags.AlignmentCheck | EFlags.Auxiliary | EFlags.Carry | EFlags.Direction | EFlags.Identification | EFlags.InterruptEnable | EFlags.IOPrivilege1 | EFlags.IOPrivilege2 | EFlags.NestedTask | EFlags.Overflow | EFlags.Parity | EFlags.Resume | EFlags.Sign | EFlags.Trap | EFlags.Zero;
        private const EFlags ModifiableFlags16 = EFlags.Auxiliary | EFlags.Carry | EFlags.Direction | EFlags.InterruptEnable | EFlags.IOPrivilege1 | EFlags.IOPrivilege2 | EFlags.NestedTask | EFlags.Overflow | EFlags.Parity | EFlags.Sign | EFlags.Trap | EFlags.Zero;
    }
}
