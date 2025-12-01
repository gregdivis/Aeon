namespace Aeon.Emulator.Instructions.Stack;

internal static class PopSegment
{
    [Opcode("1F", Name = "pop ds", AddressSize = 16 | 32)]
    public static void PopDS(VirtualMachine vm)
    {
        vm.WriteSegmentRegister(SegmentIndex.DS, vm.PeekStack16());

        vm.AddToStackPointer(2);
        vm.Processor.InstructionEpilog();
    }
    [Alternate("PopDS", AddressSize = 16 | 32)]
    public static void PopDS32(VirtualMachine vm)
    {
        vm.WriteSegmentRegister(SegmentIndex.DS, vm.PeekStack16());
        
        vm.AddToStackPointer(4);
        vm.Processor.InstructionEpilog();
    }

    [Opcode("07", Name = "pop es", AddressSize = 16 | 32)]
    public static void PopES(VirtualMachine vm)
    {
        vm.WriteSegmentRegister(SegmentIndex.ES, vm.PeekStack16());
        
        vm.AddToStackPointer(2);
        vm.Processor.InstructionEpilog();
    }
    [Alternate("PopES", AddressSize = 16 | 32)]
    public static void PopES32(VirtualMachine vm)
    {
        vm.WriteSegmentRegister(SegmentIndex.ES, vm.PeekStack16());
        
        vm.AddToStackPointer(4);
        vm.Processor.InstructionEpilog();
    }
    
    [Opcode("17", Name = "pop ss", AddressSize = 16 | 32)]
    public static void PopSS(VirtualMachine vm)
    {
        vm.WriteSegmentRegister(SegmentIndex.SS, vm.PeekStack16());

        vm.AddToStackPointer(2);
        vm.Processor.InstructionEpilog();
        vm.Processor.TemporaryInterruptMask = true;
    }
    
    [Opcode("0FA1", Name = "pop fs", AddressSize = 16 | 32)]
    public static void PopFS(VirtualMachine vm)
    {
        vm.WriteSegmentRegister(SegmentIndex.FS, vm.PeekStack16());

        vm.AddToStackPointer(2);
        vm.Processor.InstructionEpilog();
    }
    [Alternate("PopFS", AddressSize = 16 | 32)]
    public static void PopFS32(VirtualMachine vm)
    {
        vm.WriteSegmentRegister(SegmentIndex.FS, vm.PeekStack16());

        vm.AddToStackPointer(4);
        vm.Processor.InstructionEpilog();
    }
    
    [Opcode("0FA9", Name = "pop gs", AddressSize = 16 | 32)]
    public static void PopGS(VirtualMachine vm)
    {
        vm.WriteSegmentRegister(SegmentIndex.GS, vm.PeekStack16());

        vm.AddToStackPointer(2);
        vm.Processor.InstructionEpilog();
    }
    [Alternate("PopGS", AddressSize = 16 | 32)]
    public static void PopGS32(VirtualMachine vm)
    {
        vm.WriteSegmentRegister(SegmentIndex.GS, vm.PeekStack16());

        vm.AddToStackPointer(4);
        vm.Processor.InstructionEpilog();
    }

    [Alternate(nameof(PopSS), AddressSize = 16 | 32)]
    public static void PopSS32(VirtualMachine vm)
    {
        vm.WriteSegmentRegister(SegmentIndex.SS, vm.PeekStack16());

        vm.AddToStackPointer(4);
        vm.Processor.InstructionEpilog();
        vm.Processor.TemporaryInterruptMask = true;
    }
}
