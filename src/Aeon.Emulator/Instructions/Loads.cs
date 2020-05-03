using System.Runtime.CompilerServices;

namespace Aeon.Emulator.Instructions
{
    internal static class Lds
    {
        [Opcode("C5/r rw,mptr", AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void LoadDS(VirtualMachine vm, out ushort operand1, uint operand2)
        {
            vm.WriteSegmentRegister(SegmentIndex.DS, (ushort)(operand2 >> 16));
            operand1 = (ushort)operand2;
        }
        [Alternate(nameof(LoadDS), AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void LoadDS32(VirtualMachine vm, out uint operand1, ulong operand2)
        {
            vm.WriteSegmentRegister(SegmentIndex.DS, (ushort)(operand2 >> 32));
            operand1 = (uint)operand2;
        }
    }

    internal static class Les
    {
        [Opcode("C4/r rw,mptr", AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void LoadES(VirtualMachine vm, out ushort operand1, uint operand2)
        {
            vm.WriteSegmentRegister(SegmentIndex.ES, (ushort)(operand2 >> 16));
            operand1 = (ushort)operand2;
        }
        [Alternate(nameof(LoadES), AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void LoadES32(VirtualMachine vm, out uint operand1, ulong operand2)
        {
            vm.WriteSegmentRegister(SegmentIndex.ES, (ushort)(operand2 >> 32));
            operand1 = (uint)operand2;
        }
    }

    internal static class Lss
    {
        [Opcode("0FB2/r rw,mptr", AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void LoadSS(VirtualMachine vm, out ushort operand1, uint operand2)
        {
            vm.WriteSegmentRegister(SegmentIndex.SS, (ushort)(operand2 >> 16));
            operand1 = (ushort)operand2;
        }

        [Alternate(nameof(LoadSS), AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void LoadSS32(VirtualMachine vm, out uint operand1, ulong operand2)
        {
            vm.WriteSegmentRegister(SegmentIndex.SS, (ushort)(operand2 >> 32));
            operand1 = (uint)operand2;
        }
    }

    internal static class Lfs
    {
        [Opcode("0FB4/r rw,mptr", AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void LoadFS(VirtualMachine vm, out ushort operand1, uint operand2)
        {
            vm.WriteSegmentRegister(SegmentIndex.FS, (ushort)(operand2 >> 16));
            operand1 = (ushort)operand2;
        }

        [Alternate(nameof(LoadFS), AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void LoadFS32(VirtualMachine vm, out uint operand1, ulong operand2)
        {
            vm.WriteSegmentRegister(SegmentIndex.FS, (ushort)(operand2 >> 32));
            operand1 = (uint)operand2;
        }
    }

    internal static class Lgs
    {
        [Opcode("0FB5/r rw,mptr", AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void LoadGS(VirtualMachine vm, out ushort operand1, uint operand2)
        {
            vm.WriteSegmentRegister(SegmentIndex.GS, (ushort)(operand2 >> 16));
            operand1 = (ushort)operand2;
        }

        [Alternate(nameof(LoadGS), AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void LoadGS32(VirtualMachine vm, out uint operand1, ulong operand2)
        {
            vm.WriteSegmentRegister(SegmentIndex.GS, (ushort)(operand2 >> 32));
            operand1 = (uint)operand2;
        }
    }

    internal static class Lea
    {
        [Opcode("8D/r rw,addr:rmw", OperandSize = 16, AddressSize = 16)]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void Lea16_16(VirtualMachine vm, out ushort dest, ushort effectiveAddress)
        {
            dest = effectiveAddress;
        }

        [Alternate(nameof(Lea16_16), OperandSize = 32, AddressSize = 16)]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void Lea32_16(VirtualMachine vm, out uint dest, ushort effectiveAddress)
        {
            dest = effectiveAddress;
        }

        [Alternate(nameof(Lea16_16), OperandSize = 16, AddressSize = 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void Lea16_32(VirtualMachine vm, out ushort dest, uint effectiveAddress)
        {
            dest = (ushort)effectiveAddress;
        }

        [Alternate(nameof(Lea16_16), OperandSize = 32, AddressSize = 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void Lea32_32(VirtualMachine vm, out uint dest, uint effectiveAddress)
        {
            dest = effectiveAddress;
        }
    }
}
