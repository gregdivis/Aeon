using System.Runtime.CompilerServices;

namespace Aeon.Emulator.Instructions.FPU;

internal static class Fdiv
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Opcode("D8/6 mf32", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void DivideST032(Processor p, float value)
    {
        DivideST064(p, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Opcode("DC/6 mf64|D8F0+ st|DCF8 st0", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void DivideST064(Processor p, double value)
    {
        p.FPU.ST0_Ref /= value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Opcode("DCF9", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void DivideToST1(Processor p)
    {
        p.FPU.GetRegisterRef(1) /= p.FPU.ST0_Ref;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Opcode("DCFA", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void DivideToST2(Processor p)
    {
        p.FPU.GetRegisterRef(2) /= p.FPU.ST0_Ref;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Opcode("DCFB", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void DivideToST3(Processor p)
    {
        p.FPU.GetRegisterRef(3) /= p.FPU.ST0_Ref;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Opcode("DCFC", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void DivideToST4(Processor p)
    {
        p.FPU.GetRegisterRef(4) /= p.FPU.ST0_Ref;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Opcode("DCFD", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void DivideToST5(Processor p)
    {
        p.FPU.GetRegisterRef(5) /= p.FPU.ST0_Ref;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Opcode("DCFE", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void DivideToST6(Processor p)
    {
        p.FPU.GetRegisterRef(6) /= p.FPU.ST0_Ref;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Opcode("DCFF", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void DivideToST7(Processor p)
    {
        p.FPU.GetRegisterRef(7) /= p.FPU.ST0_Ref;
    }
}

internal static class Fdivp
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Opcode("DEF8", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void DivideToST0(VirtualMachine vm)
    {
        throw new InvalidOperationException();
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Opcode("DEF9", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void DivideToST1(Processor p)
    {
        Fdiv.DivideToST1(p);
        p.FPU.Pop();
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Opcode("DEFA", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void DivideToST2(Processor p)
    {
        Fdiv.DivideToST2(p);
        p.FPU.Pop();
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Opcode("DEFB", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void DivideToST3(Processor p)
    {
        Fdiv.DivideToST3(p);
        p.FPU.Pop();
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Opcode("DEFC", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void DivideToST4(Processor p)
    {
        Fdiv.DivideToST4(p);
        p.FPU.Pop();
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Opcode("DEFD", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void DivideToST5(Processor p)
    {
        Fdiv.DivideToST5(p);
        p.FPU.Pop();
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Opcode("DEFE", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void DivideToST6(Processor p)
    {
        Fdiv.DivideToST6(p);
        p.FPU.Pop();
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Opcode("DEFF", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void DivideToST7(Processor p)
    {
        Fdiv.DivideToST7(p);
        p.FPU.Pop();
    }
}
