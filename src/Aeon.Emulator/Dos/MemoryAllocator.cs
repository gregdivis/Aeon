using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Aeon.Emulator.Dos.Programs;
using Aeon.Emulator.Memory;

//http://www.drdos.com/dosdoc/sysprog/chap4.htm
//http://www.piclist.com/techref/dos/pss.htm

namespace Aeon.Emulator.Dos
{
    /// <summary>
    /// Manages DOS memory allocations and loading executable files.
    /// </summary>
    internal sealed class MemoryAllocator : IDisposable
    {
        private static readonly RealModeAddress SwappableDataArea = new RealModeAddress(0xB2, 0);

        private readonly VirtualMachine vm;
        private readonly Dictionary<ushort, DosProcess> allProcesses = new Dictionary<ushort, DosProcess>();
        private readonly MemoryBlockList blockList;
        private readonly ReaderWriterLockSlim listLock = new ReaderWriterLockSlim();
        private bool disposed;

        public MemoryAllocator(VirtualMachine vm)
        {
            this.vm = vm;
            this.blockList = new MemoryBlockList(vm.PhysicalMemory);
        }

        /// <summary>
        /// Gets the current DOS process.
        /// </summary>
        public DosProcess CurrentProcess
        {
            get
            {
                this.allProcesses.TryGetValue(this.CurrentProcessId, out var process);
                return process;
            }
        }
        /// <summary>
        /// Gets or sets the current DOS errorlevel value.
        /// </summary>
        public byte ErrorLevel { get; set; }
        /// <summary>
        /// Gets or sets the memory allocation strategy.
        /// </summary>
        public AllocationStrategy AllocationStrategy
        {
            get => this.blockList.AllocationStrategy;
            set => this.blockList.AllocationStrategy = value;
        }
        /// <summary>
        /// Gets or sets the current process ID.
        /// </summary>
        public ushort CurrentProcessId
        {
            get
            {
                var ptr = vm.PhysicalMemory.GetPointer(SwappableDataArea.Segment, SwappableDataArea.Offset);
                unsafe
                {
                    var sda = (SwappableDataArea*)ptr.ToPointer();
                    return sda->CurrentPSP;
                }
            }
            set
            {
                var ptr = vm.PhysicalMemory.GetPointer(SwappableDataArea.Segment, SwappableDataArea.Offset);
                unsafe
                {
                    var sda = (SwappableDataArea*)ptr.ToPointer();
                    sda->CurrentPSP = value;
                }
            }
        }

