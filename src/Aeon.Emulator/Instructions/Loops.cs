using System.Runtime.CompilerServices;

namespace Aeon.Emulator.Instructions
{
    internal static class Loop
    {
        #region Loop While Not Zero
        [Opcode("E2 ib", Name = "loop", OperandSize = 16, AddressSize = 16)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LoopWhileNotZero_16_16(Processor p, sbyte offset)
        {
            unsafe
            {
                ushort* cx = (ushort*)p.PCX;
                (*cx)--;
                if(*cx != 0)
                    p.EIP = (ushort)((int)p.IP + offset);
            }
        }
        [Alternate(nameof(LoopWhileNotZero_16_16), OperandSize = 32, AddressSize = 16)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LoopWhileNotZero_32_16(Processor p, sbyte offset)
        {
            unsafe
            {
                ushort* cx = (ushort*)p.PCX;
                (*cx)--;
                if(*cx != 0)
                    p.EIP = (uint)((int)p.EIP + offset);
            }
        }
        [Alternate(nameof(LoopWhileNotZero_16_16), OperandSize = 16, AddressSize = 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LoopWhileNotZero_16_32(Processor p, sbyte offset)
        {
            unsafe
            {
                uint* ecx = (uint*)p.PCX;
                (*ecx)--;
                if(*ecx != 0)
                    p.EIP = (ushort)((int)p.IP + offset);
            }
        }
        [Alternate(nameof(LoopWhileNotZero_16_16), OperandSize = 32, AddressSize = 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LoopWhileNotZero_32_32(Processor p, sbyte offset)
        {
            unsafe
            {
                uint* ecx = (uint*)p.PCX;
                (*ecx)--;
                if(*ecx != 0)
                    p.EIP = (uint)((int)p.EIP + offset);
            }
        }
        #endregion

        #region Loop While Equal
        [Opcode("E1 ib", Name = "loope", OperandSize = 16, AddressSize = 16)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LoopWhileEqual_16_16(Processor p, sbyte offset)
        {
            p.CX--;
            if (p.CX != 0 && p.Flags.Zero)
                p.EIP = (ushort)((int)p.IP + offset);
        }
        [Alternate(nameof(LoopWhileEqual_16_16), OperandSize = 32, AddressSize = 16)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LoopWhileEqual_32_16(Processor p, sbyte offset)
        {
            p.CX--;
            if(p.CX != 0 && p.Flags.Zero)
                p.EIP = (uint)((int)p.EIP + offset);
        }
        [Alternate(nameof(LoopWhileEqual_16_16), OperandSize = 16, AddressSize = 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LoopWhileEqual_16_32(Processor p, sbyte offset)
        {
            p.ECX--;
            if(p.ECX != 0 && p.Flags.Zero)
                p.EIP = (ushort)((int)p.IP + offset);
        }
        [Alternate(nameof(LoopWhileEqual_16_16), OperandSize = 32, AddressSize = 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LoopWhileEqual_32_32(Processor p, sbyte offset)
        {
            p.ECX--;
            if(p.ECX != 0 && p.Flags.Zero)
                p.EIP = (uint)((int)p.EIP + offset);
        }
        #endregion

        #region Loop While Not Equal
        [Opcode("E0 ib", Name = "loopne", OperandSize = 16, AddressSize = 16)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LoopWhileNotEqual_16_16(Processor p, sbyte offset)
        {
            unsafe
            {
                ushort* cx = (ushort*)p.PCX;
                (*cx)--;
                if(*cx != 0 && !p.Flags.Zero)
                    p.EIP = (ushort)((int)p.IP + offset);
            }
        }
        [Alternate(nameof(LoopWhileNotEqual_16_16), OperandSize = 32, AddressSize = 16)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LoopWhileNotEqual_32_16(Processor p, sbyte offset)
        {
            unsafe
            {
                ushort* cx = (ushort*)p.PCX;
                (*cx)--;
                if(*cx != 0 && !p.Flags.Zero)
                    p.EIP = (uint)((int)p.EIP + offset);
            }
        }
        [Alternate(nameof(LoopWhileNotEqual_16_16), OperandSize = 16, AddressSize = 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LoopWhileNotEqual_16_32(Processor p, sbyte offset)
        {
            unsafe
            {
                uint* ecx = (uint*)p.PCX;
                (*ecx)--;
                if(*ecx != 0 && !p.Flags.Zero)
                    p.EIP = (ushort)((int)p.IP + offset);
            }
        }
        [Alternate(nameof(LoopWhileNotEqual_16_16), OperandSize = 32, AddressSize = 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LoopWhileNotEqual_32_32(Processor p, sbyte offset)
        {
            unsafe
            {
                uint* ecx = (uint*)p.PCX;
                (*ecx)--;
                if(*ecx != 0 && !p.Flags.Zero)
                    p.EIP = (uint)((int)p.EIP + offset);
            }
        }
        #endregion
    }
}
