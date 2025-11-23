namespace Aeon.Emulator.CommandInterpreter;

public sealed class DirectoryCommand(string path, DirectoryOptions options = default, IEnumerable<DirectoryAttributeFilter>? attributeFilter = null, IEnumerable<DirectorySort>? sortBy = null) : CommandStatement
{
    private readonly DirectoryAttributeFilter[] attributeFilter = attributeFilter?.ToArray() ?? [];
    private readonly DirectorySort[] sortBy = sortBy?.ToArray() ?? [];

    public string Path { get; } = path;
    public DirectoryOptions Options { get; } = options;
    public ReadOnlySpan<DirectoryAttributeFilter> AttributeFilter => this.attributeFilter;
    public ReadOnlySpan<DirectorySort> SortBy => this.sortBy;

    internal override CommandResult Run(CommandProcessor processor) => processor.RunCommand(this);
}

[Flags]
public enum DirectoryOptions
{
    None = 0,
    Pause = 1 << 0,
    Wide = 1 << 1,
    Recursive = 1 << 2,
    Bare = 1 << 3,
    Lowercase = 1 << 4,
    Verbose = 1 << 5
}

public readonly struct DirectoryAttributeFilter(FileAttributes attribute, bool include)
{
    public FileAttributes Attribute { get; } = attribute;
    public bool Include { get; } = include;
}

public enum DirectorySortKey
{
    Name,
    Size,
    Extension,
    Date,
    GroupDirectoriesFirst
}

public readonly struct DirectorySort(DirectorySortKey key, bool descending)
{
    public DirectorySortKey Key { get; } = key;
    public bool Descending { get; } = descending;
}
