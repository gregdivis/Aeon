namespace Aeon.Emulator.Instructions.Strings;

internal static class Cmpsb
{
    [Opcode("A6", OperandSize = 16 | 32, AddressSize = 16)]
    public static void CompareByte(VirtualMachine vm)
    {
        if (vm.Processor.RepeatPrefix == RepeatPrefix.Repe)
            CompareBytesWhileEqual(vm);
        else if (vm.Processor.RepeatPrefix == RepeatPrefix.Repne)
            CompareBytesWhileNotEqual(vm);
        else
            CompareSingleByte(vm);

        vm.Processor.InstructionEpilog();
    }
    private static void CompareSingleByte(VirtualMachine vm)
    {
        var srcBase = vm.Processor.GetOverrideBase(SegmentIndex.DS);

        byte src = vm.PhysicalMemory.GetByte(srcBase + vm.Processor.SI);
        byte dest = vm.PhysicalMemory.GetByte(vm.Processor.ESBase + vm.Processor.DI);

        Cmp.GenericCompare(vm.Processor, src, dest);

        if (!vm.Processor.Flags.Direction)
        {
            vm.Processor.SI++;
            vm.Processor.DI++;
        }
        else
        {
            vm.Processor.SI--;
            vm.Processor.DI--;
        }
    }
    private static void CompareBytesWhileEqual(VirtualMachine vm)
    {
        if (vm.Processor.CX != 0)
        {
            CompareSingleByte(vm);
            vm.Processor.CX--;
            if (vm.Processor.CX != 0 && vm.Processor.Flags.Zero)
                vm.Processor.EIP -= (ushort)(1 + vm.Processor.PrefixCount);
        }
    }
    private static void CompareBytesWhileNotEqual(VirtualMachine vm)
    {
        if (vm.Processor.CX != 0)
        {
            CompareSingleByte(vm);
            vm.Processor.CX--;
            if (vm.Processor.CX != 0 && !vm.Processor.Flags.Zero)
                vm.Processor.EIP -= (ushort)(1 + vm.Processor.PrefixCount);
        }
    }

    [Alternate(nameof(CompareByte), OperandSize = 16 | 32, AddressSize = 32)]
    public static void CompareByte32(VirtualMachine vm)
    {
        if (vm.Processor.RepeatPrefix == RepeatPrefix.Repe)
            CompareBytesWhileEqual32(vm);
        else if (vm.Processor.RepeatPrefix == RepeatPrefix.Repne)
            CompareBytesWhileNotEqual32(vm);
        else
            CompareSingleByte32(vm);

        vm.Processor.InstructionEpilog();

    }
    private static void CompareSingleByte32(VirtualMachine vm)
    {
        var srcBase = vm.Processor.GetOverrideBase(SegmentIndex.DS);

        byte src = vm.PhysicalMemory.GetByte(srcBase + vm.Processor.ESI);
        byte dest = vm.PhysicalMemory.GetByte(vm.Processor.ESBase + vm.Processor.EDI);

        Cmp.GenericCompare(vm.Processor, src, dest);

        if (!vm.Processor.Flags.Direction)
        {
            vm.Processor.ESI++;
            vm.Processor.EDI++;
        }
        else
        {
            vm.Processor.ESI--;
            vm.Processor.EDI--;
        }
    }
    private static void CompareBytesWhileEqual32(VirtualMachine vm)
    {
        if (vm.Processor.ECX != 0)
        {
            CompareSingleByte32(vm);
            vm.Processor.ECX--;
            if (vm.Processor.ECX != 0 && vm.Processor.Flags.Zero)
                vm.Processor.EIP -= (ushort)(1 + vm.Processor.PrefixCount);
        }
    }
    private static void CompareBytesWhileNotEqual32(VirtualMachine vm)
    {
        if (vm.Processor.ECX != 0)
        {
            CompareSingleByte32(vm);
            vm.Processor.ECX--;
            if (vm.Processor.ECX != 0 && !vm.Processor.Flags.Zero)
                vm.Processor.EIP -= (ushort)(1 + vm.Processor.PrefixCount);
        }
    }
}

internal static class Cmpsw
{
    [Opcode("A7", OperandSize = 16, AddressSize = 16)]
    public static void CompareWord(VirtualMachine vm)
    {
        if (vm.Processor.RepeatPrefix == RepeatPrefix.Repe)
            CompareWordsWhileEqual(vm);
        else if (vm.Processor.RepeatPrefix == RepeatPrefix.Repne)
            CompareWordsWhileNotEqual(vm);
        else
            CompareSingleWord(vm);

        vm.Processor.InstructionEpilog();
    }
    private static void CompareSingleWord(VirtualMachine vm)
    {
        var srcSegment = vm.Processor.GetOverrideBase(SegmentIndex.DS);
        ushort src = vm.PhysicalMemory.GetUInt16(srcSegment + vm.Processor.SI);
        ushort dest = vm.PhysicalMemory.GetUInt16(vm.Processor.ESBase + vm.Processor.DI);

        Cmp.GenericCompare(vm.Processor, src, dest);

        if (!vm.Processor.Flags.Direction)
        {
            vm.Processor.SI += 2;
            vm.Processor.DI += 2;
        }
        else
        {
            vm.Processor.SI -= 2;
            vm.Processor.DI -= 2;
        }
    }
    private static void CompareWordsWhileEqual(VirtualMachine vm)
    {
        if (vm.Processor.CX != 0)
        {
            CompareSingleWord(vm);
            vm.Processor.CX--;
            if (vm.Processor.CX != 0 && vm.Processor.Flags.Zero)
                vm.Processor.EIP -= (ushort)(1 + vm.Processor.PrefixCount);
        }
    }
    private static void CompareWordsWhileNotEqual(VirtualMachine vm)
    {
        if (vm.Processor.CX != 0)
        {
            CompareSingleWord(vm);
            vm.Processor.CX--;
            if (vm.Processor.CX != 0 && !vm.Processor.Flags.Zero)
                vm.Processor.EIP -= (ushort)(1 + vm.Processor.PrefixCount);
        }
    }

