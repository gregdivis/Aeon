using System;
using System.Runtime.CompilerServices;

namespace Aeon.Emulator.Decoding
{
    internal static class RuntimeCalls
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe uint NewLoadSibMod0Address(Processor processor, int sib)
        {
            uint address;
            int scale = (sib >> 6) & 0x3;
            int index = (sib >> 3) & 0x7;
            int baseIndex = sib & 0x7;

            if (baseIndex == 5)
            {
                address = *(uint*)processor.CachedIP;
                processor.CachedIP += 4;
            }
            else
                address = *(uint*)processor.GetRegisterWordPointer(baseIndex);

            if (index != 4)
                address += (*(uint*)processor.GetRegisterWordPointer(index)) << scale;

            uint* basePtr = processor.baseOverrides[(int)processor.SegmentOverride];
            if (basePtr == null)
                basePtr = processor.defaultSibSegments32Mod0[baseIndex];

            return address + *basePtr;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe uint NewLoadSibMod12Address(Processor processor, int sib, uint displacement)
        {
            uint address = displacement;

            int index = (sib >> 3) & 0x7;
            int baseIndex = sib & 0x7;

            if (index != 4)
            {
                int scale = (sib >> 6) & 0x3;
                address += (*(uint*)processor.GetRegisterWordPointer(index)) << scale;
            }

            address += *(uint*)processor.GetRegisterWordPointer(baseIndex);

            uint* basePtr = processor.baseOverrides[(int)processor.SegmentOverride];
            if (basePtr == null)
                basePtr = processor.defaultSibSegments32Mod12[baseIndex];

            return address + *basePtr;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe uint NewLoadSibMod0Offset(Processor processor, int sib)
        {
            uint address;
            int scale = (sib >> 6) & 0x3;
            int index = (sib >> 3) & 0x7;
            int baseIndex = sib & 0x7;

            if (baseIndex == 5)
            {
                address = *(uint*)processor.CachedIP;
                processor.CachedIP += 4;
            }
            else
                address = *(uint*)processor.GetRegisterWordPointer(baseIndex);

            if (index != 4)
                address += (*(uint*)processor.GetRegisterWordPointer(index)) << scale;

            return address;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe uint NewLoadSibMod12Offset(Processor processor, int sib, uint displacement)
        {
            uint address = displacement;

            int index = (sib >> 3) & 0x7;
            int baseIndex = sib & 0x7;

            if (index != 4)
            {
                int scale = (sib >> 6) & 0x3;
                address += (*(uint*)processor.GetRegisterWordPointer(index)) << scale;
            }

            address += *(uint*)processor.GetRegisterWordPointer(baseIndex);

            return address;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint GetMoffsAddress32(Processor processor, uint displacement)
        {
            var baseAddress = processor.GetOverrideBase(SegmentIndex.DS);
            return baseAddress + displacement;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe uint GetModRMAddress32(Processor processor, int mod, int rm)
        {
            uint offset;
            SegmentIndex segment;

            switch (mod)
            {
                case 0:
                default:
                    offset = GetModRMAddress32Mod0(processor, rm, out segment);
                    break;

                case 1:
                    offset = GetModRMAddress32Mod1(processor, rm, out segment);
                    break;

                case 2:
                    offset = GetModRMAddress32Mod2(processor, rm, out segment);
                    break;
            }

            uint baseAddress;

            unsafe
            {
                uint* segmentPtr = processor.baseOverrides[(int)processor.SegmentOverride];
                if (segmentPtr != null)
                    baseAddress = *segmentPtr;
                else
                    baseAddress = processor.segmentBases[(int)segment];
            }

            return baseAddress + offset;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LoadEffectiveAddress32(VirtualMachine vm)
        {
            unsafe
            {
                //byte* ip = vm.PhysicalMemory.RawView + vm.Processor.CSBase + vm.Processor.EIP;
                byte* ip = vm.Processor.CachedIP;
                int mod = (*ip & 0xC0) >> 6;
                int rm = (*ip & 0x07);
                int reg = (*ip & 0x38) >> 3;

                // Advance past ModR/M byte.
                vm.Processor.CachedIP++;
                //vm.Processor.EIP++;
                //ip++;

                uint* destReg = (uint*)vm.Processor.GetRegisterWordPointer(reg);
                SegmentIndex segment;

                switch (mod)
                {
                    case 0:
                    default:
                        *destReg = GetModRMAddress32Mod0(vm.Processor, rm, out segment);
                        break;

                    case 1:
                        *destReg = GetModRMAddress32Mod1(vm.Processor, rm, out segment);
                        break;

                    case 2:
                        *destReg = GetModRMAddress32Mod2(vm.Processor, rm, out segment);
                        break;
                }

                vm.Processor.EIP = (uint)(vm.Processor.CachedIP - vm.Processor.CachedInstruction) + vm.Processor.StartEIP;

                vm.Processor.InstructionEpilog();
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LoadEffectiveAddress32Operand16(VirtualMachine vm)
        {
            unsafe
            {
                //byte* ip = vm.PhysicalMemory.RawView + vm.Processor.CSBase + vm.Processor.EIP;
                byte* ip = vm.Processor.CachedIP;
                int mod = (*ip & 0xC0) >> 6;
                int rm = (*ip & 0x07);
                int reg = (*ip & 0x38) >> 3;

                // Advance past ModR/M byte.
                //vm.Processor.EIP++;
                vm.Processor.CachedIP++;
                //ip++;

                ushort* destReg = (ushort*)vm.Processor.GetRegisterWordPointer(reg);
                SegmentIndex segment;

                switch (mod)
                {
                    case 0:
                    default:
                        *destReg = (ushort)GetModRMAddress32Mod0(vm.Processor, rm, out segment);
                        break;

                    case 1:
                        *destReg = (ushort)GetModRMAddress32Mod1(vm.Processor, rm, out segment);
                        break;

                    case 2:
                        *destReg = (ushort)GetModRMAddress32Mod2(vm.Processor, rm, out segment);
                        break;
                }

                vm.Processor.EIP = (uint)(vm.Processor.CachedIP - vm.Processor.CachedInstruction) + vm.Processor.StartEIP;

                vm.Processor.InstructionEpilog();
            }
        }
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowException(Exception ex) => throw ex;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe uint GetModRMAddress32Mod0(Processor processor, int rm, out SegmentIndex segment)
        {
            uint address;
            segment = SegmentIndex.DS;

            if (rm == 5)
            {
                //*(int*)processor.PIP += 4;
                address = *(uint*)processor.CachedIP;
                processor.CachedIP += 4;
            }
            else if (rm == 4)
            {
                //*(int*)processor.PIP = *(int*)processor.PIP + 1;

                int sib = *processor.CachedIP;
                processor.CachedIP++;
                address = GetSibAddress32ModZero(processor, sib, ref segment);
            }
            else
                address = *(uint*)processor.GetRegisterWordPointer(rm);

            return address;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe uint GetModRMAddress32Mod1(Processor processor, int rm, out SegmentIndex segment)
        {
            uint address;
            segment = SegmentIndex.DS;

            if (rm == 4)
            {
                int sib = *processor.CachedIP;
                processor.CachedIP++;

                int displacement = *(sbyte*)processor.CachedIP;
                processor.CachedIP++;

                //*(int*)processor.PIP = *(int*)processor.PIP + 2;

                address = (uint)(GetSibAddress32(processor, displacement, sib, ref segment));
            }
            else
            {
                if (rm == 5)
                    segment = SegmentIndex.SS;

                int displacement = *(sbyte*)processor.CachedIP;
                processor.CachedIP++;
                //*(int*)processor.PIP = *(int*)processor.PIP + 1;

                address = (uint)(*(int*)processor.GetRegisterWordPointer(rm) + displacement);
            }

            return address;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe uint GetModRMAddress32Mod2(Processor processor, int rm, out SegmentIndex segment)
        {
            uint address;
            segment = SegmentIndex.DS;

            if (rm == 4)
            {
                int sib = *processor.CachedIP;
                processor.CachedIP++;

                int displacement = *(int*)processor.CachedIP;
                processor.CachedIP += 4;

                //*(int*)processor.PIP = *(int*)processor.PIP + 5;

                address = (uint)(GetSibAddress32(processor, displacement, sib, ref segment));
            }
            else
            {
                if (rm == 5)
                    segment = SegmentIndex.SS;

                int displacement = *(int*)processor.CachedIP;
                processor.CachedIP += 4;
                //*(int*)processor.PIP = *(int*)processor.PIP + 4;

                address = (uint)(*(int*)processor.GetRegisterWordPointer(rm) + displacement);
            }

            return address;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe uint GetSibAddress32(Processor processor, int displacement, int sib, ref SegmentIndex segment)
        {
            int scale = (sib >> 6) & 0x3;
            int index = (sib >> 3) & 0x7;
            int baseIndex = sib & 0x7;

            int indexValue = 0;
            if (index != 4)
                indexValue = (*(int*)processor.GetRegisterWordPointer(index)) << scale;

            int baseValue = *(int*)processor.GetRegisterWordPointer(baseIndex);

            if (baseIndex == 4 || baseIndex == 5)
                segment = SegmentIndex.SS;

            uint address = (uint)(baseValue + indexValue + displacement);

            return address;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe uint GetSibAddress32ModZero(Processor processor, int sib, ref SegmentIndex segment)
        {
            int scale = (sib >> 6) & 0x3;
            int index = (sib >> 3) & 0x7;
            int baseIndex = sib & 0x7;
            int displacement = 0;

            int indexValue = 0;
            if (index != 4)
                indexValue = (*(int*)processor.GetRegisterWordPointer(index)) << scale;

            int baseValue = 0;
            if (baseIndex != 5)
                baseValue = *(int*)processor.GetRegisterWordPointer(baseIndex);
            else
            {
                displacement = *(int*)processor.CachedIP;
                processor.CachedIP += 4;
                //*(int*)processor.PIP = *(int*)processor.PIP + 4;
            }

            if (baseIndex == 4)
                segment = SegmentIndex.SS;

            uint address = (uint)(baseValue + indexValue + displacement);

            return address;
        }
    }
}
