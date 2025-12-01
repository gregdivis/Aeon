namespace Aeon.Emulator.Instructions.FPU;

internal static class Fsubr
{
    [Opcode("D8/5 mf32", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void SubtractST032(Processor p, float value)
    {
        ref var st0 = ref p.FPU.ST0_Ref;
        st0 = value - st0;
    }

    [Opcode("DC/5 mf64|D8E8+ st|DCE0 st0", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void SubtractST064(Processor p, double value)
    {
        ref var st0 = ref p.FPU.ST0_Ref;
        st0 = value - st0;
    }

    [Opcode("DCE1", Name = "fsubr st1", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void SubtractST1(VirtualMachine vm)
    {
        ref var value = ref vm.Processor.FPU.GetRegisterRef(1);
        value = vm.Processor.FPU.ST0_Ref - value;
    }

    [Opcode("DCE2", Name = "fsubr st2", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void SubtractST2(VirtualMachine vm)
    {
        ref var value = ref vm.Processor.FPU.GetRegisterRef(2);
        value = vm.Processor.FPU.ST0_Ref - value;
    }

    [Opcode("DCE3", Name = "fsubr st3", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void SubtractST3(VirtualMachine vm)
    {
        ref var value = ref vm.Processor.FPU.GetRegisterRef(3);
        value = vm.Processor.FPU.ST0_Ref - value;
    }

    [Opcode("DCE4", Name = "fsubr st4", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void SubtractST4(VirtualMachine vm)
    {
        ref var value = ref vm.Processor.FPU.GetRegisterRef(4);
        value = vm.Processor.FPU.ST0_Ref - value;
    }

    [Opcode("DCE5", Name = "fsubr st5", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void SubtractST5(VirtualMachine vm)
    {
        ref var value = ref vm.Processor.FPU.GetRegisterRef(5);
        value = vm.Processor.FPU.ST0_Ref - value;
    }

    [Opcode("DCE6", Name = "fsubr st6", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void SubtractST6(VirtualMachine vm)
    {
        ref var value = ref vm.Processor.FPU.GetRegisterRef(6);
        value = vm.Processor.FPU.ST0_Ref - value;
    }

    [Opcode("DCE7", Name = "fsubr st7", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void SubtractST7(VirtualMachine vm)
    {
        ref var value = ref vm.Processor.FPU.GetRegisterRef(7);
        value = vm.Processor.FPU.ST0_Ref - value;
    }
}

internal static class Fsubrp
{
    [Opcode("DEE0", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void SubtractST0(VirtualMachine vm)
    {
        vm.Processor.FPU.Pop();
    }

    [Opcode("DEE1", Name = "fsubrp st1", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void SubtractST1(VirtualMachine vm)
    {
        Fsubr.SubtractST1(vm);
        vm.Processor.FPU.Pop();
    }

    [Opcode("DEE2", Name = "fsubrp st2", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void SubtractST2(VirtualMachine vm)
    {
        Fsubr.SubtractST2(vm);
        vm.Processor.FPU.Pop();
    }

    [Opcode("DEE3", Name = "fsubrp st3", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void SubtractST3(VirtualMachine vm)
    {
        Fsubr.SubtractST3(vm);
        vm.Processor.FPU.Pop();
    }

    [Opcode("DEE4", Name = "fsubrp st4", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void SubtractST4(VirtualMachine vm)
    {
        Fsubr.SubtractST4(vm);
        vm.Processor.FPU.Pop();
    }

    [Opcode("DEE5", Name = "fsubrp st5", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void SubtractST5(VirtualMachine vm)
    {
        Fsubr.SubtractST5(vm);
        vm.Processor.FPU.Pop();
    }

    [Opcode("DEE6", Name = "fsubrp st6", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void SubtractST6(VirtualMachine vm)
    {
        Fsubr.SubtractST6(vm);
        vm.Processor.FPU.Pop();
    }

    [Opcode("DEE7", Name = "fsubrp st7", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void SubtractST7(VirtualMachine vm)
    {
        Fsubr.SubtractST7(vm);
        vm.Processor.FPU.Pop();
    }
}
