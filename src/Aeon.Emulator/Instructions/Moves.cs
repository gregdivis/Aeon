using System;
using System.Runtime.CompilerServices;

namespace Aeon.Emulator.Instructions
{
    internal static class Mov
    {
        [Opcode("88/r rmb,rb|8A/r rb,rmb|A0 al,moffsb|A2 moffsb,al|B0+ rb,ib|C6/0 rmb,ib", AddressSize = 16 | 32, OperandSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MoveByte(VirtualMachine vm, out byte dest, byte src)
        {
            dest = src;
        }

        [Opcode("89/r rmw,rw|8B/r rw,rmw|A1 ax,moffsw|A3 moffsw,ax|B8+ rw,iw|C7/0 rmw,iw", AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MoveWord(VirtualMachine vm, out ushort dest, ushort src)
        {
            dest = src;
        }
        [Alternate("MoveWord", AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MoveDWord(VirtualMachine vm, out uint dest, uint src)
        {
            dest = src;
        }

        [Opcode("8C/r rm16,sreg|8E/r sreg,rm16", AddressSize = 16 | 32, OperandSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MoveSegmentRegister(VirtualMachine vm, out ushort dest, ushort src)
        {
            dest = src;
        }

        [Opcode("0F21/r rm32,dr|0F23/r dr,rm32", AddressSize = 16 | 32, OperandSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MoveDebugRegister(VirtualMachine vm, out uint dest, uint src)
        {
            dest = src;
        }
    }

    internal static class Movzx
    {
        [Opcode("0FB6/r rw,rmb", AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ExtendByte(VirtualMachine vm, out ushort dest, byte src)
        {
            dest = src;
        }
        [Alternate("ExtendByte", AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ExtendByte32(VirtualMachine vm, out uint dest, byte src)
        {
            dest = src;
        }

        [Opcode("0FB7/r rw,rmw", AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ExtendWord(VirtualMachine vm, out ushort dest, ushort src)
        {
            dest = src;
        }
        [Alternate("ExtendWord", AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ExtendWord32(VirtualMachine vm, out uint dest, uint src)
        {
            dest = src & 0xFFFFu;
        }
    }

    internal static class Movsx
    {
        [Opcode("0FBE/r rw,rmb", AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ExtendByte(VirtualMachine vm, out short dest, sbyte src)
        {
            dest = src;
        }
        [Alternate("ExtendByte", AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ExtendByte32(VirtualMachine vm, out int dest, sbyte src)
        {
            dest = src;
        }

        [Opcode("0FBF/r rw,rmw", AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ExtendWord(VirtualMachine vm, out short dest, short src)
        {
            throw new InvalidOperationException();
        }
        [Alternate("ExtendWord", AddressSize = 16 | 32)]
        public static void ExtendWord32(VirtualMachine vm, out int dest, int src)
        {
            dest = (short)(src & 0xFFFF);
        }
    }

    internal static class Xchg
    {
        [Opcode("86/r rmb,rb", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ExchangeBytes(VirtualMachine vm, ref byte value1, ref byte value2)
        {
            byte swap = value1;
            value1 = value2;
            value2 = swap;
        }
        
        [Opcode("90+ ax,rw|87/r rmw,rw", AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ExchangeWords(VirtualMachine vm, ref ushort value1, ref ushort value2)
        {
            ushort swap = value1;
            value1 = value2;
            value2 = swap;
        }
        [Alternate("ExchangeWords", AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ExchangeDWords(VirtualMachine vm, ref uint value1, ref uint value2)
        {
            uint swap = value1;
            value1 = value2;
            value2 = swap;
        }
    }
}
