namespace Aeon.Emulator.Instructions.FPU;

internal static class Fmul
{
    [Opcode("D8/1 mf32", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void MultiplyST032(Processor p, float value)
    {
        p.FPU.ST0_Ref *= value;
    }

    [Opcode("DC/1 mf64|D8C8+ st|DCC8 st0", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void MultiplyST064(Processor p, double value)
    {
        p.FPU.ST0_Ref *= value;
    }

    [Opcode("DCC9", Name = "fmul st1", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void MultplyToST1(Processor p)
    {
        p.FPU.GetRegisterRef(1) *= p.FPU.ST0_Ref;
    }

    [Opcode("DCCA", Name = "fmul st2", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void MultplyToST2(Processor p)
    {
        p.FPU.GetRegisterRef(2) *= p.FPU.ST0_Ref;
    }

    [Opcode("DCCB", Name = "fmul st3", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void MultplyToST3(Processor p)
    {
        p.FPU.GetRegisterRef(3) *= p.FPU.ST0_Ref;
    }

    [Opcode("DCCC", Name = "fmul st4", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void MultplyToST4(Processor p)
    {
        p.FPU.GetRegisterRef(4) *= p.FPU.ST0_Ref;
    }

    [Opcode("DCCD", Name = "fmul st5", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void MultplyToST5(Processor p)
    {
        p.FPU.GetRegisterRef(5) *= p.FPU.ST0_Ref;
    }

    [Opcode("DCCE", Name = "fmul st6", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void MultplyToST6(Processor p)
    {
        p.FPU.GetRegisterRef(6) *= p.FPU.ST0_Ref;
    }

    [Opcode("DCCF", Name = "fmul st7", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void MultplyToST7(Processor p)
    {
        p.FPU.GetRegisterRef(7) *= p.FPU.ST0_Ref;
    }
}

internal static class Fmulp
{
    [Opcode("DEC8", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void MultiplyToST0(VirtualMachine vm)
    {
        throw new InvalidOperationException();
    }

    [Opcode("DEC9", Name = "fmulp st1", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void MultplyToST1(Processor p)
    {
        Fmul.MultplyToST1(p);
        p.FPU.Pop();
    }

    [Opcode("DECA", Name = "fmulp st2", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void MultplyToST2(Processor p)
    {
        Fmul.MultplyToST2(p);
        p.FPU.Pop();
    }

    [Opcode("DECB", Name = "fmulp st3", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void MultplyToST3(Processor p)
    {
        Fmul.MultplyToST3(p);
        p.FPU.Pop();
    }

    [Opcode("DECC", Name = "fmulp st4", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void MultplyToST4(Processor p)
    {
        Fmul.MultplyToST4(p);
        p.FPU.Pop();
    }

    [Opcode("DECD", Name = "fmulp st5", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void MultplyToST5(Processor p)
    {
        Fmul.MultplyToST5(p);
        p.FPU.Pop();
    }

    [Opcode("DECE", Name = "fmulp st6", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void MultplyToST6(Processor p)
    {
        Fmul.MultplyToST6(p);
        p.FPU.Pop();
    }

    [Opcode("DECF", Name = "fmulp st7", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void MultplyToST7(Processor p)
    {
        Fmul.MultplyToST7(p);
        p.FPU.Pop();
    }
}
