namespace Aeon.Emulator.Instructions.Strings;

internal static class Stosb
{
    [Opcode("AA", AddressSize = 16, OperandSize = 16 | 32)]
    public static void StoreByte(VirtualMachine vm)
    {
        if (vm.Processor.RepeatPrefix == RepeatPrefix.None)
            StoreSingleByte(vm);
        else
            StoreBytes(vm);

        vm.Processor.InstructionEpilog();
    }
    private static void StoreSingleByte(VirtualMachine vm)
    {
        var processor = vm.Processor;

        vm.PhysicalMemory.SetByte(processor.ESBase + processor.DI, processor.AL);

        if (!processor.Flags.Direction)
            processor.DI++;
        else
            processor.DI--;
    }
    private static void StoreBytes(VirtualMachine vm)
    {
        if (vm.Processor.CX != 0)
        {
            StoreSingleByte(vm);
            vm.Processor.CX--;
            vm.Processor.IP -= (ushort)(1 + vm.Processor.PrefixCount);
        }
    }

    [Alternate(nameof(StoreByte), AddressSize = 32, OperandSize = 16 | 32)]
    public static void StoreByte32(VirtualMachine vm)
    {
        if (vm.Processor.RepeatPrefix == RepeatPrefix.None)
            StoreSingleByte32(vm);
        else
            StoreBytes32(vm);

        vm.Processor.InstructionEpilog();
    }
    private static void StoreSingleByte32(VirtualMachine vm)
    {
        var processor = vm.Processor;

        vm.PhysicalMemory.SetByte(processor.ESBase + processor.EDI, processor.AL);

        if (!processor.Flags.Direction)
            processor.EDI++;
        else
            processor.EDI--;
    }
    private static void StoreBytes32(VirtualMachine vm)
    {
        if (vm.Processor.ECX != 0)
        {
            StoreSingleByte32(vm);
            vm.Processor.ECX--;
            vm.Processor.EIP -= (ushort)(1 + vm.Processor.PrefixCount);
        }
    }
}

internal static class Stosw
{
    [Opcode("AB", AddressSize = 16, OperandSize = 16)]
    public static void StoreWord(VirtualMachine vm)
    {
        if (vm.Processor.RepeatPrefix == RepeatPrefix.None)
            StoreSingleWord(vm);
        else
            StoreWords(vm);

        vm.Processor.InstructionEpilog();
    }
    private static void StoreSingleWord(VirtualMachine vm)
    {
        vm.PhysicalMemory.SetUInt16(vm.Processor.ESBase + vm.Processor.DI, (ushort)vm.Processor.AX);

        if (!vm.Processor.Flags.Direction)
            vm.Processor.DI += 2;
        else
            vm.Processor.DI -= 2;
    }
    private static void StoreWords(VirtualMachine vm)
    {
        if (vm.Processor.CX != 0)
        {
            StoreSingleWord(vm);
            vm.Processor.CX--;
            vm.Processor.IP -= (ushort)(1 + vm.Processor.PrefixCount);
        }
    }

    [Alternate(nameof(StoreWord), AddressSize = 16, OperandSize = 32)]
    public static void StoreDWord(VirtualMachine vm)
    {
        if (vm.Processor.RepeatPrefix == RepeatPrefix.None)
            StoreSingleDWord(vm);
        else
            StoreDWords(vm);

        vm.Processor.InstructionEpilog();
    }
    private static void StoreSingleDWord(VirtualMachine vm)
    {
        vm.PhysicalMemory.SetUInt32(vm.Processor.ESBase + vm.Processor.DI, (uint)vm.Processor.EAX);

        if (!vm.Processor.Flags.Direction)
            vm.Processor.DI += 4;
        else
            vm.Processor.DI -= 4;
    }
    private static void StoreDWords(VirtualMachine vm)
    {
        if (vm.Processor.CX != 0)
        {
            StoreSingleDWord(vm);
            vm.Processor.CX--;
            vm.Processor.IP -= (ushort)(1 + vm.Processor.PrefixCount);
        }
    }

    [Alternate(nameof(StoreWord), AddressSize = 32, OperandSize = 16)]
    public static void StoreWord32(VirtualMachine vm)
    {
        if (vm.Processor.RepeatPrefix == RepeatPrefix.None)
            StoreSingleWord32(vm);
        else
            StoreWords32(vm);

        vm.Processor.InstructionEpilog();
    }
    private static void StoreSingleWord32(VirtualMachine vm)
    {
        vm.PhysicalMemory.SetUInt16(vm.Processor.ESBase + vm.Processor.EDI, (ushort)vm.Processor.AX);

        if (!vm.Processor.Flags.Direction)
            vm.Processor.EDI += 2;
        else
            vm.Processor.EDI -= 2;
    }
    private static void StoreWords32(VirtualMachine vm)
    {
        if (vm.Processor.ECX != 0)
        {
            StoreSingleWord32(vm);
            vm.Processor.ECX--;
            vm.Processor.EIP -= (ushort)(1 + vm.Processor.PrefixCount);
        }
    }

    [Alternate(nameof(StoreWord), AddressSize = 32, OperandSize = 32)]
    public static void StoreDWord32(VirtualMachine vm)
    {
        if (vm.Processor.RepeatPrefix == RepeatPrefix.None)
            StoreSingleDWord32(vm);
        else
            StoreDWords32(vm);

        vm.Processor.InstructionEpilog();
    }
    private static void StoreSingleDWord32(VirtualMachine vm)
    {
        vm.PhysicalMemory.SetUInt32(vm.Processor.ESBase + vm.Processor.EDI, (uint)vm.Processor.EAX);

        if (!vm.Processor.Flags.Direction)
            vm.Processor.EDI += 4;
        else
            vm.Processor.EDI -= 4;
    }
    private static void StoreDWords32(VirtualMachine vm)
    {
        if (vm.Processor.ECX != 0)
        {
            StoreSingleDWord32(vm);
            vm.Processor.ECX--;
            vm.Processor.EIP -= (ushort)(1 + vm.Processor.PrefixCount);
        }
    }
}
