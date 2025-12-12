using System.Numerics;
using System.Runtime.CompilerServices;

namespace Aeon.Emulator.Instructions.Stack;

internal static class Push
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Opcode("6A ibx", AddressSize = 16 | 32, OperandSize = 16 | 32)]
    public static void PushByteGeneric<TValue>(VirtualMachine vm, TValue value) where TValue : unmanaged, IBinaryInteger<TValue>, ISignedNumber<TValue>
    {
        vm.PushToStackGeneric(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Opcode("FF/6 rmw|50+ rw|68 iw", AddressSize = 16 | 32, OperandSize = 16 | 32)]
    public static void PushWordGeneric<TValue>(VirtualMachine vm, TValue value) where TValue : unmanaged, IBinaryInteger<TValue>
    {
        vm.PushToStackGeneric(value);
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
