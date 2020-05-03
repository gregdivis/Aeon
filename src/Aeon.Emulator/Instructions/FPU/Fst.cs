using System.Runtime.CompilerServices;

namespace Aeon.Emulator.Instructions.FPU
{
    internal static class Fst
    {
        [Opcode("D9/2 mf32", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public static void StoreReal32(Processor p, out float value)
        {
            value = (float)p.FPU.ST0_Ref;
        }

        [Opcode("DD/2 mf64", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public static void StoreReal64(Processor p, out double value)
        {
            value = p.FPU.ST0_Ref;
        }

        [Opcode("DDD0", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public static void StoreReal64_0(VirtualMachine vm)
        {
        }

        [Opcode("DDD1", Name = "fst st1", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public static void StoreReal64_1(VirtualMachine vm)
        {
            vm.Processor.FPU.SetRegisterValue(1, vm.Processor.FPU.ST0_Ref);
        }

        [Opcode("DDD2", Name = "fst st2", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public static void StoreReal64_2(VirtualMachine vm)
        {
            vm.Processor.FPU.SetRegisterValue(2, vm.Processor.FPU.ST0_Ref);
        }

        [Opcode("DDD3", Name = "fst st3", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public static void StoreReal64_3(VirtualMachine vm)
        {
            vm.Processor.FPU.SetRegisterValue(3, vm.Processor.FPU.ST0_Ref);
        }

        [Opcode("DDD4", Name = "fst st5", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public static void StoreReal64_4(VirtualMachine vm)
        {
            vm.Processor.FPU.SetRegisterValue(4, vm.Processor.FPU.ST0_Ref);
        }

        [Opcode("DDD5", Name = "fst st5", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public static void StoreReal64_5(VirtualMachine vm)
        {
            vm.Processor.FPU.SetRegisterValue(5, vm.Processor.FPU.ST0_Ref);
        }

        [Opcode("DDD6", Name = "fst st6", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public static void StoreReal64_6(VirtualMachine vm)
        {
            vm.Processor.FPU.SetRegisterValue(6, vm.Processor.FPU.ST0_Ref);
        }

        [Opcode("DDD7", Name = "fst st7", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public static void StoreReal64_7(VirtualMachine vm)
        {
            vm.Processor.FPU.SetRegisterValue(7, vm.Processor.FPU.ST0_Ref);
        }
    }

    internal static class Fstp
    {
        [Opcode("D9/3 mf32", Name = "fstp", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public static void StoreRealPop32(Processor p, out float value)
        {
            Fst.StoreReal32(p, out value);
            p.FPU.Pop();
        }

        [Opcode("DB/7 mf80", Name = "fstp", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public static void StoreRealPop80(Processor p, out Real10 value)
        {
            value = p.FPU.ST0_Ref;
            p.FPU.Pop();
        }

        [Opcode("DD/3 mf64", Name = "fstp", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public static void StoreRealPop64(Processor p, out double value)
        {
            Fst.StoreReal64(p, out value);
            p.FPU.Pop();
        }

        [Opcode("DDD8", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public static void StoreReal64_0(VirtualMachine vm)
        {
            vm.Processor.FPU.Pop();
        }

        [Opcode("DDD9", Name = "fstp st1", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public static void StoreRealPop64_1(VirtualMachine vm)
        {
            Fst.StoreReal64_1(vm);
            vm.Processor.FPU.Pop();
        }

        [Opcode("DDDA", Name = "fstp st2", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public static void StoreRealPop64_2(VirtualMachine vm)
        {
            Fst.StoreReal64_2(vm);
            vm.Processor.FPU.Pop();
        }

        [Opcode("DDDB", Name = "fstp st3", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void StoreRealPop64_3(VirtualMachine vm)
        {
            Fst.StoreReal64_3(vm);
            vm.Processor.FPU.Pop();
        }

        [Opcode("DDDC", Name = "fstp st5", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public static void StoreRealPop64_4(VirtualMachine vm)
        {
            Fst.StoreReal64_4(vm);
            vm.Processor.FPU.Pop();
        }

        [Opcode("DDDD", Name = "fstp st5", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public static void StoreRealPop64_5(VirtualMachine vm)
        {
            Fst.StoreReal64_5(vm);
            vm.Processor.FPU.Pop();
        }

        [Opcode("DDDE", Name = "fst st6", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public static void StoreRealPop64_6(VirtualMachine vm)
        {
            Fst.StoreReal64_6(vm);
            vm.Processor.FPU.Pop();
        }

        [Opcode("DDDF", Name = "fstp st7", OperandSize = 16 | 32, AddressSize = 16 | 32)]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        public static void StoreRealPop64_7(VirtualMachine vm)
        {
            Fst.StoreReal64_7(vm);
            vm.Processor.FPU.Pop();
        }
    }
}
