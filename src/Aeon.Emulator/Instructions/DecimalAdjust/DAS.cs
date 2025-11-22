namespace Aeon.Emulator.Instructions.DecimalAdjust;

internal static class DAS
{
    [Opcode("2F", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void DecimalAdjustAfterSubtraction(Processor p)
    {
        uint old_al = p.AL;
        bool old_cf = p.Flags.Carry;

        p.Flags.Carry = false;

        if (((old_al & 0x0Fu) > 9) || p.Flags.Auxiliary)
        {
            uint al = old_al - 6u;
            p.AL = (byte)al;
            p.Flags.Carry = old_cf || (al & 0x10u) != 0;
            p.Flags.Auxiliary = true;
        }
        else
        {
            p.Flags.Auxiliary = false;
        }

        if ((old_al > 0x99u) | old_cf)
        {
            p.AL -= (byte)0x60u;
            p.Flags.Carry = true;
        }
        else
        {
            p.Flags.Carry = false;
        }

        p.Flags.Update_Value_Byte(p.AL);
    }
}
