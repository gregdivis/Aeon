using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Aeon.Emulator.CommandInterpreter;
using Aeon.Emulator.Dos.Programs;
using Aeon.Emulator.Dos.VirtualFileSystem;
using Aeon.Emulator.Memory;

namespace Aeon.Emulator.Dos
{
    /// <summary>
    /// Provides all of the emulated DOS functions.
    /// </summary>
    internal sealed class DosHandler : IInterruptHandler
    {
        /// <summary>
        /// The location of offset 0 of the DOS list of lists.
        /// </summary>
        internal static readonly RealModeAddress ListOfListsAddress = new(0x0080, 0x0026);
        /// <summary>
        /// The location of the current directory in the form A:\.
        /// </summary>
        internal static readonly RealModeAddress CurrentDirectoryAddress = new(0x0080, 0x001A);

        private readonly VirtualMachine vm;
        private readonly FileControl fileControl;
        private readonly MemoryAllocator memoryAllocator;
        private readonly List<byte> readLineBuffer = new();
        private byte extendedCode;
        private byte terminationType;

        private static readonly RealModeAddress SwappableDataArea = new(0xB2, 0);

        public DosHandler(VirtualMachine vm)
        {
            this.vm = vm;
            this.fileControl = new FileControl(vm);
            this.memoryAllocator = new MemoryAllocator(vm);
            InitializeSDA();
        }

        public IEnumerable<InterruptHandlerInfo> HandledInterrupts => new InterruptHandlerInfo[] { 0x21, 0x27, 0x20 };
        public Stream StdOut => this.fileControl.StdOut;
        public Stream StdIn => this.fileControl.StdIn;
        public DosProcess CurrentProcess => this.memoryAllocator.CurrentProcess;
        public byte ErrorLevel => this.memoryAllocator.ErrorLevel;

