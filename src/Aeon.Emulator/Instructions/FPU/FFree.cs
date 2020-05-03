
namespace Aeon.Emulator.Instructions.FPU
{
    internal static class FFree
    {
        [Opcode("DDC0", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        public static void FreeST0(VirtualMachine vm)
        {
            vm.Processor.FPU.FreeRegister(0);
        }

        [Opcode("DDC1", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        public static void FreeST1(VirtualMachine vm)
        {
            vm.Processor.FPU.FreeRegister(1);
        }

        [Opcode("DDC2", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        public static void FreeST2(VirtualMachine vm)
        {
            vm.Processor.FPU.FreeRegister(2);
        }

        [Opcode("DDC3", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        public static void FreeST3(VirtualMachine vm)
        {
            vm.Processor.FPU.FreeRegister(3);
        }

        [Opcode("DDC4", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        public static void FreeST4(VirtualMachine vm)
        {
            vm.Processor.FPU.FreeRegister(4);
        }

        [Opcode("DDC5", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        public static void FreeST5(VirtualMachine vm)
        {
            vm.Processor.FPU.FreeRegister(5);
        }

        [Opcode("DDC6", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        public static void FreeST6(VirtualMachine vm)
        {
            vm.Processor.FPU.FreeRegister(6);
        }

        [Opcode("DDC7", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        public static void FreeST7(VirtualMachine vm)
        {
            vm.Processor.FPU.FreeRegister(7);
        }
    }
}
