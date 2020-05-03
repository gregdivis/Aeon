using System;
using Aeon.Emulator.Dos.VirtualFileSystem;

namespace Aeon.Emulator.CommandInterpreter.Commands
{
    /// <summary>
    /// The change directory command.
    /// </summary>
    [Command("CD")]
    [Command("CHDIR")]
    public sealed class Chdir : Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Chdir"/> class.
        /// </summary>
        public Chdir()
        {
        }

        /// <summary>
        /// Gets the directory to change to.
        /// </summary>
        public VirtualPath Directory { get; private set; }

        /// <summary>
        /// Attempts to run the command.
        /// </summary>
        /// <param name="vm">Current VirtualMachine instance.</param>
        /// <returns>Result of the command.</returns>
        public override CommandResult Run(VirtualMachine vm)
        {
            if (vm == null)
                throw new ArgumentNullException(nameof(vm));

            var path = this.Directory;
            if (path != null)
            {
                try
                {
                    path = vm.FileSystem.ResolvePath(path);
                    if (vm.FileSystem.DirectoryExists(path))
                        vm.FileSystem.ChangeDirectory(path);
                    else
                        vm.Console.WriteLine("Directory not found.");
                }
                catch (ArgumentException)
                {
                    vm.Console.WriteLine("Invalid directory.");
                }
            }

            return CommandResult.Continue;
        }

        /// <summary>
        /// Attempts to parse the command's arguments.
        /// </summary>
        /// <param name="arguments">Arguments to parse.</param>
        /// <returns>Value indicating whether the parsing was successful.</returns>
        protected override bool ParseArguments(string arguments)
        {
            if (string.IsNullOrEmpty(arguments))
            {
                this.Directory = VirtualPath.RelativeCurrent;
            }
            else
            {
                var path = VirtualPath.TryParse(arguments);
                this.Directory = path;
                if (path == null)
                    return false;
            }

            return true;
        }
    }
}
