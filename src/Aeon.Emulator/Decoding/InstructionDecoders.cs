using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Aeon.Emulator.Decoding;

internal static partial class InstructionDecoders
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe T ReadImmediate<T>(Processor p) where T : unmanaged
    {
        var value = *(T*)p.CachedIP;
        p.CachedIP += sizeof(T);
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe byte GetReg(Processor p) => (byte)Intrinsics.ExtractBits(*p.CachedIP, 3, 3, 0x38);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void GetModRm(Processor p, out byte mod, out byte rm)
    {
        rm = (byte)(*p.CachedIP & 0x07u);
        mod = (byte)Intrinsics.ExtractBits(*p.CachedIP, 6, 2, 0xC0);
        p.CachedIP++;
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
            return new RmwValue<T>(sizeof(T) == 1 ? p.GetRegisterBytePointer(rm) : p.GetRegisterWordPointer(rm));
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
            return new RmwValue<T>(sizeof(T) == 1 ? p.GetRegisterBytePointer(rm) : p.GetRegisterWordPointer(rm));
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
        private readonly nuint ptr;

        public unsafe RmwValue(void* regPtr)
        {
            this.ptr = (nuint)regPtr;
            this.IsPointer = true;
        }
        public RmwValue(uint address)
        {
            this.ptr = address;
            this.IsPointer = false;
        }

        public bool IsPointer { get; }
        public ref T RegisterValue
        {
            get
            {
                unsafe
                {
                    return ref Unsafe.AsRef<T>((void*)this.ptr);
                }
            }
        }
        public unsafe T* RegisterPointer => (T*)this.ptr;
        public uint Address => (uint)this.ptr;
    }
}
