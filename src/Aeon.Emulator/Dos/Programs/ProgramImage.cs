using Aeon.Emulator.Dos.VirtualFileSystem;

namespace Aeon.Emulator.Dos.Programs;

/// <summary>
/// Represents an executable file image to load into a virtual machine.
/// </summary>
public abstract class ProgramImage
{
    private protected ProgramImage(VirtualPath path, Stream stream)
    {
        this.FullPath = path;
        Read(stream);
    }

    /// <summary>
    /// Gets the file name of the program image with no path or extension.
    /// </summary>
    public string FileName
    {
        get
        {
            var fileName = this.FullPath.LastElement;
            int dotIndex = fileName.IndexOf('.');
            if (dotIndex > 0)
                return fileName[..dotIndex];
            else
                return fileName;
        }
    }
    /// <summary>
    /// Gets the full path to the program in the virtual file system.
    /// </summary>
    public VirtualPath FullPath { get; }

    /// <summary>
    /// Gets the maximum number of paragraphs to allocate when the program is loaded.
    /// </summary>
    internal abstract ushort MaximumParagraphs { get; }

    /// <summary>
    /// Loads the program image into a virtual machine's address space and sets
    /// any processor registers required to run the image.
    /// </summary>
    /// <param name="vm">Virtual machine to load the image into.</param>
    /// <param name="dataSegment">Default data segment for the program and the location of its PSP.</param>
    internal abstract void Load(VirtualMachine vm, ushort dataSegment);
    /// <summary>
    /// Loads the program image as an overlay at a specified address.
    /// </summary>
    /// <param name="vm">Virtual machine to load the image into.</param>
    /// <param name="overlaySegment">Segment to load the overlay.</param>
    /// <param name="relocationFactor">Value to add to relocation segments.</param>
    internal abstract void LoadOverlay(VirtualMachine vm, ushort overlaySegment, int relocationFactor);
    /// <summary>
    /// Reads the program file from a stream.
    /// </summary>
    /// <param name="stream">Stream from which image is read.</param>
    internal abstract void Read(Stream stream);

    /// <summary>
    /// Reads an executable file image into memory.
    /// </summary>
    /// <param name="fileName">Full path to file to load in the firtual file system.</param>
    /// <param name="vm">Virtual machine instance which is loading the file.</param>
    /// <returns>Loaded program image.</returns>
    public static ProgramImage Load(string fileName, VirtualMachine vm)
    {
        ArgumentNullException.ThrowIfNull(fileName);
        ArgumentNullException.ThrowIfNull(vm);

        var fullPath = new VirtualPath(fileName);
        if (fullPath.PathType == VirtualPathType.Relative)
            fullPath = VirtualPath.Combine(vm.FileSystem.WorkingDirectory, fullPath);

        int signature;

        using var fileStream = vm.FileSystem.OpenFile(fullPath, FileMode.Open, FileAccess.Read).Result
            ?? throw new FileNotFoundException("Executable file not found.", fileName);

        signature = fileStream.ReadByte();
        signature |= fileStream.ReadByte() << 8;

        fileStream.Position = 0;
        if (signature == 0x5A4D)
            return new ExeFile(fullPath, fileStream);
        else
            return new ComFile(fullPath, fileStream);
    }
}
