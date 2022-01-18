namespace Aeon.Emulator.Instructions
{
    internal static class Enter
    {
        [Opcode("C8 i16,ib", AddressSize = 16 | 32)]
        public static void Enter16(VirtualMachine vm, ushort frameSize, byte nestingLevel)
        {
            uint level = nestingLevel & 0x1Fu;
            uint frameTemp;

            vm.PushToStack(vm.Processor.BP);
            frameTemp = vm.Processor.SP;

            if (level > 0)
                ThrowHelper.ThrowNotImplementedException();

            vm.Processor.BP = (ushort)frameTemp;
            vm.Processor.SP = (ushort)(vm.Processor.BP - frameSize);
        }
        [Alternate(nameof(Enter16), AddressSize = 16 | 32)]
        public static void Enter32(VirtualMachine vm, ushort frameSize, byte nestingLevel)
        {
            uint level = nestingLevel & 0x1Fu;
            uint frameTemp;

            vm.PushToStack32(vm.Processor.EBP);
            frameTemp = vm.Processor.ESP;

            if (level > 0)
                ThrowHelper.ThrowNotImplementedException();

            vm.Processor.EBP = frameTemp;
            vm.Processor.ESP = (uint)(vm.Processor.EBP - frameSize);
        }
    }
}
