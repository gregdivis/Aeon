using System.Runtime.CompilerServices;

namespace Aeon.Emulator.Instructions.FPU
{
    internal static class Fldenv
    {
        [Opcode("D9/4 m64", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LoadEnvironment(Processor p, ulong state)
        {
            var fpu = p.FPU;
            fpu.TagWord = (ushort)(state >> 32);
            fpu.StatusWord = (ushort)(state >> 16);
            fpu.ControlWord = (ushort)state;
        }
    }
}
