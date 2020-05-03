using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Aeon.Emulator
{
    /// <summary>
    /// Contains methods for validating and filtering DOS file paths.
    /// </summary>
    public static class VirtualDirectory
    {
        private static readonly Regex LegalNameRegex = new Regex("^" + NameExpression + "$", RegexOptions.CultureInvariant | RegexOptions.Compiled | RegexOptions.Singleline);

        private const string ValidCharacters = @"0-9a-zA-Z\!\@\#\$\%\^\&\(\)\-_\`\'\~\{\}";
        private const string NameExpression = "[" + ValidCharacters + "]{1,8}(\\.[" + ValidCharacters + "]{1,3})?";

        /// <summary>
        /// Returns a value indicating whether a name conforms to the DOS 8.3 file format.
        /// </summary>
        /// <param name="name">File system name to test.</param>
        /// <returns>True if name is a legal 8.3 DOS name; otherwise false.</returns>
        public static bool IsLegalDosName(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            return LegalNameRegex.IsMatch(name);
        }
        /// <summary>
        /// Returns a value indicating whether a path conforms to the DOS 8.3 file format.
        /// </summary>
        /// <param name="path">File system path to test.</param>
        /// <returns>True if path is a legal 8.3 DOS path; otherwise false.</returns>
        public static bool IsLegalDosPath(string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            if (path == string.Empty)
                return true;

            int firstElement = 0;
            string[] elements = path.Split('\\');
            if (elements[0].Length == 2 && elements[0][1] == ':')
            {
                if (char.IsLetter(elements[0][0]))
                    firstElement = 1;
                else
                    return false;
            }

            for (int i = firstElement; i < elements.Length; i++)
            {
                if (!IsLegalDosName(elements[i]))
                    return false;
            }

            return true;
        }
        /// <summary>
        /// Tests a file or directory name against a DOS wildcard filter.
        /// </summary>
        /// <param name="filter">DOS wildcard filter.</param>
        /// <param name="name">File system name to test.</param>
        /// <returns>True if name matches the filter; otherwise false.</returns>
        public static bool MatchesFilter(string filter, string name)
        {
            if (filter == null)
                throw new ArgumentNullException(nameof(filter));
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            if (name == "*.*")
                return true;

            Regex filterRegex = BuildFilterRegex(filter);
            return filterRegex.IsMatch(name);
        }
        /// <summary>
        /// Filters a set of file system names using DOS wild cards.
        /// </summary>
        /// <typeparam name="T">Type of element to filter.</typeparam>
        /// <param name="filter">DOS wildcard filter.</param>
        /// <param name="source">Collection of items to filter.</param>
        /// <param name="getName">Delegate invoked on each item to return its name.</param>
        /// <returns>Filtered collection of items.</returns>
        public static IEnumerable<T> ApplyFilter<T>(string filter, IEnumerable<T> source, Func<T, string> getName)
        {
            if (filter == null)
                throw new ArgumentNullException(nameof(filter));
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (getName == null)
                throw new ArgumentNullException(nameof(getName));

            if (filter == "*.*" || filter == "*")
                return source;

            var filterRegex = BuildFilterRegex(filter);

            return from item in source
                   where filterRegex.IsMatch(getName(item))
                   select item;
        }

        private static Regex BuildFilterRegex(string filter)
        {
            var sb = new StringBuilder();
            sb.Append('^');
            foreach (char c in filter)
            {
                if (c == '*')
                    sb.Append(@"[^\.]*");
                else if (c == '?')
                    sb.Append(@"[^\.]");
                else
                    sb.Append(c);
            }

            sb.Append('$');

            return new Regex(sb.ToString(), RegexOptions.Singleline);
        }
    }
}
