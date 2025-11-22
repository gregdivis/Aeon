namespace Aeon.Emulator.Instructions.Arithmetic;

internal static class Mul
{
    [Opcode("F6/4 rmb", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void ByteMultiply(Processor p, byte multiplicand)
    {
        p.AX = (short)((uint)p.AL * multiplicand);
        p.Flags.Update_Mul(p.AH);
    }

    [Opcode("F7/4 rmw", AddressSize = 16 | 32)]
    public static void WordMultiply(Processor p, ushort multiplicand)
    {
        ref var ax = ref p.AX;
        ref var dx = ref p.DX;

        uint fullResult = (ushort)ax * (uint)multiplicand;
        unsafe
        {
            short* parts = (short*)&fullResult;
            ax = parts[0];
            dx = parts[1];
        }

        p.Flags.Update_Mul((ushort)dx);
    }
    [Alternate(nameof(WordMultiply), AddressSize = 16 | 32)]
    public static void DWordMultiply(Processor p, uint multiplicand)
    {
        ref var eax = ref p.EAX;
        ref var edx = ref p.EDX;

        ulong fullResult = (ulong)(uint)eax * multiplicand;
        unsafe
        {
            int* parts = (int*)&fullResult;
            eax = parts[0];
            edx = parts[1];
        }

        p.Flags.Update_Mul((uint)edx);
    }
}
