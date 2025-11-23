namespace Aeon.Emulator.Dos.VirtualFileSystem;

/// <summary>
/// Collection of drives on an emulated file system.
/// </summary>
public sealed class DriveList : IList<VirtualDrive>
{
    private readonly VirtualDrive[] drives = new VirtualDrive[26];

    internal DriveList()
    {
        for (int i = 0; i < this.drives.Length; i++)
            this.drives[i] = new VirtualDrive();
    }

    /// <summary>
    /// Gets the drive at the specified index.
    /// </summary>
    /// <param name="index">Index of drive to return.</param>
    /// <returns>Drive with the specified index.</returns>
    public VirtualDrive this[int index] => this.drives[index];
    /// <summary>
    /// Gets the drive with the specified letter.
    /// </summary>
    /// <param name="letter">Letter of drive to return.</param>
    /// <returns>Drive with the specified letter.</returns>
    public VirtualDrive this[DriveLetter letter] => this.drives[letter.Index];
    VirtualDrive IList<VirtualDrive>.this[int index]
    {
        get => this.drives[index];
        set => throw new NotSupportedException();
    }

    /// <summary>
    /// Gets the number of drives.
    /// </summary>
    public int Count => this.drives.Length;
    bool ICollection<VirtualDrive>.IsReadOnly => true;

    int IList<VirtualDrive>.IndexOf(VirtualDrive item) => Array.IndexOf(this.drives, item);
    void IList<VirtualDrive>.Insert(int index, VirtualDrive item) => throw new NotSupportedException();
    void IList<VirtualDrive>.RemoveAt(int index) => throw new NotSupportedException();
    void ICollection<VirtualDrive>.Add(VirtualDrive item) => throw new NotSupportedException();
    void ICollection<VirtualDrive>.Clear() => throw new NotSupportedException();
    bool ICollection<VirtualDrive>.Contains(VirtualDrive item) => Array.IndexOf(this.drives, item) >= 0;
    void ICollection<VirtualDrive>.CopyTo(VirtualDrive[] array, int arrayIndex) => this.drives.CopyTo(array, arrayIndex);
    bool ICollection<VirtualDrive>.Remove(VirtualDrive item) => throw new NotSupportedException();
    public IEnumerator<VirtualDrive> GetEnumerator() => Array.AsReadOnly(drives).GetEnumerator();
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => this.GetEnumerator();
}
