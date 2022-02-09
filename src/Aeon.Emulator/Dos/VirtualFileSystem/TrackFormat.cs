namespace Aeon.Emulator.Dos.VirtualFileSystem
{
    /// <summary>
    /// Specifies the format of a CD track.
    /// </summary>
    public enum TrackFormat
    {
        /// <summary>
        /// Standard audio track with 2352 bytes/block for user data and 2352 bytes/block for raw data.
        /// </summary>
        Audio,
        /// <summary>
        /// Stadard data track with 2048 bytes/block for user data and 2352 bytes/block for raw data.
        /// </summary>
        Mode1
    }
}
