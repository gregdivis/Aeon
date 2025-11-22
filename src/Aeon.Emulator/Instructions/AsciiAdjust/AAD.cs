namespace Aeon.Emulator.Instructions.AsciiAdjust;

internal static class AAD
{
    [Opcode("D5 ib", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void AdjustBeforeDivision(Processor p, byte value)
    {
        p.AL += (byte)(p.AH * value);
        p.AH = 0;
        p.Flags.Update_Value_Byte(p.AL);
    }
}
