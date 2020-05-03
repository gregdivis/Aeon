using System.Runtime.InteropServices;

namespace Aeon.Emulator.Memory
{
    /// <summary>
    /// Represents an unknown type of descriptor.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Descriptor
    {
        private unsafe fixed byte data[8];

        /// <summary>
        /// Gets the type of the descriptor.
        /// </summary>
        public DescriptorType DescriptorType
        {
            get
            {
                byte type;
                unsafe
                {
                    fixed (byte* ptr = this.data)
                    {
                        type = ptr[5];
                    }
                }
                if ((type & (1 << 4)) != 0)
                {
                    return DescriptorType.Segment;
                }
                else
                {
                    type &= 0b1111;

                    if (type == 0x0C)
                        return DescriptorType.CallGate;
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
        public bool IsSystemDescriptor
        {
            get
            {
                unsafe
                {
                    fixed (byte* ptr = this.data)
                    {
                        return (ptr[5] & (1 << 4)) == 0;
                    }
                }
            }
        }
        /// <summary>
        /// Gets the access rights for the descriptor.
        /// </summary>
        public ushort AccessRights
        {
            get
            {
                unsafe
                {
                    fixed (byte* ptr = this.data)
                    {
                        return *(ushort*)&ptr[5];
                    }
                }
            }
        }

        /// <summary>
        /// Casts a descriptor to a segment descriptor.
        /// </summary>
        /// <param name="descriptor">Descriptor to cast.</param>
        /// <returns>Resulting segment descriptor.</returns>
        public static explicit operator SegmentDescriptor(Descriptor descriptor)
        {
            unsafe
            {
                return *(SegmentDescriptor*)&descriptor;
            }
        }
        /// <summary>
        /// Casts a descriptor to an interupt descriptor.
        /// </summary>
        /// <param name="descriptor">Descriptor to cast.</param>
        /// <returns>Resulting interrupt descriptor.</returns>
        public static explicit operator InterruptDescriptor(Descriptor descriptor)
        {
            unsafe
            {
                return *(InterruptDescriptor*)&descriptor;
            }
        }
        /// <summary>
        /// Casts a descriptor to a call gate descriptor.
        /// </summary>
        /// <param name="descriptor">Descriptor to cast.</param>
        /// <returns>Resulting call gate descriptor.</returns>
        public static explicit operator CallGateDescriptor(Descriptor descriptor)
        {
            unsafe
            {
                return *(CallGateDescriptor*)&descriptor;
            }
        }
        /// <summary>
        /// Casts a descriptor to a task segment descriptor.
        /// </summary>
        /// <param name="descriptor">Descriptor to cast.</param>
        /// <returns>Resulting task segment descriptor.</returns>
        public static explicit operator TaskSegmentDescriptor(Descriptor descriptor)
        {
            unsafe
            {
                return *(TaskSegmentDescriptor*)&descriptor;
            }
        }
    }

    public enum DescriptorType
    {
        Segment,
        CallGate,
        TaskGate,
        InterruptGate,
        TrapGate,
        TaskSegmentSelector
    }
}
