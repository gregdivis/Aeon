using System;

namespace Aeon.Emulator.Memory
{
    /// <summary>
    /// Represents a real-mode memory address.
    /// </summary>
    public readonly struct RealModeAddress : IEquatable<RealModeAddress>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RealModeAddress"/> struct.
        /// </summary>
        /// <param name="segment">The segment.</param>
        /// <param name="offset">The offset.</param>
        public RealModeAddress(ushort segment, ushort offset)
        {
            this.Segment = segment;
            this.Offset = offset;
        }

        public static bool operator ==(RealModeAddress entryA, RealModeAddress entryB) => entryA.Equals(entryB);
        public static bool operator !=(RealModeAddress entryA, RealModeAddress entryB) => !entryA.Equals(entryB);

        /// <summary>
        /// Gets the segment value.
        /// </summary>
        public ushort Segment { get; }
        /// <summary>
        /// Gets the offset value.
        /// </summary>
        public ushort Offset { get; }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString() => $"{this.Segment:X4}:{this.Offset:X4}";
        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        /// True if the specified <see cref="System.Object"/> is equal to this instance; otherwise, false.
        /// </returns>
        public override bool Equals(object obj) => obj is RealModeAddress a ? this.Equals(a) : false;
        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode() => (this.Segment << 16) | this.Offset;
        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// True if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        public bool Equals(RealModeAddress other) => this.Segment == other.Segment && this.Offset == other.Offset;
    }
}
