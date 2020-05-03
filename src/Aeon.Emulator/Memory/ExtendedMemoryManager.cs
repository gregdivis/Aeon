using System;
using System.Collections.Generic;
using System.Linq;

namespace Aeon.Emulator.Memory
{
    /// <summary>
    /// Provides DOS applications with XMS memory.
    /// </summary>
    internal sealed class ExtendedMemoryManager : IMultiplexInterruptHandler, ICallbackProvider, IInputPort, IOutputPort
    {
        private VirtualMachine vm;
        private RealModeAddress callbackAddress;
        private int a20EnableCount;
        private readonly LinkedList<XmsBlock> xms = new LinkedList<XmsBlock>();
        private readonly SortedList<int, int> handles = new SortedList<int, int>();

        /// <summary>
        /// Specifies the starting physical address of XMS.
        /// </summary>
        private const uint XmsBaseAddress = PhysicalMemory.ConvMemorySize + 65536 + 0x4000 + 1024 * 1024;
        /// <summary>
        /// Total number of handles available at once.
        /// </summary>
        private const int MaxHandles = 128;

        /// <summary>
        /// Gets the largest free block of memory in bytes.
        /// </summary>
        public uint LargestFreeBlock => this.GetFreeBlocks().FirstOrDefault().Length;
        /// <summary>
        /// Gets the total amount of free memory in bytes.
        /// </summary>
        public long TotalFreeMemory => this.GetFreeBlocks().Sum(b => (long)b.Length);
        /// <summary>
        /// Gets the total amount of extended memory.
        /// </summary>
        public int ExtendedMemorySize => this.vm.PhysicalMemory.MemorySize - (int)XmsBaseAddress;

        IEnumerable<int> IInputPort.InputPorts => new int[] { 0x92 };
        IEnumerable<int> IOutputPort.OutputPorts => new int[] { 0x92 };
        int IMultiplexInterruptHandler.Identifier => 0x43;
        bool ICallbackProvider.IsHookable => true;
        RealModeAddress ICallbackProvider.CallbackAddress
        {
            set => this.callbackAddress = value;
        }

