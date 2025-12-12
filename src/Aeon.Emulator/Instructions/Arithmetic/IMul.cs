using System.Runtime.CompilerServices;

namespace Aeon.Emulator.Instructions.Arithmetic;

internal static class IMul
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Opcode("F6/5 rmb", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void ByteMultiply(Processor p, sbyte multiplicand)
    {
        p.AX = (short)((sbyte)p.AL * multiplicand);
        p.Flags.Update_IMul((sbyte)p.AH);
    }
    [Opcode("F7/5 rmw", AddressSize = 16 | 32)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WordMultiply(Processor p, short multiplicand)
    {
        ref var ax = ref p.AX;
        ref var dx = ref p.DX;

        int fullResult = ax * multiplicand;
        unsafe
        {
            short* parts = (short*)&fullResult;
            ax = parts[0];
            dx = parts[1];
        }

        p.Flags.Update_IMul(dx);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Alternate(nameof(WordMultiply), AddressSize = 16 | 32)]
    public static void DWordMultiply(Processor p, int multiplicand)
    {
        ref var eax = ref p.EAX;
        ref var edx = ref p.EDX;

        long fullResult = Math.BigMul(eax, multiplicand);
        unsafe
        {
            int* parts = (int*)&fullResult;
            eax = parts[0];
            edx = parts[1];
        }

        p.Flags.Update_IMul(edx);
    }

    [Opcode("0FAF/r rw,rmw", AddressSize = 16 | 32)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WordMultiply2(Processor p, ref short value1, short value2)
    {
        int temp = value1 * value2;
        p.Flags.Update_IMul23_Word(value1, value2);
        value1 = (short)temp;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Alternate(nameof(WordMultiply2), AddressSize = 16 | 32)]
    public static void DWordMultiply2(Processor p, ref int value1, int value2)
    {
        long temp = Math.BigMul(value1, value2);
        p.Flags.Update_IMul23_DWord(value1, value2);
        value1 = (int)temp;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Opcode("6B/r rw,rmw,ibx|69/r rw,rmw,iw", AddressSize = 16 | 32)]
    public static void WordMultiply3(Processor p, out short result, short multiplicand1, short multiplicand2)
    {
        int temp = multiplicand1 * multiplicand2;
        p.Flags.Update_IMul23_Word(multiplicand1, multiplicand2);
        result = (short)temp;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Alternate(nameof(WordMultiply3), AddressSize = 16 | 32)]
    public static void DWordMultiply3(Processor p, out int result, int multiplicand1, int multiplicand2)
    {
        long temp = Math.BigMul(multiplicand1, multiplicand2);
        p.Flags.Update_IMul23_DWord(multiplicand1, multiplicand2);
        result = (int)temp;
    }
}