    [Alternate(nameof(CompareWord), OperandSize = 16, AddressSize = 32)]
    public static void CompareWord32(VirtualMachine vm)
    {
        if (vm.Processor.RepeatPrefix == RepeatPrefix.Repe)
            CompareWordsWhileEqual32(vm);
        else if (vm.Processor.RepeatPrefix == RepeatPrefix.Repne)
            CompareWordsWhileNotEqual32(vm);
        else
            CompareSingleWord32(vm);

        vm.Processor.InstructionEpilog();
    }
    private static void CompareSingleWord32(VirtualMachine vm)
    {
        var srcSegment = vm.Processor.GetOverrideBase(SegmentIndex.DS);
        ushort src = vm.PhysicalMemory.GetUInt16(srcSegment + vm.Processor.ESI);
        ushort dest = vm.PhysicalMemory.GetUInt16(vm.Processor.ESBase + vm.Processor.EDI);

        Cmp.GenericCompare(vm.Processor, src, dest);

        if (!vm.Processor.Flags.Direction)
        {
            vm.Processor.ESI += 2;
            vm.Processor.EDI += 2;
        }
        else
        {
            vm.Processor.ESI -= 2;
            vm.Processor.EDI -= 2;
        }
    }
    private static void CompareWordsWhileEqual32(VirtualMachine vm)
    {
        if (vm.Processor.ECX != 0)
        {
            CompareSingleWord32(vm);
            vm.Processor.ECX--;
            if (vm.Processor.ECX != 0 && vm.Processor.Flags.Zero)
                vm.Processor.EIP -= (ushort)(1 + vm.Processor.PrefixCount);
        }
    }
    private static void CompareWordsWhileNotEqual32(VirtualMachine vm)
    {
        if (vm.Processor.ECX != 0)
        {
            CompareSingleWord32(vm);
            vm.Processor.ECX--;
            if (vm.Processor.ECX != 0 && !vm.Processor.Flags.Zero)
                vm.Processor.EIP -= (ushort)(1 + vm.Processor.PrefixCount);
        }
    }

    [Alternate(nameof(CompareWord), OperandSize = 32, AddressSize = 32)]
    public static void CompareDWord32(VirtualMachine vm)
    {
        if (vm.Processor.RepeatPrefix == RepeatPrefix.Repe)
            CompareDWordsWhileEqual32(vm);
        else if (vm.Processor.RepeatPrefix == RepeatPrefix.Repne)
            CompareDWordsWhileNotEqual32(vm);
        else
            CompareSingleDWord32(vm);

        vm.Processor.InstructionEpilog();
    }
    private static void CompareSingleDWord32(VirtualMachine vm)
    {
        var srcSegment = vm.Processor.GetOverrideBase(SegmentIndex.DS);
        uint src = vm.PhysicalMemory.GetUInt32(srcSegment + vm.Processor.ESI);
        uint dest = vm.PhysicalMemory.GetUInt32(vm.Processor.ESBase + vm.Processor.EDI);

        Cmp.GenericCompare(vm.Processor, src, dest);

        if (!vm.Processor.Flags.Direction)
        {
            vm.Processor.ESI += 4;
            vm.Processor.EDI += 4;
        }
        else
        {
            vm.Processor.ESI -= 4;
            vm.Processor.EDI -= 4;
        }
    }
    private static void CompareDWordsWhileEqual32(VirtualMachine vm)
    {
        if (vm.Processor.ECX != 0)
        {
            CompareSingleDWord32(vm);
            vm.Processor.ECX--;
            if (vm.Processor.ECX != 0 && vm.Processor.Flags.Zero)
                vm.Processor.EIP -= (ushort)(1 + vm.Processor.PrefixCount);
        }
    }
    private static void CompareDWordsWhileNotEqual32(VirtualMachine vm)
    {
        if (vm.Processor.ECX != 0)
        {
            CompareSingleDWord32(vm);
            vm.Processor.ECX--;
            if (vm.Processor.ECX != 0 && !vm.Processor.Flags.Zero)
                vm.Processor.EIP -= (ushort)(1 + vm.Processor.PrefixCount);
        }
    }
}
