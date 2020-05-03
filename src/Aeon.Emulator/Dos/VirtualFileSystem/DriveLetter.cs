using System;

namespace Aeon.Emulator.Dos.VirtualFileSystem
{
    /// <summary>
    /// Uniquely identifies a drive in DOS.
    /// </summary>
    [Serializable]
    public readonly struct DriveLetter : IEquatable<DriveLetter>, IComparable<DriveLetter>
    {
        private readonly byte driveIndex;

        /// <summary>
        /// The A: drive.
        /// </summary>
        public static DriveLetter A = new DriveLetter(0);
        /// <summary>
        /// The B: drive.
        /// </summary>
        public static DriveLetter B = new DriveLetter(1);
        /// <summary>
        /// The C: drive.
        /// </summary>
        public static DriveLetter C = new DriveLetter(2);
        /// <summary>
        /// The D: drive.
        /// </summary>
        public static DriveLetter D = new DriveLetter(3);
        /// <summary>
        /// The E: drive.
        /// </summary>
        public static DriveLetter E = new DriveLetter(4);
        /// <summary>
        /// The F: drive.
        /// </summary>
        public static DriveLetter F = new DriveLetter(5);
        /// <summary>
        /// The G: drive.
        /// </summary>
        public static DriveLetter G = new DriveLetter(6);
        /// <summary>
        /// The H: drive.
        /// </summary>
        public static DriveLetter H = new DriveLetter(7);
        /// <summary>
        /// The I: drive.
        /// </summary>
        public static DriveLetter I = new DriveLetter(8);
        /// <summary>
        /// The J: drive.
        /// </summary>
        public static DriveLetter J = new DriveLetter(9);
        /// <summary>
        /// The K: drive.
        /// </summary>
        public static DriveLetter K = new DriveLetter(10);
        /// <summary>
        /// The L: drive.
        /// </summary>
        public static DriveLetter L = new DriveLetter(11);
        /// <summary>
        /// The M: drive.
        /// </summary>
        public static DriveLetter M = new DriveLetter(12);
        /// <summary>
        /// The N: drive.
        /// </summary>
        public static DriveLetter N = new DriveLetter(13);
        /// <summary>
        /// The O: drive.
        /// </summary>
        public static DriveLetter O = new DriveLetter(14);
        /// <summary>
        /// The P: drive.
        /// </summary>
        public static DriveLetter P = new DriveLetter(15);
        /// <summary>
        /// The Q: drive.
        /// </summary>
        public static DriveLetter Q = new DriveLetter(16);
        /// <summary>
        /// The R: drive.
        /// </summary>
        public static DriveLetter R = new DriveLetter(17);
        /// <summary>
        /// The S: drive.
        /// </summary>
        public static DriveLetter S = new DriveLetter(18);
        /// <summary>
        /// The T: drive.
        /// </summary>
        public static DriveLetter T = new DriveLetter(19);
        /// <summary>
        /// The U: drive.
        /// </summary>
        public static DriveLetter U = new DriveLetter(20);
        /// <summary>
        /// The V: drive.
        /// </summary>
        public static DriveLetter V = new DriveLetter(21);
        /// <summary>
        /// The W: drive.
        /// </summary>
        public static DriveLetter W = new DriveLetter(22);
        /// <summary>
        /// The X: drive.
        /// </summary>
        public static DriveLetter X = new DriveLetter(23);
        /// <summary>
        /// The Y: drive.
        /// </summary>
        public static DriveLetter Y = new DriveLetter(24);
        /// <summary>
        /// The Z: drive.
        /// </summary>
        public static DriveLetter Z = new DriveLetter(25);

        /// <summary>
        /// Initializes a new instance of the <see cref="DriveLetter"/> struct.
        /// </summary>
        /// <param name="driveIndex">Index of the drive.</param>
        public DriveLetter(int driveIndex)
        {
            if(driveIndex < 0 || driveIndex >= 26)
                throw new ArgumentOutOfRangeException();

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
                throw new ArgumentOutOfRangeException();

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
        public override bool Equals(object obj) => obj is DriveLetter d ? this.Equals(d) : false;
        public override int GetHashCode() => this.driveIndex.GetHashCode();
        public override string ToString() => new string(new[] { (char)('A' + this.driveIndex), ':' });
    }
}
