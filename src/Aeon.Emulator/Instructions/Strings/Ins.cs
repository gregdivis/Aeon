using System.Runtime.CompilerServices;

namespace Aeon.Emulator.Instructions.Strings
{
    internal static class Ins
    {
        [Opcode("6C", Name = "insb", AddressSize = 16, OperandSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void InByte(VirtualMachine vm)
        {
            if (vm.Processor.RepeatPrefix == RepeatPrefix.None)
                InSingleByte(vm);
            else
                InBytes(vm);

            vm.Processor.InstructionEpilog();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InSingleByte(VirtualMachine vm)
        {
            var p = vm.Processor;
            byte value = vm.ReadPortByte((ushort)p.DX);
            vm.PhysicalMemory.SetByte(p.ESBase + p.DI, value);

            if (!p.Flags.Direction)
                p.DI++;
            else
                p.DI--;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InBytes(VirtualMachine vm)
        {
            if (vm.Processor.CX != 0)
            {
                InSingleByte(vm);
                vm.Processor.EIP -= (ushort)(1 + vm.Processor.PrefixCount);
                vm.Processor.CX--;
            }
        }

        [Alternate(nameof(InByte), AddressSize = 32, OperandSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void InByte32(VirtualMachine vm)
        {
            if (vm.Processor.RepeatPrefix == RepeatPrefix.None)
                InSingleByte32(vm);
            else
                InBytes32(vm);

            vm.Processor.InstructionEpilog();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InSingleByte32(VirtualMachine vm)
        {
            var p = vm.Processor;
            byte value = vm.ReadPortByte((ushort)p.DX);
            vm.PhysicalMemory.SetByte(p.ESBase + p.EDI, value);

            if (!p.Flags.Direction)
                p.EDI++;
            else
                p.EDI--;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InBytes32(VirtualMachine vm)
        {
            if (vm.Processor.ECX != 0)
            {
                InSingleByte32(vm);
                vm.Processor.EIP -= (uint)(1 + vm.Processor.PrefixCount);
                vm.Processor.ECX--;
            }
        }
    }
}
