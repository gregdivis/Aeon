namespace Aeon.Emulator.Dos;

/// <summary>
/// A reference-counted stream.
/// </summary>
internal sealed class FileHandle(Stream stream)
{
    private readonly Stream stream = stream;
    private int referenceCount = 1;

    /// <summary>
    /// Gets the stream represented by the handle.
    /// </summary>
    public Stream Stream => this.stream;
    /// <summary>
    /// Gets or sets the SFT index which refers to this file.
    /// </summary>
    public int SFTIndex { get; set; } = -1;
    /// <summary>
    /// Gets or sets additional information about the file.
    /// </summary>
    public VirtualFileInfo? FileInfo { get; set; }

    /// <summary>
    /// Adds a reference to the handle.
    /// </summary>
    public void AddReference() => this.referenceCount++;
    /// <summary>
    /// Decrements the handle's reference count.
    /// </summary>
    /// <returns>True if stream has been closed; otherwise false.</returns>
    public bool Close()
    {
        if (this.referenceCount > 1)
        {
            this.referenceCount--;
            return false;
        }
        else if (this.referenceCount == 1)
        {
            this.referenceCount = 0;
            this.stream.Dispose();
        }

        return true;
    }
}
