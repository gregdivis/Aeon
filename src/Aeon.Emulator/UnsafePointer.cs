using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Aeon.Emulator;

public readonly ref struct UnsafePointer<T> where T : unmanaged
{
    private readonly ref T pointer;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public UnsafePointer(ref T ptr) => this.pointer = ref ptr;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public UnsafePointer(ReadOnlySpan<T> span) => this.pointer = ref MemoryMarshal.GetReference(span);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public UnsafePointer(Span<T> span) => this.pointer = ref MemoryMarshal.GetReference(span);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public UnsafePointer(Span<byte> span) => this.pointer = ref Unsafe.As<byte, T>(ref MemoryMarshal.GetReference(span));

    public ref T this[int index] => ref Unsafe.Add(ref this.pointer, index);
    public ref T this[uint index] => ref Unsafe.Add(ref this.pointer, index);

    public ref T Value => ref this.pointer;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UnsafePointer<T> operator +(UnsafePointer<T> ptr, int offset) => new(ref Unsafe.Add(ref ptr.pointer, offset));
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UnsafePointer<T> operator ++(UnsafePointer<T> ptr) => new(ref Unsafe.Add(ref ptr.pointer, 1));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public UnsafePointer<TCast> As<TCast>() where TCast : unmanaged => new(ref Unsafe.As<T, TCast>(ref this.pointer));
}
