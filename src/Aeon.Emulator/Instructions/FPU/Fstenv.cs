namespace Aeon.Emulator.Instructions.FPU;

internal static class Fstenv
{
    [Opcode("D9/6 m64", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void StoreEnvironment(VirtualMachine vm, out ulong buffer)
    {
        var fpu = vm.Processor.FPU;
        buffer = fpu.ControlWord;
        buffer |= ((ulong)fpu.StatusWord) << 16;
        buffer |= ((ulong)fpu.TagWord) << 32;
    }
}
