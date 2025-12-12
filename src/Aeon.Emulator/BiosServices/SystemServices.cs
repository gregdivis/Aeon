namespace Aeon.Emulator.BiosServices;

/// <summary>
/// Implements BIOS system services.
/// </summary>
internal sealed class SystemServices(VirtualMachine vm) : IInterruptHandler
{
    private readonly VirtualMachine vm = vm;

    IEnumerable<InterruptHandlerInfo> IInterruptHandler.HandledInterrupts => [0x11, 0x12, 0x15];

    void IInterruptHandler.HandleInterrupt(int interrupt)
    {
        if (interrupt == 0x11)
        {
            vm.Processor.AX = unchecked((short)0xD426);
            return;
        }

        if (interrupt == 0x12)
        {
            vm.Processor.AX = 640;
            return;
        }

        bool error = false;

        switch (vm.Processor.AH)
        {
            case Functions.GetConfiguration:
                vm.WriteSegmentRegister(SegmentIndex.ES, PhysicalMemory.BiosConfigurationAddress.Segment);
                vm.Processor.BX = (short)PhysicalMemory.BiosConfigurationAddress.Offset;
                vm.Processor.AH = 0;
                break;

            case Functions.GetExtendedMemorySize:
                vm.Processor.AX = 0;
                break;

            default:
                System.Diagnostics.Debug.WriteLine($"System int 15h command {vm.Processor.AH:X2} not implemented.");
                error = true;
                break;
        }

        vm.Processor.Flags.Carry = error;
        vm.PhysicalMemory.SetUInt16(vm.Processor.SS, (ushort)(vm.Processor.SP + 4), (ushort)vm.Processor.Flags.Value);
    }
}
