namespace Aeon.Emulator.Instructions.BitwiseLogic
{
    internal static class Bsf
    {
        [Opcode("0FBC/r rw,rmw", OperandSize = 16, AddressSize = 16 | 32)]
        public static void BitScanReverse16(Processor p, ref ushort index, ushort value)
        {
            for (int i = 0; i <= 15; i++)
            {
                if ((value & (1 << i)) != 0)
                {
                    index = (ushort)i;
                    p.Flags.Zero = false;
                    return;
                }
            }

            p.Flags.Zero = true;
        }

        [Alternate(nameof(BitScanReverse16), OperandSize = 32, AddressSize = 16 | 32)]
        public static void BitScanReverse32(Processor p, ref uint index, uint value)
        {
            for (int i = 0; i <= 31; i++)
            {
                if ((value & (1 << i)) != 0)
                {
                    index = (uint)i;
                    p.Flags.Zero = false;
                    return;
                }
            }

            p.Flags.Zero = true;
        }
    }
}
