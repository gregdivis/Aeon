using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Aeon.Emulator.Memory;

/// <summary>
/// Represents an unknown type of descriptor.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 8)]
public readonly struct Descriptor
{
    private readonly DescriptorData data;

    /// <summary>
    /// Gets the type of the descriptor.
    /// </summary>
    public DescriptorType DescriptorType
    {
        get
        {
            byte type = this.data.Type;
            if ((type & (1 << 4)) != 0)
            {
                return DescriptorType.Segment;
            }
            else
            {
                type &= 0b1111;

                if (type == 0x0C)
                    return DescriptorType.CallGate;
                else if (type == 0x02)
                    return DescriptorType.Ldt;
                else if (type == 0x05)
                    return DescriptorType.TaskGate;
                else if (type == 0x06 || type == 0x0E)
                    return DescriptorType.InterruptGate;
                else if (type == 0x09 || type == 0x0B)
                    return DescriptorType.TaskSegmentSelector;
                else
                    return DescriptorType.TrapGate;
            }
        }
    }
    /// <summary>
    /// Gets a value indicating whether this is a system descriptor.
    /// </summary>
    public bool IsSystemDescriptor => (this.data.Type & (1 << 4)) == 0;
    /// <summary>
    /// Gets the access rights for the descriptor.
    /// </summary>
    public ushort AccessRights => this.data.AccessRights;

    /// <summary>
    /// Casts a descriptor to a segment descriptor.
    /// </summary>
    /// <param name="descriptor">Descriptor to cast.</param>
    /// <returns>Resulting segment descriptor.</returns>
    public static explicit operator SegmentDescriptor(Descriptor descriptor) => Unsafe.BitCast<Descriptor, SegmentDescriptor>(descriptor);
    /// <summary>
    /// Casts a descriptor to an interupt descriptor.
    /// </summary>
    /// <param name="descriptor">Descriptor to cast.</param>
    /// <returns>Resulting interrupt descriptor.</returns>
    public static explicit operator InterruptDescriptor(Descriptor descriptor) => Unsafe.BitCast<Descriptor, InterruptDescriptor>(descriptor);
    /// <summary>
    /// Casts a descriptor to a call gate descriptor.
    /// </summary>
    /// <param name="descriptor">Descriptor to cast.</param>
    /// <returns>Resulting call gate descriptor.</returns>
    public static explicit operator CallGateDescriptor(Descriptor descriptor) => Unsafe.BitCast<Descriptor, CallGateDescriptor>(descriptor);
    /// <summary>
    /// Casts a descriptor to a task segment descriptor.
    /// </summary>
    /// <param name="descriptor">Descriptor to cast.</param>
    /// <returns>Resulting task segment descriptor.</returns>
    public static explicit operator TaskSegmentDescriptor(Descriptor descriptor) => Unsafe.BitCast<Descriptor, TaskSegmentDescriptor>(descriptor);

    [StructLayout(LayoutKind.Explicit, Size = 8)]
    private readonly struct DescriptorData
    {
        [FieldOffset(5)]
        public readonly byte Type;
        [FieldOffset(5)]
        public readonly ushort AccessRights;
    }
}