        public void LoadImage(ProgramImage image, ushort environmentSegment = 0, string commandLineArgs = null, string stdOut = null)
        {
            memoryAllocator.LoadImage(image, environmentSegment, commandLineArgs);
            UpdateSDA();

            if (!string.IsNullOrEmpty(stdOut))
                this.fileControl.SetStdOut(stdOut);

            vm.OnCurrentProcessChanged(EventArgs.Empty);
        }
        /// <summary>
        /// Ends the current process.
        /// </summary>
        /// <param name="residentParagraphs">Number of paragraphs to stay resident in memory.</param>
        public void EndCurrentProcess(int residentParagraphs)
        {
            if (residentParagraphs == 0)
                this.fileControl.CloseAllFiles(this.memoryAllocator.CurrentProcessId);
            memoryAllocator.EndCurrentProcess(residentParagraphs);
            UpdateSDA();
        }
        public CommandProcessor GetCommandProcessor()
        {
            var p = this.CurrentProcess;
            if (p.Interpreter != null)
                return p.Interpreter;

            p.Interpreter = new CommandProcessor(this.vm);
            return p.Interpreter;
        }
        public ConventionalMemoryInfo GetAllocations() => memoryAllocator.GetAllocations();
        public void HandleInterrupt(int interrupt)
        {
            if (interrupt == 0x20)
            {
                terminationType = 0x00;
                EndCurrentProcess(0);
                if (this.CurrentProcess == null)
                {
                    vm.OnCurrentProcessChanged(EventArgs.Empty);
                    throw new EndOfProgramException();
                }

                return;
            }

            if (interrupt == 0x27)
            {
                terminationType = 0x03;
                EndCurrentProcess((ushort)vm.Processor.DX >> 4);
                if (this.CurrentProcess == null)
                {
                    vm.OnCurrentProcessChanged(EventArgs.Empty);
                    throw new EndOfProgramException();
                }

                return;
            }

            bool saveFlags = true;
            vm.Processor.Flags.Carry = false;

            switch (vm.Processor.AH)
            {
                case Functions.GetVersionNumber:
                    GetVersionNumber();
                    break;

                case Functions.GlobalCodePageTable:
                    if (vm.Processor.AL == 0x01)
                    {
                        // Return US code page
                        vm.Processor.BX = 437;
                        vm.Processor.DX = 437;
                    }
                    break;

                case Functions.SetHandleCount:
                    break;

                case Functions.CloseFile:
                    fileControl.CloseFile();
                    break;

                case Functions.TerminateProgram:
                    terminationType = 0x00;
                    this.EndCurrentProcess(0);
                    if (this.CurrentProcess == null)
                    {
                        vm.OnCurrentProcessChanged(EventArgs.Empty);
                        throw new EndOfProgramException();
                    }
                    saveFlags = false;
                    break;

                case Functions.TerminateAndStayResident:
                    terminationType = 0x03;
                    this.EndCurrentProcess((ushort)this.vm.Processor.DX);
                    if (this.CurrentProcess == null)
                    {
                        vm.OnCurrentProcessChanged(EventArgs.Empty);
                        throw new EndOfProgramException();
                    }
                    saveFlags = false;
                    break;

                case Functions.GetReturnCode:
                    vm.Processor.AL = memoryAllocator.ErrorLevel;
                    vm.Processor.AH = this.terminationType;
                    break;

                case Functions.OpenFile:
                    fileControl.OpenFile();
                    break;

                case Functions.CreateFile:
                    fileControl.CreateFile();
                    break;

                case Functions.WriteToFile:
                    fileControl.WriteToFile();
                    break;

                case Functions.ReadFromFile:
                    fileControl.ReadFromFile();
                    break;

                case Functions.Ioctl:
                    fileControl.Ioctl();
                    break;

                case Functions.WriteToStdOut:
                    WriteToStdOut();
                    break;

                case Functions.ConsoleIO:
                    ConsoleIO();
                    break;

                case Functions.ConsoleInput:
                case Functions.ConsoleInputNoEcho:
                    saveFlags = ConsoleInput();
                    break;

                case Functions.CheckStandardInput:
                    CheckStandardInput();
                    break;

                case Functions.ConsoleOutput:
                    ConsoleOutput();
                    break;

                case Functions.ConsoleReadLine:
                    saveFlags = ConsoleReadLine();
                    break;

                case Functions.Seek:
                    fileControl.Seek();
                    break;

                case Functions.GetDefaultDrive:
                    fileControl.GetDefaultDrive();
                    break;

                case Functions.AllocateMemory:
                    memoryAllocator.AllocateMemory();
                    break;

                case Functions.DeallocateMemory:
                    memoryAllocator.DeallocateMemory();
                    break;

                case Functions.ModifyMemoryAllocation:
                    memoryAllocator.ModifyMemoryAllocation();
                    break;

                case Functions.GetSystemTime:
                    GetSystemTime();
                    break;

                case Functions.GetSystemDate:
                    GetSystemDate();
                    break;

                case Functions.SetSystemDate:
                case Functions.SetSystemTime:
                    // Ignore this.
                    break;

                case Functions.GetInterruptVector:
                    GetInterruptVector();
                    break;

                case Functions.SetInterruptVector:
                    SetInterruptVector();
                    break;

                case Functions.FileAttributes:
                    switch (vm.Processor.AL)
                    {
                        case Functions.FileAttributes_GetFileAttributes:
                            fileControl.GetFileAttributes();
                            break;

                        default:
                            throw new NotImplementedException($"DOS function 43{vm.Processor.AL:X2}h not implemented.");
                    }
                    break;

                case Functions.GetDiskTransferAreaAddress:
                    fileControl.GetDiskTransferAddress(memoryAllocator.CurrentProcess);
                    break;

                case Functions.SetDiskTransferAreaAddress:
                    fileControl.SetDiskTransferAddress(memoryAllocator.CurrentProcess);
                    break;

                case Functions.FindFirstFile:
                    fileControl.FindFirstFile(memoryAllocator.CurrentProcess);
                    break;

                case Functions.FindNextFile:
                    fileControl.FindNextFile(memoryAllocator.CurrentProcess);
                    break;

                case Functions.ResetDisk:
                    break;

                case Functions.GetCurrentDirectory:
                    fileControl.GetCurrentDirectory();
                    break;

                case Functions.ChangeDirectory:
                    fileControl.ChangeDirectory();
                    break;

                case Functions.CreateDirectory:
                    fileControl.CreateDirectory();
                    break;

                case Functions.GetFreeSpace:
                    fileControl.GetFreeSpace();
                    break;

                case Functions.SelectDefaultDrive:
                    fileControl.SelectDefaultDrive();
                    break;

                case Functions.DuplicateFileHandle:
                    fileControl.DuplicateFileHandle();
                    break;

                case Functions.GetCurrentPsp:
                    memoryAllocator.GetCurrentPsp();
                    break;

                case Functions.DeleteFile:
                    fileControl.DeleteFile();
                    break;

                case Functions.GetListOfLists:
                    System.Diagnostics.Debug.WriteLine("Get list of lists");
                    vm.WriteSegmentRegister(SegmentIndex.ES, ListOfListsAddress.Segment);
                    vm.Processor.BX = (short)ListOfListsAddress.Offset;
                    break;

                case Functions.ParseToFcb:
                    fileControl.ParseToFcb();
                    break;

                case Functions.ExecuteProgram:
                    switch (vm.Processor.AL)
                    {
                        case Functions.ExecuteProgram_LoadAndRun:
                            LoadAndRun();
                            // Do not want to save flags here, becuase this command
                            // munges the stack and instruction pointers.
                            saveFlags = false;
                            break;

                        case Functions.ExecuteProgram_LoadOverlay:
                            LoadOverlay();
                            break;

                        default:
                            throw new NotImplementedException();
                    }
                    break;

                case Functions.GetInDosPointer:
                    // The in-DOS flag never actually gets set, so just return the address
                    // of something that's always 0.
                    vm.WriteSegmentRegister(SegmentIndex.ES, 0x00B2);
                    vm.Processor.BX = 0x0001;
                    break;

                case Functions.GetCountryInfo:
                    GetCountryInfo();
                    break;

                case Functions.GetCurrentProcessId:
                    vm.Processor.BX = (short)this.memoryAllocator.CurrentProcessId;
                    break;

                case Functions.SetCurrentProcessId:
                    this.memoryAllocator.CurrentProcessId = (ushort)vm.Processor.BX;
                    break;

                case Functions.CreateChildPsp:
                    this.memoryAllocator.CreateChildPsp();
                    break;

                case 0x46:
                    break;

                case 0x0C:
                    break;

                case Functions.Function33:
                    switch (vm.Processor.AL)
                    {
                        case Functions.Function33_GetExtendedBreakChecking:
                            // Always return 0 for now.
                            vm.Processor.DL = 0;
                            break;

                        case Functions.Function33_SetExtendedBreakChecking:
                            // Ignore for now.
                            break;

                        case Functions.Function33_GetBootDrive:
                            vm.Processor.DL = 3;
                            break;

                        case Functions.Function33_GetTrueVersionNumber:
                            // DOS 5.0
                            vm.Processor.BL = 5;
                            vm.Processor.BH = 0;
                            vm.Processor.DX = 0;
                            vm.Processor.AL = 0;
                            break;

                        default:
                            throw new NotImplementedException($"DOS function 33{vm.Processor.AL:X2}h not implemented.");
                    }
                    break;

                case Functions.CreateTemporaryFile:
                    fileControl.CreateTempFile();
                    break;

                case Functions.AllocationStrategy:
                    switch (vm.Processor.AL)
                    {
                        case Functions.AllocationStrategy_Get:
                            vm.Processor.AX = (short)this.memoryAllocator.AllocationStrategy;
                            break;

                        case Functions.AllocationStrategy_Set:
                            if (Enum.IsDefined(typeof(AllocationStrategy), (int)vm.Processor.BL))
                            {
                                this.memoryAllocator.AllocationStrategy = (AllocationStrategy)vm.Processor.BL;
                                vm.Processor.BH = 0;
                            }
                            else
                            {
                                vm.Processor.AX = 1;
                                vm.Processor.Flags.Carry = true;
                            }
                            break;

                        case Functions.AllocationStrategy_GetUMB:
                            vm.Processor.AL = 0; // UMB's not part of DOS memory chain
                            break;

                        case Functions.AllocationStrategy_SetUMB:
                            break;

                        default:
                            System.Diagnostics.Debug.WriteLine($"DOS function 58{vm.Processor.AL:X2}h not implemented.");
                            vm.Processor.Flags.Carry = true;
                            break;
                    }
                    break;

                case Functions.Internal:
                    switch (vm.Processor.AL)
                    {
                        case Functions.Internal_GetSwappableDataArea:
                            System.Diagnostics.Debug.WriteLine("Get DOS SDA");
                            vm.WriteSegmentRegister(SegmentIndex.DS, SwappableDataArea.Segment);
                            vm.Processor.SI = SwappableDataArea.Offset;
                            vm.Processor.CX = 0x80;
                            vm.Processor.DX = 0x1A;
                            break;

                        default:
                            System.Diagnostics.Debug.WriteLine($"DOS function 5D{vm.Processor.AL:X2}h not implemented.");
                            vm.Processor.Flags.Carry = true;
                            break;
                    }
                    break;

                case Functions.FileInfo:
                    switch (vm.Processor.AL)
                    {
                        case Functions.FileInfo_GetDateTime:
                            fileControl.GetFileDateTime();
                            break;

                        case Functions.FileInfo_SetDateTime:
                            System.Diagnostics.Debug.WriteLine("Set file DateTime not implemented.");
                            break;

                        default:
                            System.Diagnostics.Debug.WriteLine($"DOS function 57{vm.Processor.AL:X2}h not implemented.");
                            vm.Processor.Flags.Carry = true;
                            break;
                    }
                    break;

                case Functions.CanonicalizePath:
                    fileControl.CanonicalizePath();
                    break;

                case Functions.GetDriveInfo:
                    fileControl.GetDriveInfo();
                    break;

                case Functions.RenameFile:
                    fileControl.MoveFile();
                    break;

                default:
                    System.Diagnostics.Debug.WriteLine($"DOS function {vm.Processor.AH:X2}h not implemented.");
                    vm.Processor.Flags.Carry = true;
                    break;
            }

            if (saveFlags)
                SaveFlags(EFlags.Carry | EFlags.Overflow | EFlags.Zero | EFlags.Sign);
        }
        /// <summary>
        /// Must be called once when all other initialization is complete.
        /// </summary>
        public void InitializationComplete()
        {
            InitializeListOfLists();
            this.fileControl.Initialize(this.memoryAllocator);
            this.vm.FileSystem.CurrentDriveChanged += (s, e) => this.vm.PhysicalMemory.SetString(CurrentDirectoryAddress.Segment, CurrentDirectoryAddress.Offset, this.vm.FileSystem.CurrentDrive + "\\", true);
        }
        public void Dispose()
        {
            fileControl.Dispose();
            memoryAllocator.Dispose();
        }

