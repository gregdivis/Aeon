using System.Globalization;

namespace Aeon.Emulator.DebugSupport;

/// <summary>
/// Represents an address qualified by mode.
/// </summary>
public readonly struct QualifiedAddress : IEquatable<QualifiedAddress>
{
    private readonly AddressType type;
    private readonly ushort segment;
    private readonly uint offset;

    /// <summary>
    /// Initializes a new instance of the <see cref="QualifiedAddress"/> struct.
    /// </summary>
    /// <param name="addressType">Mode of address qualification.</param>
    /// <param name="segment">Segment or selector of the address.</param>
    /// <param name="offset">Offset of the address.</param>
    public QualifiedAddress(AddressType addressType, ushort segment, uint offset)
    {
        this.type = addressType;
        this.segment = segment;
        this.offset = offset;
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="QualifiedAddress"/> struct.
    /// </summary>
    /// <param name="addressType">Mode of address qualification.</param>
    /// <param name="offset">Offset of the address.</param>
    public QualifiedAddress(AddressType addressType, uint offset)
    {
        this.type = addressType;
        this.offset = offset;
        this.segment = 0;
    }

    public static bool operator ==(QualifiedAddress valueA, QualifiedAddress valueB) => valueA.Equals(valueB);
    public static bool operator !=(QualifiedAddress valueA, QualifiedAddress valueB) => !valueA.Equals(valueB);
    public static QualifiedAddress operator +(QualifiedAddress valueA, int valueB) => new QualifiedAddress(valueA.type, valueA.segment, (uint)(valueA.offset + valueB));

    /// <summary>
    /// Gets the addressing mode.
    /// </summary>
    public AddressType AddressType => this.type;
    /// <summary>
    /// Gets the segment or selector of the address. Null indicates a linear address.
    /// </summary>
    public ushort? Segment
    {
        get
        {
            if (this.type == AddressType.RealMode || this.type == AddressType.ProtectedMode)
                return this.segment;
            else
                return null;
        }
    }
    /// <summary>
    /// Gets the offset of the address. For linear addresses, this specifies the entire address.
    /// </summary>
    public uint Offset => this.offset;

    /// <summary>
    /// Returns a QualifiedAddress instance representing a physical address.
    /// </summary>
    /// <param name="physicalAddress">Physial memory address.</param>
    /// <returns>QualifiedAddress instance representing a physical address.</returns>
    public static QualifiedAddress FromPhysicalAddress(uint physicalAddress) => new QualifiedAddress(AddressType.PhysicalLinear, physicalAddress);
    /// <summary>
    /// Returns a QualifiedAddress instance respresenting a logical address.
    /// </summary>
    /// <param name="logicalAddress">Logical memory address.</param>
    /// <returns>QualifiedAddress instance representing a logical address.</returns>
    public static QualifiedAddress FromLogicalAddress(uint logicalAddress) => new QualifiedAddress(AddressType.LogicalLinear, logicalAddress);
    /// <summary>
    /// Returns a QualifiedAddress instance representing a real-mode memory address.
    /// </summary>
    /// <param name="segment">Real-mode address segment.</param>
    /// <param name="offset">Real-mode address offset.</param>
    /// <returns>QualifiedAddress representing a real-mode memory address.</returns>
    public static QualifiedAddress FromRealModeAddress(ushort segment, ushort offset) => new QualifiedAddress(AddressType.RealMode, segment, offset);
    /// <summary>
    /// Returns a QualifiedAddress instance representing a protected-mode memory address.
    /// </summary>
    /// <param name="selector">Protected-mode segment selector.</param>
    /// <param name="offset">Protected-mode segment offset.</param>
    /// <returns>QualifiedAddress representing a protected-mode memory address.</returns>
    public static QualifiedAddress FromProtectedModeAddress(ushort selector, uint offset) => new QualifiedAddress(AddressType.ProtectedMode, selector, offset);
    /// <summary>
    /// Parses an address string into a QualifiedAddress instance.
    /// </summary>
    /// <param name="s">Address string to parse.</param>
    /// <returns>QualifiedAddress instance if parsing was successful; otherwise null.</returns>
    public static QualifiedAddress? TryParse(string? s)
    {
        s = s?.Trim();
        if (string.IsNullOrEmpty(s))
            return null;

        string[] parts;

        if (s[0] == '@')
        {
            s = s.Substring(1);
            parts = s.Split(':');
            if (parts.Length == 1)
            {
                if (uint.TryParse(s, NumberStyles.HexNumber, CultureInfo.InvariantCulture.NumberFormat, out uint offset))
                    return FromPhysicalAddress(offset);

                return null;
            }
            else if (parts.Length == 2)
            {
                if (!uint.TryParse(parts[0], NumberStyles.HexNumber, CultureInfo.InvariantCulture.NumberFormat, out uint segment) || segment > ushort.MaxValue)
                    return null;

                if (!uint.TryParse(parts[1], NumberStyles.HexNumber, CultureInfo.InvariantCulture.NumberFormat, out uint offset) || offset > ushort.MaxValue)
                    return null;

                return FromRealModeAddress((ushort)segment, (ushort)offset);
            }

            return null;
        }

        parts = s.Split(':');
        if (parts.Length == 1)
        {
            if (uint.TryParse(s, NumberStyles.HexNumber, CultureInfo.InvariantCulture.NumberFormat, out uint offset))
                return FromLogicalAddress(offset);

            return null;
        }
        else if (parts.Length == 2)
        {
            if (!uint.TryParse(parts[0], NumberStyles.HexNumber, CultureInfo.InvariantCulture.NumberFormat, out uint segment) || segment > ushort.MaxValue)
                return null;

            if (!uint.TryParse(parts[1], NumberStyles.HexNumber, CultureInfo.InvariantCulture.NumberFormat, out uint offset))
                return null;

            return FromProtectedModeAddress((ushort)segment, offset);
        }

        return null;
    }

    /// <summary>
    /// Returns a string representation of the QualifiedAddress instance.
    /// </summary>
    /// <returns>String representation of the QualifiedAddress.</returns>
    public override string ToString()
    {
        switch (this.type)
        {
            case AddressType.LogicalLinear:
                return this.Offset.ToString("X8");

            case AddressType.PhysicalLinear:
                return "@" + this.Offset.ToString("X8");

            case AddressType.RealMode:
                return $"@{this.segment:X4}:{this.offset:X4}";

            case AddressType.ProtectedMode:
                return $"{this.segment:X4}:{this.offset:X8}";

            default:
                return string.Empty;
        }
    }
    /// <summary>
    /// Tests for equality with another QualifiedAddress instance.
    /// </summary>
    /// <param name="other">Other QualifiedAddress instance to test for equality.</param>
    /// <returns>True if addresses are equal; otherwise false.</returns>
    public bool Equals(QualifiedAddress other) => this.type == other.type && this.Segment == other.Segment && this.Offset == other.Offset;
    /// <summary>
    /// Tests for equality with another object.
    /// </summary>
    /// <param name="obj">Other object to test for equality.</param>
    /// <returns>True if objects are equal; otherwise false.</returns>
    public override bool Equals(object? obj) => obj is QualifiedAddress a && this.Equals(a);
    /// <summary>
    /// Returns a hash code for the address.
    /// </summary>
    /// <returns>Hash code for the address.</returns>
    public override int GetHashCode() => this.offset.GetHashCode();
}

/// <summary>
/// Describes the type of a qualified address.
/// </summary>
public enum AddressType : byte
{
    /// <summary>
    /// The address is a logical 32-bit (paged) address.
    /// </summary>
    LogicalLinear,
    /// <summary>
    /// The address is a physical 32-bit (nonpaged) address.
    /// </summary>
    PhysicalLinear,
    /// <summary>
    /// The address is a real-mode segment:offset pair.
    /// </summary>
    RealMode,
    /// <summary>
    /// The address is a protected-mode selector:offset pair.
    /// </summary>
    ProtectedMode
}
