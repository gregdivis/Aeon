using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Aeon.Emulator.Decoding;

internal static partial class InstructionDecoders
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe T ReadImmediate<T>(Processor p) where T : unmanaged
    {
        var value = Unsafe.ReadUnaligned<T>(in p.CachedIP);
        p.EIP += (uint)sizeof(T);
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe byte GetReg(Processor p)
    {
        return (byte)Intrinsics.ExtractBits(p.CachedIP, 3, 3, 0x38);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void GetModRm(Processor p, out byte mod, out byte rm)
    {
        rm = (byte)(p.CachedIP & 0x07u);
        mod = (byte)Intrinsics.ExtractBits(p.CachedIP, 6, 2, 0xC0);
        p.EIP++;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe RmwValue<T> GetRegRmw16<T>(Processor p, int mod, int rm, bool memoryOnly)
        where T : unmanaged
    {
        if (mod != 3)
        {
            return new RmwValue<T>(RegRmw16Loads.LoadAddress(rm, mod, p));
        }
        else if (!memoryOnly)
        {
            if (Unsafe.SizeOf<T>() == 1)
                return new RmwValue<T>(ref Unsafe.As<byte, T>(ref p.GetByteRegister(rm)));
            else
                return new RmwValue<T>(ref p.GetWordRegister<T>(rm));
        }
        else
        {
            ThrowMod3Exception();
            return default;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe RmwValue<T> GetRegRmw32<T>(Processor p, int mod, int rm, bool memoryOnly)
        where T : unmanaged
    {
        if (mod != 3)
        {
            return new RmwValue<T>(RegRmw32Loads.LoadAddress(rm, mod, p));
        }
        else if (!memoryOnly)
        {
            if (Unsafe.SizeOf<T>() == 1)
                return new RmwValue<T>(ref Unsafe.As<byte, T>(ref p.GetByteRegister(rm)));
            else
                return new RmwValue<T>(ref p.GetWordRegister<T>(rm));
        }
        else
        {
            ThrowMod3Exception();
            return default;
        }
    }

    [DoesNotReturn]
    public static void ThrowMod3Exception() => throw new Mod3Exception();

    public readonly ref struct RmwValue<T>
        where T : unmanaged
    {
        private readonly ref T ptr;
        private readonly uint address;

        public RmwValue(ref T regPtr)
        {
            this.ptr = ref regPtr;
            this.address = 0;
        }
        public RmwValue(uint address)
        {
            this.ptr = ref Unsafe.NullRef<T>();
            this.address = address;
        }

        public bool IsPointer => !Unsafe.IsNullRef(ref this.ptr);
        public ref T RegisterValue => ref this.ptr;
        public uint Address => this.address;
    }
}
