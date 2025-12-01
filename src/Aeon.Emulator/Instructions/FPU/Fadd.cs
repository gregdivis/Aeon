using System.Runtime.CompilerServices;

namespace Aeon.Emulator.Instructions.FPU;

internal static class Fadd
{
    [Opcode("D8/0 mf32", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AddToST032(Processor p, float value)
    {
        AddToST064(p, value);
    }

    [Opcode("D8C0+ st|DCC0 st0|DC/0 mf64", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AddToST064(Processor p, double value)
    {
        p.FPU.ST0_Ref += value;
    }

    [Opcode("DCC1", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AddToST1(Processor p)
    {
        p.FPU.GetRegisterRef(1) += p.FPU.ST0_Ref;
    }

    [Opcode("DCC2", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AddToST2(Processor p)
    {
        p.FPU.GetRegisterRef(2) += p.FPU.ST0_Ref;
    }

    [Opcode("DCC3", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AddToST3(Processor p)
    {
        p.FPU.GetRegisterRef(3) += p.FPU.ST0_Ref;
    }

    [Opcode("DCC4", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AddToST4(Processor p)
    {
        p.FPU.GetRegisterRef(4) += p.FPU.ST0_Ref;
    }

    [Opcode("DCC5", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AddToST5(Processor p)
    {
        p.FPU.GetRegisterRef(5) += p.FPU.ST0_Ref;
    }

    [Opcode("DCC6", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AddToST6(Processor p)
    {
        p.FPU.GetRegisterRef(6) += p.FPU.ST0_Ref;
    }

    [Opcode("DCC7", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AddToST7(Processor p)
    {
        p.FPU.GetRegisterRef(7) += p.FPU.ST0_Ref;
    }
}

internal static class Faddp
{
    [Opcode("DEC0", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AddToST0(VirtualMachine vm)
    {
        throw new InvalidOperationException();
    }

    [Opcode("DEC1", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AddToST1(Processor p)
    {
        Fadd.AddToST1(p);
        p.FPU.Pop();
    }

    [Opcode("DEC2", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AddToST2(Processor p)
    {
        Fadd.AddToST2(p);
        p.FPU.Pop();
    }

    [Opcode("DEC3", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AddToST3(Processor p)
    {
        Fadd.AddToST3(p);
        p.FPU.Pop();
    }

    [Opcode("DEC4", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AddToST4(Processor p)
    {
        Fadd.AddToST4(p);
        p.FPU.Pop();
    }

    [Opcode("DEC5", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AddToST5(Processor p)
    {
        Fadd.AddToST5(p);
        p.FPU.Pop();
    }

    [Opcode("DEC6", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AddToST6(Processor p)
    {
        Fadd.AddToST6(p);
        p.FPU.Pop();
    }

    [Opcode("DEC7", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AddToST7(Processor p)
    {
        Fadd.AddToST7(p);
        p.FPU.Pop();
    }
}
