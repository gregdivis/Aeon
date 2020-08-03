using System;
using System.IO;
using Aeon.Emulator.Dos.Programs;
using Aeon.Emulator.Dos.VirtualFileSystem;

namespace Aeon.Emulator.CommandInterpreter.Commands
{
    /// <summary>
    /// Launches an application. This is the default command.
    /// </summary>
    public sealed class Launch : Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Launch"/> class.
        /// </summary>
        public Launch()
        {
        }

        /// <summary>
        /// Gets the path to the program to launch.
        /// </summary>
        public VirtualPath Target { get; private set; }
        /// <summary>
        /// Gets the program arguments.
        /// </summary>
        public string Arguments { get; private set; }

        /// <summary>
        /// Attempts to run the command.
        /// </summary>
        /// <param name="vm">Current VirtualMachine instance.</param>
        /// <returns>Result of the command.</returns>
        public override CommandResult Run(VirtualMachine vm)
        {
            var target = this.Target;
            if (target != null)
            {
                string targetPath = target.ToString();
                if (!string.IsNullOrEmpty(targetPath))
                {
                    if (!Path.HasExtension(targetPath))
                    {
                        string newPath = targetPath + ".COM";
                        if (FileExists(newPath, vm))
                        {
                            targetPath = newPath;
                        }
                        else
                        {
                            newPath = targetPath + ".EXE";
                            if (FileExists(newPath, vm))
                                targetPath = newPath;
                            else
                                targetPath = null;
                        }
                    }
                    else
                    {
                        string extension = Path.GetExtension(targetPath);
                        if (!FileExists(targetPath, vm) || (!extension.Equals(".EXE", StringComparison.OrdinalIgnoreCase) && !extension.Equals(".COM", StringComparison.OrdinalIgnoreCase)))
                            targetPath = null;
                    }
                }

                if (!string.IsNullOrEmpty(targetPath))
                {
                    var program = ProgramImage.Load(targetPath, vm);
                    if (program != null)
                    {
                        vm.LoadImage(program, this.Arguments, this.RedirectOutputTarget);
                        return CommandResult.Launch;
                    }
                }
                else
                {
                    vm.Console.WriteLine("Bad command or file name");
                    vm.Console.WriteLine();
                    return CommandResult.Continue;
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
                return false;

            string target = arguments;

            int targetLength = arguments.IndexOf(' ');
            if (targetLength >= 0)
            {
                target = arguments.Substring(0, targetLength);
                arguments = arguments.Substring(targetLength);
            }
            else
            {
                arguments = string.Empty;
            }

            var targetPath = VirtualPath.TryParse(target);
            if (targetPath == null)
                return false;

            this.Target = targetPath;
            this.Arguments = arguments;

            return true;
        }

        /// <summary>
        /// Returns a value indicating whether a file exists.
        /// </summary>
        /// <param name="path">Path to file.</param>
        /// <param name="vm">Current VirtualMachine instance.</param>
        /// <returns>Value indicating whether the file exists.</returns>
        private static bool FileExists(string path, VirtualMachine vm)
        {
            var fileInfo = vm.FileSystem.GetFileInfo(path);
            return fileInfo.Result != null && !fileInfo.Result.Attributes.HasFlag(VirtualFileAttributes.Directory);
        }
    }
}
