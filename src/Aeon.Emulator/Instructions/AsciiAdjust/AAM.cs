namespace Aeon.Emulator.Instructions.AsciiAdjust
{
    internal static class AAM
    {
        [Opcode("D4 ib", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        public static void AdjustAfterMultiply(Processor p, byte value)
        {
            uint al = p.AL;
            p.AH = (byte)(al / value);
            p.AL = (byte)(al % value);
        }
    }
}