        void IMultiplexInterruptHandler.HandleInterrupt()
        {
            switch (vm.Processor.AL)
            {
                case XmsHandlerFunctions.InstallationCheck:
                    vm.Processor.AL = 0x80;
                    break;

                case XmsHandlerFunctions.GetCallbackAddress:
                    vm.Processor.BX = (short)this.callbackAddress.Offset;
                    vm.WriteSegmentRegister(SegmentIndex.ES, this.callbackAddress.Segment);
                    break;

                default:
                    throw new NotImplementedException($"XMS interrupt handler function {vm.Processor.AL:X2}h not implemented.");
            }
        }
        void ICallbackProvider.InvokeCallback()
        {
            switch (vm.Processor.AH)
            {
                case XmsFunctions.GetVersionNumber:
                    vm.Processor.AX = 0x0300; // Return version 3.00
                    vm.Processor.BX = 0; // Internal version
                    vm.Processor.DX = 1; // HMA exists
                    break;

                case XmsFunctions.RequestHighMemoryArea:
                    vm.Processor.AX = 0; // Didn't work
                    vm.Processor.BL = 0x91; // HMA already in use
                    break;

                case XmsFunctions.ReleaseHighMemoryArea:
                    vm.Processor.AX = 0; // Didn't work
                    vm.Processor.BL = 0x93; // HMA not allocated
                    break;

                case XmsFunctions.GlobalEnableA20:
                    vm.PhysicalMemory.EnableA20 = true;
                    vm.Processor.AX = 1; // Success
                    break;

                case XmsFunctions.GlobalDisableA20:
                    vm.PhysicalMemory.EnableA20 = false;
                    vm.Processor.AX = 1; // Success
                    break;

                case XmsFunctions.LocalEnableA20:
                    EnableLocalA20();
                    vm.Processor.AX = 1; // Success
                    break;

                case XmsFunctions.LocalDisableA20:
                    DisableLocalA20();
                    vm.Processor.AX = 1; // Success
                    break;

                case XmsFunctions.QueryA20:
                    vm.Processor.AX = (this.a20EnableCount > 0) ? (short)1 : (short)0;
                    break;

                case XmsFunctions.QueryFreeExtendedMemory:
                    if (this.LargestFreeBlock <= ushort.MaxValue * 1024u)
                        vm.Processor.AX = (short)(this.LargestFreeBlock / 1024u);
                    else
                        vm.Processor.AX = unchecked((short)ushort.MaxValue);

                    if (this.TotalFreeMemory <= ushort.MaxValue * 1024u)
                        vm.Processor.DX = (short)(this.TotalFreeMemory / 1024);
                    else
                        vm.Processor.DX = unchecked((short)ushort.MaxValue);

                    if (vm.Processor.AX == 0 && vm.Processor.DX == 0)
                        vm.Processor.BL = 0xA0;
                    break;

                case XmsFunctions.AllocateExtendedMemoryBlock:
                    AllocateBlock((ushort)vm.Processor.DX);
                    break;

                case XmsFunctions.FreeExtendedMemoryBlock:
                    FreeBlock();
                    break;

                case XmsFunctions.LockExtendedMemoryBlock:
                    LockBlock();
                    break;

                case XmsFunctions.UnlockExtendedMemoryBlock:
                    UnlockBlock();
                    break;

                case XmsFunctions.GetHandleInformation:
                    GetHandleInformation();
                    break;

                case XmsFunctions.MoveExtendedMemoryBlock:
                    MoveMemoryBlock();
                    break;

                case XmsFunctions.RequestUpperMemoryBlock:
                    vm.Processor.BL = 0xB1; // No UMB's available.
                    vm.Processor.AX = 0; // Didn't work.
                    break;

                case XmsFunctions.QueryAnyFreeExtendedMemory:
                    QueryAnyFreeExtendedMemory();
                    break;

                case XmsFunctions.AllocateAnyExtendedMemory:
                    AllocateBlock((uint)vm.Processor.EDX);
                    break;

                default:
                    throw new NotImplementedException($"XMS function {vm.Processor.AH:X2}h not implemented.");
            }
        }
        byte IInputPort.ReadByte(int port) => this.vm.PhysicalMemory.EnableA20 ? (byte)0x02 : (byte)0x00;
        ushort IInputPort.ReadWord(int port) => throw new NotSupportedException();
        void IOutputPort.WriteByte(int port, byte value) => this.vm.PhysicalMemory.EnableA20 = (value & 0x02) != 0;
        void IOutputPort.WriteWord(int port, ushort value) => throw new NotSupportedException();
        void IVirtualDevice.Pause()
        {
        }
        void IVirtualDevice.Resume()
        {
        }
        void IVirtualDevice.DeviceRegistered(VirtualMachine vm)
        {
            this.vm = vm;
            InitializeMemoryMap();
        }
        void IDisposable.Dispose()
        {
        }

