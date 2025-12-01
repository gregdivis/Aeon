namespace Aeon.Emulator.Instructions.ProtectedMode;

internal static class Misc
{
    [Opcode("0F06", Name = "clts", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void Clts(VirtualMachine vm)
    {
        vm.Processor.CR0 &= ~CR0.TaskSwitched;
        vm.Processor.InstructionEpilog();
    }
}
