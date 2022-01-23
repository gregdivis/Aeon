using System;
using System.Runtime.CompilerServices;

namespace Aeon.Emulator.Instructions.FPU
{
    internal static class Fbstp
    {
        [SkipLocalsInit]
        [Opcode("DF/6 mf80", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        public static void StoreBCD(VirtualMachine vm, out Real10 value)
        {
            var fpu = vm.Processor.FPU;
            double st0 = fpu.Round(fpu.ST0);
            long sourceValue = Math.Abs((long)st0);

            unsafe
            {
                byte* buffer = stackalloc byte[10];
                long power = 1;

                for(int i = 0; i < 9; i++)
                {
                    int digit = (int)((sourceValue / power) % 10);
                    buffer[i] = (byte)digit;

                    power *= 10;
                    digit = (int)((sourceValue / power) % 10);
                    buffer[i] |= (byte)(digit << 4);

                    power *= 10;
                }

                if(st0 < 0)
                    buffer[9] = 0x80;
                else
                    buffer[9] = 0;

                value = *(Real10*)buffer;
            }

            fpu.Pop();
        }
    }
}
