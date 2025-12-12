using System.Runtime.CompilerServices;

namespace Aeon.Emulator.Instructions.AsciiAdjust;

internal static class AAA
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Opcode("37", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void AdjustAfterAddition(Processor p)
    {
        if (((p.AL & 0xFu) > 9u) || p.Flags.Auxiliary)
        {
            p.AX += 0x106;
            p.Flags.Auxiliary = true;
            p.Flags.Carry = true;
        }
        else
        {
            p.Flags.Auxiliary = false;
            p.Flags.Carry = false;
        }

        p.AL &= 0xF;
    }
}
