using System;
using System.Text;
using Aeon.Emulator.Dos.VirtualFileSystem;

namespace Aeon.Emulator.CommandInterpreter.Commands
{
    /// <summary>
    /// The type command.
    /// </summary>
    [Command("TYPE")]
    public sealed class Type : Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Type"/> class.
        /// </summary>
        public Type()
        {
        }

        /// <summary>
        /// Gets the path of the file to display.
        /// </summary>
        public VirtualPath Target { get; private set; }

        /// <summary>
        /// Attempts to run the command.
        /// </summary>
        /// <param name="vm">Current VirtualMachine instance.</param>
        /// <returns>Result of the command.</returns>
        public override CommandResult Run(VirtualMachine vm)
        {
            var path = this.Target ?? VirtualPath.RelativeCurrent;
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
                var fileInfo = vm.FileSystem.GetFileInfo(path).Result;
                if (fileInfo != null && (fileInfo.Attributes & VirtualFileAttributes.Directory) == 0)
                {
                    using (var stream = vm.FileSystem.OpenFile(path, System.IO.FileMode.Open, System.IO.FileAccess.Read).Result)
                    {
                        byte[] buffer = new byte[64];
                        int bytesRead = stream.Read(buffer, 0, buffer.Length);
                        while (bytesRead > 0)
                        {
                            string s = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                            vm.Console.Write(s);
                            bytesRead = stream.Read(buffer, 0, buffer.Length);
                        }
                    }

                    vm.Console.WriteLine();

                    return CommandResult.Continue;
                }
            }

            vm.Console.WriteLine("File not found.");
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
                return false;

            var path = VirtualPath.TryParse(arguments);
            if (path == null)
                return false;

            this.Target = path;
            return true;
        }
    }
}
