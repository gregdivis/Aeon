using System.Runtime.CompilerServices;

namespace Aeon.Emulator.Instructions;

internal static class In
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Opcode("E4 al,ib", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void InByte(VirtualMachine vm, out byte value, byte port)
    {
        value = vm.ReadPortByte(port);
    }
    [Opcode("E5 ax,ib", AddressSize = 16 | 32)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void InWord(VirtualMachine vm, out ushort value, byte port)
    {
        value = vm.ReadPortWord(port);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Opcode("EC al,dx", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void InByte2(VirtualMachine vm, out byte value, ushort port)
    {
        value = vm.ReadPortByte(port);
    }
    [Opcode("ED ax,dx", AddressSize = 16 | 32)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void InWord2(VirtualMachine vm, out ushort value, ushort port)
    {
        value = vm.ReadPortWord(port);
    }
}

internal static class Out
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Opcode("E6 ib,al", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void OutByte(VirtualMachine vm, byte port, byte value)
    {
        vm.WritePortByte(port, value);
    }
    [Opcode("E7 ib,ax", AddressSize = 16 | 32)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void OutWord(VirtualMachine vm, byte port, ushort value)
    {
        vm.WritePortWord(port, value);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Opcode("EE dx,al", OperandSize = 16 | 32, AddressSize = 16 | 32)]
    public static void OutByte2(VirtualMachine vm, ushort port, byte value)
    {
        vm.WritePortByte(port, value);
    }
    [Opcode("EF dx,ax", AddressSize = 16 | 32)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void OutWord2(VirtualMachine vm, ushort port, ushort value)
    {
        vm.WritePortWord(port, value);
    }
}
