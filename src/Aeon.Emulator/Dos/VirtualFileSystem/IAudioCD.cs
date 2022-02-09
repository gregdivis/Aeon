using System.Collections.Generic;

namespace Aeon.Emulator.Dos.VirtualFileSystem
{
    /// <summary>
    /// Describes a CD with audio tracks.
    /// </summary>
    public interface IAudioCD
    {
        /// <summary>
        /// Gets information about the disc's tracks.
        /// </summary>
        IReadOnlyList<TrackInfo> Tracks { get; }
        /// <summary>
        /// Gets or sets the current playback sector.
        /// </summary>
        int PlaybackSector { get; set; }
        /// <summary>
        /// Gets the total number of sectors on the disc.
        /// </summary>
        int TotalSectors { get; }
        /// <summary>
        /// Gets a value indicating whether an audio track is playing.
        /// </summary>
        bool Playing { get; }

        /// <summary>
        /// Begins playback from the current value of <see cref="PlaybackSector"/>.
        /// </summary>
        /// <param name="sectors">Number of frames to play.</param>
        void Play(int? sectors = null);
        /// <summary>
        /// Stops playback.
        /// </summary>
        void Stop();
    }
}
