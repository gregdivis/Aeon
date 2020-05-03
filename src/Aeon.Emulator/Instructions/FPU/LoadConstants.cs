using System;
using System.Runtime.CompilerServices;

namespace Aeon.Emulator.Instructions.FPU
{
    internal static class LoadConstants
    {
        [Opcode("D9EE", Name = "fldz", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void LoadZero(VirtualMachine vm)
        {
            vm.Processor.FPU.Push(0.0);
        }

        [Opcode("D9ED", Name = "fldln2", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void LoadLn2(VirtualMachine vm)
        {
            vm.Processor.FPU.Push(Ln2);
        }

        [Opcode("D9EA", Name = "fldl2e", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void LoadLog2e(VirtualMachine vm)
        {
            vm.Processor.FPU.Push(Log2e);
        }

        [Opcode("D9E8", Name = "fld1", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void LoadOne(VirtualMachine vm)
        {
            vm.Processor.FPU.Push(1.0);
        }

        [Opcode("D9E9", Name = "fldl2t", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void LoadLog210(VirtualMachine vm)
        {
            vm.Processor.FPU.Push(Log210);
        }

        [Opcode("D9EB", Name = "fldpi", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void LoadPi(VirtualMachine vm)
        {
            vm.Processor.FPU.Push(Math.PI);
        }

        [Opcode("D9EC", Name = "fldlg2", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void LoadLog102(VirtualMachine vm)
        {
            vm.Processor.FPU.Push(Log102);
        }

        private static readonly double Ln2 = Math.Log(2.0);
        private static readonly double Log2e = Math.Log(Math.E, 2.0);
        private static readonly double Log210 = Math.Log(10, 2.0);
        private static readonly double Log102 = Math.Log(2.0, 10);
    }
}
