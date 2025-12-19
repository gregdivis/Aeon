using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Aeon.Emulator.Memory;

namespace Aeon.Emulator;

/// <summary>
/// Contains information about a protected mode task segment.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 8)]
public struct TaskSegmentDescriptor
{
    /// <summary>
    /// 4KB granularity mode in Attributes2.
    /// </summary>
    public const uint Granularity = 1 << 7;
    /// <summary>
    /// Busy flag in Attributes1.
    /// </summary>
    public const uint Busy = 1 << 1;

    private readonly ushort limit1;
    private readonly ushort base1;
    private readonly byte base2;
    private byte attributes1;
    private readonly byte attributes2;
    private readonly byte base3;

    /// <summary>
    /// Casts a task segment descriptor to a descriptor.
    /// </summary>
    /// <param name="descriptor">Task segment descriptor to cast.</param>
    /// <returns>Resulting descriptor.</returns>
    public static implicit operator Descriptor(TaskSegmentDescriptor descriptor) => Unsafe.BitCast<TaskSegmentDescriptor, Descriptor>(descriptor);

    /// <summary>
    /// Gets a value indicating whether the task is busy.
    /// </summary>
    public bool IsBusy
    {
        readonly get => (this.attributes1 & Busy) != 0;
        internal set
        {
            if (value)
                this.attributes1 |= (byte)Busy;
            else
                this.attributes1 &= unchecked((byte)~Busy);
        }
    }
    /// <summary>
    /// Gets the physical base address of the segment.
    /// </summary>
    public readonly uint Base => (uint)this.base1 | ((uint)this.base2 << 16) | ((uint)this.base3 << 24);
    /// <summary>
    /// Gets the size of the segment.
    /// </summary>
    public readonly uint Limit => (uint)this.limit1 | (((uint)this.attributes2 & 0x0Fu) << 16);
    /// <summary>
    /// Gets attribute byte 1 of the descriptor.
    /// </summary>
    public readonly byte Attributes1 => attributes1;
    /// <summary>
    /// Gets attribute byte 2 of the descriptor.
    /// </summary>
    public readonly byte Attributes2 => attributes2;
    /// <summary>
    /// Gets the privilege level of the descriptor.
    /// </summary>
    public readonly uint PrivilegeLevel => (uint)(this.attributes1 >> 5) & 0b11u;
    /// <summary>
    /// Gets the limit of the descriptor in bytes.
    /// </summary>
    public readonly uint ByteLimit => (this.attributes2 & Granularity) == 0 ? this.Limit : (this.Limit << 12) | 0xFFFu;
}
