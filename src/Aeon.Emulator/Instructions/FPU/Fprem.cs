using System.Runtime.CompilerServices;

namespace Aeon.Emulator.Instructions.FPU
{
    internal static class Fprem
    {
        [Opcode("D9F8", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DivideInt16(Processor p)
        {
            p.FPU.ST0_Ref %= p.FPU.GetRegisterRef(1);
        }
    }
}
