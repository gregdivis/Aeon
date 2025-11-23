namespace Aeon.Emulator.Dos;

/// <summary>
/// Contains information about a DOS memory allocation.
/// </summary>
internal sealed class MemoryControlBlock
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MemoryControlBlock"/> class.
    /// </summary>
    /// <param name="segment">Segment of the memory control block.</param>
    /// <param name="pspSegment">Segment of the PSP which owns the block.</param>
    /// <param name="imageName">Name of the process which owns the block.</param>
    public MemoryControlBlock(ushort segment, ushort pspSegment, string imageName)
    {
        this.Segment = segment;
        this.PspSegment = pspSegment;
        this.ImageName = imageName;
        this.IsLast = true;
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="MemoryControlBlock"/> class.
    /// </summary>
    /// <param name="memory">Emulated memory associated with the block.</param>
    /// <param name="segment">Segment of the memory control block to read.</param>
    public MemoryControlBlock(PhysicalMemory memory, ushort segment)
    {
        this.Segment = segment;
        this.IsLast = memory.GetByte(segment, 0) == 0x5A;
        this.PspSegment = memory.GetUInt16(segment, 1);
        this.Length = memory.GetUInt16(segment, 3);
        this.ImageName = memory.GetString(segment, 8, 8, 0);
    }

    /// <summary>
    /// Gets or sets the segment where the block is found.
    /// </summary>
    public ushort Segment { get; set; }
    /// <summary>
    /// Gets or sets the segment of the PSP which owns the block.
    /// </summary>
    public ushort PspSegment { get; set; }
    /// <summary>
    /// Gets or sets the length of the block in 16-byte paragraphs.
    /// </summary>
    public ushort Length { get; set; }
    /// <summary>
    /// Gets or sets the name of the process which owns the block.
    /// </summary>
    public string ImageName { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether this is the last block.
    /// </summary>
    public bool IsLast { get; set; }
    /// <summary>
    /// Gets a value indicating whether the block is free.
    /// </summary>
    public bool IsInUse => this.PspSegment != 0;

    /// <summary>
    /// Writes the values of the memory control block to emulated memory.
    /// </summary>
    /// <param name="memory">Emulated memory where block will be written.</param>
    public void Write(PhysicalMemory memory)
    {
        memory.SetByte(this.Segment, 0, this.IsLast ? (byte)0x5A : (byte)0x4D);
        memory.SetUInt16(this.Segment, 1, this.PspSegment);
        memory.SetUInt16(this.Segment, 3, this.Length);
        
        // These are just to clear the name area to make sure it is padded with nulls.
        memory.SetUInt32(this.Segment, 8, 0);
        memory.SetUInt32(this.Segment, 12, 0);

        memory.SetString(this.Segment, 8, this.ImageName, false);
    }
    /// <summary>
    /// Marks the block as free.
    /// </summary>
    public void Free() => this.PspSegment = 0;
    /// <summary>
    /// Gets a string representation of the memory control block.
    /// </summary>
    /// <returns>String representation of the memory control block.</returns>
    public override string ToString() => $"{this.Segment:X4}: {this.Length:X4} {(this.IsInUse ? this.ImageName : "<Free>")}";
}