        void IVirtualDevice.Pause()
        {
        }
        void IVirtualDevice.Resume()
        {
        }
        void IVirtualDevice.DeviceRegistered(VirtualMachine vm)
        {
        }

        private void SaveFlags(EFlags modified)
        {
            var oldFlags = (EFlags)vm.PhysicalMemory.GetUInt16(vm.Processor.SS, (ushort)(vm.Processor.SP + 4));
            oldFlags &= ~modified;
            vm.PhysicalMemory.SetUInt16(vm.Processor.SS, (ushort)(vm.Processor.SP + 4), (ushort)(oldFlags | (vm.Processor.Flags.Value & modified)));
        }
        private void InitializeSDA()
        {
            var ptr = vm.PhysicalMemory.GetPointer(SwappableDataArea.Segment, SwappableDataArea.Offset);
            unsafe
            {
                var sda = (SwappableDataArea*)ptr.ToPointer();
                sda->ErrorDrive = 0xFF;
            }
        }
        private void UpdateSDA()
        {
            var process = this.CurrentProcess;
            if (process != null)
            {
                var ptr = vm.PhysicalMemory.GetPointer(SwappableDataArea.Segment, SwappableDataArea.Offset);
                unsafe
                {
                    var sda = (SwappableDataArea*)ptr.ToPointer();
                    sda->CurrentDTASegment = process.DiskTransferAreaSegment;
                    sda->CurrentDTAOffset = process.DiskTransferAreaOffset;
                }
            }
        }
        private void InitializeListOfLists()
        {
            setWord(ListOfListsOffsets.SegmentOfFirstMCB, MemoryBlockList.FirstSegment);
            setDWord(ListOfListsOffsets.FirstDriveParamaterBlock, uint.MaxValue);
            setDWord(ListOfListsOffsets.DiskBufferInfoRecord, uint.MaxValue);
            setDWord(ListOfListsOffsets.CurrentDirectoryStructures, (uint)(CurrentDirectoryAddress.Segment << 16) | CurrentDirectoryAddress.Offset);
            setDWord(ListOfListsOffsets.RoutineForResidentIFSUtilityFuncctions, uint.MaxValue);
            setDWord(ListOfListsOffsets.ChainOfIFSDrivers, uint.MaxValue);
            setWord(ListOfListsOffsets.NumberOfBuffers, 20);
            setWord(ListOfListsOffsets.NumberOfLookaheadBuffers, 20);
            setByte(ListOfListsOffsets.BootDrive, 3);
            setWord(ListOfListsOffsets.ExtendedMemorySize, (ushort)(vm.PhysicalMemory.MemorySize / 1024));

            void setByte(int o, byte v) => vm.PhysicalMemory.SetByte(ListOfListsAddress.Segment, (uint)(ListOfListsAddress.Offset + o), v);
            void setWord(int o, ushort v) => vm.PhysicalMemory.SetUInt16(ListOfListsAddress.Segment, (uint)(ListOfListsAddress.Offset + o), v);
            void setDWord(int o, uint v) => vm.PhysicalMemory.SetUInt32(ListOfListsAddress.Segment, (uint)(ListOfListsAddress.Offset + o), v);
        }
        private void WriteToStdOut()
        {
            string s = vm.PhysicalMemory.GetString(vm.Processor.DS, (ushort)vm.Processor.DX, ushort.MaxValue, (byte)'$');
            var buffer = Encoding.ASCII.GetBytes(s);
            this.fileControl.StdOut.Write(buffer, 0, buffer.Length);
        }
        private void ConsoleOutput()
        {
            this.fileControl.StdOut.WriteByte(vm.Processor.DL);
            vm.Processor.AL = vm.Processor.DL;
        }
        private void ConsoleIO()
        {
            // Read a character if DL == 0xFF
            if (vm.Processor.DL == 0xFF)
            {
                int inputValue = this.fileControl.StdIn.ReadByte();
                if (inputValue >= 0)
                {
                    vm.Processor.AL = (byte)inputValue;

                    // Echo the character to the console.
                    this.fileControl.StdOut.WriteByte((byte)inputValue);

                    vm.Processor.Flags.Zero = false;
                }
                else
                {
                    vm.Processor.Flags.Zero = true;
                    vm.Processor.AL = 0;
                }
            }
            // Otherwise output DL to the console.
            else
                this.fileControl.StdOut.WriteByte(vm.Processor.DL);
        }
        private bool ConsoleInput()
        {
            if (extendedCode != 0)
            {
                vm.Processor.AL = extendedCode;
                vm.Processor.Flags.Carry = false;
                extendedCode = 0;
                return true;
            }

            if (vm.Keyboard.HasTypeAheadDataAvailable)
            {
                ushort value = vm.Keyboard.DequeueTypeAhead();
                if ((value & 0x00FF) == 0)
                {
                    extendedCode = (byte)(value >> 8);
                    vm.Processor.AL = 0;
                }
                else
                    vm.Processor.AL = (byte)(value & 0xFF);
                vm.Processor.Flags.Carry = false;

                // Echo the character to the console.
                // No echo for this command.
                //if(fileControl.TextConsole != null)
                //    fileControl.TextConsole.Write(vm.Processor.AL);

                return true;
            }
            else
            {
                vm.Processor.IP -= 3;
                vm.Processor.Flags.InterruptEnable = true;
                return false;
            }
        }
        private void CheckStandardInput()
        {
            if (vm.Keyboard.HasTypeAheadDataAvailable || this.extendedCode != 0)
                vm.Processor.AL = 0xFF;
            else
                vm.Processor.AL = 0;
        }
        private bool ConsoleReadLine()
        {
            if (vm.Keyboard.HasTypeAheadDataAvailable)
            {
                int inputValue = this.fileControl.StdIn.ReadByte();
                byte asciiCode = inputValue >= 0 ? (byte)inputValue : (byte)0;
                if (asciiCode != 0)
                {
                    bool writeByte = true;

                    if (asciiCode != '\r')
                    {
                        if (asciiCode == 8)
                        {
                            if (readLineBuffer.Count > 0)
                                readLineBuffer.RemoveAt(readLineBuffer.Count - 1);
                            else
                                writeByte = false;
                        }
                        else
                            readLineBuffer.Add(asciiCode);

                        vm.Processor.IP -= 3;
                        vm.Processor.Flags.InterruptEnable = true;
                    }
                    else
                    {
                        int maxBytes = vm.PhysicalMemory.GetByte(vm.Processor.DS, (ushort)vm.Processor.DX);
                        maxBytes = Math.Min(maxBytes, readLineBuffer.Count);

                        vm.PhysicalMemory.SetByte(vm.Processor.DS, (ushort)((ushort)vm.Processor.DX + 1), (byte)maxBytes);
                        for (int i = 0; i < maxBytes; i++)
                            vm.PhysicalMemory.SetByte(vm.Processor.DS, (ushort)((ushort)vm.Processor.DX + 2 + i), readLineBuffer[i]);
                        readLineBuffer.Clear();
                    }

                    // Echo the character to the console.
                    if (writeByte)
                        this.fileControl.StdOut.WriteByte(asciiCode);
                }
                else
                {
                    vm.Processor.IP -= 3;
                    vm.Processor.Flags.InterruptEnable = true;
                }
            }
            else
            {
                vm.Processor.IP -= 3;
                vm.Processor.Flags.InterruptEnable = true;
            }

            return false;
        }
        private void GetVersionNumber()
        {
            // Return DOS version 5.0
            vm.Processor.AL = 5;
            vm.Processor.AH = 0;
            unchecked
            {
                vm.Processor.BX = (short)0xF500;
            }
            vm.Processor.CX = 0;
            vm.Processor.Flags.Carry = false;
        }
        private void GetSystemTime()
        {
            DateTime now = DateTime.Now;
            vm.Processor.CH = (byte)now.Hour;
            vm.Processor.CL = (byte)now.Minute;
            vm.Processor.DH = (byte)now.Second;
            vm.Processor.DL = (byte)(now.Millisecond / 10);
        }
        private void GetSystemDate()
        {
            DateTime now = DateTime.Now;
            vm.Processor.AL = (byte)now.DayOfWeek;
            vm.Processor.CX = (short)now.Year;
            vm.Processor.DH = (byte)now.Month;
            vm.Processor.DL = (byte)now.Day;
        }
        private void GetInterruptVector()
        {
            var address = vm.PhysicalMemory.GetRealModeInterruptAddress(vm.Processor.AL);
            vm.WriteSegmentRegister(SegmentIndex.ES, address.Segment);
            vm.Processor.BX = (short)address.Offset;
        }
        private void SetInterruptVector()
        {
            vm.PhysicalMemory.SetInterruptAddress(vm.Processor.AL, vm.Processor.DS, (ushort)vm.Processor.DX);
        }
        private void LoadAndRun()
        {
            var fileName = vm.PhysicalMemory.GetString(vm.Processor.DS, (ushort)vm.Processor.DX, 256, 0);
            var program = ProgramImage.Load(fileName, this.vm);

            if (vm.Processor.ES == 0)
                LoadImage(program);
            else
            {
                ushort envSegment = vm.PhysicalMemory.GetUInt16(vm.Processor.ES, (ushort)vm.Processor.BX);
                ushort cmdLineOffset = vm.PhysicalMemory.GetUInt16(vm.Processor.ES, (ushort)vm.Processor.BX + 2u);
                ushort cmdLineSegment = vm.PhysicalMemory.GetUInt16(vm.Processor.ES, (ushort)vm.Processor.BX + 4u);

                string cmdLineArgs = null;
                if (cmdLineSegment != 0)
                {
                    int count = vm.PhysicalMemory.GetByte(cmdLineSegment, cmdLineOffset);
                    if (count > 0)
                        cmdLineArgs = vm.PhysicalMemory.GetString(cmdLineSegment, cmdLineOffset + 1u, count);
                }

                LoadImage(program, envSegment, cmdLineArgs);
            }
        }
        private void LoadOverlay()
        {
            string fileName = vm.PhysicalMemory.GetString(vm.Processor.DS, (ushort)vm.Processor.DX, 256, 0);
            var program = ProgramImage.Load(fileName, vm);
            if (program is not ExeFile)
                throw new InvalidOperationException();

            ushort overlaySegment = vm.PhysicalMemory.GetUInt16(vm.Processor.ES, (ushort)vm.Processor.BX);
            short relocationFactor = (short)vm.PhysicalMemory.GetUInt16(vm.Processor.ES, (ushort)vm.Processor.BX + 2u);

            program.LoadOverlay(vm, overlaySegment, relocationFactor);
        }
        private void GetCountryInfo()
        {
            vm.Processor.AX = 1;
            vm.Processor.BX = 1;

            uint segment = vm.Processor.DS;
            uint offset = (ushort)vm.Processor.DX;

            vm.PhysicalMemory.SetUInt16(segment, offset, 0); // date format
            vm.PhysicalMemory.SetString(segment, offset + 0x02u, "$\0\0\0\0"); // currency symbol
            vm.PhysicalMemory.SetUInt16(segment, offset + 0x07u, ','); // thousands separator
            vm.PhysicalMemory.SetUInt16(segment, offset + 0x09u, '.'); // decimal separator
            vm.PhysicalMemory.SetUInt16(segment, offset + 0x0Bu, '/'); // date separator
            vm.PhysicalMemory.SetUInt16(segment, offset + 0x0Du, ':'); // time separator
            vm.PhysicalMemory.SetByte(segment, offset + 0x0Fu, 0); // currency format
            vm.PhysicalMemory.SetByte(segment, offset + 0x10u, 2); // digits after decimal in currency
            vm.PhysicalMemory.SetByte(segment, offset + 0x11u, 0); // time format
            vm.PhysicalMemory.SetUInt32(segment, offset + 0x12u, 0); // address of case map routine
            vm.PhysicalMemory.SetUInt16(segment, offset + 0x16u, ','); // data list separator
        }
    }
}
