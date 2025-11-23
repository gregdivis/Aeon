namespace Aeon.Emulator.Dos.VirtualFileSystem;

/// <summary>
/// Uniquely identifies a drive in DOS.
/// </summary>
public readonly struct DriveLetter : IEquatable<DriveLetter>, IComparable<DriveLetter>
{
    private readonly byte driveIndex;

    /// <summary>
    /// The A: drive.
    /// </summary>
    public static readonly DriveLetter A = new(0);
    /// <summary>
    /// The B: drive.
    /// </summary>
    public static readonly DriveLetter B = new(1);
    /// <summary>
    /// The C: drive.
    /// </summary>
    public static readonly DriveLetter C = new(2);
    /// <summary>
    /// The D: drive.
    /// </summary>
    public static readonly DriveLetter D = new(3);
    /// <summary>
    /// The E: drive.
    /// </summary>
    public static readonly DriveLetter E = new(4);
    /// <summary>
    /// The F: drive.
    /// </summary>
    public static readonly DriveLetter F = new(5);
    /// <summary>
    /// The G: drive.
    /// </summary>
    public static readonly DriveLetter G = new(6);
    /// <summary>
    /// The H: drive.
    /// </summary>
    public static readonly DriveLetter H = new(7);
    /// <summary>
    /// The I: drive.
    /// </summary>
    public static readonly DriveLetter I = new(8);
    /// <summary>
    /// The J: drive.
    /// </summary>
    public static readonly DriveLetter J = new(9);
    /// <summary>
    /// The K: drive.
    /// </summary>
    public static readonly DriveLetter K = new(10);
    /// <summary>
    /// The L: drive.
    /// </summary>
    public static readonly DriveLetter L = new(11);
    /// <summary>
    /// The M: drive.
    /// </summary>
    public static readonly DriveLetter M = new(12);
    /// <summary>
    /// The N: drive.
    /// </summary>
    public static readonly DriveLetter N = new(13);
    /// <summary>
    /// The O: drive.
    /// </summary>
    public static readonly DriveLetter O = new(14);
    /// <summary>
    /// The P: drive.
    /// </summary>
    public static readonly DriveLetter P = new(15);
    /// <summary>
    /// The Q: drive.
    /// </summary>
    public static readonly DriveLetter Q = new(16);
    /// <summary>
    /// The R: drive.
    /// </summary>
    public static readonly DriveLetter R = new(17);
    /// <summary>
    /// The S: drive.
    /// </summary>
    public static readonly DriveLetter S = new(18);
    /// <summary>
    /// The T: drive.
    /// </summary>
    public static readonly DriveLetter T = new(19);
    /// <summary>
    /// The U: drive.
    /// </summary>
    public static readonly DriveLetter U = new(20);
    /// <summary>
    /// The V: drive.
    /// </summary>
    public static readonly DriveLetter V = new(21);
    /// <summary>
    /// The W: drive.
    /// </summary>
    public static readonly DriveLetter W = new(22);
    /// <summary>
    /// The X: drive.
    /// </summary>
    public static readonly DriveLetter X = new(23);
    /// <summary>
    /// The Y: drive.
    /// </summary>
    public static readonly DriveLetter Y = new(24);
    /// <summary>
    /// The Z: drive.
    /// </summary>
    public static readonly DriveLetter Z = new(25);

    /// <summary>
    /// Initializes a new instance of the <see cref="DriveLetter"/> struct.
    /// </summary>
    /// <param name="driveIndex">Index of the drive.</param>
    public DriveLetter(int driveIndex)
    {
        if(driveIndex < 0 || driveIndex >= 26)
            throw new ArgumentOutOfRangeException(nameof(driveIndex));

        this.driveIndex = (byte)driveIndex;
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="DriveLetter"/> struct.
    /// </summary>
    /// <param name="driveLetter">The drive letter.</param>
    public DriveLetter(char driveLetter)
    {
        var letter = char.ToUpperInvariant(driveLetter);
        if(letter < 'A' || letter > 'Z')
            throw new ArgumentOutOfRangeException(nameof(driveLetter));

        this.driveIndex = (byte)(letter - 'A');
    }

    public static bool operator ==(DriveLetter letter1, DriveLetter letter2) => letter1.driveIndex == letter2.driveIndex;
    public static bool operator !=(DriveLetter letter1, DriveLetter letter2) => letter1.driveIndex != letter2.driveIndex;

    /// <summary>
    /// Gets the index of the drive.
    /// </summary>
    /// <remarks>
    /// Drive A: = 0.
    /// </remarks>
    public int Index => this.driveIndex;

    public int CompareTo(DriveLetter other) => this.driveIndex.CompareTo(other.driveIndex);
    public bool Equals(DriveLetter other) => this.driveIndex == other.driveIndex;
    public override bool Equals(object? obj) => obj is DriveLetter d && this.Equals(d);
    public override int GetHashCode() => this.driveIndex.GetHashCode();
    public override string ToString()
    {
        Span<char> buffer = [(char)('A' + this.driveIndex), ':'];
        return new string(buffer);
    }
}
