namespace Aeon.Emulator.Dos.VirtualFileSystem
{
    /// <summary>
    /// Describes the index of a CD track.
    /// </summary>
    /// <param name="Number">The index number.</param>
    /// <param name="Position">The index position.</param>
    public readonly record struct TrackIndex(int Number, CDTimeSpan Position);
}
