using System.Numerics;
using System.Runtime.CompilerServices;

namespace Aeon.Emulator.Instructions.BitwiseLogic;

internal static class BT
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Opcode("0FA3 rmw,rw|0FBA/4 rmw,ib", AddressSize = 16 | 32, OperandSize = 16 | 32)]
    public static void BitTest<TValue>(Processor p, TValue value, byte bit) where TValue : unmanaged, IBinaryInteger<TValue>
    {
        int mod = Unsafe.SizeOf<TValue>() * 8;
        p.Flags.Carry = (uint.CreateTruncating(value) & (1u << (bit % mod))) != 0;
    }
}

internal static class BTC
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Opcode("0FBB rmw,rw|0FBA/7 rmw,ib", AddressSize = 16 | 32, OperandSize = 16 | 32)]
    public static void BitTestComplement<TValue>(Processor p, ref TValue value, byte bit) where TValue : unmanaged, IBinaryInteger<TValue>
    {
        int mod = Unsafe.SizeOf<TValue>() * 8;
        uint mask = 1u << (bit % mod);
        p.Flags.Carry = (uint.CreateTruncating(value) & mask) != 0;
        value ^= TValue.CreateTruncating(mask);
    }
}

internal static class BTR
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Opcode("0FB3 rmw,rw|0FBA/6 rmw,ib", AddressSize = 16 | 32, OperandSize = 16 | 32)]
    public static void BitTestReset<TValue>(Processor p, ref TValue value, byte bit) where TValue : unmanaged, IBinaryInteger<TValue>
    {
        int mod = Unsafe.SizeOf<TValue>() * 8;
        uint mask = 1u << (bit % mod);
        p.Flags.Carry = (uint.CreateTruncating(value) & mask) != 0;
        value &= TValue.CreateTruncating(~mask);
    }
}

internal static class BTS
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Opcode("0FAB rmw,rw|0FBA/5 rmw,ib", AddressSize = 16 | 32, OperandSize = 16 | 32)]
    public static void BitSet<TValue>(Processor p, ref TValue value, byte bit) where TValue : unmanaged, IBinaryInteger<TValue>
    {
        int mod = Unsafe.SizeOf<TValue>() * 8;
        uint mask = 1u << (bit % mod);
        p.Flags.Carry = (uint.CreateTruncating(value) & mask) != 0;
        value |= TValue.CreateTruncating(mask);
    }
}