        /// <summary>
        /// Loads an execuable file into the emulated system and sets up the processor.
        /// </summary>
        /// <param name="image">Program file to load.</param>
        /// <param name="environmentSegment">Segment of the process's environment block.</param>
        /// <param name="commandLineArgs">Command line arguments used to launch the process.</param>
        public void LoadImage(ProgramImage image, ushort environmentSegment, string commandLineArgs)
        {
            listLock.EnterWriteLock();

            try
            {
                // Save the processor state before the image is loaded.
                byte[] processorState = vm.Processor.GetCurrentState();

                // Nice name, huh?
                bool reAss = false;

                // Each process gets its own copy of the system environment strings,
                // so these need to be allocated in a separate block first.
                byte[] environmentBlock;

                if (environmentSegment == 0)
                    environmentBlock = BuildEnvironmentBlock();
                else
                    environmentBlock = ReadEnvironmentBlock(environmentSegment);

                var fullImagePath = image.FullPath.FullPath;

                environmentSegment = blockList.Allocate((ushort)((environmentBlock.Length >> 4) + 1 + fullImagePath.Length + 1), DosProcess.NullProcess);
                if (environmentSegment == 0)
                    throw new InvalidOperationException();

                // Copy the environment block to emulated memory.
                var environmentPtr = vm.PhysicalMemory.GetPointer(environmentSegment, 0);
                Marshal.Copy(environmentBlock, 0, environmentPtr, environmentBlock.Length);
                vm.PhysicalMemory.SetString(environmentSegment, (uint)environmentBlock.Length, fullImagePath);
                reAss = true;

                // Create the new process object and reassign the environment block
                // so the process is marked as the owner.
                var newProcess = new DosProcess(image, blockList.NextPspSegment, environmentSegment, commandLineArgs ?? string.Empty);

                // Only reassign if it's a new environment block copy.
                if (reAss)
                    blockList.Reassign(environmentSegment, newProcess);

                // Allocate the largest free block for the new process.
                ushort segment = blockList.Allocate(Math.Min((ushort)image.MaximumParagraphs, blockList.LargestFreeBlock), newProcess);
                //ushort segment = blockList.Allocate(blockList.LargestFreeBlock, newProcess);
                if (segment == 0)
                    throw new InvalidOperationException();

                // Generate the program segment prefix data for the process and copy it to emulated memory.
                ushort parentPsp = this.CurrentProcessId;
                byte[] psp = BuildPsp(newProcess, parentPsp);
                var pspPtr = vm.PhysicalMemory.GetPointer(newProcess.PrefixSegment, 0);
                Marshal.Copy(psp, 0, pspPtr, psp.Length);

                // Set the process as current and perform image-specific initialization.
                newProcess.InitialProcessorState = processorState;
                this.allProcesses.Add(newProcess.PrefixSegment, newProcess);
                this.CurrentProcessId = newProcess.PrefixSegment;
                image.Load(vm, newProcess.PrefixSegment);
            }
            finally
            {
                listLock.ExitWriteLock();
            }
        }
        /// <summary>
        /// Allocates a block of memory.
        /// </summary>
        public void AllocateMemory()
        {
            ushort requestedParagraphs = (ushort)vm.Processor.BX;

            listLock.EnterWriteLock();

            try
            {
                ushort segment = blockList.Allocate(requestedParagraphs, this.CurrentProcess);
                if (segment != 0)
                {
                    vm.Processor.Flags.Carry = false;
                    vm.Processor.AX = (short)segment;
                }
                else
                {
                    vm.Processor.Flags.Carry = true;
                    vm.Processor.AX = 8;
                    vm.Processor.BX = (short)blockList.LargestFreeBlock;
                }
            }
            finally
            {
                listLock.ExitWriteLock();
            }
        }
        /// <summary>
        /// Allocates conventional memory for use by the system.
        /// </summary>
        /// <param name="paragraphs">Paragraphs to allocate.</param>
        /// <returns>Segment of allocated memory.</returns>
        public ushort AllocateSystemMemory(ushort paragraphs)
        {
            listLock.EnterWriteLock();

            try
            {
                return blockList.Allocate(paragraphs, DosProcess.NullProcess);
            }
            finally
            {
                listLock.ExitWriteLock();
            }
        }
        /// <summary>
        /// Frees a block of memory.
        /// </summary>
        public void DeallocateMemory()
        {
            ushort segment = vm.Processor.ES;

            listLock.EnterWriteLock();

            try
            {
                if (blockList.Free(segment))
                    vm.Processor.Flags.Carry = false;
                else
                {
                    vm.Processor.Flags.Carry = true;
                    vm.Processor.AX = 7;
                }
            }
            finally
            {
                listLock.ExitWriteLock();
            }
        }
        /// <summary>
        /// Changes the size of an allocated block.
        /// </summary>
        public void ModifyMemoryAllocation()
        {
            ushort segment = vm.Processor.ES;
            ushort requestedParagraphs = (ushort)vm.Processor.BX;

            listLock.EnterWriteLock();

            try
            {
                bool? res = blockList.Reallocate(segment, requestedParagraphs);
                if (res == true)
                {
                    vm.Processor.Flags.Carry = false;
                    // MS-DOS does this in its implementation
                    vm.Processor.AX = (short)segment;
                }
                else if (res == false)
                {
                    vm.Processor.Flags.Carry = true;
                    vm.Processor.AX = 8;
                    vm.Processor.BX = (short)blockList.GetMaximumBlockSize(segment);
                    blockList.Reallocate(segment, (ushort)vm.Processor.BX);
                }
                else
                {
                    vm.Processor.Flags.Carry = true;
                    vm.Processor.AX = 7;
                }
            }
            finally
            {
                listLock.ExitWriteLock();
            }
        }
        /// <summary>
        /// Writes the segment of the current PSP into the BX register.
        /// </summary>
        public void GetCurrentPsp()
        {
            vm.Processor.BX = (short)this.CurrentProcessId;
        }
        /// <summary>
        /// Creates a new child PSP in emulated RAM.
        /// </summary>
        public void CreateChildPsp()
        {
            ushort pspSegment = (ushort)vm.Processor.DX;
            // Create the new process object and reassign the environment block
            // so the process is marked as the owner.

            var newProcess = this.CurrentProcess.CreateChildProcess(pspSegment);
            this.allProcesses[pspSegment] = newProcess;
            var pspBytes = BuildPsp(newProcess, this.CurrentProcessId, vm.Processor.SI);

            var pspPtr = vm.PhysicalMemory.GetPointer(pspSegment, 0);
            Marshal.Copy(pspBytes, 0, pspPtr, pspBytes.Length);
        }
        /// <summary>
        /// Ends a process and optionally frees its memory.
        /// </summary>
        /// <param name="residentParagraphs">Number of paragraphs to stay resident in memory.</param>
        public void EndCurrentProcess(int residentParagraphs)
        {
            var currentProcess = this.CurrentProcess;
            if (currentProcess == null)
                return;

            ushort currentId = this.CurrentProcessId;

            listLock.EnterWriteLock();

            try
            {
                var processBlocks = from block in this.blockList.UsedBlocks
                                    where block.PspSegment == this.CurrentProcess.PrefixSegment
                                    select block;

                if (residentParagraphs == 0)
                {
                    var segments = from block in processBlocks
                                   select block.Segment + 1;

                    var segmentList = new List<int>(segments);
                    foreach (int segment in segmentList)
                        blockList.Free((ushort)segment);

                    this.allProcesses.Remove(currentId);
                }
                else
                {
                    var mainBlock = (from block in processBlocks
                                     where block.PspSegment == block.Segment + 1
                                     select block).First();

                    var otherSegments = from block in processBlocks
                                        where block.PspSegment != block.Segment + 1
                                        select block.Segment + 1;

                    var segmentList = new List<int>(otherSegments);
                    foreach (int segment in segmentList)
                        blockList.Free((ushort)segment);

                    this.blockList.Reallocate((ushort)(mainBlock.Segment + 1), (ushort)residentParagraphs);
                }
            }
            finally
            {
                listLock.ExitWriteLock();
            }

            // AL stores the current errorlevel.
            this.ErrorLevel = vm.Processor.AL;

            // Restore the processor state.
            vm.Processor.SetCurrentState(currentProcess.InitialProcessorState);
            this.CurrentProcessId = GetParentProcessId(currentId);
        }
        /// <summary>
        /// Returns an object containing information about current allocations.
        /// </summary>
        /// <returns>Information about current allocations.</returns>
        public ConventionalMemoryInfo GetAllocations()
        {
            List<MemoryControlBlock> usedBlocks;
            int largestFree;

            listLock.EnterReadLock();
            try
            {
                usedBlocks = new List<MemoryControlBlock>(blockList.UsedBlocks);
                largestFree = blockList.LargestFreeBlock * 16;
            }
            finally
            {
                listLock.ExitReadLock();
            }

            var sizes = new Dictionary<string, int>();
            foreach (var block in usedBlocks)
            {
                if (!sizes.TryGetValue(block.ImageName, out int size))
                    sizes[block.ImageName] = block.Length * 16;
                else
                    sizes[block.ImageName] = size + block.Length * 16;
            }

            var processes = from s in sizes
                            select new ProcessAllocation(s.Key, s.Value);

            return new ConventionalMemoryInfo(processes, largestFree);
        }
        /// <summary>
        /// Releases resources used by the instance.
        /// </summary>
        public void Dispose()
        {
            if (!this.disposed)
            {
                this.disposed = true;
                listLock.Dispose();
            }
        }

