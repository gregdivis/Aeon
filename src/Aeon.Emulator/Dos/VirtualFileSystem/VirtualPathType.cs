namespace Aeon.Emulator.Dos.VirtualFileSystem
{
    /// <summary>
    /// Specifies the type of virtual path represented.
    /// </summary>
    public enum VirtualPathType
    {
        /// <summary>
        /// The path is absolute and rooted at a virtual drive.
        /// </summary>
        Absolute,
        /// <summary>
        /// The path is relative.
        /// </summary>
        Relative
    }
}
