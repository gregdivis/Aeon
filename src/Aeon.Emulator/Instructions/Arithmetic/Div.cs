namespace Aeon.Emulator.Instructions.Arithmetic;

internal static class Div
{
    [Opcode("F6/6 rmb", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void ByteDivide(Processor p, byte divisor)
    {
        if (divisor != 0)
        {
            var (quotient, remainder) = Math.DivRem((ushort)p.AX, divisor);
            p.AL = (byte)quotient;
            p.AH = (byte)remainder;
        }
        else
        {
            ThrowHelper.ThrowEmulatedDivideByZeroException();
        }
    }

    [Opcode("F7/6 rmw", AddressSize = 16 | 32)]
    public static void WordDivide(Processor p, ushort divisor)
    {
        if (divisor != 0)
        {
            uint fullValue;
            ref var ax = ref p.AX;
            ref var dx = ref p.DX;
            unsafe
            {
                var parts = (ushort*)&fullValue;
                parts[0] = (ushort)ax;
                parts[1] = (ushort)dx;
            }

            var (quotient, remainder) = Math.DivRem(fullValue, divisor);
            ax = (short)(ushort)quotient;
            dx = (short)(ushort)remainder;
        }
        else
        {
            ThrowHelper.ThrowEmulatedDivideByZeroException();
        }
    }
    [Alternate(nameof(WordDivide), AddressSize = 16 | 32)]
    public static void DWordDivide(Processor p, uint divisor)
    {
        if (divisor != 0)
        {
            ulong fullValue;
            ref var eax = ref p.EAX;
            ref var edx = ref p.EDX;
            unsafe
            {
                var parts = (uint*)&fullValue;
                parts[0] = (uint)eax;
                parts[1] = (uint)edx;
            }

            var (quotient, remainder) = Math.DivRem(fullValue, divisor);
            eax = (int)(uint)quotient;
            edx = (int)remainder;
        }
        else
        {
            ThrowHelper.ThrowEmulatedDivideByZeroException();
        }
    }
}
