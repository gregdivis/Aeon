namespace Aeon.Emulator;

/// <summary>
/// Provides a simple allocation-only native memory heap.
/// </summary>
internal sealed class NativeHeap
{
    private readonly UnsafeBuffer<byte> memory;
    private readonly IntPtr pointer;
    private int nextOffset;

    /// <summary>
    /// Initializes a new instance of the <see cref="NativeHeap"/> class.
    /// </summary>
    /// <param name="size">The size of the heap in bytes.</param>
    public NativeHeap(int size)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(size);

        this.Size = size;
        this.memory = new UnsafeBuffer<byte>(size);
        unsafe
        {
            this.pointer = new IntPtr(memory.ToPointer());
        }
    }

    /// <summary>
    /// Gets the size of the heap in bytes.
    /// </summary>
    public int Size { get; }
    /// <summary>
    /// Gets the number of bytes available for allocation in the heap.
    /// </summary>
    public int Free => this.Size - this.nextOffset;

    /// <summary>
    /// Allocates bytes in the heap at a specified alignment.
    /// </summary>
    /// <param name="size">Number of bytes to allocate.</param>
    /// <param name="alignment">Required alignment of the allocation.</param>
    /// <returns>Pointer to the allocated bytes.</returns>
    public IntPtr Allocate(int size, int alignment)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(size);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(alignment);

        int offset = this.nextOffset;
        if ((offset % alignment) != 0)
            offset += alignment - (offset % alignment);

        if (offset >= this.Size)
            throw new ArgumentException("Not enough memory.");

        this.nextOffset = offset + size;
        return IntPtr.Add(this.pointer, offset);
    }
}
