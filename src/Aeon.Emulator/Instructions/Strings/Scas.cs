namespace Aeon.Emulator.Instructions.Strings;

internal static class Scasb
{
    [Opcode("AE", OperandSize = 16 | 32, AddressSize = 16)]
    public static void ScanByte(VirtualMachine vm)
    {
        if (vm.Processor.RepeatPrefix == RepeatPrefix.Repne)
            ScanBytesWhileNotEqual(vm);
        else if (vm.Processor.RepeatPrefix == RepeatPrefix.Repe)
            ScanBytesWhileEqual(vm);
        else
            ScanSingleByte(vm);

        vm.Processor.InstructionEpilog();
    }
    private static void ScanSingleByte(VirtualMachine vm)
    {
        byte dest = vm.PhysicalMemory.GetByte(vm.Processor.ESBase + vm.Processor.DI);
        Cmp.ByteCompare(vm.Processor, vm.Processor.AL, dest);

        if (!vm.Processor.Flags.Direction)
            vm.Processor.DI++;
        else
            vm.Processor.DI--;
    }
    private static void ScanBytesWhileEqual(VirtualMachine vm)
    {
        if (vm.Processor.CX != 0)
        {
            ScanSingleByte(vm);
            vm.Processor.CX--;
            if (vm.Processor.CX != 0 && vm.Processor.Flags.Zero)
                vm.Processor.EIP -= (ushort)(1 + vm.Processor.PrefixCount);
        }
    }
    private static void ScanBytesWhileNotEqual(VirtualMachine vm)
    {
        if (vm.Processor.CX != 0)
        {
            ScanSingleByte(vm);
            vm.Processor.CX--;
            if (vm.Processor.CX != 0 && !vm.Processor.Flags.Zero)
                vm.Processor.EIP -= (ushort)(1 + vm.Processor.PrefixCount);
        }
    }

    [Alternate(nameof(ScanByte), OperandSize = 16 | 32, AddressSize = 32)]
    public static void ScanByte32(VirtualMachine vm)
    {
        if (vm.Processor.RepeatPrefix == RepeatPrefix.Repne)
            ScanBytesWhileNotEqual32(vm);
        else if (vm.Processor.RepeatPrefix == RepeatPrefix.Repe)
            ScanBytesWhileEqual32(vm);
        else
            ScanSingleByte32(vm);

        vm.Processor.InstructionEpilog();
    }
    private static void ScanSingleByte32(VirtualMachine vm)
    {
        byte dest = vm.PhysicalMemory.GetByte(vm.Processor.ESBase + vm.Processor.EDI);
        Cmp.ByteCompare(vm.Processor, vm.Processor.AL, dest);

        if (!vm.Processor.Flags.Direction)
            vm.Processor.EDI++;
        else
            vm.Processor.EDI--;
    }
    private static void ScanBytesWhileEqual32(VirtualMachine vm)
    {
        if (vm.Processor.ECX != 0)
        {
            ScanSingleByte32(vm);
            vm.Processor.ECX--;
            if (vm.Processor.ECX != 0 && vm.Processor.Flags.Zero)
                vm.Processor.EIP -= (uint)(1 + vm.Processor.PrefixCount);
        }
    }
    private static void ScanBytesWhileNotEqual32(VirtualMachine vm)
    {
        if (vm.Processor.ECX != 0)
        {
            ScanSingleByte32(vm);
            vm.Processor.ECX--;
            if (vm.Processor.ECX != 0 && !vm.Processor.Flags.Zero)
                vm.Processor.EIP -= (uint)(1 + vm.Processor.PrefixCount);
        }
    }
}

internal static class Scasw
{
    [Opcode("AF", OperandSize = 16, AddressSize = 16)]
    public static void ScanWord(VirtualMachine vm)
    {
        if (vm.Processor.RepeatPrefix == RepeatPrefix.Repne)
            ScanWordsWhileNotEqual(vm);
        else if (vm.Processor.RepeatPrefix == RepeatPrefix.Repe)
            ScanWordsWhileEqual(vm);
        else
            ScanSingleWord(vm);

        vm.Processor.InstructionEpilog();
    }
    private static void ScanSingleWord(VirtualMachine vm)
    {
        ushort dest = vm.PhysicalMemory.GetUInt16(vm.Processor.ESBase + vm.Processor.DI);
        Cmp.WordCompare(vm.Processor, (ushort)vm.Processor.AX, dest);

        if (!vm.Processor.Flags.Direction)
            vm.Processor.DI += 2;
        else
            vm.Processor.DI -= 2;
    }
    private static void ScanWordsWhileEqual(VirtualMachine vm)
    {
        if (vm.Processor.CX != 0)
        {
            ScanSingleWord(vm);
            vm.Processor.CX--;
            if (vm.Processor.CX != 0 && vm.Processor.Flags.Zero)
                vm.Processor.EIP -= (ushort)(1 + vm.Processor.PrefixCount);
        }
    }
    private static void ScanWordsWhileNotEqual(VirtualMachine vm)
    {
        if (vm.Processor.CX != 0)
        {
            ScanSingleWord(vm);
            vm.Processor.CX--;
            if (vm.Processor.CX != 0 && !vm.Processor.Flags.Zero)
                vm.Processor.EIP -= (ushort)(1 + vm.Processor.PrefixCount);
        }
    }

