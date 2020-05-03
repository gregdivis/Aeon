using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Aeon.Emulator.CommandInterpreter
{
    /// <summary>
    /// Abstract base class for DOS command implementations.
    /// </summary>
    public abstract class Command
    {
        private static readonly Regex variableRegex = new Regex(@"%\w+%", RegexOptions.Compiled);
        private static readonly Regex redirectRegex = new Regex(@">\s*(?<1>\S+)", RegexOptions.Compiled | RegexOptions.ExplicitCapture);

        /// <summary>
        /// Initializes a new instance of the <see cref="Command"/> class.
        /// </summary>
        protected Command()
        {
        }

        /// <summary>
        /// Gets a value indicating whether the command has been successfully parsed.
        /// </summary>
        public bool IsParsed { get; private set; }
        /// <summary>
        /// Gets the name of the file or device to redirect STDOUT to.
        /// </summary>
        public string RedirectOutputTarget { get; private set; }

        /// <summary>
        /// Attempts to parse the command's arguments.
        /// </summary>
        /// <param name="arguments">Arguments to parse.</param>
        /// <remarks>If the parsing was successful, the IsParsed property is set to true.</remarks>
        public void Parse(string arguments)
        {
            if (arguments == null)
                throw new ArgumentNullException(nameof(arguments));

            var args = arguments;

            var m = redirectRegex.Match(arguments);
            if (m.Success)
            {
                this.RedirectOutputTarget = m.Groups[1].Value;
                args = arguments.Remove(m.Index, m.Length);
            }

            this.IsParsed = ParseArguments(args);
        }
        /// <summary>
        /// Attempts to run the command.
        /// </summary>
        /// <param name="vm">Current VirtualMachine instance.</param>
        /// <returns>Result of the command.</returns>
        public virtual CommandResult Run(VirtualMachine vm) => CommandResult.Continue;

        /// <summary>
        /// Replaces variables with values in a string.
        /// </summary>
        /// <param name="s">String in which to replace variables.</param>
        /// <param name="variables">Variables and values used for substitutions.</param>
        /// <returns>String with expanded variable values.</returns>
        protected static string SubstituteVariables(string s, IDictionary<string, string> variables)
        {
            if (s == null)
                throw new ArgumentNullException(nameof(s));
            if (variables == null)
                throw new ArgumentNullException(nameof(variables));

            return variableRegex.Replace(s, m => variables.TryGetValue(m.Value[1..^1], out string value) ? value : m.Value);
        }

        /// <summary>
        /// Attempts to parse the command's arguments.
        /// </summary>
        /// <param name="arguments">Arguments to parse.</param>
        /// <returns>Value indicating whether the parsing was successful.</returns>
        protected virtual bool ParseArguments(string arguments) => true;
    }
}
