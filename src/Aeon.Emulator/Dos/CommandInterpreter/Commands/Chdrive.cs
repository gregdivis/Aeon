using System;
using Aeon.Emulator.Dos.VirtualFileSystem;

namespace Aeon.Emulator.CommandInterpreter.Commands
{
    /// <summary>
    /// The change drive command.
    /// </summary>
    public sealed class Chdrive : Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Chdrive"/> class.
        /// </summary>
        public Chdrive()
        {
        }

        /// <summary>
        /// Gets the drive letter to change to.
        /// </summary>
        public DriveLetter DriveLetter { get; private set; }

        /// <summary>
        /// Attempts to run the command.
        /// </summary>
        /// <param name="vm">Current VirtualMachine instance.</param>
        /// <returns>Result of the command.</returns>
        public override CommandResult Run(VirtualMachine vm)
        {
            if (vm == null)
                throw new ArgumentNullException(nameof(vm));

            var drive = vm.FileSystem.Drives[this.DriveLetter];
            if (drive.DriveType != DriveType.None)
                vm.FileSystem.CurrentDrive = this.DriveLetter;
            else
                vm.Console.WriteLine($"Drive {this.DriveLetter} not ready");

            return CommandResult.Continue;
        }

        /// <summary>
        /// Attempts to parse the command's arguments.
        /// </summary>
        /// <param name="arguments">Arguments to parse.</param>
        /// <returns>Value indicating whether the parsing was successful.</returns>
        protected override bool ParseArguments(string arguments)
        {
            if (!string.IsNullOrEmpty(arguments) && arguments.Length == 2 && arguments[1] == ':')
            {
                if ((arguments[0] >= 'A' && arguments[0] <= 'Z') || (arguments[0] >= 'a' && arguments[0] <= 'z'))
                {
                    this.DriveLetter = new DriveLetter(arguments[0]);
                    return true;
                }
            }

            return false;
        }
    }
}
