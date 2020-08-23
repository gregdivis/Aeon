using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Aeon.Emulator.Dos.VirtualFileSystem;

namespace Aeon.Emulator.Dos
{
    internal sealed class FileControl : IDisposable
    {
        private const short StdInHandle = 0;
        private const short StdOutHandle = 1;
        private const short StdErrHandle = 2;
        private const short StdAuxHandle = 3;
        private const short StdPrnHandle = 4;

        private readonly VirtualMachine vm;
        private readonly OpenFileDictionary fileHandles = new OpenFileDictionary();
        private readonly Random random = new Random();
        private Queue<VirtualFileInfo> findFiles;
        private short emmHandle;

        public FileControl(VirtualMachine vm)
        {
            this.vm = vm;
            this.AddDefaultHandles();
        }

        /// <summary>
        /// Gets the standard output stream.
        /// </summary>
        public Stream StdOut
        {
            get
            {
                this.fileHandles.TryGetValue(StdOutHandle, out var s);
                return s?.BaseStream ?? Stream.Null;
            }
        }
        /// <summary>
        /// Gets the standard input stream.
        /// </summary>
        public Stream StdIn
        {
            get
            {
                this.fileHandles.TryGetValue(StdInHandle, out var s);
                return s?.BaseStream ?? Stream.Null;
            }
        }
        /// <summary>
        /// Gets the standard error stream.
        /// </summary>
        public Stream StdErr
        {
            get
            {
                this.fileHandles.TryGetValue(StdErrHandle, out var s);
                return s?.BaseStream ?? Stream.Null;
            }
        }
        /// <summary>
        /// Gets the current process ID.
        /// </summary>
        public int CurrentProcessId
        {
            get
            {
                var process = this.vm.CurrentProcess;
                if (process != null)
                    return process.PrefixSegment;
                else
                    return 0;
            }
        }

        public void SetStdOut(string target)
        {
            if (string.IsNullOrEmpty(target))
                return;

            if (string.Equals(target, "NUL", StringComparison.OrdinalIgnoreCase))
            {
                throw new NotImplementedException();
            }
            else
            {
                var filePath = new VirtualPath(target).Truncate();
                var s = vm.FileSystem.OpenFile(filePath, FileMode.Create, FileAccess.Write);
                if (s.Result == null)
                    throw new NotImplementedException();
                var fileInfo = vm.FileSystem.GetFileInfo(filePath);
                this.fileHandles.Add(StdOutHandle, new DosStream(s.Result, this.CurrentProcessId), fileInfo.Result, true); 
            }
        }

