using System;

namespace Aeon.Emulator.CommandInterpreter
{
    /// <summary>
    /// Provides information about a DOS command.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class CommandAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandAttribute"/> class.
        /// </summary>
        /// <param name="name">Name of the command in DOS.</param>
        public CommandAttribute(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));

            this.Name = name;
        }

        /// <summary>
        /// Gets the DOS command name.
        /// </summary>
        public string Name { get; }
    }
}
