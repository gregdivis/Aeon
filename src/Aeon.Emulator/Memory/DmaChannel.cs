using System.Diagnostics;
using Aeon.Emulator.Interrupts;

namespace Aeon.Emulator;

/// <summary>
/// Contains information about a DMA channel.
/// </summary>
public sealed class DmaChannel
{
    private bool isActive;
    private bool addressByteRead;
    private bool addressByteWritten;
    private bool countByteRead;
    private bool countByteWritten;
    private volatile int bytesRemaining;
    private byte bytesRemainingHighByte;
    private byte addressHighByte;
    private int transferRate;
    private readonly Stopwatch transferTimer = new();

    internal DmaChannel()
    {
    }

    /// <summary>
    /// Occurs when the <see cref="IsActive"/> property has changed.
    /// </summary>
    internal event EventHandler? IsActiveChanged;

    /// <summary>
    /// Gets or sets a value indicating whether a DMA transfer is active.
    /// </summary>
    public bool IsActive
    {
        get => this.isActive;
        set
        {
            if (this.isActive != value)
            {
                if (value)
                    this.transferTimer.Start();
                else
                    this.transferTimer.Reset();

                this.isActive = value;
                this.OnIsActiveChanged(EventArgs.Empty);
            }
        }
    }
    /// <summary>
    /// Gets a value indicating whether the channel is masked (disabled).
    /// </summary>
    public bool IsMasked { get; internal set; }
    /// <summary>
    /// Gets the current DMA transfer mode of the channel.
    /// </summary>
    public DmaTransferMode TransferMode { get; internal set; }
    /// <summary>
    /// Gets the DMA transfer memory page.
    /// </summary>
    public byte Page { get; internal set; }
    /// <summary>
    /// Gets the DMA transfer memory address.
    /// </summary>
    public ushort Address { get; internal set; }
    /// <summary>
    /// Gets the number of bytes to transfer.
    /// </summary>
    public ushort Count { get; internal set; }
    public int TransferBytesRemaining
    {
        get => bytesRemaining;
        internal set => bytesRemaining = value;
    }
    /// <summary>
    /// Gets or sets the desired transfer rate in bytes/second.
    /// </summary>
    public int TransferRate
    {
        get => this.transferRate;
        set
        {
            int period = 1;
            int chunkSize = value / 1000;

            if (chunkSize < 1)
            {
                chunkSize = 1;
                period = value / 1000;
            }

            this.transferRate = value;
            this.TransferPeriod = InterruptTimer.StopwatchTicksPerMillisecond * period;
            this.TransferChunkSize = chunkSize;
        }
    }

    /// <summary>
    /// Gets or sets the device which is connected to the DMA channel.
    /// </summary>
    internal IDmaDevice8? Device { get; set; }

    /// <summary>
    /// Gets or sets the period between DMA transfers in stopwatch ticks.
    /// </summary>
    private long TransferPeriod { get; set; }
    /// <summary>
    /// Gets or sets the size of each DMA transfer chunk.
    /// </summary>
    private int TransferChunkSize { get; set; }

    /// <summary>
    /// Returns a string representation of the DmaChannel.
    /// </summary>
    /// <returns>String representation of the DmaChannel.</returns>
    public override string ToString() => $"{this.Page:X2}:{this.Address:X4}";

    /// <summary>
    /// Returns the next byte of the memory address.
    /// </summary>
    /// <returns>Next byte of the memory address.</returns>
    internal byte ReadAddressByte()
    {
        try
        {
            if (!this.addressByteRead)
            {
                ushort address = (ushort)(this.Address + this.Count - (this.TransferBytesRemaining - 1));
                this.addressHighByte = Intrinsics.HighByte(address);
                return Intrinsics.LowByte(address);
            }
            else
            {
                return this.addressHighByte;
            }
        }
        finally
        {
            this.addressByteRead = !this.addressByteRead;
        }
    }
    /// <summary>
    /// Writes the next byte of the memory address.
    /// </summary>
    /// <param name="value">Next byte of the memory address.</param>
    internal void WriteAddressByte(byte value)
    {
        try
        {
            if (!this.addressByteWritten)
                this.Address = value;
            else
                this.Address |= (ushort)(value << 8);
        }
        finally
        {
            this.addressByteWritten = !this.addressByteWritten;
        }
    }
    /// <summary>
    /// Returns the next byte of the memory address.
    /// </summary>
    /// <returns>Next byte of the memory address.</returns>
    internal byte ReadCountByte()
    {
        try
        {
            if (!this.countByteRead)
            {
                ushort count = (ushort)(this.TransferBytesRemaining - 1);
                this.bytesRemainingHighByte = Intrinsics.HighByte(count);
                return Intrinsics.LowByte(count);
            }
            else
            {
                return bytesRemainingHighByte;
            }
        }
        finally
        {
            this.countByteRead = !this.countByteRead;
        }
    }
    /// <summary>
    /// Writes the next byte of the memory address.
    /// </summary>
    /// <param name="value">Next byte of the memory address.</param>
    internal void WriteCountByte(byte value)
    {
        try
        {
            if (!this.countByteWritten)
            {
                this.Count = value;
            }
            else
            {
                this.Count |= (ushort)(value << 8);
                this.TransferBytesRemaining = this.Count + 1;
            }
        }
        finally
        {
            this.countByteWritten = !this.countByteWritten;
        }
    }
    /// <summary>
    /// Performs a DMA transfer.
    /// </summary>
    /// <param name="memory">Current PhysicalMemory instance.</param>
    /// <remarks>
    /// This method should only be called if the channel is active.
    /// </remarks>
    internal void Transfer(PhysicalMemory memory)
    {
        var device = this.Device;
        if (device != null && this.transferTimer.ElapsedTicks >= this.TransferPeriod)
        {
            uint memoryAddress = ((uint)this.Page << 16) | this.Address;
            uint sourceOffset = (uint)this.Count + 1 - (uint)this.TransferBytesRemaining;

            int count = Math.Min(this.TransferChunkSize, this.TransferBytesRemaining);
#warning this is probaly supposed to be a flat memory model
            var source = memory.GetPagedSpan(memoryAddress + sourceOffset, count);

            count = device.WriteBytes(source);

            this.TransferBytesRemaining -= count;

            if (this.TransferBytesRemaining <= 0)
            {
                if (this.TransferMode == DmaTransferMode.SingleCycle)
                {
                    this.IsActive = false;
                    device.SingleCycleComplete();
                }
                else
                {
                    this.TransferBytesRemaining = this.Count + 1;
                }
            }

            this.transferTimer.Reset();
            this.transferTimer.Start();
        }
    }

    private void OnIsActiveChanged(EventArgs e) => this.IsActiveChanged?.Invoke(this, e);
}
