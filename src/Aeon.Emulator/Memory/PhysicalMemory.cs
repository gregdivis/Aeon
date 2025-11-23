using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using System.Text;
using Aeon.Emulator.DebugSupport;
using Aeon.Emulator.Memory;
using Aeon.Emulator.RuntimeExceptions;

namespace Aeon.Emulator;

/// <summary>
/// Contains the RAM of an emulated system.
/// </summary>
public sealed class PhysicalMemory : IMemorySource
{
    /// <summary>
    /// Size of the page address cache in dwords.
    /// </summary>
    private const int PageAddressCacheSize = 1 << 20;
    /// <summary>
    /// Bit in page table entry which indicates that a page is present.
    /// </summary>
    private const uint PagePresent = 1 << 0;

    /// <summary>
    /// Array of cached physical page addresses.
    /// </summary>
    private unsafe uint* pageCache;

    /// <summary>
    /// Pointer to emulated physical memory.
    /// </summary>
    internal unsafe byte* RawView;

    /// <summary>
    /// The linear address of the page table directory.
    /// </summary>
    private uint directoryAddress;

    /// <summary>
    /// Address of the BIOS int15h C0 data table.
    /// </summary>
    internal static readonly RealModeAddress BiosConfigurationAddress = new(0xF000, 0x0100);
    /// <summary>
    /// Address of the default interrupt handler.
    /// </summary>
    internal static readonly RealModeAddress NullInterruptHandler = new(HandlerSegment, 4095);

    private ushort nextHandlerOffset = 4096;
    private uint addressMask = 0x000FFFFFu;
    private readonly MetaAllocator metaAllocator = new();

    /// <summary>
    /// Starting physical address of video RAM.
    /// </summary>
    private const int VramAddress = 0xA000 << 4;
    /// <summary>
    /// Last physical address of video RAM.
    /// </summary>

    /// <summary>
    /// The highest address which is mapped to <see cref="Video.VideoHandler"/>.
    /// </summary>
    /// <remarks>
    /// Video RAM mapping is technically up to 0xBFFF0 normally.
    /// </remarks>
    private const int VramUpperBound = 0xBFFF << 4;

    /// <summary>
    /// Segment where font data is stored.
    /// </summary>
    internal const ushort FontSegment = 0xC000;
    /// <summary>
    /// Offset into the font segment where the 8x8 font is found.
    /// </summary>
    internal const ushort Font8x8Offset = 0x0100;
    /// <summary>
    /// Offset into the font segment where the 8x14 font is found.
    /// </summary>
    internal const ushort Font8x14Offset = 0x0900;
    /// <summary>
    /// Offset into the font segment where the 8x16 font is found.
    /// </summary>
    internal const ushort Font8x16Offset = 0x1700;
    /// <summary>
    /// Size of conventional memory in bytes.
    /// </summary>
    internal const uint ConvMemorySize = 1024 * 1024;

    /// <summary>
    /// Segment for interrupt/callback proxies.
    /// </summary>
    private const ushort HandlerSegment = 0xF100;

    internal PhysicalMemory()
        : this(1024 * 1024 * 16)
    {
    }
    internal PhysicalMemory(int memorySize)
    {
        if (memorySize < ConvMemorySize)
            throw new ArgumentException("Memory size must be at least 1 MB.");

        this.MemorySize = memorySize;
        unsafe
        {
            this.RawView = (byte*)NativeMemory.AllocZeroed((nuint)memorySize, 1);
            this.pageCache = (uint*)NativeMemory.AllocZeroed(PageAddressCacheSize, 4);
        }

        // Reserve room for the real-mode interrupt table.
        this.Reserve(0x0000, 256 * 4);

        // Reserve VGA video RAM window.
        this.Reserve(0xA000, VramUpperBound - VramAddress + 16u);

        // Make sure there is an IRET instruction at the default interrupt handler address.
        SetByte(NullInterruptHandler.Segment, NullInterruptHandler.Offset, 0xCF);

        // Make sure most interrupts have a handler.
        // For some reason, at least one DOS extender freaks out if every interrupt is handled.
        for (int i = 0; i < 128; i++)
            SetInterruptAddress((byte)i, NullInterruptHandler.Segment, NullInterruptHandler.Offset);

        Bios = new Bios(this);
        InitializeFonts();
        InitializeBiosData();
    }

    ~PhysicalMemory() => this.InternalDispose();

    /// <summary>
    /// Gets the amount of emulated RAM in bytes.
    /// </summary>
    public int MemorySize { get; }
    /// <summary>
    /// Gets the location and size of base memory in the system.
    /// </summary>
    public ReservedBlock? BaseMemory { get; private set; }
    /// <summary>
    /// Gets the entire emulated RAM as a <see cref="Span{byte}"/>.
    /// </summary>
    public Span<byte> Span
    {
        get
        {
            unsafe
            {
                return new Span<byte>(this.RawView, this.MemorySize);
            }
        }
    }
    /// <summary>
    /// Gets the BIOS mapped regions of memory.
    /// </summary>
    internal Bios Bios { get; }
    /// <summary>
    /// Gets or sets the emulated video device.
    /// </summary>
    internal Video.VideoHandler? Video { get; set; }
    /// <summary>
    /// Gets or sets the current linear offset of the global descriptor table.
    /// </summary>
    internal uint GDTAddress { get; set; }
    /// <summary>
    /// Gets or sets the size of the GDT.
    /// </summary>
    internal uint GDTLimit { get; set; }
    /// <summary>
    /// Gets or sets the GDT selector of the current LDT.
    /// </summary>
    internal ushort LDTSelector { get; set; }
    /// <summary>
    /// Gets or sets the current linear offset of the interrupt descriptor table.
    /// </summary>
    internal uint IDTAddress { get; set; }
    /// <summary>
    /// Gets or sets the size of the IDT.
    /// </summary>
    internal uint IDTLimit { get; set; }
    /// <summary>
    /// Gets or sets the GDT selector of the current task.
    /// </summary>
    internal ushort TaskSelector { get; set; }
    /// <summary>
    /// Gets or sets the directory address for paging.
    /// </summary>
    internal uint DirectoryAddress
    {
        get => this.directoryAddress;
        set
        {
            this.directoryAddress = value;
            // flush the page cache
            unsafe
            {
                new Span<uint>(this.pageCache, PageAddressCacheSize).Clear();
            }
        }
    }
    /// <summary>
    /// Gets or sets a value indicating whether the A20 line is enabled.
    /// </summary>
    internal bool EnableA20
    {
        get => this.addressMask == uint.MaxValue;
        set => this.addressMask = value ? uint.MaxValue : 0x000FFFFFu;
    }
    /// <summary>
    /// Gets or sets a value indicating whether paging is enabled.
    /// </summary>
    internal bool PagingEnabled { get; set; }

