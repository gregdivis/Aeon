using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;

namespace Aeon.Emulator.Instructions.Arithmetic;

#pragma warning disable SYSLIB5004 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

internal static class Div
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Opcode("F6/6 rmb", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void ByteDivide(Processor p, byte divisor)
    {
        if (divisor != 0)
        {
            uint quotient;
            uint remainder;

            if (X86Base.IsSupported)
                (quotient, remainder) = X86Base.DivRem((ushort)p.AX, 0, (uint)divisor);
            else
                (quotient, remainder) = Math.DivRem((ushort)p.AX, divisor);

            p.AL = (byte)quotient;
            p.AH = (byte)remainder;
        }
        else
        {
            ThrowHelper.ThrowEmulatedDivideByZeroException();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Opcode("F7/6 rmw", AddressSize = 16 | 32)]
    public static void WordDivide(Processor p, ushort divisor)
    {
        if (divisor != 0)
        {
            ref var ax = ref p.AX;
            ref var dx = ref p.DX;
            uint quotient;
            uint remainder;

            uint fullValue = ((uint)(ushort)dx << 16) | (ushort)ax;

            if (X86Base.IsSupported)
                (quotient, remainder) = X86Base.DivRem(fullValue, 0, (uint)divisor);
            else
                (quotient, remainder) = Math.DivRem(fullValue, divisor);

            ax = (short)(ushort)quotient;
            dx = (short)(ushort)remainder;
        }
        else
        {
            ThrowHelper.ThrowEmulatedDivideByZeroException();
        }
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Alternate(nameof(WordDivide), AddressSize = 16 | 32)]
    public static void DWordDivide(Processor p, uint divisor)
    {
        if (divisor != 0)
        {
            ref var eax = ref p.EAX;
            ref var edx = ref p.EDX;
            uint quotient;
            uint remainder;

            if (X86Base.IsSupported)
            {
                (quotient, remainder) = X86Base.DivRem((uint)eax, (uint)edx, (uint)divisor);
            }
            else
            {
                var (q, r) = Math.DivRem(((ulong)(uint)edx << 32) | (uint)eax, divisor);
                quotient = (uint)q;
                remainder = (uint)r;
            }

            eax = (int)quotient;
            edx = (int)remainder;
        }
        else
        {
            ThrowHelper.ThrowEmulatedDivideByZeroException();
        }
    }
}
