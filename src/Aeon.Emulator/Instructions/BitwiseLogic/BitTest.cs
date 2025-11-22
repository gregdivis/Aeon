namespace Aeon.Emulator.Instructions.BitwiseLogic;

internal static class BT
{
    [Opcode("0FA3 rmw,rw|0FBA/4 rmw,ib", AddressSize = 16 | 32)]
    public static void BitTest16(Processor p, ushort value, byte bit)
    {
        p.Flags.Carry = (value & (1 << bit)) != 0;
    }

    [Alternate("BitTest16", AddressSize = 16 | 32)]
    public static void BitTest32(Processor p, uint value, byte bit)
    {
        p.Flags.Carry = (value & (1 << bit)) != 0;
    }
}

internal static class BTC
{
    [Opcode("0FBB rmw,rw|0FBA/7 rmw,ib", AddressSize = 16 | 32)]
    public static void BitTestComplement16(Processor p, ref ushort value, byte bit)
    {
        uint mask = 1u << bit;
        p.Flags.Carry = (value & mask) != 0;

        value ^= (ushort)mask;
    }

    [Alternate(nameof(BitTestComplement16), AddressSize = 16 | 32)]
    public static void BitTestComplement32(Processor p, ref uint value, byte bit)
    {
        uint mask = 1u << bit;
        p.Flags.Carry = (value & mask) != 0;

        value ^= mask;
    }
}

internal static class BTR
{
    [Opcode("0FB3 rmw,rw|0FBA/6 rmw,ib", AddressSize = 16 | 32)]
    public static void BitTestReset16(Processor p, ref ushort value, byte bit)
    {
        uint mask = 1u << bit;
        p.Flags.Carry = (value & mask) != 0;

        value &= (ushort)~mask;
    }

    [Alternate(nameof(BitTestReset16), AddressSize = 16 | 32)]
    public static void BitTestReset32(Processor p, ref uint value, byte bit)
    {
        uint mask = 1u << bit;
        p.Flags.Carry = (value & mask) != 0;

        value &= ~mask;
    }
}

internal static class BTS
{
    [Opcode("0FAB rmw,rw|0FBA/5 rmw,ib", AddressSize = 16 | 32)]
    public static void BitSet16(Processor p, ref ushort value, byte bit)
    {
        uint mask = 1u << bit;
        p.Flags.Carry = (value & mask) != 0;

        value |= (ushort)mask;
    }
    [Alternate(nameof(BitSet16), AddressSize = 16 | 32)]
    public static void BitSet32(Processor p, ref uint value, byte bit)
    {
        uint mask = 1u << bit;
        p.Flags.Carry = (value & mask) != 0;

        value |= mask;
    }
}
