using System.Runtime.CompilerServices;

namespace Aeon.Emulator.Instructions.FPU
{
    internal static class Fsub
    {
        [Opcode("D8/4 mf32", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SubtractST032(VirtualMachine vm, float value)
        {
            vm.Processor.FPU.ST0_Ref -= value;
        }

        [Opcode("DC/4 mf64|D8E0+ st|DCE8 st0", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SubtractST064(VirtualMachine vm, double value)
        {
            vm.Processor.FPU.ST0_Ref -= value;
        }

        [Opcode("DCE9", Name = "fsub st1", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SubtractFromST1(VirtualMachine vm)
        {
            vm.Processor.FPU.GetRegisterRef(1) -= vm.Processor.FPU.ST0_Ref;
        }

        [Opcode("DCEA", Name = "fsub st2", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SubtractFromST2(VirtualMachine vm)
        {
            vm.Processor.FPU.GetRegisterRef(2) -= vm.Processor.FPU.ST0_Ref;
        }

        [Opcode("DCEB", Name = "fsub st3", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SubtractFromST3(VirtualMachine vm)
        {
            vm.Processor.FPU.GetRegisterRef(3) -= vm.Processor.FPU.ST0_Ref;
        }

        [Opcode("DCEC", Name = "fsub st4", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SubtractFromST4(VirtualMachine vm)
        {
            vm.Processor.FPU.GetRegisterRef(4) -= vm.Processor.FPU.ST0_Ref;
        }

        [Opcode("DCED", Name = "fsub st5", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SubtractFromST5(VirtualMachine vm)
        {
            vm.Processor.FPU.GetRegisterRef(5) -= vm.Processor.FPU.ST0_Ref;
        }

        [Opcode("DCEE", Name = "fsub st6", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SubtractFromST6(VirtualMachine vm)
        {
            vm.Processor.FPU.GetRegisterRef(6) -= vm.Processor.FPU.ST0_Ref;
        }

        [Opcode("DCEF", Name = "fsub st7", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SubtractFromST7(VirtualMachine vm)
        {
            vm.Processor.FPU.GetRegisterRef(7) -= vm.Processor.FPU.ST0_Ref;
        }
    }

    internal static class Fsubp
    {
        [Opcode("DEE8", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SubtractFromST0(VirtualMachine vm)
        {
            vm.Processor.FPU.Pop();
        }

        [Opcode("DEE9", Name = "fsubp st1", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SubtractFromST1(VirtualMachine vm)
        {
            Fsub.SubtractFromST1(vm);
            vm.Processor.FPU.Pop();
        }

        [Opcode("DEEA", Name = "fsubp st2", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SubtractFromST2(VirtualMachine vm)
        {
            Fsub.SubtractFromST2(vm);
            vm.Processor.FPU.Pop();
        }

        [Opcode("DEEB", Name = "fsubp st3", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SubtractFromST3(VirtualMachine vm)
        {
            Fsub.SubtractFromST3(vm);
            vm.Processor.FPU.Pop();
        }

        [Opcode("DEEC", Name = "fsubp st4", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SubtractFromST4(VirtualMachine vm)
        {
            Fsub.SubtractFromST4(vm);
            vm.Processor.FPU.Pop();
        }

        [Opcode("DEED", Name = "fsubp st5", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SubtractFromST5(VirtualMachine vm)
        {
            Fsub.SubtractFromST5(vm);
            vm.Processor.FPU.Pop();
        }

        [Opcode("DEEE", Name = "fsubp st6", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SubtractFromST6(VirtualMachine vm)
        {
            Fsub.SubtractFromST6(vm);
            vm.Processor.FPU.Pop();
        }

        [Opcode("DEEF", Name = "fsubp st7", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SubtractFromST7(VirtualMachine vm)
        {
            Fsub.SubtractFromST7(vm);
            vm.Processor.FPU.Pop();
        }
    }
}
