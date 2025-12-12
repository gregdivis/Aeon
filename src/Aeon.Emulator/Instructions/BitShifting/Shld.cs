using System.Runtime.CompilerServices;

namespace Aeon.Emulator.Instructions.BitShifting;

internal static class Shld
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Opcode("0FA4/r rmw,rw,ib|0FA5/r rmw,rw,cl", AddressSize = 16 | 32)]
    public static void Shld16(Processor p, ref ushort dest, ushort src, byte count)
    {
        int actualCount = count & 0x1F;
        if (actualCount == 0)
            return;

        uint full = ((uint)dest << 16) | src;
        uint shifted = full << actualCount;
        ushort result = (ushort)full;

        p.Flags.Carry = (shifted & 0x80000000u) != 0;
        if (actualCount == 1)
            p.Flags.Overflow = ((result ^ dest) & 0x8000) != 0;

        p.Flags.Update_Value_Word(result);
        dest = result;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Alternate(nameof(Shld16), AddressSize = 16 | 32)]
    public static void Shld32(Processor p, ref uint dest, uint src, byte count)
    {
        int actualCount = count & 0x1F;
        if (actualCount == 0)
            return;

        ulong full = ((ulong)dest << 32) | src;
        ulong shifted = full << actualCount;
        uint result = (uint)(shifted >>> 32);

        p.Flags.Carry = (shifted & 0x8000000000000000UL) != 0;

        if (actualCount == 1)
            p.Flags.Overflow = ((result ^ dest) & 0x80000000u) != 0;

        p.Flags.Update_Value_DWord(result);
        dest = result;
    }
}
