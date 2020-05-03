using System;
using System.Collections.Generic;
using System.IO;
using Aeon.Emulator.Dos.VirtualFileSystem;

namespace Aeon.Emulator.CommandInterpreter.Commands
{
    /// <summary>
    /// The show directory command.
    /// </summary>
    [Command("DIR")]
    public sealed class Dir : Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Dir"/> class.
        /// </summary>
        public Dir() => this.Path = VirtualPath.RelativeCurrent;

        /// <summary>
        /// Gets the target directory.
        /// </summary>
        public VirtualPath Path { get; private set; }

        /// <summary>
        /// Attempts to run the command.
        /// </summary>
        /// <param name="vm">Current VirtualMachine instance.</param>
        /// <returns>Result of the command.</returns>
        public override CommandResult Run(VirtualMachine vm)
        {
            var path = this.Path ?? VirtualPath.RelativeCurrent;
            if (path == VirtualPath.RelativeCurrent)
                path = new VirtualPath("*.*");

            try
            {
                path = vm.FileSystem.ResolvePath(path);
            }
            catch (ArgumentException)
            {
                path = null;
            }

            if (path != null)
            {
                var data = vm.FileSystem.GetDirectory(path);
                if (data.Result == null)
                {
                    vm.Console.WriteLine("Directory not found.");
                    vm.Console.WriteLine();
                    return CommandResult.Continue;
                }

                var files = data.Result;

                vm.Console.WriteLine();
                vm.Console.WriteLine(" Directory of {0}", path);
                vm.Console.WriteLine();

                int count = 0;
                long size = 0;

                foreach (var file in files)
                {
                    string fileName = System.IO.Path.GetFileNameWithoutExtension(file.Name);
                    string fileExtension = System.IO.Path.GetExtension(file.Name).TrimStart('.');
                    string date = file.ModifyDate.ToString("MM/dd/yy  hh:mmt").ToLowerInvariant();

                    if ((file.Attributes & VirtualFileAttributes.Directory) != 0)
                        vm.Console.WriteLine("{0,-8} {1,-3} <DIR>         {2}", fileName, fileExtension, date);
                    else
                        vm.Console.WriteLine("{0,-8} {1,-3} {2,13:#,#} {3}", fileName, fileExtension, file.Length, date);

                    count++;
                    size += file.Length;
                }

                vm.Console.WriteLine("{0,9:#,#} file(s) {1,14:#,#} bytes", count, size);
            }
            else
            {
                vm.Console.WriteLine("Invalid directory.");
            }

            vm.Console.WriteLine();

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
                this.Path = VirtualPath.RelativeCurrent;
                return true;
            }
            else
            {
                if (arguments.Length > 1 && arguments[0] == '.' && arguments[1] != '.')
                    arguments = "*" + arguments;

                var path = VirtualPath.TryParse(arguments);
                if (path != null)
                {
                    this.Path = path;
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}
