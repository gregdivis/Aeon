using System;
using System.Runtime.CompilerServices;
using Aeon.Emulator.RuntimeExceptions;

namespace Aeon.Emulator.Instructions.Arithmetic
{
    internal static class Div
    {
        [Opcode("F6/6 rmb", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        public static void ByteDivide(Processor p, byte divisor)
        {
            if (divisor != 0)
            {
                uint quotient = (ushort)p.AX / (uint)divisor;
                // this is faster than another DIV, which % would use
                uint remainder = (ushort)p.AX - (divisor * quotient);

                p.AL = (byte)quotient;
                p.AH = (byte)remainder;
            }
            else
            {
                throw new EmulatedDivideByZeroException();
            }
        }

        [Opcode("F7/6 rmw", AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
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

                uint quotient = fullValue / divisor;
                // this is faster than another DIV, which % would use
                uint remainder = fullValue - (divisor * quotient);

                ax = (short)(ushort)quotient;
                dx = (short)(ushort)remainder;
            }
            else
            {
                throw new EmulatedDivideByZeroException();
            }
        }
        [Alternate(nameof(WordDivide), AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
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

                ulong quotient = fullValue / divisor;
                // this is faster than another DIV, which % would use
                uint remainder = (uint)(fullValue - (divisor * quotient));

                eax = (int)(uint)quotient;
                edx = (int)remainder;
            }
            else
            {
                throw new EmulatedDivideByZeroException();
            }
        }
    }

    internal static class IDiv
    {
        [Opcode("F6/7 rmb", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
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
                throw new EmulatedDivideByZeroException();
            }
        }

        [Opcode("F7/7 rmw", AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.NoInlining)]
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
                throw new EmulatedDivideByZeroException();
            }
        }
        [Alternate(nameof(WordDivide), AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
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
                throw new EmulatedDivideByZeroException();
            }
        }
    }
}
