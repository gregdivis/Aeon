using System;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;
using Aeon.Emulator.RuntimeExceptions;

namespace Aeon.Emulator.Memory
{
    /// <summary>
    /// Provides read/write access to emulated physical memory with paging.
    /// </summary>
    internal sealed class PagedMemoryAccessor : MemoryAccessor, IDisposable
    {
        /// <summary>
        /// The linear address of the page table directory.
        /// </summary>
        private uint directoryAddress;
        /// <summary>
        /// Array of cached physical page addresses.
        /// </summary>
        private unsafe readonly uint* pageCache;
        /// <summary>
        /// Manages the page cache's memory allocation.
        /// </summary>
        private readonly NativeMemory nativeMemory;

        /// <summary>
        /// Size of the page address cache in bytes.
        /// </summary>
        private const int CacheSize = 1 << 20;
        /// <summary>
        /// Bit in page table entry which indicates that a page is present.
        /// </summary>
        private const uint PagePresent = 1 << 0;

        public PagedMemoryAccessor(IntPtr rawView)
            : base(rawView)
        {
            unsafe
            {
                this.nativeMemory = new NativeMemory(CacheSize * sizeof(uint));
                this.pageCache = (uint*)this.nativeMemory.Pointer.ToPointer();
            }
        }

        /// <summary>
        /// Gets or sets the linear address of the page table directory.
        /// </summary>
        public uint DirectoryAddress
        {
            get => this.directoryAddress;
            set
            {
                this.directoryAddress = value;
                this.FlushCache();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public override byte GetByte(uint address)
        {
            address = GetPhysicalAddress(address, PageFaultCause.Read);

            if ((address & PhysicalMemory.VramMask) != PhysicalMemory.VramAddress)
            {
                unsafe
                {
                    return RawView[address];
                }
            }
            else
            {
                return video.GetVramByte(address - PhysicalMemory.VramAddress);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public override void SetByte(uint address, byte value)
        {
            address = GetPhysicalAddress(address, PageFaultCause.Write);

            if ((address & PhysicalMemory.VramMask) != PhysicalMemory.VramAddress)
            {
                unsafe
                {
                    RawView[address] = value;
                }
            }
            else
            {
                video.SetVramByte(address - PhysicalMemory.VramAddress, value);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public override ushort GetUInt16(uint address)
        {
            uint baseAddress = GetPhysicalAddress(address, PageFaultCause.Read);

            if ((address & PhysicalMemory.VramMask) != PhysicalMemory.VramAddress)
            {
                unsafe
                {
                    if ((baseAddress & 0xFFFu) != 0xFFEu)
                    {
                        return *(ushort*)(RawView + baseAddress);
                    }
                    else
                    {
                        byte* buf = stackalloc byte[2];
                        buf[0] = RawView[baseAddress];
                        buf[1] = RawView[GetPhysicalAddress(address + 1u, PageFaultCause.Read)];
                        return *(ushort*)buf;
                    }
                }
            }
            else
            {
                return this.video.GetVramWord(baseAddress - PhysicalMemory.VramAddress);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public override void SetUInt16(uint address, ushort value)
        {
            uint baseAddress = GetPhysicalAddress(address, PageFaultCause.Write);

            if ((address & PhysicalMemory.VramMask) != PhysicalMemory.VramAddress)
            {
                unsafe
                {
                    if ((baseAddress & 0xFFFu) != 0xFFEu)
                    {
                        *(ushort*)(RawView + baseAddress) = value;
                    }
                    else
                    {
                        byte* buf = (byte*)&value;
                        RawView[baseAddress] = buf[0];
                        RawView[GetPhysicalAddress(address + 1u, PageFaultCause.Write)] = buf[1];
                    }
                }
            }
            else
            {
                this.video.SetVramWord(address - PhysicalMemory.VramAddress, value);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public override uint GetUInt32(uint address)
        {
            uint baseAddress = GetPhysicalAddress(address, PageFaultCause.Read);

            if ((address & PhysicalMemory.VramMask) != PhysicalMemory.VramAddress)
            {
                unsafe
                {
                    if ((baseAddress & 0xFFFu) < 4096u - 4u)
                    {
                        return *(uint*)(RawView + baseAddress);
                    }
                    else
                    {
                        byte* buf = stackalloc byte[4];
                        buf[0] = RawView[baseAddress];
                        buf[1] = RawView[GetPhysicalAddress(address + 1u, PageFaultCause.Read)];
                        buf[2] = RawView[GetPhysicalAddress(address + 2u, PageFaultCause.Read)];
                        buf[3] = RawView[GetPhysicalAddress(address + 3u, PageFaultCause.Read)];
                        return *(uint*)buf;
                    }
                }
            }
            else
            {
                return this.video.GetVramDWord(baseAddress - PhysicalMemory.VramAddress);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public override void SetUInt32(uint address, uint value)
        {
            uint baseAddress = GetPhysicalAddress(address, PageFaultCause.Write);

            if ((address & PhysicalMemory.VramMask) != PhysicalMemory.VramAddress)
            {
                unsafe
                {
                    if ((baseAddress & 0xFFFu) < 4096u - 4u)
                    {
                        *(uint*)(RawView + baseAddress) = value;
                    }
                    else
                    {
                        byte* buf = (byte*)&value;
                        RawView[baseAddress] = buf[0];
                        RawView[GetPhysicalAddress(address + 1u, PageFaultCause.Write)] = buf[1];
                        RawView[GetPhysicalAddress(address + 2u, PageFaultCause.Write)] = buf[2];
                        RawView[GetPhysicalAddress(address + 3u, PageFaultCause.Write)] = buf[3];
                    }
                }
            }
            else
            {
                this.video.SetVramDWord(address - PhysicalMemory.VramAddress, value);
                return;
            }
        }
        public override ulong GetUInt64(uint address)
        {
            uint baseAddress = GetPhysicalAddress(address, PageFaultCause.Read);

            unsafe
            {
                if ((baseAddress & 0xFFFu) < 4096u - 8u)
                {
                    return *(ulong*)(RawView + baseAddress);
                }
                else
                {
                    byte* buf = stackalloc byte[8];
                    buf[0] = RawView[baseAddress];
                    buf[1] = RawView[GetPhysicalAddress(address + 1u, PageFaultCause.Read)];
                    buf[2] = RawView[GetPhysicalAddress(address + 2u, PageFaultCause.Read)];
                    buf[3] = RawView[GetPhysicalAddress(address + 3u, PageFaultCause.Read)];
                    buf[4] = RawView[GetPhysicalAddress(address + 4u, PageFaultCause.Read)];
                    buf[5] = RawView[GetPhysicalAddress(address + 5u, PageFaultCause.Read)];
                    buf[6] = RawView[GetPhysicalAddress(address + 6u, PageFaultCause.Read)];
                    buf[7] = RawView[GetPhysicalAddress(address + 7u, PageFaultCause.Read)];
                    return *(ulong*)buf;
                }
            }
        }
        public override void SetUInt64(uint address, ulong value)
        {
            uint baseAddress = GetPhysicalAddress(address, PageFaultCause.Write);

            unsafe
            {
                if ((baseAddress & 0xFFFu) < 4096u - 8u)
                {
                    *(ulong*)(RawView + baseAddress) = value;
                }
                else
                {
                    byte* buf = (byte*)&value;
                    RawView[baseAddress] = buf[0];
                    RawView[GetPhysicalAddress(address + 1u, PageFaultCause.Write)] = buf[1];
                    RawView[GetPhysicalAddress(address + 2u, PageFaultCause.Write)] = buf[2];
                    RawView[GetPhysicalAddress(address + 3u, PageFaultCause.Write)] = buf[3];
                    RawView[GetPhysicalAddress(address + 4u, PageFaultCause.Write)] = buf[4];
                    RawView[GetPhysicalAddress(address + 5u, PageFaultCause.Write)] = buf[5];
                    RawView[GetPhysicalAddress(address + 6u, PageFaultCause.Write)] = buf[6];
                    RawView[GetPhysicalAddress(address + 7u, PageFaultCause.Write)] = buf[7];
                }
            }
        }
        public override float GetReal32(uint address)
        {
            uint baseAddress = GetPhysicalAddress(address, PageFaultCause.Read);

            unsafe
            {
                if ((baseAddress & 0xFFFu) < 4096u - 4u)
                {
                    return *(float*)(RawView + baseAddress);
                }
                else
                {
                    byte* buf = stackalloc byte[4];
                    buf[0] = RawView[baseAddress];
                    buf[1] = RawView[GetPhysicalAddress(address + 1u, PageFaultCause.Read)];
                    buf[2] = RawView[GetPhysicalAddress(address + 2u, PageFaultCause.Read)];
                    buf[3] = RawView[GetPhysicalAddress(address + 3u, PageFaultCause.Read)];
                    return *(float*)buf;
                }
            }
        }
        public override void SetReal32(uint address, float value)
        {
            uint baseAddress = GetPhysicalAddress(address, PageFaultCause.Write);

            unsafe
            {
                if ((baseAddress & 0xFFFu) < 4096u - 4u)
                {
                    *(float*)(RawView + baseAddress) = value;
                }
                else
                {
                    byte* buf = (byte*)&value;
                    RawView[baseAddress] = buf[0];
                    RawView[GetPhysicalAddress(address + 1u, PageFaultCause.Write)] = buf[1];
                    RawView[GetPhysicalAddress(address + 2u, PageFaultCause.Write)] = buf[2];
                    RawView[GetPhysicalAddress(address + 3u, PageFaultCause.Write)] = buf[3];
                }
            }
        }
        public override double GetReal64(uint address)
        {
            uint baseAddress = GetPhysicalAddress(address, PageFaultCause.Read);

            unsafe
            {
                if ((baseAddress & 0xFFFu) < 4096u - 8u)
                {
                    return *(double*)(RawView + baseAddress);
                }
                else
                {
                    byte* buf = stackalloc byte[8];
                    buf[0] = RawView[baseAddress];
                    buf[1] = RawView[GetPhysicalAddress(address + 1u, PageFaultCause.Read)];
                    buf[2] = RawView[GetPhysicalAddress(address + 2u, PageFaultCause.Read)];
                    buf[3] = RawView[GetPhysicalAddress(address + 3u, PageFaultCause.Read)];
                    buf[4] = RawView[GetPhysicalAddress(address + 4u, PageFaultCause.Read)];
                    buf[5] = RawView[GetPhysicalAddress(address + 5u, PageFaultCause.Read)];
                    buf[6] = RawView[GetPhysicalAddress(address + 6u, PageFaultCause.Read)];
                    buf[7] = RawView[GetPhysicalAddress(address + 7u, PageFaultCause.Read)];
                    return *(double*)buf;
                }
            }
        }
        public override void SetReal64(uint address, double value)
        {
            uint baseAddress = GetPhysicalAddress(address, PageFaultCause.Write);

            unsafe
            {
                if ((baseAddress & 0xFFFu) < 4096u - 8u)
                {
                    *(double*)(RawView + baseAddress) = value;
                }
                else
                {
                    byte* buf = (byte*)&value;
                    RawView[baseAddress] = buf[0];
                    RawView[GetPhysicalAddress(address + 1u, PageFaultCause.Write)] = buf[1];
                    RawView[GetPhysicalAddress(address + 2u, PageFaultCause.Write)] = buf[2];
                    RawView[GetPhysicalAddress(address + 3u, PageFaultCause.Write)] = buf[3];
                    RawView[GetPhysicalAddress(address + 4u, PageFaultCause.Write)] = buf[4];
                    RawView[GetPhysicalAddress(address + 5u, PageFaultCause.Write)] = buf[5];
                    RawView[GetPhysicalAddress(address + 6u, PageFaultCause.Write)] = buf[6];
                    RawView[GetPhysicalAddress(address + 7u, PageFaultCause.Write)] = buf[7];
                }
            }
        }
        public override Real10 GetReal80(uint address)
        {
            uint baseAddress = GetPhysicalAddress(address, PageFaultCause.Read);

            unsafe
            {
                if ((baseAddress & 0xFFFu) < 4096u - 10u)
                {
                    return *(Real10*)(RawView + baseAddress);
                }
                else
                {
                    byte* buf = stackalloc byte[10];
                    buf[0] = RawView[baseAddress];
                    buf[1] = RawView[GetPhysicalAddress(address + 1u, PageFaultCause.Read)];
                    buf[2] = RawView[GetPhysicalAddress(address + 2u, PageFaultCause.Read)];
                    buf[3] = RawView[GetPhysicalAddress(address + 3u, PageFaultCause.Read)];
                    buf[4] = RawView[GetPhysicalAddress(address + 4u, PageFaultCause.Read)];
                    buf[5] = RawView[GetPhysicalAddress(address + 5u, PageFaultCause.Read)];
                    buf[6] = RawView[GetPhysicalAddress(address + 6u, PageFaultCause.Read)];
                    buf[7] = RawView[GetPhysicalAddress(address + 7u, PageFaultCause.Read)];
                    buf[8] = RawView[GetPhysicalAddress(address + 8u, PageFaultCause.Read)];
                    buf[9] = RawView[GetPhysicalAddress(address + 9u, PageFaultCause.Read)];
                    return *(Real10*)buf;
                }
            }
        }
        public override void SetReal80(uint address, Real10 value)
        {
            uint baseAddress = GetPhysicalAddress(address, PageFaultCause.Write);

            unsafe
            {
                if ((baseAddress & 0xFFFu) < 4096u - 10u)
                {
                    *(Real10*)(RawView + baseAddress) = value;
                }
                else
                {
                    byte* buf = (byte*)&value;
                    RawView[baseAddress] = buf[0];
                    RawView[GetPhysicalAddress(address + 1u, PageFaultCause.Write)] = buf[1];
                    RawView[GetPhysicalAddress(address + 2u, PageFaultCause.Write)] = buf[2];
                    RawView[GetPhysicalAddress(address + 3u, PageFaultCause.Write)] = buf[3];
                    RawView[GetPhysicalAddress(address + 4u, PageFaultCause.Write)] = buf[4];
                    RawView[GetPhysicalAddress(address + 5u, PageFaultCause.Write)] = buf[5];
                    RawView[GetPhysicalAddress(address + 6u, PageFaultCause.Write)] = buf[6];
                    RawView[GetPhysicalAddress(address + 7u, PageFaultCause.Write)] = buf[7];
                    RawView[GetPhysicalAddress(address + 8u, PageFaultCause.Write)] = buf[8];
                    RawView[GetPhysicalAddress(address + 9u, PageFaultCause.Write)] = buf[9];
                }
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public override unsafe void FetchInstruction(uint address, byte* buffer)
        {
            uint baseAddress = GetPhysicalAddress(address, PageFaultCause.InstructionFetch);

            if ((address & 0xFFFu) < 4096 - 16)
            {
                *(ulong*)buffer = *(ulong*)(RawView + baseAddress);
                ((ulong*)buffer)[1] = ((ulong*)(RawView + baseAddress))[1];
            }
            else
            {
                for (uint i = 0; i < 16; i++)
                {
                    buffer[i] = RawView[baseAddress];
                    baseAddress = GetPhysicalAddress(address + i + 1u, PageFaultCause.InstructionFetch);
                }
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public override unsafe void* GetSafePointer(uint address, uint size)
        {
            uint baseAddress = GetPhysicalAddress(address, PageFaultCause.Read);
            if ((address & 0xFFFu) + size > 4096)
                GetPhysicalAddress(address + 4096u, PageFaultCause.Read);

            return RawView + baseAddress;
        }
        /// <summary>
        /// Flushes the page address cache.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public void FlushCache() => this.nativeMemory.Clear();
        /// <summary>
        /// Returns the physical address from a paged logical address.
        /// </summary>
        /// <param name="logicalAddress">Logical address to resolve.</param>
        /// <returns>Physical address of the supplied logical address.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetPhysicalAddress(uint logicalAddress) => GetPhysicalAddress(logicalAddress, PageFaultCause.Read);
        public void Dispose() => this.nativeMemory.Dispose();

        /// <summary>
        /// Returns the physical address from a paged linear address.
        /// </summary>
        /// <param name="linearAddress">Paged linear address.</param>
        /// <param name="operation">Type of operation attempted in case of a page fault.</param>
        /// <returns>Physical address of the supplied linear address.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private uint GetPhysicalAddress(uint linearAddress, PageFaultCause operation)
        {
            uint pageCacheIndex = linearAddress >> 12;

            unsafe
            {
                if (this.pageCache[pageCacheIndex] != 0)
                    return this.pageCache[pageCacheIndex] | (linearAddress & 0xFFFu);
            }

            uint baseAddress = linearAddress & 0xFFFFFC00u;

            var physicalAddress = GetPage(linearAddress, operation);

            unsafe
            {
                this.pageCache[pageCacheIndex] = physicalAddress;
            }

            return physicalAddress | (linearAddress & 0xFFFu);
        }
        /// <summary>
        /// Looks up a page's physical address.
        /// </summary>
        /// <param name="linearAddress">Paged linear address.</param>
        /// <param name="operation">Type of operation attempted in case of a page fault.</param>
        /// <returns>Physical address of the page.</returns>
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
        private uint GetPage(uint linearAddress, PageFaultCause operation)
        {
            uint page;
            if (Bmi1.IsSupported)
                page = Bmi1.BitFieldExtract(linearAddress, 0x0A0C);
            else
                page = (linearAddress >> 12) & 0x3FFu;

            uint dir = linearAddress >> 22;

            unsafe
            {
                uint* dirPtr = (uint*)(RawView + directoryAddress);
                if ((dirPtr[dir] & PagePresent) == 0)
                    throw new PageFaultException(linearAddress, operation);

                uint pageAddress = dirPtr[dir] & 0xFFFFF000u;
                uint* pagePtr = (uint*)(RawView + pageAddress);
                if ((pagePtr[page] & PagePresent) == 0)
                    throw new PageFaultException(linearAddress, operation);

                return pagePtr[page] & 0xFFFFF000u;
            }
        }
    }
}
