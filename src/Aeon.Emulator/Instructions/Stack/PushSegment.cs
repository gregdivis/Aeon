using System.Runtime.CompilerServices;

namespace Aeon.Emulator.Instructions.Stack
{
    internal static class PushSegment
    {
        [Opcode("0E", Name = "push cs", AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PushCS(VirtualMachine vm)
        {
            vm.PushToStack(vm.Processor.CS);
            vm.Processor.InstructionEpilog();
        }
        [Alternate("PushCS", AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PushCS32(VirtualMachine vm)
        {
            vm.PushToStack32(vm.Processor.CS);
            vm.Processor.InstructionEpilog();
        }

        [Opcode("16", Name = "push ss", AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PushSS(VirtualMachine vm)
        {
            vm.PushToStack(vm.Processor.SS);
            vm.Processor.InstructionEpilog();
        }
        [Alternate("PushSS", AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PushSS32(VirtualMachine vm)
        {
            vm.PushToStack32(vm.Processor.SS);
            vm.Processor.InstructionEpilog();
        }

        [Opcode("1E", Name = "push ds", AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PushDS(VirtualMachine vm)
        {
            vm.PushToStack(vm.Processor.DS);
            vm.Processor.InstructionEpilog();
        }
        [Alternate("PushDS", AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PushDS32(VirtualMachine vm)
        {
            vm.PushToStack32(vm.Processor.DS);
            vm.Processor.InstructionEpilog();
        }

        [Opcode("06", Name = "push es", AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PushES(VirtualMachine vm)
        {
            vm.PushToStack(vm.Processor.ES);
            vm.Processor.InstructionEpilog();
        }
        [Alternate("PushES", AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PushES32(VirtualMachine vm)
        {
            vm.PushToStack32(vm.Processor.ES);
            vm.Processor.InstructionEpilog();
        }
        
        [Opcode("0FA0", Name = "push fs", AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PushFS(VirtualMachine vm)
        {
            vm.PushToStack(vm.Processor.FS);
            vm.Processor.InstructionEpilog();
        }
        [Alternate("PushFS", AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PushFS32(VirtualMachine vm)
        {
            vm.PushToStack32(vm.Processor.FS);
            vm.Processor.InstructionEpilog();
        }

        [Opcode("0FA8", Name = "push gs", AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PushGS(VirtualMachine vm)
        {
            vm.PushToStack(vm.Processor.GS);
            vm.Processor.InstructionEpilog();
        }
        [Alternate("PushGS", AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PushGS32(VirtualMachine vm)
        {
            vm.PushToStack32(vm.Processor.GS);
            vm.Processor.InstructionEpilog();
        }
    }
}