    [Alternate(nameof(ScanWord), OperandSize = 16, AddressSize = 32)]
    public static void ScanWord32(VirtualMachine vm)
    {
        if (vm.Processor.RepeatPrefix == RepeatPrefix.Repne)
            ScanWordsWhileNotEqual32(vm);
        else if (vm.Processor.RepeatPrefix == RepeatPrefix.Repe)
            ScanWordsWhileEqual32(vm);
        else
            ScanSingleWord32(vm);

        vm.Processor.InstructionEpilog();
    }
    private static void ScanSingleWord32(VirtualMachine vm)
    {
        ushort dest = vm.PhysicalMemory.GetUInt16(vm.Processor.ESBase + vm.Processor.EDI);
        Cmp.WordCompare(vm.Processor, (ushort)vm.Processor.AX, dest);

        if (!vm.Processor.Flags.Direction)
            vm.Processor.EDI += 2;
        else
            vm.Processor.EDI -= 2;
    }
    private static void ScanWordsWhileEqual32(VirtualMachine vm)
    {
        if (vm.Processor.CX != 0)
        {
            ScanSingleWord32(vm);
            vm.Processor.CX--;
            if (vm.Processor.CX != 0 && vm.Processor.Flags.Zero)
                vm.Processor.EIP -= (ushort)(1 + vm.Processor.PrefixCount);
        }
    }
    private static void ScanWordsWhileNotEqual32(VirtualMachine vm)
    {
        if (vm.Processor.CX != 0)
        {
            ScanSingleWord32(vm);
            vm.Processor.CX--;
            if (vm.Processor.CX != 0 && !vm.Processor.Flags.Zero)
                vm.Processor.EIP -= (ushort)(1 + vm.Processor.PrefixCount);
        }
    }

    [Alternate(nameof(ScanWord), OperandSize = 32, AddressSize = 16)]
    public static void ScanDWord(VirtualMachine vm)
    {
        if (vm.Processor.RepeatPrefix == RepeatPrefix.Repne)
            ScanDWordsWhileNotEqual(vm);
        else if (vm.Processor.RepeatPrefix == RepeatPrefix.Repe)
            ScanDWordsWhileEqual(vm);
        else
            ScanSingleDWord(vm);

        vm.Processor.InstructionEpilog();
    }
    private static void ScanSingleDWord(VirtualMachine vm)
    {
        uint dest = vm.PhysicalMemory.GetUInt32(vm.Processor.ESBase + vm.Processor.DI);
        Cmp.DWordCompare(vm.Processor, (uint)vm.Processor.EAX, dest);

        if (!vm.Processor.Flags.Direction)
            vm.Processor.DI += 4;
        else
            vm.Processor.DI -= 4;
    }
    private static void ScanDWordsWhileEqual(VirtualMachine vm)
    {
        if (vm.Processor.CX != 0)
        {
            ScanSingleDWord(vm);
            vm.Processor.CX--;
            if (vm.Processor.CX != 0 && vm.Processor.Flags.Zero)
                vm.Processor.EIP -= (ushort)(1 + vm.Processor.PrefixCount);
        }
    }
    private static void ScanDWordsWhileNotEqual(VirtualMachine vm)
    {
        if (vm.Processor.CX != 0)
        {
            ScanSingleDWord(vm);
            vm.Processor.CX--;
            if (vm.Processor.CX != 0 && !vm.Processor.Flags.Zero)
                vm.Processor.EIP -= (ushort)(1 + vm.Processor.PrefixCount);
        }
    }

    [Alternate(nameof(ScanWord), OperandSize = 32, AddressSize = 32)]
    public static void ScanDWord32(VirtualMachine vm)
    {
        if (vm.Processor.RepeatPrefix == RepeatPrefix.Repne)
            ScanDWordsWhileNotEqual32(vm);
        else if (vm.Processor.RepeatPrefix == RepeatPrefix.Repe)
            ScanDWordsWhileEqual32(vm);
        else
            ScanSingleDWord32(vm);

        vm.Processor.InstructionEpilog();
    }
    private static void ScanSingleDWord32(VirtualMachine vm)
    {
        uint dest = vm.PhysicalMemory.GetUInt32(vm.Processor.ESBase + vm.Processor.EDI);
        Cmp.DWordCompare(vm.Processor, (uint)vm.Processor.EAX, dest);

        if (!vm.Processor.Flags.Direction)
            vm.Processor.EDI += 4;
        else
            vm.Processor.EDI -= 4;
    }
    private static void ScanDWordsWhileEqual32(VirtualMachine vm)
    {
        if (vm.Processor.ECX != 0)
        {
            ScanSingleDWord32(vm);
            vm.Processor.ECX--;
            if (vm.Processor.ECX != 0 && vm.Processor.Flags.Zero)
                vm.Processor.EIP -= (ushort)(1 + vm.Processor.PrefixCount);
        }
    }
    private static void ScanDWordsWhileNotEqual32(VirtualMachine vm)
    {
        if (vm.Processor.ECX != 0)
        {
            ScanSingleDWord32(vm);
            vm.Processor.ECX--;
            if (vm.Processor.ECX != 0 && !vm.Processor.Flags.Zero)
                vm.Processor.EIP -= (ushort)(1 + vm.Processor.PrefixCount);
        }
    }
}
