using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Aeon.Emulator.Decoding
{
    internal static partial class InstructionDecoders
    {
        private static unsafe byte* ReadAndAdvance(ref byte* ip, int increment)
        {
            byte* a = ip;
            ip += increment;
            return a;
        }
        private static unsafe T ReadImmediate<T>(ref byte* ip)
            where T : unmanaged
        {
            var value = *(T*)ip;
            ip += sizeof(T);
            return value;
        }

        public static unsafe void Move(VirtualMachine vm)
        {
            var p = vm.Processor;
            byte* ip = p.CachedIP;
            int rm = *ip & 0x7;
            int mod = (*ip & 0xC0) >> 6;

            var arg2 = *(ushort*)p.GetRegisterWordPointer((*ip & 0x38) >> 3);

            var arg1 = GetRegRmw16<ushort>(ref ip, p, mod, rm, false, false);
            ushort arg1Temp = default;
            //ref ushort arg1Ref = ref arg1.IsPointer ? ref arg1.RegisterValue : ref arg1Temp;

            //Instructions.Mov.MoveWord(vm, out arg1Ref, arg2);
            Instructions.Mov.MoveWord(vm, out *(arg1.IsPointer ? arg1.RegisterPointer : &arg1Temp), arg2);

            if (!arg1.IsPointer)
                vm.PhysicalMemory.SetUInt16(arg1.Address, arg1Temp);

            vm.Processor.InstructionEpilog();
        }

        public static unsafe void Add(VirtualMachine vm)
        {
            var p = vm.Processor;
            byte* ip = p.CachedIP;
            int rm = *ip & 0x7;
            int mod = (*ip & 0xC0) >> 6;

            var arg2 = *(ushort*)p.GetRegisterWordPointer((*ip & 0x38) >> 3);

            var arg1 = GetRegRmw16<ushort>(ref ip, p, mod, rm, false, false);
            if (arg1.IsPointer)
            {
                Instructions.Arithmetic.Add.WordAdd(p, ref arg1.RegisterValue, arg2);
            }
            else
            {
                var temp = vm.PhysicalMemory.GetUInt16(arg1.Address);
                Instructions.Arithmetic.Add.WordAdd(p, ref temp, arg2);
                vm.PhysicalMemory.SetUInt16(arg1.Address, temp);
            }

            vm.Processor.InstructionEpilog();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe RmwValue<T> GetRegRmw16<T>(ref byte* ip, Processor p, int mod, int rm, bool offsetOnly, bool memoryOnly)
            where T : unmanaged
        {
            if (mod != 3)
                return new RmwValue<T>(GenerateAddress16(ref ip, p, mod, rm, offsetOnly));
            else
                return !memoryOnly ? new RmwValue<T>(p.GetRegisterWordPointer(rm)) : throw new Mod3Exception();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe uint GenerateAddress16(ref byte* ip, Processor p, int mod, int rm, bool offsetOnly)
        {
            uint displacement = 0;

            if (mod == 0)
            {
                if (rm != 6)
                {
                    displacement = 0;
                    ip++;
                }
                else
                {
                    displacement = *(ushort*)(ip + 1);
                    ip += 3;
                }
            }
            else if (mod == 1)
            {
                displacement = (uint)*(sbyte*)(ip + 1);
                ip += 2;
            }
            else if (mod == 2)
            {
                displacement = (uint)*(short*)(ip + 1);
                ip += 3;
            }

            if (!offsetOnly)
            {
                uint* baseOverride = p.baseOverrides[(int)p.SegmentOverride];
                uint baseAddress = baseOverride != null ? *baseOverride : p.segmentBases[(int)SegmentIndex.DS];
                return baseAddress + p.GetRM16Offset(rm, (ushort)displacement);
            }
            else
            {
                return p.GetRM16Offset(rm, (ushort)displacement);
            }
        }

        private readonly ref struct RmwValue<T>
            where T : unmanaged
        {
            private readonly UIntPtr ptr;

            public unsafe RmwValue(void* regPtr)
            {
                this.ptr = new UIntPtr(regPtr);
                this.IsPointer = true;
            }
            public RmwValue(uint address)
            {
                this.ptr = new UIntPtr(address);
                this.IsPointer = false;
            }

            public bool IsPointer { get; }
            public ref T RegisterValue
            {
                get
                {
                    unsafe
                    {
                        return ref *(T*)this.ptr.ToPointer();
                    }
                }
            }
            public unsafe T* RegisterPointer
            {
                get
                {
                    return (T*)this.ptr.ToPointer();
                }
            }
            public uint Address => this.ptr.ToUInt32();
        }
    }
}