        /// <summary>
        /// Returns a byte array containing a process's environment block.
        /// </summary>
        /// <returns>Byte array containing the process's environment block.</returns>
        private byte[] BuildEnvironmentBlock()
        {
            byte[] environmentStrings = vm.EnvironmentVariables.GetEnvironmentBlock();
            // Need 2 bytes between strings and path and a null terminator after path.
            byte[] fullBlock = new byte[environmentStrings.Length + 2];

            environmentStrings.CopyTo(fullBlock, 0);

            // Not sure what this is for.
            fullBlock[environmentStrings.Length] = 1;

            return fullBlock;
        }
        /// <summary>
        /// Returns the parent process ID of a specified process ID.
        /// </summary>
        /// <param name="processId">Process ID whose parent ID will be returned.</param>
        /// <returns>Parent process ID of the specified process ID.</returns>
        private ushort GetParentProcessId(ushort processId)
        {
            var ptr = vm.PhysicalMemory.GetPointer(processId, 0);
            unsafe
            {
                var psp = (ProgramSegmentPrefix*)ptr.ToPointer();
                return psp->ParentProcessId;
            }
        }
        /// <summary>
        /// Returns an array containing the environment block at the specified segment.
        /// </summary>
        /// <param name="blockSegment">Segment of block to read.</param>
        /// <returns>Environment block contained at the specified segment.</returns>
        private byte[] ReadEnvironmentBlock(ushort blockSegment)
        {
            unsafe
            {
                byte* ptr = (byte*)vm.PhysicalMemory.GetPointer(blockSegment, 0).ToPointer();
                for (int i = 1; i < 4096; i++)
                {
                    if (ptr[i] == 0 && ptr[i - 1] == 0)
                    {
                        var block = new byte[i + 1];
                        Marshal.Copy(new IntPtr(ptr), block, 0, block.Length);
                        return block;
                    }
                }
            }

            throw new InvalidOperationException();
        }