    /// <summary>
    /// Reserves a block of conventional memory.
    /// </summary>
    /// <param name="minimumSegment">Minimum segment of requested memory block.</param>
    /// <param name="length">Size of memory block in bytes.</param>
    /// <returns>Information about the reserved block of memory.</returns>
    public ReservedBlock Reserve(ushort minimumSegment, uint length) => new(this.metaAllocator.Allocate(minimumSegment, (int)length), length);
    /// <summary>
    /// Returns the descriptor for the specified segment.
    /// </summary>
    /// <param name="segment">Segment whose descriptor is returned.</param>
    /// <returns>Descriptor of the specified segment.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public Descriptor GetDescriptor(uint segment)
    {
        uint selectorIndex = segment >> 3;
        uint baseAddress;

        // Check for local descriptor.
        if ((segment & 4) != 0)
        {
            unsafe
            {
                var ldtDescriptor = (SegmentDescriptor)GetDescriptor(this.LDTSelector & 0xFFF8u);
                baseAddress = ldtDescriptor.Base;
            }
        }
        else
        {
            baseAddress = this.GDTAddress;
        }

        unsafe
        {
            ulong value = GetUInt64(baseAddress + (selectorIndex * 8u));
            //ulong value = *(ulong*)(this.RawView + baseAddress + (selectorIndex * 8u));
            Descriptor* descriptor = (Descriptor*)&value;
            return *descriptor;
        }
    }
    /// <summary>
    /// Gets the address of an interrupt handler.
    /// </summary>
    /// <param name="interrupt">Interrupt to get handler address for.</param>
    /// <returns>Segment and offset of the interrupt handler.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public RealModeAddress GetRealModeInterruptAddress(byte interrupt)
    {
        ushort offset = GetUInt16(0, (ushort)(interrupt * 4));
        ushort segment = GetUInt16(0, (ushort)(interrupt * 4 + 2));
        return new RealModeAddress(segment, offset);
    }
    /// <summary>
    /// Returns an interrupt descriptor.
    /// </summary>
    /// <param name="interrupt">Interrupt whose descriptor is returned.</param>
    /// <returns>Interrupt descriptor for the specified interrupt.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public Descriptor GetInterruptDescriptor(byte interrupt)
    {
        unsafe
        {
            ulong value = GetUInt64(this.IDTAddress + (interrupt * 8u));
            //ulong value = *(ulong*)(this.RawView + this.IDTAddress + (interrupt * 8u));
            var descriptor = (Descriptor*)&value;
            return *descriptor;
        }
    }
    /// <summary>
    /// Gets a pointer to a location in the emulated memory.
    /// </summary>
    /// <param name="segment">Segment of pointer.</param>
    /// <param name="offset">Offset of pointer.</param>
    /// <returns>Pointer to the emulated location at segment:offset.</returns>
    public IntPtr GetPointer(uint segment, uint offset)
    {
        unsafe
        {
            return new IntPtr(RawView + GetRealModePhysicalAddress(segment, offset));
        }
    }
    /// <summary>
    /// Gets a pointer to a location in the emulated memory.
    /// </summary>
    /// <param name="address">Address of pointer.</param>
    /// <returns>Pointer to the specified address.</returns>
    public IntPtr GetPointer(int address)
    {
        address &= (int)addressMask;

        unsafe
        {
            return new IntPtr(RawView + address);
        }
    }
    public Span<byte> GetSpan(uint address, int length)
    {
        address &= addressMask;

        unsafe
        {
            return new Span<byte>(RawView + address, length);
        }
    }
    public Span<byte> GetSpan(uint segment, uint offset, int length)
    {
        unsafe
        {
            uint fullAddress = GetRealModePhysicalAddress(segment, offset);
            if (fullAddress >= VramAddress && fullAddress < VramUpperBound)
                throw new ArgumentException("Not supported for video RAM mapped addresses.");

            return new Span<byte>(RawView + fullAddress, length);
        }
    }
    public int ReadFromStream(uint segment, uint offset, Stream source, int length)
    {
        uint fullAddress = GetRealModePhysicalAddress(segment, offset);
        if (fullAddress + length < VramAddress || fullAddress >= VramUpperBound)
            return source.Read(this.GetSpan(segment, offset, length));

        var buffer = ArrayPool<byte>.Shared.Rent(length);
        try
        {
            var span = buffer.AsSpan(0, length);
            if (span.IsEmpty)
                return 0;
            int bytesRead = source.Read(span);
            span = span[..bytesRead];
            for (int i = 0; i < span.Length; i++)
                this.SetByte(fullAddress + (uint)i, span[i]);

            return bytesRead;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }
    public void WriteToStream(uint segment, uint offset, Stream destination, int length)
    {
        uint fullAddress = GetRealModePhysicalAddress(segment, offset);
        if (fullAddress + length < VramAddress || fullAddress >= VramUpperBound)
        {
            destination.Write(this.GetSpan(segment, offset, length));
            return;
        }

        var buffer = ArrayPool<byte>.Shared.Rent(length);
        try
        {
            var span = buffer.AsSpan(0, length);
            for (int i = 0; i < span.Length; i++)
                span[i] = this.GetByte(fullAddress + (uint)i);

            destination.Write(span);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    /// <summary>
    /// Reads a byte from emulated memory.
    /// </summary>
    /// <param name="segment">Segment of byte to read.</param>
    /// <param name="offset">Offset of byte to read.</param>
    /// <returns>Byte at the specified segment and offset.</returns>
    public byte GetByte(uint segment, uint offset) => this.RealModeRead<byte>(segment, offset);
    /// <summary>
    /// Reads a byte from emulated memory.
    /// </summary>
    /// <param name="address">Physical address of byte to read.</param>
    /// <returns>Byte at the specified address.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public byte GetByte(uint address)
    {
        return this.PagingEnabled ? this.PagedRead<byte>(address) : this.PhysicalRead<byte>(address);
    }

    /// <summary>
    /// Writes a byte to emulated memory.
    /// </summary>
    /// <param name="segment">Segment of byte to write.</param>
    /// <param name="offset">Offset of byte to write.</param>
    /// <param name="value">Value to write to the specified segment and offset.</param>
    public void SetByte(uint segment, uint offset, byte value) => this.RealModeWrite(segment, offset, value);
    /// <summary>
    /// Writes a byte to emulated memory.
    /// </summary>
    /// <param name="address">Physical address of byte to write.</param>
    /// <param name="value">Value to write to the specified address.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void SetByte(uint address, byte value)
    {
        if (this.PagingEnabled)
            this.PagedWrite(address, value);
        else
            this.PhysicalWrite(address, value);
    }

    /// <summary>
    /// Reads an unsigned 16-bit integer from emulated memory.
    /// </summary>
    /// <param name="segment">Segment of unsigned 16-bit integer to read.</param>
    /// <param name="offset">Offset of unsigned 16-bit integer to read.</param>
    /// <returns>Unsigned 16-bit integer at the specified segment and offset.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public ushort GetUInt16(uint segment, uint offset) => this.RealModeRead<ushort>(segment, offset);
    /// <summary>
    /// Reads an unsigned 16-bit integer from emulated memory.
    /// </summary>
    /// <param name="address">Physical address of unsigned 16-bit integer to read.</param>
    /// <returns>Unsigned 16-bit integer at the specified address.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public ushort GetUInt16(uint address)
    {
        return this.PagingEnabled ? this.PagedRead<ushort>(address) : this.PhysicalRead<ushort>(address);
    }

    /// <summary>
    /// Writes an unsigned 16-bit integer to emulated memory.
    /// </summary>
    /// <param name="segment">Segment of unsigned 16-bit integer to write.</param>
    /// <param name="offset">Offset of unsigned 16-bit integer to write.</param>
    /// <param name="value">Value to write to the specified segment and offset.</param>
    public void SetUInt16(uint segment, uint offset, ushort value) => this.RealModeWrite(segment, offset, value);
    /// <summary>
    /// Writes an unsigned 16-bit integer to emulated memory.
    /// </summary>
    /// <param name="address">Physical address of unsigned 16-bit integer to write.</param>
    /// <param name="value">Value to write to the specified address.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void SetUInt16(uint address, ushort value)
    {
        if (this.PagingEnabled)
            this.PagedWrite(address, value);
        else
            this.PhysicalWrite(address, value);
    }

    /// <summary>
    /// Reads an unsigned 32-bit integer from emulated memory.
    /// </summary>
    /// <param name="segment">Segment of unsigned 32-bit integer to read.</param>
    /// <param name="offset">Offset of unsigned 32-bit integer to read.</param>
    /// <returns>Unsigned 32-bit integer at the specified segment and offset.</returns>
    public uint GetUInt32(uint segment, uint offset) => this.RealModeRead<uint>(segment, offset);
    /// <summary>
    /// Reads an unsigned 32-bit integer from emulated memory.
    /// </summary>
    /// <param name="address">Physical address of unsigned 32-bit integer to read.</param>
    /// <returns>Unsigned 32-bit integer at the specified address.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public uint GetUInt32(uint address)
    {
        return this.PagingEnabled ? this.PagedRead<uint>(address) : this.PhysicalRead<uint>(address);
    }

    /// <summary>
    /// Writes an unsigned 32-bit integer to emulated memory.
    /// </summary>
    /// <param name="segment">Segment of unsigned 32-bit integer to write.</param>
    /// <param name="offset">Offset of unsigned 32-bit integer to write.</param>
    /// <param name="value">Value to write to the specified segment and offset.</param>
    public void SetUInt32(uint segment, uint offset, uint value) => this.RealModeWrite(segment, offset, value);
    /// <summary>
    /// Writes an unsigned 32-bit integer to emulated memory.
    /// </summary>
    /// <param name="address">Physical address of unsigned 32-bit integer to write.</param>
    /// <param name="value">Value to write to the specified address.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void SetUInt32(uint address, uint value)
    {
        if (this.PagingEnabled)
            this.PagedWrite(address, value);
        else
            this.PhysicalWrite(address, value);
    }

    /// <summary>
    /// Reads an unsigned 64-bit integer from emulated memory.
    /// </summary>
    /// <param name="segment">Segment of unsigned 64-bit integer to read.</param>
    /// <param name="offset">Offset of unsigned 64-bit integer to read.</param>
    /// <returns>Unsigned 64-bit integer at the specified segment and offset.</returns>
    public ulong GetUInt64(uint segment, uint offset) => RealModeRead<ulong>(segment, offset);
    /// <summary>
    /// Reads an unsigned 64-bit integer from emulated memory.
    /// </summary>
    /// <param name="address">Physical address of unsigned 64-bit integer to read.</param>
    /// <returns>Unsigned 64-bit integer at the specified address.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public ulong GetUInt64(uint address)
    {
        return this.PagingEnabled ? this.PagedRead<ulong>(address) : this.PhysicalRead<ulong>(address);
    }

    /// <summary>
    /// Writes an unsigned 64-bit integer to emulated memory.
    /// </summary>
    /// <param name="segment">Segment of unsigned 64-bit integer to write.</param>
    /// <param name="offset">Offset of unsigned 64-bit integer to write.</param>
    /// <param name="value">Value to write to the specified segment and offset.</param>
    public void SetUInt64(uint segment, uint offset, ulong value) => this.RealModeWrite(segment, offset, value);
    /// <summary>
    /// Writes an unsigned 64-bit integer to emulated memory.
    /// </summary>
    /// <param name="address">Physical address of unsigned 64-bit integer to write.</param>
    /// <param name="value">Value to write to the specified address.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void SetUInt64(uint address, ulong value)
    {
        if (this.PagingEnabled)
            this.PagedWrite(address, value);
        else
            this.PhysicalWrite(address, value);
    }

    /// <summary>
    /// Returns a System.Single value read from an address in emulated memory.
    /// </summary>
    /// <param name="address">Address of value to read.</param>
    /// <returns>32-bit System.Single value read from the specified address.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public float GetReal32(uint address)
    {
        return this.PagingEnabled ? this.PagedRead<float>(address) : this.PhysicalRead<float>(address);
    }

    /// <summary>
    /// Writes a System.Single value to an address in emulated memory.
    /// </summary>
    /// <param name="address">Address where value will be written.</param>
    /// <param name="value">32-bit System.Single value to write at the specified address.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void SetReal32(uint address, float value)
    {
        if (this.PagingEnabled)
            this.PagedWrite(address, value);
        else
            this.PhysicalWrite(address, value);
    }

    /// <summary>
    /// Returns a System.Double value read from an address in emulated memory.
    /// </summary>
    /// <param name="address">Address of value to read.</param>
    /// <returns>64-bit System.Double value read from the specified address.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public double GetReal64(uint address)
    {
        return this.PagingEnabled ? this.PagedRead<double>(address) : this.PhysicalRead<double>(address);
    }

    /// <summary>
    /// Writes a System.Double value to an address in emulated memory.
    /// </summary>
    /// <param name="address">Address where value will be written.</param>
    /// <param name="value">64-bit System.Double value to write at the specified address.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void SetReal64(uint address, double value)
    {
        if (this.PagingEnabled)
            this.PagedWrite(address, value);
        else
            this.PhysicalWrite(address, value);
    }

    /// <summary>
    /// Returns a Real10 value read from an address in emulated memory.
    /// </summary>
    /// <param name="address">Address of value to read.</param>
    /// <returns>80-bit Real10 value read from the specified address.</returns>
    public Real10 GetReal80(uint address)
    {
        return this.PagingEnabled ? this.PagedRead<Real10>(address) : this.PhysicalRead<Real10>(address);
    }
    /// <summary>
    /// Writes a Real10 value to an address in emulated memory.
    /// </summary>
    /// <param name="address">Address where value will be written.</param>
    /// <param name="value">80-bit Real10 value to write at the specified address.</param>
    public void SetReal80(uint address, Real10 value)
    {
        if (this.PagingEnabled)
            this.PagedWrite(address, value);
        else
            this.PhysicalWrite(address, value);
    }

    public TValue Get<TValue>(uint address) where TValue : unmanaged => this.PagingEnabled ? this.PagedRead<TValue>(address) : this.PhysicalRead<TValue>(address);
    public void Set<TValue>(uint address, TValue value) where TValue : unmanaged
    {
        if (this.PagingEnabled)
            this.PagedWrite(address, value);
        else
            this.PhysicalWrite(address, value);
    }

    /// <summary>
    /// Reads an ANSI string from emulated memory with a specified length.
    /// </summary>
    /// <param name="segment">Segment of string to read.</param>
    /// <param name="offset">Offset of string to read.</param>
    /// <param name="length">Length of the string in bytes.</param>
    /// <returns>String read from the specified segment and offset.</returns>
    public string GetString(uint segment, uint offset, int length)
    {
        var ptr = GetPointer(segment, offset);
        return System.Runtime.InteropServices.Marshal.PtrToStringAnsi(ptr, length);
    }
    /// <summary>
    /// Reads an ANSI string from emulated memory with a maximum length and end sentinel character.
    /// </summary>
    /// <param name="segment">Segment of string to read.</param>
    /// <param name="offset">Offset of string to read.</param>
    /// <param name="maxLength">Maximum number of bytes to read.</param>
    /// <param name="sentinel">End sentinel character of the string to read.</param>
    /// <returns>String read from the specified segment and offset.</returns>
    public string GetString(uint segment, uint offset, int maxLength, byte sentinel)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(maxLength);
        try
        {
            uint i;

            for (i = 0; i < maxLength; i++)
            {
                byte value = this.GetByte(segment, offset + i);
                if (value == sentinel)
                    break;
                buffer[i] = value;
            }

            return Encoding.Latin1.GetString(buffer.AsSpan(0, (int)i));
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }
    /// <summary>
    /// Writes a string to memory as a null-terminated ANSI byte array.
    /// </summary>
    /// <param name="segment">Segment to write string.</param>
    /// <param name="offset">Offset to write string.</param>
    /// <param name="value">String to write to the specified address.</param>
    /// <param name="writeNull">Value indicating whether a null should be written after the string.</param>
    public void SetString(uint segment, uint offset, string value, bool writeNull)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(value.Length);
        try
        {
            uint length = (uint)Encoding.Latin1.GetBytes(value, buffer);
            for (uint i = 0; i < length; i++)
                this.SetByte(segment, offset + i, buffer[(int)i]);

            if (writeNull)
                this.SetByte(segment, offset + length, 0);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }
    /// <summary>
    /// Writes a string to memory as a null-terminated ANSI byte array.
    /// </summary>
    /// <param name="segment">Segment to write string.</param>
    /// <param name="offset">Offset to write string.</param>
    /// <param name="value">String to write to the specified address.</param>
    public void SetString(uint segment, uint offset, string value) => SetString(segment, offset, value, true);
    /// <summary>
    /// Searches emulated memory for all occurrences of a string of bytes.
    /// </summary>
    /// <param name="match">String of bytes to search for.</param>
    /// <returns>Raw index into memory where string was found.</returns>
    public IEnumerable<int> Find(byte[] match)
    {
        ArgumentNullException.ThrowIfNull(match);
        if (match.Length == 0)
            throw new ArgumentException("Array must not be empty.");

        int length = match.Length;
        for (int i = 0; i < VramAddress - length; i++)
        {
            if (SafeArrayAccess(i) == match[0])
            {
                bool isMatch = true;
                for (int j = 1; j < length; j++)
                {
                    if (SafeArrayAccess(i + j) != match[j])
                    {
                        isMatch = false;
                        break;
                    }
                }

                if (isMatch)
                    yield return i;
            }
        }
    }
    /// <summary>
    /// Searches emulated memory for all occurrences of an ASCII string.
    /// </summary>
    /// <param name="match">ASCII string to search for.</param>
    /// <returns>Raw index into memory where string was found.</returns>
    public IEnumerable<int> Find(string match)
    {
        ArgumentNullException.ThrowIfNull(match);
        if (match == string.Empty)
            throw new ArgumentException("String cannot be empty.");

        var buffer = Encoding.ASCII.GetBytes(match);
        return Find(buffer);
    }
    /// <summary>
    /// Reads bytes from memory into a buffer.
    /// </summary>
    /// <param name="buffer">Buffer into which bytes will be written.</param>
    /// <param name="bufferOffset">Offset in buffer to start writing.</param>
    /// <param name="address">Address in memory to start copying from.</param>
    /// <param name="count">Number of bytes to copy.</param>
    public void ReadBytes(byte[] buffer, int bufferOffset, QualifiedAddress address, int count)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        if (bufferOffset < 0 || bufferOffset > buffer.Length - count)
            throw new ArgumentOutOfRangeException(nameof(bufferOffset));
        ArgumentOutOfRangeException.ThrowIfNegative(count);

        if (count == 0)
            return;

        // Treat real mode as a physical address even if paging is enabled.
        if (address.AddressType == AddressType.RealMode || address.AddressType == AddressType.PhysicalLinear)
        {
            uint offset;
            if (address.AddressType == AddressType.RealMode)
                offset = GetRealModePhysicalAddress((ushort)address.Segment!, address.Offset);
            else
                offset = address.Offset;

            unsafe
            {
                for (int i = 0; i < count; i++)
                    buffer[bufferOffset + i] = this.RawView[offset + i];
            }

            return;
        }

        var logical = GetLogicalAddress(address) ?? throw new ArgumentException("The address could not be resolved.");
        var logicalOffset = logical.Offset;
        for (int i = 0; i < count; i++)
            buffer[bufferOffset + i] = this.GetByte(logicalOffset + (uint)i);
    }
    /// <summary>
    /// Returns the logical address of a real-mode or protected-mode address.
    /// </summary>
    /// <param name="source">Real-mode or protected-mode address to resolve.</param>
    /// <returns>Logical address of the provided address or null if the address could not be resolved.</returns>
    public QualifiedAddress? GetLogicalAddress(QualifiedAddress source)
    {
        if (source.AddressType == AddressType.PhysicalLinear)
            throw new ArgumentException("Cannot convert a physical address to a logical address.");

        if (source.AddressType == AddressType.LogicalLinear)
            return source;

        if (source.AddressType == AddressType.RealMode)
            return QualifiedAddress.FromLogicalAddress(GetRealModePhysicalAddress((ushort)source.Segment!, source.Offset));

        if (source.AddressType == AddressType.ProtectedMode)
        {
            var descriptor = (SegmentDescriptor)GetDescriptor((ushort)source.Segment!);
            if (!descriptor.IsPresent || descriptor.ByteLimit == 0)
                return null;

            return QualifiedAddress.FromLogicalAddress(descriptor.Base + source.Offset);
        }

        throw new ArgumentException("Unsupported address type.");
    }
    /// <summary>
    /// Returns the physical address of a real-mode, protected-mode, or logical address.
    /// </summary>
    /// <param name="source">Real-mode, protected-mode, or logical address to resolve.</param>
    /// <returns>Physical address of the provided address or null if the address could not be resolved.</returns>
    public QualifiedAddress? GetPhysicalAddress(QualifiedAddress source)
    {
        if (source.AddressType == AddressType.PhysicalLinear)
            return source;

        var logical = GetLogicalAddress(source);
        if (logical == null)
            return null;

        if (!this.PagingEnabled)
            return QualifiedAddress.FromPhysicalAddress(logical.Value.Offset);

        try
        {
            return QualifiedAddress.FromPhysicalAddress(this.GetPagedPhysicalAddress(logical.Value.Offset, PageFaultCause.Read));
        }
        catch (PageFaultException)
        {
            return null;
        }
    }

    /// <summary>
    /// Writes the descriptor for the specified segment.
    /// </summary>
    /// <param name="segment">Segment whose descriptor is written.</param>
    /// <param name="descriptor">Descriptor to write.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    internal void SetDescriptor(uint segment, Descriptor descriptor)
    {
        uint selectorIndex = segment >> 3;
        uint baseAddress;

        // Check for local descriptor.
        if ((segment & 4) != 0)
        {
            unsafe
            {
                var ldtDescriptor = (SegmentDescriptor)GetDescriptor(this.LDTSelector & 0xFFF8u);
                baseAddress = ldtDescriptor.Base;
            }
        }
        else
            baseAddress = this.GDTAddress;

        unsafe
        {
            ulong value = *(ulong*)&descriptor;
            SetUInt64(baseAddress + (selectorIndex * 8u), value);
        }
    }
    /// <summary>
    /// Reserves the largest free block of conventional memory as base memory.
    /// </summary>
    internal void ReserveBaseMemory()
    {
        uint length = this.metaAllocator.GetLargestFreeBlockSize();
        ushort segment = this.metaAllocator.Allocate(0x0000, (int)length);
        this.BaseMemory = new ReservedBlock(segment, length);
    }
    /// <summary>
    /// Writes a new callback handler and returns its address.
    /// </summary>
    /// <param name="id">Unique ID of the callback handler.</param>
    /// <param name="hookable">Value indicating whether the callback is hookable.</param>
    /// <returns>Address of the callback handler.</returns>
    internal RealModeAddress AddCallbackHandler(byte id, bool hookable)
    {
        var ptr = GetPointer(HandlerSegment, nextHandlerOffset);
        int length = 4;
        unsafe
        {
            byte* writePtr = (byte*)ptr.ToPointer();
            if (hookable)
            {
                writePtr[0] = 0xE9; // JMP iw
                writePtr[1] = 3; // jump past the 3 nops
                writePtr[2] = 0;
                writePtr[3] = 0x90;
                writePtr[4] = 0x90;
                writePtr[5] = 0x90;

                writePtr += 6;
                length += 6;
            }

            writePtr[0] = 0x0F;
            writePtr[1] = 0x56;
            writePtr[2] = id;
            writePtr[3] = 0xCB; // RETF
        }

        var address = new RealModeAddress(HandlerSegment, nextHandlerOffset);
        nextHandlerOffset += (ushort)length;

        return address;
    }
    /// <summary>
    /// Writes a new address to the interrupt table.
    /// </summary>
    /// <param name="interrupt">Interrupt to set handler address for.</param>
    /// <param name="segment">Segment of the interrupt handler.</param>
    /// <param name="offset">Offset of the interrupt handler.</param>
    public void SetInterruptAddress(byte interrupt, ushort segment, ushort offset)
    {
        SetUInt16(0, (ushort)(interrupt * 4), offset);
        SetUInt16(0, (ushort)(interrupt * 4 + 2), segment);
    }
    /// <summary>
    /// Adds an interrupt handler to the virtual interrupt table.
    /// </summary>
    /// <param name="interrupt">Interrupt of new handler.</param>
    /// <param name="savedRegisters">Registers to be saved before the interrupt handler and restored afterward.</param>
    /// <param name="isHookable">Value indicating whether the callback is hookable.</param>
    /// <param name="clearInterruptFlag">Value indicating whether the interrupt handler should clear the CPU Interrupt Enable flag.</param>
    internal void AddInterruptHandler(byte interrupt, Registers savedRegisters, bool isHookable, bool clearInterruptFlag)
    {
        SetInterruptAddress(interrupt, (ushort)HandlerSegment, nextHandlerOffset);
        var ptr = GetPointer(HandlerSegment, nextHandlerOffset);
        unsafe
        {
            byte* startPtr = (byte*)ptr.ToPointer();
            byte* offsetPtr = startPtr;

            if (isHookable)
            {
                offsetPtr[0] = 0xE9; // JMP iw
                offsetPtr[1] = 3; // jump past the 3 nops
                offsetPtr[2] = 0;
                offsetPtr[3] = 0x90;
                offsetPtr[4] = 0x90;
                offsetPtr[5] = 0x90;
                offsetPtr += 6;
            }

            // Write a CLI instruction if requested.
            if (clearInterruptFlag)
            {
                *offsetPtr = 0xFA;
                offsetPtr++;
            }

            WritePushInstructions(ref offsetPtr, savedRegisters);

            // Write instructions 0F55 interrupt, iret to memory.
            *offsetPtr = 0x0F;
            offsetPtr++;
            *offsetPtr = 0x55;
            offsetPtr++;
            *offsetPtr = interrupt;
            offsetPtr++;

            WritePopInstructions(ref offsetPtr, savedRegisters);

            *offsetPtr = 0xCF;
            offsetPtr++;

            ushort length = (ushort)(offsetPtr - startPtr);
            nextHandlerOffset += length;
        }
    }
    /// <summary>
    /// Adds a default INT 08h handler that invokes INT 1Ch.
    /// </summary>
    internal void AddTimerInterruptHandler()
    {
        SetInterruptAddress(0x08, (ushort)HandlerSegment, nextHandlerOffset);

        // Write instruction push ax.
        SetByte(HandlerSegment, nextHandlerOffset, 0x50);
        nextHandlerOffset++;

        // Write instruction mov al,20h.
        SetUInt16(HandlerSegment, nextHandlerOffset, 0x20B0);
        nextHandlerOffset += 2;

        // Write instruction out 20h,al.
        SetUInt16(HandlerSegment, nextHandlerOffset, 0x20E6);
        nextHandlerOffset += 2;

        // Write instruction pop ax.
        SetByte(HandlerSegment, nextHandlerOffset, 0x58);
        nextHandlerOffset++;

        // Write instruction int 1ch.
        SetUInt16(HandlerSegment, nextHandlerOffset, 0x1CCD);
        nextHandlerOffset += 2;

        // Write instruction iret to memory.
        SetByte(HandlerSegment, nextHandlerOffset, 0xCF);
        nextHandlerOffset++;
    }
    /// <summary>
    /// Reads 16 bytes from emulated memory into a buffer.
    /// </summary>
    /// <param name="address">Address where bytes will be read from.</param>
    /// <param name="buffer">Buffer into which bytes will be written.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    internal unsafe void FetchInstruction(uint address, byte* buffer)
    {
        if (this.PagingEnabled)
            PagedFetchInstruction(address, buffer);
        else
            Unsafe.CopyBlock(buffer, this.RawView + (address & this.addressMask), 16);
    }

    /// <summary>
    /// Returns a pointer to a block of memory, making sure it is paged in.
    /// </summary>
    /// <param name="address">Logical address of block.</param>
    /// <param name="size">Number of bytes in block of memory.</param>
    /// <returns>Pointer to block of memory.</returns>
    internal unsafe void* GetSafePointer(uint address, uint size)
    {
        if (this.PagingEnabled)
        {
            uint baseAddress = GetPagedPhysicalAddress(address, PageFaultCause.Read);
            if ((address & 0xFFFu) + size > 4096)
                GetPagedPhysicalAddress(address + 4096u, PageFaultCause.Read);

            return RawView + baseAddress;
        }
        else
        {
            return RawView + address;
        }
    }

    /// <summary>
    /// Frees unmanaged memory.
    /// </summary>
    /// <remarks>
    /// Implemented this way instead of with <see cref="IDisposable"/> because it's not
    /// intended to be publicly exposed. <see cref="VirtualMachine"/> is responsible for
    /// calling this.
    /// </remarks>
    internal void InternalDispose()
    {
        unsafe
        {
            if (this.pageCache != null)
            {
                NativeMemory.Free(this.pageCache);
                this.pageCache = null;
            }

            if (this.RawView != null)
            {
                NativeMemory.Free(this.RawView);
                this.RawView = null;
            }

#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize
            GC.SuppressFinalize(this);
#pragma warning restore CA1816 // Dispose methods should call SuppressFinalize
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
    private unsafe void PagedFetchInstruction(uint address, byte* buffer)
    {
        uint fullPagedAddress = GetPagedPhysicalAddress(address, PageFaultCause.InstructionFetch);
        if ((fullPagedAddress & 0xFFFu) < 4096u - 16u)
        {
            Unsafe.CopyBlock(buffer, this.RawView + fullPagedAddress, 16);
        }
        else
        {
            var ptr = (ulong*)buffer;
            ptr[0] = this.PagedRead<ulong>(address, PageFaultCause.InstructionFetch, checkVram: false);
            ptr[1] = this.PagedRead<ulong>(address + 8u, PageFaultCause.InstructionFetch, checkVram: false);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private T RealModeRead<T>(uint segment, uint offset) where T : unmanaged => PhysicalRead<T>(GetRealModePhysicalAddress(segment, offset));
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void RealModeWrite<T>(uint segment, uint offset, T value) where T : unmanaged => this.PhysicalWrite(this.GetRealModePhysicalAddress(segment, offset), value);

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    private T PhysicalRead<T>(uint address, bool mask = true, bool checkVram = true) where T : unmanaged
    {
        uint fullAddress = mask ? (address & this.addressMask) : address;

        unsafe
        {
            if (!checkVram || fullAddress is < VramAddress or >= VramUpperBound)
            {
                if (fullAddress >= (uint)this.MemorySize)
                    ThrowOutOfRange(fullAddress);

                return Unsafe.ReadUnaligned<T>(this.RawView + fullAddress);
            }
            else
            {
                if (sizeof(T) == 1)
                {
                    byte b = this.Video!.GetVramByte(fullAddress - VramAddress);
                    return Unsafe.As<byte, T>(ref b);
                }
                else if (sizeof(T) == 2)
                {
                    ushort s = this.Video!.GetVramWord(fullAddress - VramAddress);
                    return Unsafe.As<ushort, T>(ref s);
                }
                else
                {
                    uint i = this.Video!.GetVramDWord(fullAddress - VramAddress);
                    return Unsafe.As<uint, T>(ref i);
                }
            }
        }
    }
    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    private void PhysicalWrite<T>(uint address, T value, bool mask = true) where T : unmanaged
    {
        uint fullAddress = mask ? (address & this.addressMask) : address;

        unsafe
        {
            if (fullAddress is < VramAddress or > VramUpperBound)
            {
                if (fullAddress >= (uint)this.MemorySize)
                    ThrowOutOfRange(fullAddress);

                Unsafe.WriteUnaligned(this.RawView + fullAddress, value);
            }
            else
            {
                if (sizeof(T) == 1)
                    this.Video!.SetVramByte(fullAddress - VramAddress, Unsafe.As<T, byte>(ref value));
                else if (sizeof(T) == 2)
                    this.Video!.SetVramWord(fullAddress - VramAddress, Unsafe.As<T, ushort>(ref value));
                else
                    this.Video!.SetVramDWord(fullAddress - VramAddress, Unsafe.As<T, uint>(ref value));
            }
        }
    }

    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private T PagedRead<T>(uint logicalAddress, PageFaultCause mode = PageFaultCause.Read, bool checkVram = true) where T : unmanaged
    {
        uint physicalAddress = GetPagedPhysicalAddress(logicalAddress, mode);

        unsafe
        {
            if (sizeof(T) == 1 || (physicalAddress & 0xFFFu) < 4096u - sizeof(T))
            {
                return PhysicalRead<T>(physicalAddress, false, checkVram);
            }
            else
            {
                var buffer = stackalloc byte[sizeof(T)];
                for (uint i = 0; i < sizeof(T); i++)
                    buffer[i] = PagedRead<byte>(logicalAddress + i, mode, checkVram);

                return *(T*)buffer;
            }
        }
    }
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private void PagedWrite<T>(uint logicalAddress, T value) where T : unmanaged
    {
        uint physicalAddress = GetPagedPhysicalAddress(logicalAddress, PageFaultCause.Write);

        unsafe
        {
            if (sizeof(T) == 1 || (physicalAddress & 0xFFFu) < 4096u - sizeof(T))
            {
                this.PhysicalWrite(physicalAddress, value);
            }
            else
            {
                byte* ptr = (byte*)&value;
                for (uint i = 0; i < sizeof(T); i++)
                    this.PagedWrite(logicalAddress + i, ptr[i]);
            }
        }
    }

    /// <summary>
    /// Returns the physical address from a paged linear address.
    /// </summary>
    /// <param name="linearAddress">Paged linear address.</param>
    /// <param name="operation">Type of operation attempted in case of a page fault.</param>
    /// <returns>Physical address of the supplied linear address.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private uint GetPagedPhysicalAddress(uint linearAddress, PageFaultCause operation)
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

    /// <summary>
    /// Returns the physical address of the specified segment and offset.
    /// </summary>
    /// <param name="segment">Memory segment or selector.</param>
    /// <param name="offset">Offset into segment.</param>
    /// <returns>Physical address of the specified segment and offset.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private uint GetRealModePhysicalAddress(uint segment, uint offset) => ((segment << 4) + offset) & this.addressMask;
    /// <summary>
    /// Provides access to emulated memory for a safe context.
    /// </summary>
    /// <param name="index">Index of byte to return.</param>
    /// <returns>Byte in emulated memory at specified index.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private byte SafeArrayAccess(int index)
    {
        unsafe
        {
            return RawView[index];
        }
    }
    /// <summary>
    /// Copies font data to emulated memory.
    /// </summary>
    private void InitializeFonts()
    {
        var ibm8x8 = Fonts.IBM8x8;
        ibm8x8.CopyTo(this.GetSpan(FontSegment, Font8x8Offset, ibm8x8.Length));

        Reserve(0xF000, (uint)ibm8x8.Length / 2u);

        SetInterruptAddress(0x43, 0xF000, 0xFA6E);

        // Only the first half of the 8x8 font should go here.
        ibm8x8[..(ibm8x8.Length / 2)].CopyTo(this.GetSpan(0xF000, 0xFA6E, ibm8x8.Length / 2));

        Fonts.VGA8x16.CopyTo(this.GetSpan(FontSegment, Font8x16Offset, Fonts.VGA8x16.Length));

        Fonts.EGA8x14.CopyTo(this.GetSpan(FontSegment, Font8x14Offset, Fonts.EGA8x14.Length));
    }
    /// <summary>
    /// Writes static BIOS configuration data to emulated memory.
    /// </summary>
    private void InitializeBiosData()
    {
        var segment = BiosConfigurationAddress.Segment;
        var offset = BiosConfigurationAddress.Offset;

        SetUInt16(segment, offset, 8);
        SetByte(segment, offset + 2u, 0xFC);
        SetByte(segment, offset + 5u, 0x70);
        SetByte(segment, offset + 6u, 0x40);
    }

    private static unsafe void WriteInstruction(ref byte* ptr, byte instruction)
    {
        *ptr = instruction;
        ptr++;
    }
    private static unsafe void WritePushInstructions(ref byte* ptr, Registers registers)
    {
        if ((registers & Registers.AX) != 0)
            WriteInstruction(ref ptr, 0x50);

        if ((registers & Registers.BX) != 0)
            WriteInstruction(ref ptr, 0x53);

        if ((registers & Registers.CX) != 0)
            WriteInstruction(ref ptr, 0x51);

        if ((registers & Registers.DX) != 0)
            WriteInstruction(ref ptr, 0x52);

        if ((registers & Registers.BP) != 0)
            WriteInstruction(ref ptr, 0x55);

        if ((registers & Registers.SI) != 0)
            WriteInstruction(ref ptr, 0x56);

        if ((registers & Registers.DI) != 0)
            WriteInstruction(ref ptr, 0x57);

        if ((registers & Registers.DS) != 0)
            WriteInstruction(ref ptr, 0x1E);

        if ((registers & Registers.ES) != 0)
            WriteInstruction(ref ptr, 0x06);
    }
    private static unsafe void WritePopInstructions(ref byte* ptr, Registers registers)
    {
        if ((registers & Registers.ES) != 0)
            WriteInstruction(ref ptr, 0x07);

        if ((registers & Registers.DS) != 0)
            WriteInstruction(ref ptr, 0x1F);

        if ((registers & Registers.DI) != 0)
            WriteInstruction(ref ptr, 0x58 + 7);

        if ((registers & Registers.SI) != 0)
            WriteInstruction(ref ptr, 0x58 + 6);

        if ((registers & Registers.BP) != 0)
            WriteInstruction(ref ptr, 0x58 + 5);

        if ((registers & Registers.DX) != 0)
            WriteInstruction(ref ptr, 0x58 + 2);

        if ((registers & Registers.CX) != 0)
            WriteInstruction(ref ptr, 0x58 + 1);

        if ((registers & Registers.BX) != 0)
            WriteInstruction(ref ptr, 0x58 + 3);

        if ((registers & Registers.AX) != 0)
            WriteInstruction(ref ptr, 0x58);
    }
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowOutOfRange(uint address) => throw new InvalidOperationException($"Attempted to access invalid physical address 0x{address:X8}.");
}
