namespace Aeon.Emulator.Instructions.ProtectedMode;

internal static class LoadControl
{
    [Opcode("0F22/0 rm32", Name = "ldcr0", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void MovToCR0(VirtualMachine vm, uint value)
    {
        vm.Processor.CR0 = (CR0)value;

        bool paging = false;
        if (vm.Processor.CR0.HasFlag(CR0.Paging | CR0.ProtectedModeEnable))
            paging = true;

        vm.PhysicalMemory.PagingEnabled = paging;
    }

    [Opcode("0F01/6 rm16", Name = "lmsw")]
    public static void LoadMachineStatusWord(VirtualMachine vm, ushort value)
    {
        vm.Processor.CR0 &= unchecked((CR0)0xFFFF0000);
        vm.Processor.CR0 |= (CR0)value;

        bool paging = false;
        if (vm.Processor.CR0.HasFlag(CR0.Paging | CR0.ProtectedModeEnable))
            paging = true;

        vm.PhysicalMemory.PagingEnabled = paging;
    }

    [Opcode("0F22/2 rm32", Name = "ldcr2", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void MovToCR2(Processor p, uint value)
    {
        p.CR2 = value;
    }

    [Opcode("0F22/3 rm32", Name = "ldcr3", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void MovToCR3(VirtualMachine vm, uint value)
    {
        vm.Processor.CR3 = value;
        vm.PhysicalMemory.DirectoryAddress = value;
    }
}