        /// <summary>
        /// Increments the A20 enable count.
        /// </summary>
        private void EnableLocalA20()
        {
            if (this.a20EnableCount == 0)
                this.vm.PhysicalMemory.EnableA20 = true;

            this.a20EnableCount++;
        }
        /// <summary>
        /// Decrements the A20 enable count.
        /// </summary>
        private void DisableLocalA20()
        {
            if (this.a20EnableCount == 1)
                this.vm.PhysicalMemory.EnableA20 = false;

            if (this.a20EnableCount > 0)
                this.a20EnableCount--;
        }
        /// <summary>
        /// Initializes the internal memory map.
        /// </summary>
        private void InitializeMemoryMap()
        {
            if (this.xms.Count != 0)
                throw new InvalidOperationException();

            uint memoryAvailable = (uint)vm.PhysicalMemory.MemorySize - XmsBaseAddress;
            this.xms.AddFirst(new XmsBlock(0, 0, memoryAvailable, false));
        }
        /// <summary>
        /// Returns all of the free blocks in the map sorted by size in ascending order.
        /// </summary>
        /// <returns>Sorted list of free blocks in the map.</returns>
        private IEnumerable<XmsBlock> GetFreeBlocks()
        {
            return from b in this.xms
                   where !b.IsUsed
                   orderby b.Length
                   select b;
        }
        /// <summary>
        /// Returns the block with the specified handle if found; otherwise returns null.
        /// </summary>
        /// <param name="handle">Handle of block to search for.</param>
        /// <returns>Block with the specified handle if found; otherwise returns null.</returns>
        private XmsBlock? TryGetBlock(int handle)
        {
            return (from b in this.xms
                    where b.IsUsed && b.Handle == handle
                    select new XmsBlock?(b)).FirstOrDefault();
        }
        /// <summary>
        /// Returns the next available handle for an allocation on success; returns 0 if no handles are available.
        /// </summary>
        /// <returns>New handle if available; otherwise returns null.</returns>
        private int GetNextHandle()
        {
            for (int i = 1; i <= MaxHandles; i++)
            {
                if (!this.handles.ContainsKey(i))
                    return i;
            }

            return 0;
        }
        /// <summary>
        /// Attempts to merge a free block with the following block if possible.
        /// </summary>
        /// <param name="firstBlock">Free block to merge.</param>
        private void MergeFreeBlocks(XmsBlock firstBlock)
        {
            var firstNode = this.xms.Find(firstBlock);

            if (firstNode.Next != null)
            {
                var nextNode = firstNode.Next;
                if (!nextNode.Value.IsUsed)
                {
                    var newBlock = firstBlock.Join(nextNode.Value);
                    this.xms.Remove(nextNode);
                    this.xms.Replace(firstBlock, newBlock);
                }
            }
        }
        /// <summary>
        /// Allocates a new block of memory.
        /// </summary>
        /// <param name="kbytes">Number of kilobytes requested.</param>
        private void AllocateBlock(uint kbytes)
        {
            int handle = GetNextHandle();
            if (handle == 0)
            {
                vm.Processor.AX = 0; // Didn't work.
                vm.Processor.BL = 0xA1; // All handles are used.
                return;
            }

            uint length = kbytes * 1024u;

            // Zero-length allocations are allowed.
            if (length == 0)
            {
                this.handles.Add(handle, 0);
                vm.Processor.AX = 1; // Success.
                vm.Processor.DX = (short)handle;
                return;
            }

            var smallestFreeBlock = (from b in this.GetFreeBlocks()
                                     where b.Length >= length
                                     select new Nullable<XmsBlock>(b)).FirstOrDefault();

            if (smallestFreeBlock == null)
            {
                vm.Processor.AX = 0; // Didn't work.
                vm.Processor.BL = 0xA0; // Not enough free memory.
                return;
            }

            var freeNode = this.xms.Find(smallestFreeBlock.Value);

            var newNodes = freeNode.Value.Allocate(handle, length);
            this.xms.Replace((XmsBlock)smallestFreeBlock, newNodes);

            this.handles.Add(handle, 0);

            vm.Processor.AX = 1; // Success.
            vm.Processor.DX = (short)handle;
        }
        /// <summary>
        /// Frees a block of memory.
        /// </summary>
        private void FreeBlock()
        {
            int handle = (ushort)vm.Processor.DX;

            int lockCount;
            if (!this.handles.TryGetValue(handle, out lockCount))
            {
                vm.Processor.AX = 0; // Didn't work.
                vm.Processor.BL = 0xA2; // Invalid handle.
                return;
            }

            if (lockCount > 0)
            {
                vm.Processor.AX = 0; // Didn't work.
                vm.Processor.BL = 0xAB; // Handle is locked.
                return;
            }

            var maybeBlock = this.TryGetBlock(handle);
            if (maybeBlock != null)
            {
                var block = (XmsBlock)maybeBlock;
                var freeBlock = block.Free();
                this.xms.Replace(block, freeBlock);
                this.MergeFreeBlocks(freeBlock);
            }

            this.handles.Remove(handle);
            vm.Processor.AX = 1; // Success.
        }
        /// <summary>
        /// Locks a block of memory.
        /// </summary>
        private void LockBlock()
        {
            int handle = (ushort)vm.Processor.DX;

            int lockCount;
            if (!this.handles.TryGetValue(handle, out lockCount))
            {
                vm.Processor.AX = 0; // Didn't work.
                vm.Processor.BL = 0xA2; // Invalid handle.
                return;
            }

            this.handles[handle] = lockCount + 1;

            var block = (XmsBlock)TryGetBlock(handle);
            uint fullAddress = XmsBaseAddress + block.Offset;

            vm.Processor.AX = 1; // Success.
            vm.Processor.DX = (short)(ushort)(fullAddress >> 16);
            vm.Processor.BX = (short)(fullAddress & 0xFFFFu);
        }
        /// <summary>
        /// Unlocks a block of memory.
        /// </summary>
        private void UnlockBlock()
        {
            int handle = (ushort)vm.Processor.DX;

            int lockCount;
            if (!this.handles.TryGetValue(handle, out lockCount))
            {
                vm.Processor.AX = 0; // Didn't work.
                vm.Processor.BL = 0xA2; // Invalid handle.
                return;
            }

            if (lockCount < 1)
            {
                vm.Processor.AX = 0;
                vm.Processor.BL = 0xAA; // Handle is not locked.
                return;
            }

            this.handles[handle] = lockCount - 1;

            vm.Processor.AX = 1; // Success.
        }
        /// <summary>
        /// Returns information about an XMS handle.
        /// </summary>
        private void GetHandleInformation()
        {
            int handle = (ushort)vm.Processor.DX;

            int lockCount;
            if (!this.handles.TryGetValue(handle, out lockCount))
            {
                vm.Processor.AX = 0; // Didn't work.
                vm.Processor.BL = 0xA2; // Invalid handle.
                return;
            }

            vm.Processor.BH = (byte)lockCount;
            vm.Processor.BL = (byte)(MaxHandles - this.handles.Count);

            var block = this.TryGetBlock(handle);
            if (block == null)
                vm.Processor.DX = 0;
            else
                vm.Processor.DX = (short)(((XmsBlock)block).Length / 1024u);

            vm.Processor.AX = 1; // Success.
        }
        /// <summary>
        /// Copies a block of memory.
        /// </summary>
        private void MoveMemoryBlock()
        {
            bool a20State = this.vm.PhysicalMemory.EnableA20;
            this.vm.PhysicalMemory.EnableA20 = true;

            XmsMoveData moveData;
            unsafe
            {
                moveData = *(XmsMoveData*)this.vm.PhysicalMemory.GetPointer(this.vm.Processor.DS, this.vm.Processor.SI);
            }

            IntPtr srcPtr = IntPtr.Zero;
            IntPtr destPtr = IntPtr.Zero;

            if (moveData.SourceHandle == 0)
            {
                var srcAddress = moveData.SourceAddress;
                srcPtr = this.vm.PhysicalMemory.GetPointer(srcAddress.Segment, srcAddress.Offset);
            }
            else
            {
                var srcBlock = this.TryGetBlock(moveData.SourceHandle);
                if (srcBlock != null)
                    srcPtr = this.vm.PhysicalMemory.GetPointer((int)(XmsBaseAddress + ((XmsBlock)srcBlock).Offset + moveData.SourceOffset));
            }

            if (moveData.DestHandle == 0)
            {
                var destAddress = moveData.DestAddress;
                destPtr = this.vm.PhysicalMemory.GetPointer(destAddress.Segment, destAddress.Offset);
            }
            else
            {
                var destBlock = this.TryGetBlock(moveData.DestHandle);
                if (destBlock != null)
                    destPtr = this.vm.PhysicalMemory.GetPointer((int)(XmsBaseAddress + ((XmsBlock)destBlock).Offset + moveData.DestOffset));
            }

            if (srcPtr == IntPtr.Zero)
            {
                vm.Processor.BL = 0xA3; // Invalid source handle.
                vm.Processor.AX = 0; // Didn't work.
                return;
            }
            if (destPtr == IntPtr.Zero)
            {
                vm.Processor.BL = 0xA5; // Invalid destination handle.
                vm.Processor.AX = 0; // Didn't work.
                return;
            }

            unsafe
            {
                byte* src = (byte*)srcPtr.ToPointer();
                byte* dest = (byte*)destPtr.ToPointer();

                for (uint i = 0; i < moveData.Length; i++)
                    dest[i] = src[i];
            }

            vm.Processor.AX = 1; // Success.
            this.vm.PhysicalMemory.EnableA20 = a20State;
        }
        /// <summary>
        /// Queries free memory using 32-bit registers.
        /// </summary>
        private void QueryAnyFreeExtendedMemory()
        {
            this.vm.Processor.EAX = (int)(this.LargestFreeBlock / 1024u);
            this.vm.Processor.ECX = this.vm.PhysicalMemory.MemorySize - 1;
            this.vm.Processor.EDX = (int)(uint)(this.TotalFreeMemory / 1024);

            if (this.vm.Processor.EAX == 0)
                this.vm.Processor.BL = 0xA0;
            else
                this.vm.Processor.BL = 0;
        }
    }
}
