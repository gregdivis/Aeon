using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Aeon.Emulator.Dos.VirtualFileSystem
{
    /// <summary>
    /// Represents a path in the virtual file system.
    /// </summary>
    public sealed class VirtualPath : IEquatable<VirtualPath>
    {
        /// <summary>
        /// A path which specifies the current directory.
        /// </summary>
        public static readonly VirtualPath RelativeCurrent = new();
        /// <summary>
        /// A path which specifies the parent directory.
        /// </summary>
        public static readonly VirtualPath RelativeParent = new("..");
        /// <summary>
        /// A path which specifies the root directory of the current drive.
        /// </summary>
        public static readonly VirtualPath AbsoluteRoot = new("\\");

        private readonly string[] pathElements;
        private readonly DriveLetter? driveLetter;
        private readonly VirtualPathType pathType;
        private readonly Lazy<string> cachedPath;
        private readonly Lazy<string> cachedFullPath;

        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualPath"/> class.
        /// </summary>
        public VirtualPath()
        {
            this.pathElements = Array.Empty<string>();
            this.pathType = VirtualPathType.Relative;
            this.cachedPath = new Lazy<string>(() => string.Join("\\", this.pathElements), true);
            this.cachedFullPath = new Lazy<string>(this.BuildFullPath, true);
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualPath"/> class.
        /// </summary>
        /// <param name="path">String representation of an absolute or relative path.</param>
        public VirtualPath(string path) : this()
        {
            ArgumentNullException.ThrowIfNull(path);

            var instance = Parse(path, out var error);
            if (instance == null)
                throw error!;

            this.driveLetter = instance.driveLetter;
            this.pathElements = instance.pathElements;
            this.pathType = instance.pathType;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualPath"/> class.
        /// </summary>
        /// <param name="elements">Path element array.</param>
        /// <param name="type">Type of the path.</param>
        /// <param name="drive">Drive letter of an absolute path.</param>
        private VirtualPath(string[] elements, VirtualPathType type, DriveLetter? drive)
            : this()
        {
            this.pathElements = elements.Length > 0 ? elements : Array.Empty<string>();
            this.pathType = type;
            this.driveLetter = drive;
        }

        public static bool operator ==(VirtualPath? pathA, VirtualPath? pathB)
        {
            if (pathA is not null)
                return pathA.Equals(pathB);
            else
                return pathB is null;
        }
        public static bool operator !=(VirtualPath? pathA, VirtualPath? pathB)
        {
            if (pathA is not null)
                return !pathA.Equals(pathB);
            else
                return pathB is not null;
        }
        public static VirtualPath operator +(VirtualPath pathA, VirtualPath pathB) => Combine(pathA, pathB);
        public static VirtualPath operator +(VirtualPath pathA, string pathB) => Combine(pathA, pathB);
        public static VirtualPath operator +(string pathA, VirtualPath pathB) => Combine(pathA, pathB);

        /// <summary>
        /// Gets the type of path represented.
        /// </summary>
        public VirtualPathType PathType => pathType;
        /// <summary>
        /// Gets the drive letter of the absolute path represented.
        /// </summary>
        public DriveLetter? DriveLetter => this.driveLetter;
        /// <summary>
        /// Gets a string representation of the full path.
        /// </summary>
        public string FullPath => this.cachedFullPath.Value;
        /// <summary>
        /// Gets a string representation of the path with no drive letter.
        /// </summary>
        public string Path => this.cachedPath.Value;
        /// <summary>
        /// Gets the first element in the virtual path.
        /// </summary>
        public string FirstElement
        {
            get
            {
                if (pathElements.Length > 0)
                    return pathElements[0];
                else
                    return string.Empty;
            }
        }
        /// <summary>
        /// Gets the last element in the virtual path.
        /// </summary>
        public string LastElement
        {
            get
            {
                if (pathElements.Length > 0)
                    return pathElements[^1];
                else
                    return string.Empty;
            }
        }
        /// <summary>
        /// Gets the list of elements in the virtual path.
        /// </summary>
        public ReadOnlyCollection<string> Elements => Array.AsReadOnly(pathElements);
        /// <summary>
        /// Gets the level of the path if it is absolute.
        /// </summary>
        public int AbsoluteLevel
        {
            get
            {
                if (this.PathType == VirtualPathType.Absolute)
                    return pathElements.Length;
                else
                    return -1;
            }
        }

        /// <summary>
        /// Tests for equality with another path.
        /// </summary>
        /// <param name="other">Other instance to test.</param>
        /// <returns>True if paths are the same; otherwise false.</returns>
        public bool Equals(VirtualPath? other)
        {
            if (other is null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            if (this.PathType == other.PathType)
            {
                if (this.PathType == VirtualPathType.Absolute && this.driveLetter != other.driveLetter)
                    return false;

                if (this.pathElements.Length == other.pathElements.Length)
                {
                    for (int i = 0; i < this.pathElements.Length; i++)
                    {
                        if (this.pathElements[i] != other.pathElements[i])
                            return false;
                    }

                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// Tests for equality with another object.
        /// </summary>
        /// <param name="obj">Other object to test.</param>
        /// <returns>True if objects are the same; otherwise false.</returns>
        public override bool Equals(object? obj) => this.Equals(obj as VirtualPath);
        /// <summary>
        /// Gets a hash code for the path.
        /// </summary>
        /// <returns>Hash code for the path.</returns>
        public override int GetHashCode() => this.FullPath.GetHashCode();
        /// <summary>
        /// Gets a string representation of the path.
        /// </summary>
        /// <returns>String representation of the path.</returns>
        public override string ToString() => this.FullPath;
        /// <summary>
        /// Appends a relative path to the end of this path.
        /// </summary>
        /// <param name="other">Relative path to append.</param>
        /// <returns>Result of this path appended with the other path.</returns>
        public VirtualPath Append(VirtualPath other)
        {
            ArgumentNullException.ThrowIfNull(other);
            if (other.PathType == VirtualPathType.Absolute)
                throw new ArgumentException("Cannot combine with an absolute path.");

            if (other == RelativeCurrent)
                return this;
            if (this == RelativeCurrent)
                return other;

            var newPath = new string[this.pathElements.Length + other.pathElements.Length];
            Array.Copy(this.pathElements, newPath, this.pathElements.Length);
            Array.Copy(other.pathElements, 0, newPath, this.pathElements.Length, other.pathElements.Length);

            var elements = Simplify(newPath, 0);
            if (this.PathType == VirtualPathType.Absolute && elements.Length > 0 && elements[0] == "..")
                throw new ArgumentException("Resulting absolute path would be invalid.", nameof(other));

            return new VirtualPath(elements, pathType, this.driveLetter);
        }
        /// <summary>
        /// Appends a relative path to the end of this path.
        /// </summary>
        /// <param name="other">Relative path to append.</param>
        /// <returns>Result of this path appended with the other path.</returns>
        public VirtualPath Append(string other)
        {
            ArgumentNullException.ThrowIfNull(other);
            return Append(new VirtualPath(other));
        }
        /// <summary>
        /// Gets the parent path of this path.
        /// </summary>
        /// <returns>Parent path of this path.</returns>
        public VirtualPath GetParent()
        {
            if (pathElements.Length > 0)
            {
                var newPath = new string[pathElements.Length - 1];
                Array.Copy(pathElements, newPath, newPath.Length);
                return new VirtualPath(newPath, this.PathType, driveLetter);
            }
            else if (this.PathType == VirtualPathType.Relative)
            {
                return RelativeParent;
            }
            else
            {
                return this;
            }
        }
        /// <summary>
        /// Builds a relative path based on two absolute paths.
        /// </summary>
        /// <param name="pathTo">Target absolute path.</param>
        /// <returns>Relative path from this path to the specified path.</returns>
        public VirtualPath GetRelativePath(VirtualPath pathTo)
        {
            ArgumentNullException.ThrowIfNull(pathTo);
            if (this.PathType != VirtualPathType.Absolute || pathTo.PathType != VirtualPathType.Absolute)
                throw new InvalidOperationException("Both paths must be absolute.");
            if (this.driveLetter != pathTo.driveLetter)
                throw new InvalidOperationException("Both paths must be rooted on the same drive.");

            int diffStartIndex = -1;
            for (int i = 0; i < pathElements.Length; i++)
            {
                if (pathElements[i] != pathTo.pathElements[i])
                {
                    diffStartIndex = i;
                    break;
                }
            }

            if (diffStartIndex == -1)
            {
                // pathTo is contained in this path.
                string[] newPath = new string[pathTo.pathElements.Length - this.pathElements.Length];
                for (int i = 0; i < newPath.Length; i++)
                    newPath[i] = pathTo.pathElements[i + this.pathElements.Length];

                return new VirtualPath(newPath, VirtualPathType.Relative, null);
            }
            else
            {
                List<string> buffer = new List<string>();
                int symbols = this.pathElements.Length - diffStartIndex;
                for (int i = 0; i < symbols; i++)
                    buffer.Add("..");
                for (int i = diffStartIndex; i < pathTo.pathElements.Length; i++)
                    buffer.Add(pathTo.pathElements[i]);

                return new VirtualPath(buffer.ToArray(), VirtualPathType.Relative, null);
            }
        }
        /// <summary>
        /// Returns the relative part of the path.
        /// </summary>
        /// <returns>Relative part of the path.</returns>
        public VirtualPath GetRelativePart()
        {
            if (this.PathType == VirtualPathType.Relative)
                return this;

            return new VirtualPath(this.pathElements, VirtualPathType.Relative, this.driveLetter);
        }
        /// <summary>
        /// Returns a new VirtualPath instance with a different drive assignment.
        /// </summary>
        /// <param name="newDrive">New drive letter of the path; null indicates no drive is specified.</param>
        /// <returns>VirtualPath instance with the new drive assignment.</returns>
        public VirtualPath ChangeDrive(DriveLetter? newDrive)
        {
            if (newDrive == this.driveLetter)
                return this;
            else
                return new VirtualPath(this.pathElements, this.pathType, newDrive);
        }
        /// <summary>
        /// Truncates each element of the path to DOS 8.3 standards.
        /// </summary>
        /// <returns>Truncated path.</returns>
        public VirtualPath Truncate()
        {
            bool needsWork = false;

            foreach (var e in this.pathElements)
            {
                if (!IsElementLegal(e))
                {
                    needsWork = true;
                    break;
                }
            }

            if (!needsWork)
                return this;

            var newElements = new string[this.pathElements.Length];

            for (int i = 0; i < newElements.Length; i++)
            {
                var s = this.pathElements[i];

                if (!IsElementLegal(s))
                {
                    // First remove extra extensions.
                    while (s.IndexOf('.') != s.LastIndexOf('.'))
                    {
                        s = s[..s.LastIndexOf('.')];
                    }

                    int dotPos = s.IndexOf('.');
                    if (dotPos >= 0)
                    {
                        if (dotPos < s.Length - 4)
                            s = s[..(s.Length - dotPos - 4)];

                        if (s.Length > 12)
                            s = string.Concat(s.AsSpan(0, 8), s.AsSpan(dotPos));

                        newElements[i] = s;
                    }
                    else
                    {
                        if (s.Length > 8)
                            s = s[..8];
                    }
                }

                newElements[i] = s;
            }

            return new VirtualPath(newElements, this.pathType, this.driveLetter);
        }

        /// <summary>
        /// Appends a relative path to another path.
        /// </summary>
        /// <param name="pathA">First part of path.</param>
        /// <param name="pathB">Second part of path; must be relative.</param>
        /// <returns>Combined path.</returns>
        public static VirtualPath Combine(VirtualPath pathA, VirtualPath pathB)
        {
            ArgumentNullException.ThrowIfNull(pathA);
            ArgumentNullException.ThrowIfNull(pathB);

            return pathA.Append(pathB);
        }
        /// <summary>
        /// Appends a relative path to another path.
        /// </summary>
        /// <param name="pathA">First part of path.</param>
        /// <param name="pathB">Second part of path; must be relative.</param>
        /// <returns>Combined path.</returns>
        public static VirtualPath Combine(VirtualPath pathA, string pathB)
        {
            ArgumentNullException.ThrowIfNull(pathA);
            ArgumentNullException.ThrowIfNull(pathB);

            return pathA.Append(pathB);
        }
        /// <summary>
        /// Appends a relative path to another path.
        /// </summary>
        /// <param name="pathA">First part of path.</param>
        /// <param name="pathB">Second part of path; must be relative.</param>
        /// <returns>Combined path.</returns>
        public static VirtualPath Combine(string pathA, VirtualPath pathB)
        {
            ArgumentNullException.ThrowIfNull(pathA);
            ArgumentNullException.ThrowIfNull(pathB);

            return new VirtualPath(pathA).Append(pathB);
        }
        /// <summary>
        /// Appends a relative path to a drive letter.
        /// </summary>
        /// <param name="driveLetter">Drive letter of new path.</param>
        /// <param name="path">Path on the drive; must be relative.</param>
        /// <returns>Absolute path.</returns>
        public static VirtualPath Combine(DriveLetter driveLetter, VirtualPath path)
        {
            ArgumentNullException.ThrowIfNull(path);
            return new VirtualPath(Array.Empty<string>(), VirtualPathType.Absolute, driveLetter).Append(path);
        }
        /// <summary>
        /// Attempts to parse a path string to a VirtualPath instance.
        /// </summary>
        /// <param name="path">Path string to parse.</param>
        /// <returns>VirtualPath instance if successful; otherwise null.</returns>
        public static VirtualPath? TryParse(ReadOnlySpan<char> path)
        {
            return Parse(path, out _);
        }

        private string BuildFullPath()
        {
            var path = this.Path;
            if (this.PathType == VirtualPathType.Absolute)
                path = "\\" + path;

            return driveLetter.ToString() + path;
        }

        private static VirtualPath? Parse(ReadOnlySpan<char> path, out Exception? error)
        {
            error = null;

            if (path.IsEmpty)
            {
                return RelativeCurrent;
            }
            else
            {
                string[] pathElements;
                VirtualPathType pathType;
                DriveLetter? driveLetter = null;

                if (path.Contains('/'))
                    path = path.ToString().Replace('/', '\\');

                if (path.Length >= 2 && path[1] == ':')
                {
                    if (!char.IsLetter(path[0]))
                    {
                        error = new ArgumentException("Invalid drive specifier.", nameof(path));
                        return null;
                    }

                    driveLetter = new DriveLetter(path[0]);
                    path = path[2..];
                }

                if (path.Length > 0 && (path[0] == '\\' || path[0] == '/'))
                    pathType = VirtualPathType.Absolute;
                else
                    pathType = VirtualPathType.Relative;

                var elements = path.ToString().Split(new char[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);
                pathElements = Simplify(elements, 0);

                return new VirtualPath(pathElements, pathType, driveLetter);
            }
        }
        private static string[] Simplify(string[] elements, int offset)
        {
            var result = new List<string>(elements.Length - offset);
            for (int i = offset; i < elements.Length; i++)
            {
                if (elements[i] == ".")
                {
                    // Skip it - current directory.
                }
                else if (elements[i] == "..")
                {
                    // Store parent directory symbol if necessary.
                    if (result.Count == 0 || result[^1] == "..")
                        result.Add("..");
                    else
                        result.RemoveAt(result.Count - 1);
                }
                else
                {
                    result.Add(CheckAndFixCasing(elements[i]));
                }
            }

            for (int i = 0; i < result.Count; i++)
            {
                var element = result[i];
                if (element.EndsWith(".") && element != "." && element != "..")
                    result[i] = element.TrimEnd('.');
            }

            return result.ToArray();
        }
        private static string CheckAndFixCasing(string element)
        {
            var result = element;
            foreach (char c in element)
            {
                if (char.IsLetter(c) && !char.IsUpper(c))
                {
                    result = element.ToUpper();
                    break;
                }
            }

            return result;
        }
        private static bool IsElementLegal(string element)
        {
            if (element == "." || element == "..")
                return true;

            if (element.Length > 12)
                return false;

            int first = element.IndexOf('.');
            if (first == -1 && element.Length > 8)
                return false;

            if (first != element.LastIndexOf('.'))
                return false;

            if (first != -1 && first < element.Length - 4)
                return false;

            return true;
        }
    }
}
