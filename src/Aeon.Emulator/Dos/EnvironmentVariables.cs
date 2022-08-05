using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Aeon.Emulator
{
    /// <summary>
    /// Provides access to DOS environment variables as a dictionary.
    /// </summary>
    public sealed class EnvironmentVariables : IDictionary<string, string>
    {
        private readonly Dictionary<string, string> variables = new(StringComparer.OrdinalIgnoreCase);

        internal EnvironmentVariables()
        {
        }

        /// <summary>
        /// Generates a null-separated block of environment strings.
        /// </summary>
        /// <returns>Null-separated block of environment strings.</returns>
        internal byte[] GetEnvironmentBlock()
        {
            var sb = new StringBuilder();
            foreach (var pair in variables)
            {
                sb.Append(pair.Key);
                sb.Append('=');
                sb.Append(pair.Value);
                sb.Append('\0');
            }
            sb.Append('\0');

            return Encoding.ASCII.GetBytes(sb.ToString());
        }

        public void Add(string key, string value) => this.variables.Add(key, value);
        public bool ContainsKey(string key) => this.variables.ContainsKey(key);
        public ICollection<string> Keys => this.variables.Keys;
        public bool Remove(string key) => this.variables.Remove(key);
        public bool TryGetValue(string key, [NotNullWhen(true)] out string? value) => this.variables.TryGetValue(key, out value);
        public ICollection<string> Values => this.variables.Values;
        public string this[string key]
        {
            get => this.variables[key];
            set => this.variables[key] = value;
        }

        void ICollection<KeyValuePair<string, string>>.Add(KeyValuePair<string, string> item) => ((ICollection<KeyValuePair<string, string>>)this.variables).Add(item);
        public void Clear() => this.variables.Clear();
        bool ICollection<KeyValuePair<string, string>>.Contains(KeyValuePair<string, string> item) => ((ICollection<KeyValuePair<string, string>>)this.variables).Contains(item);
        void ICollection<KeyValuePair<string, string>>.CopyTo(KeyValuePair<string, string>[] array, int arrayIndex) => ((ICollection<KeyValuePair<string, string>>)this.variables).CopyTo(array, arrayIndex);
        public int Count => this.variables.Count;
        bool ICollection<KeyValuePair<string, string>>.IsReadOnly => false;
        bool ICollection<KeyValuePair<string, string>>.Remove(KeyValuePair<string, string> item) => ((ICollection<KeyValuePair<string, string>>)this.variables).Remove(item);

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator() => this.variables.GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
