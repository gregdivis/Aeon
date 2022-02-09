using System;
using System.Diagnostics.CodeAnalysis;

namespace Aeon.Emulator.Dos.VirtualFileSystem
{
    /// <summary>
    /// Specifies the offset of a CD track.
    /// </summary>
    public readonly struct CDTimeSpan : IEquatable<CDTimeSpan>, IComparable<CDTimeSpan>
    {
        //public const int FramesPerSector = 71;
        public const int FramesPerSecond = 75;
        public const int FramesPerMinute = FramesPerSecond * 60;
        public const int SectorsPerSecond = FramesPerSecond;

        /// <summary>
        /// Initializes a new instance of the <see cref="CDTimeSpan"/> struct.
        /// </summary>
        /// <param name="totalFrames">Offset specified in the total number of sectors.</param>
        public CDTimeSpan(int totalSectors) => this.TotalSectors = totalSectors;
        /// <summary>
        /// Initializes a new instance of the <see cref="CDTimeSpan"/> struct.
        /// </summary>
        /// <param name="minutes">Offset in minutes.</param>
        /// <param name="seconds">Offset in seconds.</param>
        /// <param name="frames">Offset in frames.</param>
        public CDTimeSpan(int minutes, int seconds, int frames) : this((minutes * FramesPerMinute) + (seconds * FramesPerSecond) + frames)
        {
        }

        public static bool operator ==(CDTimeSpan left, CDTimeSpan right) => left.Equals(right);
        public static bool operator !=(CDTimeSpan left, CDTimeSpan right) => !(left == right);
        public static bool operator <(CDTimeSpan left, CDTimeSpan right) => left.CompareTo(right) < 0;
        public static bool operator <=(CDTimeSpan left, CDTimeSpan right) => left.CompareTo(right) <= 0;
        public static bool operator >(CDTimeSpan left, CDTimeSpan right) => left.CompareTo(right) > 0;
        public static bool operator >=(CDTimeSpan left, CDTimeSpan right) => left.CompareTo(right) >= 0;

        public static CDTimeSpan operator +(CDTimeSpan left, CDTimeSpan right) => new(left.TotalSectors + right.TotalSectors);
        public static CDTimeSpan operator -(CDTimeSpan left, CDTimeSpan right) => new(left.TotalSectors - right.TotalSectors);

        /// <summary>
        /// Gets the whole minutes of the offset.
        /// </summary>
        public int Minutes => this.TotalFrames / FramesPerMinute;
        /// <summary>
        /// Gets the remaining whole seconds of the offset.
        /// </summary>
        public int Seconds => this.TotalFrames % FramesPerMinute / FramesPerSecond;
        /// <summary>
        /// Gets the remaining frames of the offset.
        /// </summary>
        public int Frames => this.TotalFrames % FramesPerSecond;
        /// <summary>
        /// Gets the total number of sectors of the offset.
        /// </summary>
        public int TotalSectors { get; }
        /// <summary>
        /// Gets the total number of frames of the offset.
        /// </summary>
        public int TotalFrames => this.TotalSectors;

        /// <summary>
        /// Parses a standard mm:ss:ff offset string into a <see cref="CDTimeSpan"/> instance.
        /// </summary>
        /// <param name="text">Offset string in the format mm:ss:ff to parse.</param>
        /// <returns><see cref="CDTimeSpan"/> instance parsed from the specified value.</returns>
        /// <exception cref="FormatException"><paramref name="text"/> is not a valid offset string.</exception>
        public static CDTimeSpan Parse(ReadOnlySpan<char> text)
        {
            if (text.Length != 8 || text[2] != ':' || text[5] != ':')
                throw new FormatException($"Invalid position/length: {text}");

            int minutes = int.Parse(text[..2]);
            int seconds = int.Parse(text.Slice(3, 2));
            int frames = int.Parse(text.Slice(6, 2));

            return new CDTimeSpan(minutes, seconds, frames);
        }

        public bool Equals(CDTimeSpan other) => this.TotalSectors == other.TotalSectors;
        public override bool Equals([NotNullWhen(true)] object obj) => obj is CDTimeSpan t && this.Equals(t);
        public override int GetHashCode() => this.TotalFrames.GetHashCode();
        public override string ToString() => $"{this.Minutes:00}:{this.Seconds:00}:{this.Frames:00}";
        public int CompareTo(CDTimeSpan other) => this.TotalSectors.CompareTo(other.TotalSectors);
    }
}
