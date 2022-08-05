using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Aeon.Emulator.CommandInterpreter
{
    public sealed class DirectoryCommand : CommandStatement
    {
        private readonly DirectoryAttributeFilter[] attributeFilter;
        private readonly DirectorySort[] sortBy;

        public DirectoryCommand(string path, DirectoryOptions options = default, IEnumerable<DirectoryAttributeFilter>? attributeFilter = null, IEnumerable<DirectorySort>? sortBy = null)
        {
            this.Path = path;
            this.Options = options;
            this.attributeFilter = attributeFilter?.ToArray() ?? Array.Empty<DirectoryAttributeFilter>();
            this.sortBy = sortBy?.ToArray() ?? Array.Empty<DirectorySort>();
        }

        public string Path { get; }
        public DirectoryOptions Options { get; }
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

    public readonly struct DirectoryAttributeFilter
    {
        public DirectoryAttributeFilter(FileAttributes attribute, bool include)
        {
            this.Attribute = attribute;
            this.Include = include;
        }

        public FileAttributes Attribute { get; }
        public bool Include { get; }
    }

    public enum DirectorySortKey
    {
        Name,
        Size,
        Extension,
        Date,
        GroupDirectoriesFirst
    }

    public readonly struct DirectorySort
    {
        public DirectorySort(DirectorySortKey key, bool descending)
        {
            this.Key = key;
            this.Descending = descending;
        }

        public DirectorySortKey Key { get; }
        public bool Descending { get; }
    }
}