        /// <summary>
        /// Returns a byte array containing the program segment prefix for a process.
        /// </summary>
        /// <param name="process">Process to generate prefix information for.</param>
        /// <param name="parentPsp">Segment of the parent process's PSP.</param>
        /// <returns>Byte array containing the process's PSP./</returns>
        private static byte[] BuildPsp(DosProcess process, ushort parentPsp, ushort memorySize = 0x9FFF)
        {
            byte[] pspArray = new byte[256];

            unsafe
            {
                fixed (byte* pspPointer = pspArray)
                {
                    var psp = (ProgramSegmentPrefix*)pspPointer;
                    *psp = ProgramSegmentPrefix.CreateDefault();
                    psp->EndAddress = memorySize;
                    psp->ParentProcessId = parentPsp;
                    psp->EnvironmentSegment = process.EnvironmentSegment;
                    psp->CommandLineLength = (byte)(process.CommandLineArguments ?? "").Length;
                    var cmdLineBytes = Encoding.ASCII.GetBytes(process.CommandLineArguments);
                    Marshal.Copy(cmdLineBytes, 0, new IntPtr(psp->CommandLine), cmdLineBytes.Length);
                    psp->CommandLine[cmdLineBytes.Length] = 0x0D;
                    psp->HandleTableSegment = process.PrefixSegment;
                    psp->HandleTableOffset = (ushort)(sizeof(ProgramSegmentPrefix) - 32);
                }
            }

            return pspArray;
        }
    }
}
