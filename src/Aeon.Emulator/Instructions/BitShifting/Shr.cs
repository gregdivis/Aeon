namespace Aeon.Emulator.Instructions.BitShifting;

internal static class Shr
{
    [Opcode("D0/5 rmb", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void ByteShiftRight1(Processor p, ref byte dest)
    {
        byte value = dest;
        dest >>= 1;
        p.Flags.Update_Shr1_Byte(value, dest);
    }

    [Opcode("D2/5 rmb,cl|C0/5 rmb,ib", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void ByteShiftRight(Processor p, ref byte dest, byte count)
    {
        count &= 0x1F;
        if (count == 0)
        {
            return;
        }
        else if (count == 1)
        {
            ByteShiftRight1(p, ref dest);
            return;
        }

        byte value = dest;
        dest >>>= count;
        p.Flags.Update_Shr_Byte(value, count, dest);
    }

    [Opcode("D1/5 rmw", AddressSize = 16 | 32)]
    public static void WordShiftRight1(Processor p, ref ushort dest)
    {
        ushort value = dest;
        dest >>>= 1;
        p.Flags.Update_Shr1_Word(value, dest);
    }
    [Alternate(nameof(WordShiftRight1), AddressSize = 16 | 32)]
    public static void DWordShiftRight1(Processor p, ref uint dest)
    {
        uint value = dest;
        dest >>>= 1;
        p.Flags.Update_Shr1_DWord(value, dest);
    }

    [Opcode("D3/5 rmw,cl|C1/5 rmw,ib", AddressSize = 16 | 32)]
    public static void WordShiftRight(Processor p, ref ushort dest, byte count)
    {
        count &= 0x1F;
        if (count == 0)
        {
            return;
        }
        else if (count == 1)
        {
            WordShiftRight1(p, ref dest);
            return;
        }

        ushort value = dest;
        dest >>>= count;
        p.Flags.Update_Shr_Word(value, count, dest);
    }
    [Alternate(nameof(WordShiftRight), AddressSize = 16 | 32)]
    public static void DWordShiftRight(Processor p, ref uint dest, byte count)
    {
        count &= 0x1F;
        if (count == 0)
        {
            return;
        }
        else if (count == 1)
        {
            DWordShiftRight1(p, ref dest);
            return;
        }

        uint value = dest;
        dest >>>= count;
        p.Flags.Update_Shr_DWord(value, count, dest);
    }
}
