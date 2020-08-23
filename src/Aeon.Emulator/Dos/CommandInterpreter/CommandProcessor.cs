using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Aeon.Emulator.Dos.Programs;
using Aeon.Emulator.Dos.VirtualFileSystem;

namespace Aeon.Emulator.CommandInterpreter
{
    internal sealed class CommandProcessor
    {
        private readonly VirtualMachine vm;
        private readonly Stack<BatchInstance> batchInstances = new Stack<BatchInstance>();
        private bool echoCommand = true;

        public CommandProcessor(VirtualMachine vm)
        {
            this.vm = vm;
        }

        public bool HasBatch => this.batchInstances.Count > 0;

        private BatchInstance CurrentBatch => this.batchInstances.TryPeek(out var b) ? b : null;

        public CommandResult RunNextBatchStatement()
        {
        Start:
            var b = this.CurrentBatch;
            if (b == null)
                return CommandResult.Continue;

            if (b.CurrentLine >= b.Batch.Statements.Length)
            {
                this.batchInstances.Pop();
                goto Start;
            }

            var statement = b.Batch.Statements[b.CurrentLine];
            if (this.echoCommand && !statement.NoEcho)
                vm.Console.WriteLine(statement.ToString());

            b.CurrentLine++;
            return statement.Run(this);
        }

        public bool BeginBatch(ReadOnlySpan<char> batchFileName, ReadOnlySpan<char> args)
        {
            if (!this.LoadBatchFile(batchFileName, args, out var batchInstance))
                return false;

            this.batchInstances.Clear();
            this.batchInstances.Push(batchInstance);
            return true;
        }

        private bool LoadBatchFile(ReadOnlySpan<char> batchFileName, ReadOnlySpan<char> args, out BatchInstance batchInstance)
        {
            batchInstance = null;

            var batchFilePath = VirtualPath.TryParse(batchFileName);
            if (batchFilePath == null)
                return false;

            BatchFile batch;
            var result = vm.FileSystem.OpenFile(batchFilePath, FileMode.Open, FileAccess.Read);
            try
            {
                if (result.Result == null)
                    return false;

                batch = BatchFile.Load(result.Result);
            }
            finally
            {
                result.Result?.Dispose();
            }

            batchInstance = new BatchInstance(batch, args.ToString());
            return true;
        }

