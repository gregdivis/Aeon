using System.Runtime.CompilerServices;

namespace Aeon.Emulator.Instructions;

internal static class Conversions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Opcode("98", Name = "cbw", AddressSize = 16 | 32)]
    public static void ByteToWord(Processor p)
    {
        p.AX = (sbyte)p.AL;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Alternate(nameof(ByteToWord), AddressSize = 16 | 32)]
    public static void ByteToWord32(Processor p)
    {
        p.EAX = p.AX;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Opcode("99", Name = "cwd", AddressSize = 16 | 32)]
    public static void WordToDword(Processor p)
    {
        int result = p.AX;
        p.DX = (short)((result >> 16) & 0xFFFF);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Alternate(nameof(WordToDword), AddressSize = 16 | 32)]
    public static void WordToDword32(Processor p)
    {
        long result = p.EAX;
        p.EDX = (int)((result >> 32) & 0xFFFFFFFF);
    }
}
