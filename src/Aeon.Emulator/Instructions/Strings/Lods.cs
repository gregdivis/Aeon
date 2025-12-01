namespace Aeon.Emulator.Instructions.Strings;

internal static class Lodsb
{
    [Opcode("AC", OperandSize = 16 | 32, AddressSize = 16)]
    public static void LoadByte(VirtualMachine vm)
    {
        if (vm.Processor.RepeatPrefix == RepeatPrefix.None)
            LoadSingleByte(vm);
        else
            LoadBytes(vm);

        vm.Processor.InstructionEpilog();
    }
    private static void LoadSingleByte(VirtualMachine vm)
    {
        vm.Processor.AL = vm.PhysicalMemory.GetByte(vm.Processor.GetOverrideBase(SegmentIndex.DS) + vm.Processor.SI);

        if (!vm.Processor.Flags.Direction)
            vm.Processor.SI++;
        else
            vm.Processor.SI--;
    }
    private static void LoadBytes(VirtualMachine vm)
    {
        if (vm.Processor.CX != 0)
        {
            LoadSingleByte(vm);

            vm.Processor.CX--;
            vm.Processor.IP -= (ushort)(1 + vm.Processor.PrefixCount);
        }
    }

    [Alternate(nameof(LoadByte), OperandSize = 16 | 32, AddressSize = 32)]
    public static void LoadByte32(VirtualMachine vm)
    {
        if (vm.Processor.RepeatPrefix == RepeatPrefix.None)
            LoadSingleByte32(vm);
        else
            LoadBytes32(vm);

        vm.Processor.InstructionEpilog();
    }
    private static void LoadSingleByte32(VirtualMachine vm)
    {
        vm.Processor.AL = vm.PhysicalMemory.GetByte(vm.Processor.GetOverrideBase(SegmentIndex.DS) + vm.Processor.ESI);

        if (!vm.Processor.Flags.Direction)
            vm.Processor.ESI++;
        else
            vm.Processor.ESI--;
    }
    private static void LoadBytes32(VirtualMachine vm)
    {
        if (vm.Processor.ECX != 0)
        {
            LoadSingleByte32(vm);

            vm.Processor.ECX--;
            vm.Processor.EIP -= (ushort)(1 + vm.Processor.PrefixCount);
        }
    }
}

internal static class Lodsw
{
    [Opcode("AD", OperandSize = 16, AddressSize = 16)]
    public static void LoadWord(VirtualMachine vm)
    {
        if (vm.Processor.RepeatPrefix == RepeatPrefix.None)
            LoadSingleWord(vm);
        else
            LoadWords(vm);

        vm.Processor.InstructionEpilog();
    }
    private static void LoadSingleWord(VirtualMachine vm)
    {
        vm.Processor.AX = (short)vm.PhysicalMemory.GetUInt16(vm.Processor.GetOverrideBase(SegmentIndex.DS) + vm.Processor.SI);

        if (!vm.Processor.Flags.Direction)
            vm.Processor.SI += 2;
        else
            vm.Processor.SI -= 2;
    }
    private static void LoadWords(VirtualMachine vm)
    {
        if (vm.Processor.CX != 0)
        {
            LoadSingleWord(vm);

            vm.Processor.CX--;
            vm.Processor.IP -= (ushort)(1 + vm.Processor.PrefixCount);
        }
    }

    [Alternate(nameof(LoadWord), OperandSize = 16, AddressSize = 32)]
    public static void LoadWord32(VirtualMachine vm)
    {
        if (vm.Processor.RepeatPrefix == RepeatPrefix.None)
            LoadSingleWord32(vm);
        else
            LoadWords32(vm);

        vm.Processor.InstructionEpilog();
    }
    private static void LoadSingleWord32(VirtualMachine vm)
    {
        vm.Processor.AX = (short)vm.PhysicalMemory.GetUInt16(vm.Processor.GetOverrideBase(SegmentIndex.DS) + vm.Processor.ESI);

        if (!vm.Processor.Flags.Direction)
            vm.Processor.ESI += 2;
        else
            vm.Processor.ESI -= 2;
    }
    private static void LoadWords32(VirtualMachine vm)
    {
        if (vm.Processor.ECX != 0)
        {
            LoadSingleWord32(vm);

            vm.Processor.ECX--;
            vm.Processor.EIP -= (ushort)(1 + vm.Processor.PrefixCount);
        }
    }

    [Alternate(nameof(LoadWord), OperandSize = 32, AddressSize = 16)]
    public static void LoadDWord(VirtualMachine vm)
    {
        if (vm.Processor.RepeatPrefix == RepeatPrefix.None)
            LoadSingleDWord(vm);
        else
            LoadDWords(vm);

        vm.Processor.InstructionEpilog();
    }
    private static void LoadSingleDWord(VirtualMachine vm)
    {
        vm.Processor.EAX = (int)vm.PhysicalMemory.GetUInt32(vm.Processor.GetOverrideBase(SegmentIndex.DS) + vm.Processor.SI);

        if (!vm.Processor.Flags.Direction)
            vm.Processor.SI += 4;
        else
            vm.Processor.SI -= 4;
    }
    private static void LoadDWords(VirtualMachine vm)
    {
        if (vm.Processor.CX != 0)
        {
            LoadSingleDWord(vm);

            vm.Processor.CX--;
            vm.Processor.EIP -= (ushort)(1 + vm.Processor.PrefixCount);
        }
    }

    [Alternate(nameof(LoadWord), OperandSize = 32, AddressSize = 32)]
    public static void LoadDWord32(VirtualMachine vm)
    {
        if (vm.Processor.RepeatPrefix == RepeatPrefix.None)
            LoadSingleDWord32(vm);
        else
            LoadDWords32(vm);

        vm.Processor.InstructionEpilog();
    }
    private static void LoadSingleDWord32(VirtualMachine vm)
    {
        vm.Processor.EAX = (int)vm.PhysicalMemory.GetUInt32(vm.Processor.GetOverrideBase(SegmentIndex.DS) + vm.Processor.ESI);

        if (!vm.Processor.Flags.Direction)
            vm.Processor.ESI += 4;
        else
            vm.Processor.ESI -= 4;
    }
    private static void LoadDWords32(VirtualMachine vm)
    {
        if (vm.Processor.ECX != 0)
        {
            LoadSingleDWord32(vm);

            vm.Processor.ECX--;
            vm.Processor.EIP -= (ushort)(1 + vm.Processor.PrefixCount);
        }
    }
}