        public CommandResult Run(ReadOnlySpan<char> statement)
        {
            bool inBatch = this.HasBatch;

            var command = StatementParser.Parse(statement);
            if (command == null)
            {
                vm.Console.WriteLine("Bad command or file name");
                return CommandResult.Continue;
            }

            var result = command.Run(this);
            if (!inBatch && this.HasBatch)
            {
                // if entering a batch script from interactive mode, repeat the command execution state
                this.vm.Processor.EIP -= 3;
            }

            return result;
        }

#pragma warning disable IDE0060 // Remove unused parameter
        internal CommandResult RunCommand(CallCommand callCommand)
        {
            var batchFileName = callCommand.Target;
            if (!batchFileName.EndsWith(".BAT", StringComparison.OrdinalIgnoreCase))
                batchFileName += ".BAT";

            if (!this.LoadBatchFile(batchFileName, callCommand.Arguments, out var batchInstance))
            {
                this.vm.Console.WriteLine($"Batch file {batchFileName} not found.");
                return CommandResult.Continue;
            }

            this.batchInstances.Push(batchInstance);
            return CommandResult.Continue;
        }
        internal CommandResult RunCommand(PrintEnvironmentCommand printEnvironmentCommand)
        {
            foreach (var var in this.vm.EnvironmentVariables)
                this.vm.Console.WriteLine($"{var.Key}={var.Value}");

            return CommandResult.Continue;
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
            this.vm.EnvironmentVariables[setCommand.Variable] = setCommand.Value ?? string.Empty;
            return CommandResult.Continue;
        }
        internal CommandResult RunCommand(PrintCurrentDirectoryCommand printCurrentDirectoryCommand)
        {
            this.vm.Console.WriteLine(this.vm.FileSystem.WorkingDirectory.ToString());
            return CommandResult.Continue;
        }
        internal CommandResult RunCommand(LaunchCommand launchCommand)
        {
            var fs = this.vm.FileSystem;

            var target = VirtualPath.TryParse(this.ReplaceVariables(launchCommand.Target));
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
                            {
                                targetPath = newPath;
                            }
                            else
                            {
                                newPath = targetPath + ".BAT";
                                if (fs.FileExists(newPath))
                                    targetPath = newPath;
                                else
                                    targetPath = null;
                            }
                        }
                    }
                    else
                    {
                        var extension = Path.GetExtension(targetPath);
                        if (!fs.FileExists(targetPath) || (!extension.Equals(".EXE", StringComparison.OrdinalIgnoreCase) && !extension.Equals(".COM", StringComparison.OrdinalIgnoreCase) && !extension.Equals(".BAT", StringComparison.OrdinalIgnoreCase)))
                            targetPath = null;
                    }
                }

                if (!string.IsNullOrEmpty(targetPath))
                {
                    if (targetPath.EndsWith(".BAT", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!this.BeginBatch(targetPath, this.ReplaceVariables(launchCommand.Arguments)))
                            vm.Console.WriteLine("Invalid batch script.");

                        return CommandResult.Continue;
                    }
                    else
                    {
                        var program = ProgramImage.Load(targetPath, vm);
                        if (program != null)
                        {
                            // decrement EIP here so that when launched process returns, interpreter runs again
                            if (this.HasBatch)
                                this.vm.Processor.EIP -= 3;

                            this.vm.LoadImage(program, this.ReplaceVariables(launchCommand.Arguments));
                            return CommandResult.Launch;
                        }
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
            this.vm.Console.WriteLine(invalidCommand.Error);
            return CommandResult.Continue;
        }
        internal CommandResult RunCommand(GotoCommand gotoCommand)
        {
            var b = this.CurrentBatch;
            if (b != null)
            {
                int line = 0;
                foreach (var statement in b.Batch.Statements)
                {
                    if (statement is LabelStatement label && label.Name == gotoCommand.Label)
                    {
                        b.CurrentLine = line;
                        return CommandResult.Continue;
                    }

                    line++;
                }

                this.vm.Console.WriteLine("Cannot find batch label specified - " + gotoCommand.Label);
            }

            return CommandResult.Continue;
        }
        internal CommandResult RunCommand(ClsCommand clsCommand)
        {
            this.vm.Video?.TextConsole.Clear();
            return CommandResult.Continue;
        }
        internal CommandResult RunCommand(EchoCommand echoCommand)
        {
            if (string.Equals(echoCommand.Text, "OFF", StringComparison.OrdinalIgnoreCase))
                this.echoCommand = false;
            else
                this.vm.Console.WriteLine(this.ReplaceVariables(echoCommand.Text));

            return CommandResult.Continue;
        }
        internal CommandResult RunCommand(IfErrorLevelCommand ifErrorLevelCommand)
        {
            if (ifErrorLevelCommand.ErrorLevel == this.vm.Dos.ErrorLevel)
                return ifErrorLevelCommand.Command.Run(this);

            return CommandResult.Continue;
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
            if (vm.FileSystem.FileExists(ifFileExistsCommand.FileName))
                return ifFileExistsCommand.Command.Run(this);

            return CommandResult.Continue;
        }
        internal CommandResult RunCommand(IfEqualsCommand ifEqualsCommand)
        {
            var value1 = this.ReplaceVariables(ifEqualsCommand.Value1) ?? string.Empty;
            var value2 = this.ReplaceVariables(ifEqualsCommand.Value2) ?? string.Empty;

            if ((value1 == value2) ^ ifEqualsCommand.Not)
                return ifEqualsCommand.Command.Run(this);

            return CommandResult.Continue;
        }
#pragma warning restore IDE0060 // Remove unused parameter

        private string ReplaceVariables(string s)
        {
            if (string.IsNullOrEmpty(s) || !s.Contains('%'))
                return s;

            var result = Regex.Replace(
                s,
                @"%(?<1>\d+)",
                m =>
                {
                    var b = this.CurrentBatch;
                    if (b != null && int.TryParse(m.Groups[1].Value, out int argIndex))
                    {
                        argIndex--;
                        if (argIndex >= 0 && argIndex < b.Arguments.Length)
                            return b.Arguments[argIndex];
                    }

                    return string.Empty;
                },
                RegexOptions.Singleline | RegexOptions.ExplicitCapture
            );

            result = Regex.Replace(
                result,
                @"%(?<1>[^\s%]+)%",
                m =>
                {
                    if (vm.EnvironmentVariables.TryGetValue(m.Groups[1].Value, out var var))
                        return var;

                    return string.Empty;
                },
                RegexOptions.Singleline | RegexOptions.ExplicitCapture
            );

            return result.Replace("%%", "%");
        }
    }
}
