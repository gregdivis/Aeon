namespace Aeon.Emulator.Instructions.Stack;

internal static class Push
{
    [Opcode("6A ibx", AddressSize = 16 | 32)]
    public static void PushByte(VirtualMachine vm, short value)
    {
        vm.PushToStack((ushort)value);
    }
    [Alternate(nameof(PushByte), AddressSize = 16 | 32)]
    public static void PushByteToDWord(VirtualMachine vm, int value)
    {
        vm.PushToStack32((uint)value);
    }

    [Opcode("FF/6 rmw|50+ rw|68 iw", AddressSize = 16 | 32)]
    public static void PushWord(VirtualMachine vm, ushort value)
    {
        vm.PushToStack(value);
    }
    [Alternate(nameof(PushWord), AddressSize = 16 | 32)]
    public static void PushDWord(VirtualMachine vm, uint value)
    {
        vm.PushToStack32(value);
    }

    [Opcode("9C", Name = "pushf", AddressSize = 16 | 32)]
    public static void PushFlags(VirtualMachine vm)
    {
        var p = vm.Processor;
        vm.PushToStack((ushort)p.Flags.Value);
        p.InstructionEpilog();
    }
    [Alternate(nameof(PushFlags), AddressSize = 16 | 32)]
    public static void PushFlags32(VirtualMachine vm)
    {
        var p = vm.Processor;
        vm.PushToStack32((uint)(p.Flags.Value & ~EFlags.Virtual8086Mode));
        p.InstructionEpilog();
    }

    [Opcode("60", Name = "pusha", AddressSize = 16 | 32)]
    public static void PushAll(VirtualMachine vm)
    {
        var p = vm.Processor;
        ushort sp = p.SP;
        vm.PushToStack((ushort)p.AX, (ushort)p.CX);
        vm.PushToStack((ushort)p.DX, (ushort)p.BX);
        vm.PushToStack(sp, p.BP);
        vm.PushToStack(p.SI, p.DI);

        p.InstructionEpilog();
    }
    [Alternate(nameof(PushAll), AddressSize = 16 | 32)]
    public static void PushAll32(VirtualMachine vm)
    {
        var p = vm.Processor;
        uint esp = p.ESP;
        vm.PushToStack32((uint)p.EAX);
        vm.PushToStack32((uint)p.ECX);
        vm.PushToStack32((uint)p.EDX);
        vm.PushToStack32((uint)p.EBX);
        vm.PushToStack32(esp);
        vm.PushToStack32(p.EBP);
        vm.PushToStack32(p.ESI);
        vm.PushToStack32(p.EDI);

        p.InstructionEpilog();
    }

    [Opcode("61", Name = "popa", AddressSize = 16 | 32)]
    public static void PopAll(VirtualMachine vm)
    {
        var p = vm.Processor;
        p.DI = vm.PopFromStack();
        p.SI = vm.PopFromStack();
        p.BP = vm.PopFromStack();
        vm.AddToStackPointer(2);
        p.BX = (short)vm.PopFromStack();
        p.DX = (short)vm.PopFromStack();
        p.CX = (short)vm.PopFromStack();
        p.AX = (short)vm.PopFromStack();

        p.InstructionEpilog();
    }
    [Alternate(nameof(PopAll), AddressSize = 16 | 32)]
    public static void PopAll32(VirtualMachine vm)
    {
        var p = vm.Processor;
        p.EDI = vm.PopFromStack32();
        p.ESI = vm.PopFromStack32();
        p.EBP = vm.PopFromStack32();
        vm.AddToStackPointer(4);
        p.EBX = (int)vm.PopFromStack32();
        p.EDX = (int)vm.PopFromStack32();
        p.ECX = (int)vm.PopFromStack32();
        p.EAX = (int)vm.PopFromStack32();

        p.InstructionEpilog();
    }
}
