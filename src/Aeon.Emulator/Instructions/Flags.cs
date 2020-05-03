namespace Aeon.Emulator.Instructions
{
    internal static class FlagsInstructions
    {
        [Opcode("F8", Name = "clc", AddressSize = 16 | 32, OperandSize = 16 | 32)]
        public static void ClearCarry(VirtualMachine vm)
        {
            vm.Processor.Flags.Carry = false;
        }
        [Opcode("FC", Name = "cld", AddressSize = 16 | 32, OperandSize = 16 | 32)]
        public static void ClearDirection(VirtualMachine vm)
        {
            vm.Processor.Flags.Direction = false;
        }
        [Opcode("FA", Name = "cli", AddressSize = 16 | 32, OperandSize = 16 | 32)]
        public static void ClearInterruptEnable(VirtualMachine vm)
        {
            vm.Processor.Flags.InterruptEnable = false;
        }
        [Opcode("F5", Name = "cmc", AddressSize = 16 | 32, OperandSize = 16 | 32)]
        public static void ComplementCarry(VirtualMachine vm)
        {
            vm.Processor.Flags.Carry = !vm.Processor.Flags.Carry;
        }
        [Opcode("F9", Name = "stc", AddressSize = 16 | 32, OperandSize = 16 | 32)]
        public static void SetCarry(VirtualMachine vm)
        {
            vm.Processor.Flags.Carry = true;
        }
        [Opcode("FD", Name = "std", AddressSize = 16 | 32, OperandSize = 16 | 32)]
        public static void SetDirection(VirtualMachine vm)
        {
            vm.Processor.Flags.Direction = true;
        }
        [Opcode("FB", Name = "sti", AddressSize = 16 | 32, OperandSize = 16 | 32)]
        public static void SetInterruptEnable(VirtualMachine vm)
        {
            vm.Processor.Flags.InterruptEnable = true;
        }
        [Opcode("9F", Name = "lahf")]
        public static void CopyFlagsToAH(VirtualMachine vm)
        {
            var value = EFlags.Clear;
            var f = vm.Processor.Flags;
            if (f.Carry)
                value |= EFlags.Carry;
            if (f.Parity)
                value |= EFlags.Parity;
            if (f.Auxiliary)
                value |= EFlags.Auxiliary;
            if (f.Zero)
                value |= EFlags.Zero;
            if (f.Sign)
                value |= EFlags.Sign;

            vm.Processor.AH = (byte)value;
        }
        [Opcode("9E", Name = "sahf", AddressSize = 16 | 32, OperandSize = 16 | 32)]
        public static void CopyAHToFlags(VirtualMachine vm)
        {
            var f = vm.Processor.Flags;
            var value = (EFlags)vm.Processor.AH;

            f.Carry = value.HasFlag(EFlags.Carry);
            f.Parity = value.HasFlag(EFlags.Parity);
            f.Auxiliary = value.HasFlag(EFlags.Auxiliary);
            f.Zero = value.HasFlag(EFlags.Zero);
            f.Sign = value.HasFlag(EFlags.Sign);
        }
    }
}