        /// <summary>
        /// Opens an existing file.
        /// </summary>
        public void OpenFile()
        {
            string fileName = vm.PhysicalMemory.GetString(vm.Processor.DS, (ushort)vm.Processor.DX, ushort.MaxValue, 0).Trim();
            if (string.IsNullOrEmpty(fileName))
            {
                vm.Processor.Flags.Carry = true;
                vm.Processor.AX = 2;
                return;
            }

            System.Diagnostics.Debug.WriteLine($"Open file: {fileName}");

            // Hack to detect expanded memory manager.
            if (fileName == "EMMXXXX0")
            {
                vm.Processor.Flags.Carry = false;
                emmHandle = GetNextFileHandle();
                fileHandles.AddSpecial(emmHandle, new DosStream(Stream.Null, this.CurrentProcessId));
                vm.Processor.AX = emmHandle;
                return;
            }

            var filePath = new VirtualPath(fileName).Truncate();

            var fileInfo = vm.FileSystem.GetFileInfo(filePath);
            if (this.vm.Processor.AL == 0 && fileInfo.Result == null && fileInfo.ErrorCode != ExtendedErrorCode.FileNotFound)
            {
                vm.Processor.Flags.Carry = true;
                vm.Processor.AX = (short)fileInfo.ErrorCode;
                return;
            }

            var s = (this.vm.Processor.AL & 0x03) switch
            {
                // Read access
                0 => vm.FileSystem.OpenFile(filePath, FileMode.Open, FileAccess.Read),
                // Write access
                1 => vm.FileSystem.OpenFile(filePath, FileMode.OpenOrCreate, FileAccess.Write),
                // Read/write access
                2 => vm.FileSystem.OpenFile(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite),
                _ => ExtendedErrorCode.FunctionNumberInvalid,
            };

            if (s.Result != null)
            {
                vm.Processor.Flags.Carry = false;
                vm.Processor.AX = GetNextFileHandle();
                fileHandles.Add(vm.Processor.AX, new DosStream(s.Result, this.CurrentProcessId), fileInfo.Result ?? vm.FileSystem.GetFileInfo(filePath).Result);
            }
            else
            {
                vm.Processor.Flags.Carry = true;
                vm.Processor.AX = (short)s.ErrorCode;
            }
        }
        /// <summary>
        /// Closes an open file handle.
        /// </summary>
        public void CloseFile()
        {
            short fileHandle = vm.Processor.BX;

            if (fileHandle != StdInHandle && fileHandle != StdOutHandle && fileHandle != StdErrHandle)
            {
                if (fileHandles.TryGetValue(fileHandle, out var s))
                {
                    s.Close();
                    fileHandles.Remove(fileHandle);
                }
                else if (fileHandle == emmHandle)
                {
                    emmHandle = 0;
                }
                else
                {
                    vm.Processor.Flags.Carry = true;
                    vm.Processor.AX = 6;
                }
            }
        }
        /// <summary>
        /// Creates a new file.
        /// </summary>
        public void CreateFile()
        {
            string fileName = vm.PhysicalMemory.GetString(vm.Processor.DS, (ushort)vm.Processor.DX, ushort.MaxValue, 0);
            if (fileName == "LPT1" || fileName == "CON")
            {
                vm.Processor.Flags.Carry = true;
                return;
            }

            if (fileName == "NUL")
            {
                vm.Processor.AX = GetNextFileHandle();
                fileHandles.Add(vm.Processor.AX, new DosStream(Stream.Null, this.CurrentProcessId), new VirtualFileInfo("NUL", VirtualFileAttributes.Default, DateTime.Now, 0));
                return;
            }

            var filePath = new VirtualPath(fileName).Truncate();

            var s = vm.FileSystem.OpenFile(filePath, FileMode.Create, FileAccess.Write);
            if (s.Result != null)
            {
                vm.Processor.AX = GetNextFileHandle();
                fileHandles.Add(vm.Processor.AX, new DosStream(s.Result, this.CurrentProcessId), new VirtualFileInfo(filePath.LastElement, VirtualFileAttributes.Default, DateTime.Now, 0));
            }
            else
            {
                vm.Processor.AX = (short)s.ErrorCode;
                vm.Processor.Flags.Carry = true;
            }
        }
        /// <summary>
        /// Creates a new temporary file.
        /// </summary>
        public void CreateTempFile()
        {
            string path = vm.PhysicalMemory.GetString(vm.Processor.DS, (ushort)vm.Processor.DX, ushort.MaxValue, 0);
            var virtualPath = new VirtualPath(path + "\\");

            var newPath = virtualPath + GenerateTempFileName();
            while (vm.FileSystem.FileExists(newPath))
            {
                newPath = virtualPath + GenerateTempFileName();
            }

            var s = vm.FileSystem.OpenFile(newPath, FileMode.Create, FileAccess.ReadWrite);
            if (s.Result != null)
            {
                vm.Processor.AX = GetNextFileHandle();
                fileHandles.Add(vm.Processor.AX, new DosStream(s.Result, this.CurrentProcessId), new VirtualFileInfo(newPath.LastElement, VirtualFileAttributes.Default, DateTime.Now, 0));
                vm.Processor.Flags.Carry = false;

                uint writePos = (ushort)vm.Processor.DX + (uint)path.Length;
                vm.PhysicalMemory.SetString(vm.Processor.DS, writePos, newPath.LastElement);
            }
            else
            {
                vm.Processor.AX = (short)s.ErrorCode;
                vm.Processor.Flags.Carry = true;
            }
        }
        /// <summary>
        /// Writes data to a file.
        /// </summary>
        public void WriteToFile()
        {
            short fileHandle = vm.Processor.BX;

            if (fileHandles.TryGetValue(fileHandle, out var s))
            {
                int length = (ushort)vm.Processor.CX;
                if (length > 0)
                {
                    var ptr = vm.PhysicalMemory.GetSpan(vm.Processor.DS, (ushort)vm.Processor.DX, length);
                    s.Write(ptr);
                }

                vm.Processor.AX = vm.Processor.CX;
                vm.Processor.Flags.Carry = false;
            }
            else
            {
                vm.Processor.Flags.Carry = true;
                vm.Processor.AX = 6;
            }
        }
        /// <summary>
        /// Reads data from a file.
        /// </summary>
        public void ReadFromFile()
        {
            short fileHandle = vm.Processor.BX;

            if (fileHandles.TryGetValue(fileHandle, out var s))
            {
                if (s.CanRead)
                {
                    int bytesToRead = (ushort)vm.Processor.CX;
                    var ptr = vm.PhysicalMemory.GetSpan(vm.Processor.DS, (ushort)vm.Processor.DX, bytesToRead);
                    int bytesRead = s.Read(ptr);

                    vm.Processor.Flags.Carry = false;
                    vm.Processor.AX = (short)((ushort)bytesRead);
                }
                else
                {
                    vm.Processor.Flags.Carry = true;
                    vm.Processor.AX = 5;
                }
            }
            else
            {
                vm.Processor.Flags.Carry = true;
                vm.Processor.AX = 6;
            }
        }
        public void Ioctl()
        {
            switch (vm.Processor.AL)
            {
                // Get device info
                case 0x00:
                    if (vm.Processor.BX == emmHandle && emmHandle != 0)
                    {
                        vm.Processor.DX = unchecked((short)0xC080);
                        vm.Processor.AX = unchecked((short)0xC080);
                    }
                    else
                    {
                        if (this.fileHandles.TryGetValue(vm.Processor.BX, out var stream))
                            vm.Processor.AX = vm.Processor.DX = (short)stream.HandleInfo;
                        else
                            vm.Processor.Flags.Carry = true;
                    }
                    break;

                case 0x01:
                    vm.Processor.Flags.Carry = true;
                    return;

                case 0x05:
                    vm.Processor.AX = 1;
                    vm.Processor.Flags.Carry = true;
                    return;

                case 0x07:
                    if (vm.Processor.BX == emmHandle && emmHandle != 0)
                        vm.Processor.AL = 0xFF;
                    else
                        throw new NotImplementedException();
                    break;

                case 0x08:
                    vm.Processor.AX = 1;
                    break;

                case 0x09:
                    vm.Processor.AX = 1;
                    vm.Processor.Flags.Carry = true;
                    break;

                case 0x0D:
                    vm.Processor.AX = 1;
                    vm.Processor.Flags.Carry = true;
                    break;

                case 0x0E:
                    vm.Processor.AL = 0;
                    vm.Processor.Flags.Carry = false;
                    break;

                default:
                    throw new InvalidOperationException();
            }

            vm.Processor.Flags.Carry = false;
        }
        /// <summary>
        /// Gets the default drive.
        /// </summary>
        public void GetDefaultDrive()
        {
            // Return default drive in AL.
            vm.Processor.AL = (byte)vm.FileSystem.CurrentDrive.Index;
        }
        /// <summary>
        /// Moves the file pointer in a stream.
        /// </summary>
        public void Seek()
        {
            if (fileHandles.TryGetValue(vm.Processor.BX, out var s))
            {
                if (vm.Processor.AL <= 2)
                {
                    int distance = (int)(((ushort)vm.Processor.CX) << 16) | (ushort)vm.Processor.DX;
                    switch (vm.Processor.AL)
                    {
                        case 0:
                            s.Seek(distance, SeekOrigin.Begin);
                            break;

                        case 1:
                            s.Seek(distance, SeekOrigin.Current);
                            break;

                        case 2:
                            s.Seek((long)distance, SeekOrigin.End);
                            break;
                    }

                    vm.Processor.Flags.Carry = false;
                    uint newPos = Convert.ToUInt32(s.Position);
                    vm.Processor.DX = (short)((newPos >> 16) & 0xFFFF);
                    vm.Processor.AX = (short)(newPos & 0xFFFF);
                }
                else
                {
                    vm.Processor.Flags.Carry = true;
                    vm.Processor.AX = 1;
                }
            }
            else
            {
                vm.Processor.Flags.Carry = true;
                vm.Processor.AX = 6;
            }
        }
        /// <summary>
        /// Gets attributes for a file.
        /// </summary>
        public void GetFileAttributes()
        {
            var fileName = vm.PhysicalMemory.GetString(vm.Processor.DS, (ushort)vm.Processor.DX, ushort.MaxValue, 0);
            var fileInfo = this.vm.FileSystem.GetFileInfo(new VirtualPath(fileName));
            if (fileInfo.Result != null)
            {
                vm.Processor.AX = fileInfo.Result.DosAttributes;
                vm.Processor.CX = fileInfo.Result.DosAttributes;
            }
            else
            {
                vm.Processor.AX = (short)fileInfo.ErrorCode;
                vm.Processor.CX = (short)fileInfo.ErrorCode;
                vm.Processor.Flags.Carry = true;
            }
        }
        /// <summary>
        /// Gets the address of the disk transfer area of a process.
        /// </summary>
        /// <param name="process">Process whose DTA will be returned.</param>
        public void GetDiskTransferAddress(DosProcess process)
        {
            vm.WriteSegmentRegister(SegmentIndex.ES, process.DiskTransferAreaSegment);
            vm.Processor.BX = (short)process.DiskTransferAreaOffset;
        }
        /// <summary>
        /// Sets the address of the disk transfer area of a process.
        /// </summary>
        /// <param name="process">Process whose DTA will be updated.</param>
        public void SetDiskTransferAddress(DosProcess process)
        {
            process.DiskTransferAreaSegment = vm.Processor.DS;
            process.DiskTransferAreaOffset = (ushort)vm.Processor.DX;
        }
        /// <summary>
        /// Finds a file and writes information about it to the disk transfer area.
        /// </summary>
        /// <param name="process">Process requesting the find.</param>
        public void FindFirstFile(DosProcess process)
        {
            var fileName = vm.PhysicalMemory.GetString(vm.Processor.DS, (ushort)vm.Processor.DX, ushort.MaxValue, 0);
            if (IsValidSearchPath(fileName))
            {
                var path = vm.FileSystem.ResolvePath(new VirtualPath(fileName));

                var files = vm.FileSystem.GetDirectory(path, (VirtualFileAttributes)vm.Processor.CX);
                if (files.Result != null)
                {
                    this.findFiles = new Queue<VirtualFileInfo>(files.Result);
                    WriteFindInfo(process.DiskTransferAreaSegment, process.DiskTransferAreaOffset);
                }
                else
                {
                    vm.Processor.AX = (short)files.ErrorCode;
                    vm.Processor.Flags.Carry = true;
                    this.findFiles = null;
                }
            }
            else
            {
                vm.Processor.AX = 2;
                vm.Processor.Flags.Carry = true;
                findFiles = null;
            }
        }
        /// <summary>
        /// Finds the next file and writes information about it to the disk transfer area.
        /// </summary>
        /// <param name="process">Process requesting the find.</param>
        public void FindNextFile(DosProcess process)
        {
            WriteFindInfo(process.DiskTransferAreaSegment, process.DiskTransferAreaOffset);
        }
        /// <summary>
        /// Gets the current directory in the emulated system.
        /// </summary>
        public void GetCurrentDirectory()
        {
            var drive = vm.FileSystem.CurrentDrive;
            if (vm.Processor.DL > 0)
                drive = new DriveLetter(vm.Processor.DL - 1);

            var path = vm.FileSystem.GetCurrentDirectory(drive);
            vm.PhysicalMemory.SetString(vm.Processor.DS, vm.Processor.SI, path.Path);
            vm.Processor.Flags.Carry = false;
        }
        /// <summary>
        /// Changes the currenct directory in the emulated system.
        /// </summary>
        public void ChangeDirectory()
        {
            var path = vm.PhysicalMemory.GetString(vm.Processor.DS, (ushort)vm.Processor.DX, 64, 0);
            vm.FileSystem.ChangeDirectory(new VirtualPath(path));
            vm.Processor.Flags.Carry = false;
        }
        /// <summary>
        /// Creates a new directory.
        /// </summary>
        public void CreateDirectory()
        {
            var path = vm.PhysicalMemory.GetString(vm.Processor.DS, (ushort)vm.Processor.DX, 64, 0);
            var result = vm.FileSystem.CreateDirectory(path);
            if (result != ExtendedErrorCode.NoError)
            {
                vm.Processor.Flags.Carry = true;
                vm.Processor.AX = (short)result;
            }
        }
        /// <summary>
        /// Gets the amount of free space on the drive.
        /// </summary>
        public void GetFreeSpace()
        {
            var p = vm.Processor;
            if (p.DL <= 26)
            {
                var fs = this.vm.FileSystem;
                var drive = p.DL == 0 ? fs.Drives[fs.CurrentDrive] : fs.Drives[p.DL - 1];
                if (drive.DriveType != DriveType.None)
                {
                    if (drive.Mapping is IMagneticDrive m)
                    {
                        p.AX = (short)m.SectorsPerCluster;
                        p.CX = (short)m.BytesPerSector;
                    }
                    else
                    {
                        p.AX = 0x007F;
                        p.CX = 0x0200;
                    }

                    long freeSpace = drive.Mapping?.FreeSpace ?? 0;
                    ushort freeClusters = (ushort)Math.Clamp(freeSpace / (p.AX * p.CX), 0, ushort.MaxValue);
                    p.BX = (short)freeClusters;
                    p.DX = (short)Math.Clamp(freeClusters * 2, 0, ushort.MaxValue);
                    return;
                }
            }

            p.AX = -1;
        }
        /// <summary>
        /// Sets the default drive.
        /// </summary>
        public void SelectDefaultDrive()
        {
            // Read the new drive letter from DL.
            if (vm.Processor.DL < 26)
                vm.FileSystem.CurrentDrive = new DriveLetter(vm.Processor.DL);

            // Return the maximum number of supported drives.
            vm.Processor.AL = 26;
        }
        /// <summary>
        /// Duplicates a file handle.
        /// </summary>
        public void DuplicateFileHandle()
        {
            if (fileHandles.TryGetValue(vm.Processor.BX, out var s))
            {
                s.AddReference();
                vm.Processor.AX = GetNextFileHandle();
                fileHandles.Add(vm.Processor.AX, s, s.FileInfo);
                vm.Processor.Flags.Carry = false;
            }
            else
            {
                vm.Processor.AX = 6;
                vm.Processor.Flags.Carry = true;
            }
        }
        /// <summary>
        /// Deletes a file.
        /// </summary>
        public void DeleteFile()
        {
            string path = vm.PhysicalMemory.GetString(vm.Processor.DS, (ushort)vm.Processor.DX, 256, 0);
            var result = vm.FileSystem.DeleteFile(path);
            if (result != ExtendedErrorCode.NoError)
            {
                vm.Processor.Flags.Carry = true;
                vm.Processor.AX = (short)result;
            }
        }
        /// <summary>
        /// Parses a string to a file control block.
        /// </summary>
        public void ParseToFcb()
        {
            string text = vm.PhysicalMemory.GetString(vm.Processor.DS, vm.Processor.SI, 256, 0);
            string trimmedText = text.TrimStart(' ');
            int offset = text.Length - trimmedText.Length;

            string parsedItem = string.Empty;
            int firstBlank = trimmedText.IndexOf(' ');
            if (firstBlank == -1)
                firstBlank = text.Length;
            else
                parsedItem = trimmedText.Substring(0, firstBlank);

            string[] nameParts = parsedItem.Split(new char[] { '.' }, 2);
            string fileName = nameParts[0].PadRight(8, ' ');
            string fileExtension = "   ";
            if (nameParts.Length >= 2)
                fileExtension = nameParts[1].PadRight(3, ' ');

            // Parse successful.
            vm.Processor.AL = 0;
            vm.PhysicalMemory.SetByte(vm.Processor.ES, vm.Processor.DI, 0);
            vm.PhysicalMemory.SetString(vm.Processor.ES, vm.Processor.DI + 1u, fileName, false);
            vm.PhysicalMemory.SetString(vm.Processor.ES, vm.Processor.DI + 9u, fileExtension, false);
            vm.Processor.SI += (ushort)(offset + firstBlank);
            vm.Processor.Flags.Carry = false;
        }
        /// <summary>
        /// Closes all open file streams.
        /// </summary>
        public void CloseAllFiles()
        {
            this.fileHandles.CloseAll();
            AddDefaultHandles();
        }
        /// <summary>
        /// Closes all file streams opened by a specified process.
        /// </summary>
        /// <param name="ownerId">ID of process whose files will be closed.</param>
        public void CloseAllFiles(int ownerId)
        {
            this.fileHandles.CloseAllFiles(ownerId);
        }
        /// <summary>
        /// Initializes the file control object. This must be called once.
        /// </summary>
        /// <param name="memoryAllocator">DOS memory allocator.</param>
        public void Initialize(MemoryAllocator memoryAllocator)
        {
            var segment = memoryAllocator.AllocateSystemMemory(50 * 0x3B / 16);
            this.fileHandles.Initialize(vm.PhysicalMemory.GetPointer(segment, 0), 50);

            // Write the location of the SFT for the list of lists.
            vm.PhysicalMemory.SetUInt32(DosHandler.ListOfListsAddress.Segment, DosHandler.ListOfListsAddress.Offset + (uint)ListOfListsOffsets.FirstSystemFileTable, (uint)segment << 16);
        }
        /// <summary>
        /// Gets the file's date and time stamp.
        /// </summary>
        public void GetFileDateTime()
        {
            if (!this.fileHandles.TryGetValue(vm.Processor.BX, out var s))
            {
                vm.Processor.Flags.Carry = true;
                vm.Processor.AX = 6;
                return;
            }

            var info = s.FileInfo;
            if (info != null)
            {
                vm.Processor.CX = (short)info.DosModifyTime;
                vm.Processor.DX = (short)info.DosModifyDate;
            }
            else
            {
                vm.Processor.CX = 0;
                vm.Processor.DX = 0;
            }
        }
        /// <summary>
        /// Canonicalizes a path.
        /// </summary>
        public void CanonicalizePath()
        {
            var inPath = vm.PhysicalMemory.GetString(vm.Processor.DS, vm.Processor.SI, 128, 0);

            var path = VirtualPath.TryParse(inPath);
            if (path == null)
            {
                vm.Processor.Flags.Carry = true;
                vm.Processor.AX = 3;
            }

            path = vm.FileSystem.ResolvePath(path);

            vm.PhysicalMemory.SetString(vm.Processor.ES, vm.Processor.DI, path.ToString());
        }
        /// <summary>
        /// Gets information about a drive.
        /// </summary>
        public void GetDriveInfo()
        {
            int drive = vm.Processor.DL != 0 ? vm.Processor.DL - 1 : vm.FileSystem.CurrentDrive.Index;
            var driveInfo = vm.FileSystem.Drives[drive].MagneticDriveInfo;
            vm.Processor.AL = (byte)driveInfo.SectorsPerCluster;
            vm.Processor.CX = (short)(ushort)driveInfo.BytesPerSector;
            vm.Processor.DX = (short)(ushort)driveInfo.Clusters;
        }
        /// <summary>
        /// Moves or renames a file.
        /// </summary>
        public void MoveFile()
        {
            var srcFileName = this.vm.PhysicalMemory.GetString(this.vm.Processor.DS, (ushort)this.vm.Processor.DX, 500, 0);
            var destFileName = this.vm.PhysicalMemory.GetString(this.vm.Processor.ES, this.vm.Processor.DI, 500, 0);

            var srcPath = this.vm.FileSystem.ResolvePath(new VirtualPath(srcFileName));
            var destPath = this.vm.FileSystem.ResolvePath(new VirtualPath(destFileName));

            var drive = this.vm.FileSystem.Drives[(DriveLetter)srcPath.DriveLetter];
            var result = drive.MoveFile(srcPath, destPath);
            if (result != ExtendedErrorCode.NoError)
            {
                this.vm.Processor.AX = (short)result;
                this.vm.Processor.Flags.Carry = true;
            }
        }
        /// <summary>
        /// Closes all open files.
        /// </summary>
        public void Dispose()
        {
            //foreach(var s in fileHandles.Values)
            //    s.Close();
            //fileHandles.Clear();
            this.fileHandles.Dispose();
        }

