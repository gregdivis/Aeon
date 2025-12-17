using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Aeon.Emulator.Decoding;

internal static class RuntimeCalls
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe uint GetMoffsAddress16(Processor p)
    {
        uint baseAddress = p.GetOverrideBase(SegmentIndex.DS);

        uint offset = Unsafe.ReadUnaligned<ushort>(in p.CachedIP);
        p.EIP += 2;

        return baseAddress + offset;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe uint GetMoffsAddress32(Processor p)
    {
        uint baseAddress = p.GetOverrideBase(SegmentIndex.DS);

        uint offset = Unsafe.ReadUnaligned<uint>(in p.CachedIP);
        p.EIP += 4;

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
                    displacement = Unsafe.ReadUnaligned<ushort>(in processor.CachedIP);
                    processor.EIP += 2;
                }
                else
                {
                    displacement = 0;
                }
                break;
            case 1:
                displacement = (ushort)Unsafe.ReadUnaligned<sbyte>(in processor.CachedIP);
                processor.EIP++;
                break;
            case 2:
                displacement = (ushort)Unsafe.ReadUnaligned<short>(in processor.CachedIP);
                processor.EIP += 2;
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

            baseAddress = processor.GetOverrideBase(
                rm switch
                {
                    6 when mod == 0 => SegmentIndex.DS,
                    0 or 1 or 4 or 5 or 7 => SegmentIndex.DS,
                    2 or 3 or 6 => SegmentIndex.SS,
                    _ => throw new ArgumentOutOfRangeException(nameof(rm))
                }
            );

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
            uint baseAddress = processor.GetOverrideBase(segment);
            return baseAddress + offset;
        }
    }
    [DoesNotReturn]
    public static void ThrowException(Exception ex) => throw ex;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe uint GetModRMAddress32Mod0(Processor processor, int rm, out SegmentIndex segment)
    {
        uint address;
        segment = SegmentIndex.DS;

        if (rm == 5)
        {
            address = Unsafe.ReadUnaligned<uint>(in processor.CachedIP);
            processor.EIP += 4;
        }
        else if (rm == 4)
        {
            byte sib = processor.CachedIP;
            processor.EIP++;
            address = GetSibAddress32ModZero(processor, sib, ref segment);
        }
        else
        {
            address = processor.GetWordRegister<uint>(rm);
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
            byte sib = processor.CachedIP;
            processor.EIP++;

            int displacement = Unsafe.ReadUnaligned<sbyte>(in processor.CachedIP);
            processor.EIP++;

            address = GetSibAddress32(processor, displacement, sib, ref segment);
        }
        else
        {
            if (rm == 5)
                segment = SegmentIndex.SS;

            int displacement = Unsafe.ReadUnaligned<sbyte>(in processor.CachedIP);
            processor.EIP++;

            address = (uint)(processor.GetWordRegister<int>(rm) + displacement);
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
            byte sib = processor.CachedIP;
            processor.EIP++;

            int displacement = Unsafe.ReadUnaligned<int>(in processor.CachedIP);
            processor.EIP += 4;

            address = GetSibAddress32(processor, displacement, sib, ref segment);
        }
        else
        {
            if (rm == 5)
                segment = SegmentIndex.SS;

            int displacement = Unsafe.ReadUnaligned<int>(in processor.CachedIP);
            processor.EIP += 4;

            address = (uint)(processor.GetWordRegister<int>(rm) + displacement);
        }

        return address;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe uint GetSibAddress32(Processor processor, int displacement, byte sib, ref SegmentIndex segment)
    {
        int scale = (int)Intrinsics.ExtractBits(sib, 6, 2, 0b1100_0000);
        int index = (int)Intrinsics.ExtractBits(sib, 3, 3, 0b0011_1000);
        int baseIndex = sib & 0x7;

        int indexValue = 0;
        if (index != 4)
            indexValue = (processor.GetWordRegister<int>(index)) << scale;

        int baseValue = processor.GetWordRegister<int>(baseIndex);

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
            indexValue = (processor.GetWordRegister<int>(index)) << scale;

        int baseValue = 0;
        if (baseIndex != 5)
        {
            baseValue = processor.GetWordRegister<int>(baseIndex);
        }
        else
        {
            displacement = Unsafe.ReadUnaligned<int>(in processor.CachedIP);
            processor.EIP += 4;
        }

        if (baseIndex == 4)
            segment = SegmentIndex.SS;

        uint address = (uint)(baseValue + indexValue + displacement);

        return address;
    }
}
