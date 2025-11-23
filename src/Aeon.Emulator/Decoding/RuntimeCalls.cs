using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Aeon.Emulator.Decoding;

internal static class RuntimeCalls
{
    public static unsafe uint GetMoffsAddress16(Processor p)
    {
        var segmentOverride = p.SegmentOverride;
        uint baseAddress = segmentOverride == SegmentRegister.Default ? p.DSBase : *p.baseOverrides[(int)segmentOverride];
        uint offset =  *(ushort*)p.CachedIP;
        p.CachedIP += 2;
        return baseAddress + offset;
    }
    public static unsafe uint GetMoffsAddress32(Processor p)
    {
        var segmentOverride = p.SegmentOverride;
        uint baseAddress = segmentOverride == SegmentRegister.Default ? p.DSBase : *p.baseOverrides[(int)segmentOverride];
        uint offset = *(uint*)p.CachedIP;
        p.CachedIP += 4;
        return baseAddress + offset;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe uint GetModRMAddress16(Processor processor, int mod, int rm, bool offsetOnly)
    {
        ushort displacement;

        switch (mod)
        {
            case 0:
                if (rm == 6)
                {
                    displacement = *(ushort*)processor.CachedIP;
                    processor.CachedIP += 2;
                }
                else
                {
                    displacement = 0;
                }
                break;
            case 1:
                displacement = (ushort)*(sbyte*)processor.CachedIP;
                processor.CachedIP++;
                break;
            case 2:
                displacement = (ushort)*(short*)processor.CachedIP;
                processor.CachedIP += 2;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(mod));
        }

        ushort offset = (ushort)(rm switch
        {
            0 => (ushort)(processor.BX + processor.SI),
            1 => (ushort)(processor.BX + processor.DI),
            2 => (ushort)(processor.BP + processor.SI),
            3 => (ushort)(processor.BP + processor.DI),
            4 => processor.SI,
            5 => processor.DI,
            6 when mod == 0 => 0,
            6 when mod != 0 => processor.BP,
            7 => (ushort)processor.BX,
            _ => throw new ArgumentOutOfRangeException(nameof(rm))
        } + displacement);

        if (!offsetOnly)
        {
            uint baseAddress;
            uint* segmentOverride = processor.baseOverrides[(int)processor.SegmentOverride];
            if (segmentOverride != null)
            {
                baseAddress = *segmentOverride;
            }
            else
            {
                baseAddress = rm switch
                {
                    6 when mod == 0 => processor.DSBase,
                    0 or 1 or 4 or 5 or 7 => processor.DSBase,
                    2 or 3 or 6 => processor.SSBase,
                    _ => throw new ArgumentOutOfRangeException(nameof(rm))
                };
            }

            return baseAddress + offset;
        }
        else
        {
            return offset;
        }
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe uint GetModRMAddress32(Processor processor, int mod, int rm, bool offsetOnly)
    {
        SegmentIndex segment;
        var offset = mod switch
        {
            0 => GetModRMAddress32Mod0(processor, rm, out segment),
            1 => GetModRMAddress32Mod1(processor, rm, out segment),
            2 => GetModRMAddress32Mod2(processor, rm, out segment),
            _ => throw new ArgumentOutOfRangeException(nameof(mod))
        };

        if (offsetOnly)
        {
            return offset;
        }
        else
        {
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
    }
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowException(Exception ex) => throw ex;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe uint GetModRMAddress32Mod0(Processor processor, int rm, out SegmentIndex segment)
    {
        uint address;
        segment = SegmentIndex.DS;

        if (rm == 5)
        {
            address = *(uint*)processor.CachedIP;
            processor.CachedIP += 4;
        }
        else if (rm == 4)
        {
            byte sib = *processor.CachedIP;
            processor.CachedIP++;
            address = GetSibAddress32ModZero(processor, sib, ref segment);
        }
        else
        {
            address = *(uint*)processor.GetRegisterWordPointer(rm);
        }

        return address;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe uint GetModRMAddress32Mod1(Processor processor, int rm, out SegmentIndex segment)
    {
        uint address;
        segment = SegmentIndex.DS;

        if (rm == 4)
        {
            byte sib = *processor.CachedIP;
            processor.CachedIP++;

            int displacement = *(sbyte*)processor.CachedIP;
            processor.CachedIP++;

            address = GetSibAddress32(processor, displacement, sib, ref segment);
        }
        else
        {
            if (rm == 5)
                segment = SegmentIndex.SS;

            int displacement = *(sbyte*)processor.CachedIP;
            processor.CachedIP++;

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
            byte sib = *processor.CachedIP;
            processor.CachedIP++;

            int displacement = *(int*)processor.CachedIP;
            processor.CachedIP += 4;

            address = GetSibAddress32(processor, displacement, sib, ref segment);
        }
        else
        {
            if (rm == 5)
                segment = SegmentIndex.SS;

            int displacement = *(int*)processor.CachedIP;
            processor.CachedIP += 4;

            address = (uint)(*(int*)processor.GetRegisterWordPointer(rm) + displacement);
        }

        return address;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe uint GetSibAddress32(Processor processor, int displacement, byte sib, ref SegmentIndex segment)
    {
        int scale = (int)Intrinsics.ExtractBits(sib, 6, 2, 0b1100_0000);
        int index = (int)Intrinsics.ExtractBits(sib, 3, 3, 0b0011_1000);
        int baseIndex = (int)(sib & 0x7);

        int indexValue = 0;
        if (index != 4)
            indexValue = (*(int*)processor.GetRegisterWordPointer(index)) << scale;

        int baseValue = *(int*)processor.GetRegisterWordPointer(baseIndex);

        if (baseIndex is 4 or 5)
            segment = SegmentIndex.SS;

        uint address = (uint)(baseValue + indexValue + displacement);

        return address;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe uint GetSibAddress32ModZero(Processor processor, byte sib, ref SegmentIndex segment)
    {
        int scale = (int)Intrinsics.ExtractBits(sib, 6, 2, 0b1100_0000);
        int index = (int)Intrinsics.ExtractBits(sib, 3, 3, 0b0011_1000);
        int baseIndex = sib & 0x7;
        int displacement = 0;

        int indexValue = 0;
        if (index != 4)
            indexValue = (*(int*)processor.GetRegisterWordPointer(index)) << scale;

        int baseValue = 0;
        if (baseIndex != 5)
        {
            baseValue = *(int*)processor.GetRegisterWordPointer(baseIndex);
        }
        else
        {
            displacement = *(int*)processor.CachedIP;
            processor.CachedIP += 4;
        }

        if (baseIndex == 4)
            segment = SegmentIndex.SS;

        uint address = (uint)(baseValue + indexValue + displacement);

        return address;
    }
}