        /// <summary>
        /// Adds file handles for StdIn, StdOut, and StdErr.
        /// </summary>
        private void AddDefaultHandles()
        {
            var stdin = new DosStream(vm.ConsoleIn, 0);
            var stdout = new DosStream(vm.ConsoleOut, 0);
            stdout.AddReference();

            var stdaux = new DosStream(NullStream.Instance, 0);
            stdaux.AddReference();

            this.fileHandles.AddSpecial(StdInHandle, stdin);
            this.fileHandles.AddSpecial(StdOutHandle, stdout);
            this.fileHandles.AddSpecial(StdErrHandle, stdout);
            this.fileHandles.AddSpecial(StdAuxHandle, stdaux);
            this.fileHandles.AddSpecial(StdPrnHandle, stdaux);
        }
        /// <summary>
        /// Writes file find information at a specified address.
        /// </summary>
        /// <param name="dtaSegment">Segment of disk transfer area.</param>
        /// <param name="dtaOffset">Offset of disk transfer area.</param>
        private void WriteFindInfo(ushort dtaSegment, ushort dtaOffset)
        {
            if (findFiles != null && findFiles.Count > 0)
            {
                var info = findFiles.Dequeue();

                vm.PhysicalMemory.SetByte(dtaSegment, dtaOffset + 21u, info.DosAttributes);
                vm.PhysicalMemory.SetUInt16(dtaSegment, dtaOffset + 22u, info.DosModifyTime);
                vm.PhysicalMemory.SetUInt16(dtaSegment, dtaOffset + 24u, info.DosModifyDate);
                vm.PhysicalMemory.SetUInt32(dtaSegment, dtaOffset + 26u, info.DosLength);
                vm.PhysicalMemory.SetString(dtaSegment, dtaOffset + 30u, info.Name);

                vm.Processor.AX = 0;
                vm.Processor.Flags.Carry = false;
            }
            else
            {
                vm.Processor.AX = 18;
                vm.Processor.Flags.Carry = true;
            }
        }
        /// <summary>
        /// Returns the first unused file handle value.
        /// </summary>
        /// <returns>Unused file handle value on success; zero if no handles are available.</returns>
        private short GetNextFileHandle() => this.fileHandles.NextHandle;
        /// <summary>
        /// Returns a randomly-generated file name.
        /// </summary>
        /// <returns>Randomly-generated file name.</returns>
        private string GenerateTempFileName()
        {
            var buffer = new StringBuilder(13);
            for (int i = 0; i < 8; i++)
                buffer.Append((char)random.Next('A', 'Z' + 1));
            buffer.Append('.');
            for (int i = 0; i < 3; i++)
                buffer.Append((char)random.Next('A', 'Z' + 1));

            return buffer.ToString();
        }

        /// <summary>
        /// Returns a value indicating whether a string is a valid DOS search path.
        /// </summary>
        /// <param name="path">DOS search path to test.</param>
        /// <returns>Value indicating whether the string is a valid DOS search path.</returns>
        private static bool IsValidSearchPath(string path)
        {
            var invalidChars = new List<char>(Path.GetInvalidPathChars());
            invalidChars.Remove('*');
            return path.IndexOfAny(invalidChars.ToArray()) < 0;
        }
    }
}
