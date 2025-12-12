using System.Runtime.CompilerServices;

namespace Aeon.Emulator.Instructions.FPU;

internal static class Fbld
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Opcode("DF/4 mf80", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void StoreBCD(VirtualMachine vm, Real10 src)
    {
        long value = 0;

        unsafe
        {
            byte* bcd = (byte*)&src;

            long power = 1;

            for (int i = 0; i < 9; i++)
            {
                int b = bcd[i];
                value += (b & 0xF) * power;

                power *= 10;
                value += (b >> 4) * power;

                power *= 10;
            }

            if (bcd[9] == 0x80)
                value *= -1;
        }

        vm.Processor.FPU.Push(value);
    }
}
