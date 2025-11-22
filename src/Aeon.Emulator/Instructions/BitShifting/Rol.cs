using System.Numerics;

namespace Aeon.Emulator.Instructions.BitShifting;

internal static class Rol
{
    [Opcode("D0/0 rmb", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void ByteRotateLeft1(Processor p, ref byte dest)
    {
        byte b = (byte)(dest << 1);
        byte c = (byte)(dest >>> 7);
        dest = (byte)(b | c);
        p.Flags.Update_Rol1_Byte(dest);
    }
    [Opcode("D2/0 rmb,cl|C0/0 rmb,ib", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void ByteRotateLeft(Processor p, ref byte dest, byte count)
    {
        count = (byte)((count & 0x1F) % 8);
        if (count == 0)
        {
            return;
        }
        else if (count == 1)
        {
            ByteRotateLeft1(p, ref dest);
            return;
        }

        byte b = (byte)(dest << count);
        byte c = (byte)(dest >>> (8 - count));
        dest = (byte)(b | c);
        p.Flags.Update_Rol_Byte(dest);
    }

    [Opcode("D1/0 rmw", AddressSize = 16 | 32)]
    public static void WordRotateLeft1(Processor p, ref ushort dest)
    {
        ushort b = (ushort)(dest << 1);
        ushort c = (ushort)(dest >>> 15);
        dest = (ushort)(b | c);
        p.Flags.Update_Rol1_Word(dest);
    }
    [Alternate(nameof(WordRotateLeft1), AddressSize = 16 | 32)]
    public static void DWordRotateLeft1(Processor p, ref uint dest)
    {
        uint b = dest << 1;
        uint c = dest >>> 31;
        dest = b | c;
        p.Flags.Update_Rol1_DWord(dest);
    }

    [Opcode("D3/0 rmw,cl|C1/0 rmw,ib", AddressSize = 16 | 32)]
    public static void WordRotateLeft(Processor p, ref ushort dest, byte count)
    {
        count = (byte)((count & 0x1F) % 16);
        if (count == 0)
        {
            return;
        }
        else if (count == 1)
        {
            WordRotateLeft1(p, ref dest);
            return;
        }

        ushort b = (ushort)(dest << count);
        ushort c = (ushort)(dest >>> (16 - count));
        dest = (ushort)(b | c);
        p.Flags.Update_Rol_Word(dest);
    }
    [Alternate(nameof(WordRotateLeft), AddressSize = 16 | 32)]
    public static void DWordRotateLeft(Processor p, ref uint dest, byte count)
    {
        count = (byte)((count & 0x1F) % 32);
        if (count == 0)
        {
            return;
        }
        else if (count == 1)
        {
            DWordRotateLeft1(p, ref dest);
            return;
        }

        dest = BitOperations.RotateLeft(dest, count);
        p.Flags.Update_Rol_DWord(dest);
    }
}
