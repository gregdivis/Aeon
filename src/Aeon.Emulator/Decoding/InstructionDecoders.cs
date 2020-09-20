using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void GetModRm(ref byte* ip, out byte mod, out byte rm)
        {
            rm = (byte)(*ip & 0x07u);
            if (Bmi1.IsSupported)
                mod = (byte)Bmi1.BitFieldExtract(*ip, 0x0206);
            else
                mod = (byte)((*ip & 0xC0u) >> 6);
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe RmwValue<T> GetRegRmw32<T>(ref byte* ip, Processor p, int mod, int rm, bool offsetOnly, bool memoryOnly)
            where T : unmanaged
        {
            if (mod != 3)
                return new RmwValue<T>(GetModRMAddress32(ref ip, p, mod, rm, offsetOnly));
            else
                return !memoryOnly ? new RmwValue<T>(p.GetRegisterWordPointer(rm)) : throw new Mod3Exception();

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe uint GetModRMAddress32(ref byte* ip, Processor p, int mod, int rm, bool offsetOnly)
        {
            uint baseAddress;

            if (rm == 4)
            {
                byte sib = ReadImmediate<byte>(ref ip);

                if (mod == 0)
                {
                    int scale = (sib >> 6) & 0x3;
                    int index = (sib >> 3) & 0x7;
                    int baseIndex = sib & 0x7;

                    baseAddress = baseIndex == 5 ? ReadImmediate<uint>(ref ip) : *(uint*)p.GetRegisterWordPointer(baseIndex);

                    if (index != 4)
                        baseAddress += (*(uint*)p.GetRegisterWordPointer(index)) << scale;

                    if (!offsetOnly)
                    {
                        uint* basePtr = p.baseOverrides[(int)p.SegmentOverride];
                        if (basePtr == null)
                            basePtr = p.defaultSibSegments32Mod0[baseIndex];

                        baseAddress += *basePtr;
                    }
                }
                else
                {
                    baseAddress = mod == 1 ? (uint)ReadImmediate<sbyte>(ref ip) : ReadImmediate<uint>(ref ip);

                    int index = (sib >> 3) & 0x7;
                    int baseIndex = sib & 0x7;

                    if (index != 4)
                    {
                        int scale = (sib >> 6) & 0x3;
                        baseAddress += (*(uint*)p.GetRegisterWordPointer(index)) << scale;
                    }

                    baseAddress += *(uint*)p.GetRegisterWordPointer(baseIndex);

                    if (!offsetOnly)
                    {
                        uint* basePtr = p.baseOverrides[(int)p.SegmentOverride];
                        if (basePtr == null)
                            basePtr = p.defaultSibSegments32Mod12[baseIndex];

                        baseAddress += *basePtr;
                    }
                }
            }
            else
            {
                if (mod == 0)
                {
                    baseAddress = rm != 5 ? *(uint*)p.GetRegisterWordPointer(rm) : ReadImmediate<uint>(ref ip);
                }
                else
                {
                    baseAddress = mod == 1 ? (uint)ReadImmediate<sbyte>(ref ip) : ReadImmediate<uint>(ref ip);
                    baseAddress += *(uint*)p.GetRegisterWordPointer(rm);
                }
            }

            return baseAddress;
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
