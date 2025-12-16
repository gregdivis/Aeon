using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;

namespace Aeon.Emulator.Instructions.Arithmetic;

#pragma warning disable SYSLIB5004 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

internal static class IDiv
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Alternate(nameof(WordDivide), AddressSize = 16 | 32)]
    public unsafe static void DWordDivide(Processor p, int divisor)
    {
        if (divisor != 0)
        {
            ref var eax = ref p.EAX;
            ref var edx = ref p.EDX;
            int quotient;
            int remainder;

            if (X86Base.IsSupported)
            {
                (quotient, remainder) = X86Base.DivRem((uint)eax, edx, divisor);
            }
            else
            {
                var (q, r) = Math.DivRem(((long)edx << 32) | (uint)eax, divisor);
                quotient = (int)q;
                remainder = (int)r;
            }

            eax = quotient;
            edx = remainder;
        }
        else
        {
            ThrowHelper.ThrowEmulatedDivideByZeroException();
        }
    }
}
