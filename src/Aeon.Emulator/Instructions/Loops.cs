using System.Runtime.CompilerServices;

namespace Aeon.Emulator.Instructions;

internal static class Loop
{
    #region Loop While Not Zero
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Opcode("E2 ib", Name = "loop", OperandSize = 16, AddressSize = 16)]
    public static void LoopWhileNotZero_16_16(Processor p, sbyte offset)
    {
        ref var cx = ref Unsafe.As<short, ushort>(ref p.CX);
        cx--;
        if (cx != 0)
            p.EIP = (ushort)((int)p.IP + offset);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Alternate(nameof(LoopWhileNotZero_16_16), OperandSize = 32, AddressSize = 16)]
    public static void LoopWhileNotZero_32_16(Processor p, sbyte offset)
    {
        ref var cx = ref Unsafe.As<short, ushort>(ref p.CX);
        cx--;
        if (cx != 0)
            p.EIP = (uint)((int)p.EIP + offset);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Alternate(nameof(LoopWhileNotZero_16_16), OperandSize = 16, AddressSize = 32)]
    public static void LoopWhileNotZero_16_32(Processor p, sbyte offset)
    {
        ref var ecx = ref Unsafe.As<int, uint>(ref p.ECX);
        ecx--;
        if (ecx != 0)
            p.EIP = (ushort)((int)p.IP + offset);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Alternate(nameof(LoopWhileNotZero_16_16), OperandSize = 32, AddressSize = 32)]
    public static void LoopWhileNotZero_32_32(Processor p, sbyte offset)
    {
        ref var ecx = ref Unsafe.As<int, uint>(ref p.ECX);
        ecx--;
        if (ecx != 0)
            p.EIP = (uint)((int)p.EIP + offset);
    }
    #endregion

    #region Loop While Equal
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Opcode("E1 ib", Name = "loope", OperandSize = 16, AddressSize = 16)]
    public static void LoopWhileEqual_16_16(Processor p, sbyte offset)
    {
        p.CX--;
        if (p.CX != 0 && p.Flags.Zero)
            p.EIP = (ushort)((int)p.IP + offset);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Alternate(nameof(LoopWhileEqual_16_16), OperandSize = 32, AddressSize = 16)]
    public static void LoopWhileEqual_32_16(Processor p, sbyte offset)
    {
        p.CX--;
        if(p.CX != 0 && p.Flags.Zero)
            p.EIP = (uint)((int)p.EIP + offset);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Alternate(nameof(LoopWhileEqual_16_16), OperandSize = 16, AddressSize = 32)]
    public static void LoopWhileEqual_16_32(Processor p, sbyte offset)
    {
        p.ECX--;
        if(p.ECX != 0 && p.Flags.Zero)
            p.EIP = (ushort)((int)p.IP + offset);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Alternate(nameof(LoopWhileEqual_16_16), OperandSize = 32, AddressSize = 32)]
    public static void LoopWhileEqual_32_32(Processor p, sbyte offset)
    {
        p.ECX--;
        if(p.ECX != 0 && p.Flags.Zero)
            p.EIP = (uint)((int)p.EIP + offset);
    }
    #endregion

    #region Loop While Not Equal
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Opcode("E0 ib", Name = "loopne", OperandSize = 16, AddressSize = 16)]
    public static void LoopWhileNotEqual_16_16(Processor p, sbyte offset)
    {
        ref ushort cx = ref Unsafe.As<short, ushort>(ref p.CX);
        cx--;
        if (cx != 0 && !p.Flags.Zero)
            p.EIP = (ushort)(p.IP + offset);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Alternate(nameof(LoopWhileNotEqual_16_16), OperandSize = 32, AddressSize = 16)]
    public static void LoopWhileNotEqual_32_16(Processor p, sbyte offset)
    {
        ref ushort cx = ref Unsafe.As<short, ushort>(ref p.CX);
        cx--;
        if (cx != 0 && !p.Flags.Zero)
            p.EIP = (uint)((int)p.EIP + offset);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Alternate(nameof(LoopWhileNotEqual_16_16), OperandSize = 16, AddressSize = 32)]
    public static void LoopWhileNotEqual_16_32(Processor p, sbyte offset)
    {
        unsafe
        {
            ref uint ecx = ref Unsafe.As<int, uint>(ref p.ECX);
            ecx--;
            if(ecx != 0 && !p.Flags.Zero)
                p.EIP = (ushort)(p.IP + offset);
        }
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Alternate(nameof(LoopWhileNotEqual_16_16), OperandSize = 32, AddressSize = 32)]
    public static void LoopWhileNotEqual_32_32(Processor p, sbyte offset)
    {
        unsafe
        {
            ref uint ecx = ref Unsafe.As<int, uint>(ref p.ECX);
            ecx--;
            if(ecx != 0 && !p.Flags.Zero)
                p.EIP = (uint)((int)p.EIP + offset);
        }
    }
    #endregion
}
