using System;
using System.IO;
using System.Text;
using Aeon.Emulator.Dos.Programs;
using Aeon.Emulator.Dos.VirtualFileSystem;

namespace Aeon.Emulator.CommandInterpreter
{
    internal sealed class CommandProcessor
    {
        private readonly VirtualMachine vm;

        public CommandProcessor(VirtualMachine vm)
        {
            this.vm = vm;
        }

        public CommandResult Run(string statement)
        {
            var command = StatementParser.Parse(statement);
            if (command == null)
            {
                vm.Console.WriteLine("Bad command or file name");
                return CommandResult.Continue;
            }

            return command.Run(this);
        }

#pragma warning disable IDE0060 // Remove unused parameter
        internal CommandResult RunCommand(CallCommand callCommand)
        {
            throw new NotImplementedException();
        }
        internal CommandResult RunCommand(PrintEnvironmentCommand printEnvironmentCommand)
        {
            throw new NotImplementedException();
        }
        internal CommandResult RunCommand(TypeCommand typeCommand)
        {
            var path = VirtualPath.TryParse(typeCommand.FileName);
            try
            {
                path = this.vm.FileSystem.ResolvePath(path);
            }
            catch (ArgumentException)
            {
                path = null;
            }

            if (path != null)
            {
                var fileInfo = this.vm.FileSystem.GetFileInfo(path).Result;
                if (fileInfo != null && !fileInfo.Attributes.HasFlag(VirtualFileAttributes.Directory))
                {
                    using (var stream = this.vm.FileSystem.OpenFile(path, FileMode.Open, FileAccess.Read).Result)
                    {
                        Span<byte> buffer = stackalloc byte[64];
                        Span<char> charBuffer = stackalloc char[64];
                        int bytesRead = stream.Read(buffer);
                        while (bytesRead > 0)
                        {
                            int charCount = Encoding.Latin1.GetChars(buffer.Slice(0, bytesRead), charBuffer);
                            this.vm.Console.Write(charBuffer.Slice(0, charCount));
                            bytesRead = stream.Read(buffer);
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
        internal CommandResult RunCommand(SetCurrentDriveCommand setCurrentDriveCommand)
        {
            var drive = this.vm.FileSystem.Drives[setCurrentDriveCommand.Drive];
            if (drive.DriveType != DriveType.None)
                this.vm.FileSystem.CurrentDrive = setCurrentDriveCommand.Drive;
            else
                this.vm.Console.WriteLine($"Drive {setCurrentDriveCommand.Drive} not ready");

            return CommandResult.Continue;
        }
        internal CommandResult RunCommand(SetCurrentDirectoryCommand setCurrentDirectoryCommand)
        {
            var fs = vm.FileSystem;
            var path = VirtualPath.TryParse(setCurrentDirectoryCommand.Path);
            if (path != null)
            {
                try
                {
                    path = fs.ResolvePath(path);
                    if (fs.DirectoryExists(path))
                        fs.ChangeDirectory(path);
                    else
                        this.vm.Console.WriteLine("Directory not found.");
                }
                catch (ArgumentException)
                {
                    this.vm.Console.WriteLine("Invalid directory.");
                }
            }

            return CommandResult.Continue;
        }
        internal CommandResult RunCommand(SetCommand setCommand)
        {
            throw new NotImplementedException();
        }
        internal CommandResult RunCommand(PrintCurrentDirectoryCommand printCurrentDirectoryCommand)
        {
            throw new NotImplementedException();
        }
        internal CommandResult RunCommand(LaunchCommand launchCommand)
        {
            var fs = this.vm.FileSystem;

            var target = VirtualPath.TryParse(launchCommand.Target);
            if (target != null)
            {
                var targetPath = target.ToString();
                if (!string.IsNullOrEmpty(targetPath))
                {
                    if (!Path.HasExtension(targetPath))
                    {
                        var newPath = targetPath + ".COM";
                        if (fs.FileExists(newPath))
                        {
                            targetPath = newPath;
                        }
                        else
                        {
                            newPath = targetPath + ".EXE";
                            if (fs.FileExists(newPath))
                                targetPath = newPath;
                            else
                                targetPath = null;
                        }
                    }
                    else
                    {
                        var extension = Path.GetExtension(targetPath);
                        if (!fs.FileExists(targetPath) || (!extension.Equals(".EXE", StringComparison.OrdinalIgnoreCase) && !extension.Equals(".COM", StringComparison.OrdinalIgnoreCase)))
                            targetPath = null;
                    }
                }

                if (!string.IsNullOrEmpty(targetPath))
                {
                    var program = ProgramImage.Load(targetPath, vm);
                    if (program != null)
                    {
                        vm.LoadImage(program, launchCommand.Arguments);
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
        internal CommandResult RunCommand(InvalidCommand invalidCommand)
        {
            throw new NotImplementedException();
        }
        internal CommandResult RunCommand(GotoCommand gotoCommand)
        {
            throw new NotImplementedException();
        }
        internal CommandResult RunCommand(ClsCommand clsCommand)
        {
            this.vm.Video?.TextConsole.Clear();
            return CommandResult.Continue;
        }
        internal CommandResult RunCommand(EchoCommand echoCommand)
        {
            this.vm.Console.WriteLine(echoCommand.Text);
            return CommandResult.Continue;
        }
        internal CommandResult RunCommand(IfErrorLevelCommand ifErrorLevelCommand)
        {
            throw new NotImplementedException();
        }
        internal CommandResult RunCommand(DirectoryCommand directoryCommand)
        {
            var path = VirtualPath.TryParse(directoryCommand.Path ?? ".");
            if (path == VirtualPath.RelativeCurrent)
                path = new VirtualPath("*.*");

            try
            {
                path = this.vm.FileSystem.ResolvePath(path);
            }
            catch (ArgumentException)
            {
                path = null;
            }

            if (path != null)
            {
                var data = this.vm.FileSystem.GetDirectory(path);
                if (data.Result == null)
                {
                    this.vm.Console.WriteLine("Directory not found.");
                    this.vm.Console.WriteLine();
                    return CommandResult.Continue;
                }

                var files = data.Result;

                this.vm.Console.WriteLine();
                this.vm.Console.WriteLine(" Directory of " + path.ToString());
                this.vm.Console.WriteLine();

                int count = 0;
                long size = 0;

                foreach (var file in files)
                {
                    var fileName = Path.GetFileNameWithoutExtension(file.Name);
                    var fileExtension = Path.GetExtension(file.Name).TrimStart('.');
                    var date = file.ModifyDate.ToString("MM/dd/yy  hh:mmt").ToLowerInvariant();

                    if ((file.Attributes & VirtualFileAttributes.Directory) != 0)
                        this.vm.Console.WriteLine($"{fileName,-8} {fileExtension,-3} <DIR>         {date}");
                    else
                        this.vm.Console.WriteLine($"{fileName,-8} {fileExtension,-3} {file.Length,13:#,#} {date}");

                    count++;
                    size += file.Length;
                }

                this.vm.Console.WriteLine($"{count,9:#,#} file(s) {size,14:#,#} bytes");
            }
            else
            {
                this.vm.Console.WriteLine("Invalid directory.");
            }

            this.vm.Console.WriteLine();

            return CommandResult.Continue;
        }
        internal CommandResult RunCommand(IfFileExistsCommand ifFileExistsCommand)
        {
            throw new NotImplementedException();
        }
        internal CommandResult RunCommand(IfEqualsCommand ifEqualsCommand)
        {
            throw new NotImplementedException();
        }
#pragma warning restore IDE0060 // Remove unused parameter
    }
}
