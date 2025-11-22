namespace Aeon.Emulator.Instructions.BitwiseLogic;

internal static class Not
{
    [Opcode("F6/2 rmb", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void ByteNot(VirtualMachine vm, ref byte dest) => dest = (byte)~dest;

    [Opcode("F7/2 rmw", AddressSize = 16 | 32)]
    public static void WordNot(VirtualMachine vm, ref ushort dest) => dest = (ushort)~dest;

    [Alternate(nameof(WordNot), AddressSize = 16 | 32)]
    public static void DWordNot(VirtualMachine vm, ref uint dest) => dest = ~dest;
}
