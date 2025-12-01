namespace Aeon.Emulator.Instructions.Arithmetic;

internal static class IDiv
{
    [Opcode("F6/7 rmb", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void ByteDivide(Processor p, sbyte divisor)
    {
        if (divisor != 0)
        {
            int quotient = Math.DivRem(p.AX, divisor, out int remainder);
            p.AL = (byte)quotient;
            p.AH = (byte)remainder;
        }
        else
        {
            ThrowHelper.ThrowEmulatedDivideByZeroException();
        }
    }

    [Opcode("F7/7 rmw", AddressSize = 16 | 32)]
    public static void WordDivide(Processor p, short divisor)
    {
        if (divisor != 0)
        {
            int fullValue;
            ref var ax = ref p.AX;
            ref var dx = ref p.DX;
            unsafe
            {
                var parts = (short*)&fullValue;
                parts[0] = ax;
                parts[1] = dx;
            }

            int quotient = Math.DivRem(fullValue, divisor, out int remainder);
            ax = (short)quotient;
            dx = (short)remainder;
        }
        else
        {
            ThrowHelper.ThrowEmulatedDivideByZeroException();
        }
    }
    [Alternate(nameof(WordDivide), AddressSize = 16 | 32)]
    public unsafe static void DWordDivide(Processor p, int divisor)
    {
        if (divisor != 0)
        {
            long fullValue;
            ref var eax = ref p.EAX;
            ref var edx = ref p.EDX;
            unsafe
            {
                var parts = (int*)&fullValue;
                parts[0] = eax;
                parts[1] = edx;
            }

            long quotient = Math.DivRem(fullValue, divisor, out long remainder);
            eax = (int)quotient;
            edx = (int)remainder;
        }
        else
        {
            ThrowHelper.ThrowEmulatedDivideByZeroException();
        }
    }
}
