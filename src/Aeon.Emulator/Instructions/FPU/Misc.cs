using System.Runtime.CompilerServices;

namespace Aeon.Emulator.Instructions.FPU;

internal static class Misc
{
    [Opcode("DBE3", Name = "finit", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void Finit(VirtualMachine vm)
    {
        vm.Processor.FPU.Reset();
    }

    [Opcode("DBE4", Name = "fnsetpm", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void Fnsetpm(VirtualMachine vm)
    {
    }

    [Opcode("DBE1", Name = "fdisi", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void Fdisi(VirtualMachine vm)
    {
    }
}

internal static class StatusWord
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Opcode("DD/7 m16|DFE0 ax", Name = "fstsw", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void StoreStatusWord(Processor p, out ushort dest)
    {
        dest = p.FPU.StatusWord;
    }
}

internal static class ControlWord
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Opcode("D9/5 m16", Name = "fldcw", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void LoadControlWord(Processor p, ushort src)
    {
        p.FPU.ControlWord = src;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Opcode("D9/7 m16", Name = "fstcw", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void StoreControlWord(Processor p, out ushort dest)
    {
        dest = p.FPU.ControlWord;
    }
}

internal static class Fclex
{
    [Opcode("DBE2", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void ClearExceptions(VirtualMachine vm)
    {
        vm.Processor.FPU.StatusFlags &= ~(FPUStatus.Precision | FPUStatus.Underflow | FPUStatus.Overflow | FPUStatus.ZeroDivide | FPUStatus.Denormalized | FPUStatus.InvalidOperation | FPUStatus.StackFault);
    }
}
