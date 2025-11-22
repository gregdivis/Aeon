using System.Numerics;

namespace Aeon.Emulator.Instructions.BitwiseLogic;

internal static class Bsr
{
    [Opcode("0FBD/r rw,rmw", OperandSize = 16, AddressSize = 16 | 32)]
    public static void BitScanReverse16(Processor p, ref ushort index, ushort value)
    {
        int tzcnt = BitOperations.LeadingZeroCount(value);
        if (tzcnt < 16)
        {
            index = (ushort)tzcnt;
            p.Flags.Zero = false;
        }
        else
        {
            index = 0;
            p.Flags.Zero = true;
        }
    }

    [Alternate(nameof(BitScanReverse16), OperandSize = 32, AddressSize = 16 | 32)]
    public static void BitScanReverse32(Processor p, ref uint index, uint value)
    {
        int tzcnt = BitOperations.LeadingZeroCount(value);
        if (tzcnt < 32)
        {
            index = (uint)tzcnt;
            p.Flags.Zero = false;
        }
        else
        {
            index = 0;
            p.Flags.Zero = true;
        }
    }
}
