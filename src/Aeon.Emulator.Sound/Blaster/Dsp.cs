using System;
using System.Threading;

namespace Aeon.Emulator.Sound.Blaster
{
    /// <summary>
    /// Emulates the Sound Blaster 16 DSP.
    /// </summary>
    internal sealed class Dsp
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the Dsp class.
        /// </summary>
        /// <param name="vm">Virtual machine instance associated with the DSP.</param>
        /// <param name="dma8">8-bit DMA channel for the DSP device.</param>
        /// <param name="dma16">16-bit DMA channel for the DSP device.</param>
        public Dsp(VirtualMachine vm, int dma8, int dma16)
        {
            this.vm = vm;
            this.dmaChannel8 = vm.DmaController.Channels[dma8];
            this.dmaChannel16 = vm.DmaController.Channels[dma16];
            this.SampleRate = 22050;
            this.BlockTransferSize = 65536;
        }
        #endregion

        #region Public Events
        /// <summary>
        /// Occurs when a buffer has been transferred in auto-initialize mode.
        /// </summary>
        public event EventHandler AutoInitBufferComplete;
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets or sets the DSP's sample rate.
        /// </summary>
        public int SampleRate { get; set; }
        /// <summary>
        /// Gets a value indicating whether the DMA mode is set to auto-initialize.
        /// </summary>
        public bool AutoInitialize { get; private set; }
        /// <summary>
        /// Gets or sets the size of a transfer block for auto-init mode.
        /// </summary>
        public int BlockTransferSize { get; set; }
        /// <summary>
        /// Gets a value indicating whether the waveform data is 16-bit.
        /// </summary>
        public bool Is16Bit
        {
            get { return this.is16Bit; }
        }
        /// <summary>
        /// Gets a value indicating whether the waveform data is stereo.
        /// </summary>
        public bool IsStereo
        {
            get { return this.isStereo; }
        }
        /// <summary>
        /// Gets or sets a value indicating whether a DMA transfer is active.
        /// </summary>
        public bool IsEnabled { get; set; }
        #endregion

        #region Public Methods
        /// <summary>
        /// Starts a new DMA transfer.
        /// </summary>
        /// <param name="is16Bit">Value indicating whether this is a 16-bit transfer.</param>
        /// <param name="isStereo">Value indicating whether this is a stereo transfer.</param>
        /// <param name="autoInitialize">Value indicating whether the DMA controller is in auto-initialize mode.</param>
        /// <param name="compression">Compression level of the expected data.</param>
        /// <param name="referenceByte">Value indicating whether a reference byte is expected.</param>
        public void Begin(bool is16Bit, bool isStereo, bool autoInitialize, CompressionLevel compression = CompressionLevel.None, bool referenceByte = false)
        {
            this.is16Bit = is16Bit;
            this.isStereo = isStereo;
            this.AutoInitialize = autoInitialize;
            this.referenceByteExpected = referenceByte;
            this.compression = compression;
            this.IsEnabled = true;

            this.decodeRemainderOffset = -1;

            switch(compression)
            {
            case CompressionLevel.ADPCM2:
                this.decoder = new ADPCM2();
                break;

            case CompressionLevel.ADPCM3:
                this.decoder = new ADPCM3();
                break;

            case CompressionLevel.ADPCM4:
                this.decoder = new ADPCM4();
                break;

            default:
                this.decoder = null;
                break;
            }

            this.currentChannel = this.dmaChannel8;

            int transferRate = this.SampleRate;
            if(this.is16Bit)
                transferRate *= 2;
            if(this.isStereo)
                transferRate *= 2;

            double factor = 1.0;
            if(autoInitialize)
                factor = 1.5;

            this.currentChannel.TransferRate = (int)(transferRate * factor);
            this.currentChannel.IsActive = true;
        }
        /// <summary>
        /// Exits autoinitialize mode.
        /// </summary>
        public void ExitAutoInit()
        {
            this.AutoInitialize = false;
        }
        /// <summary>
        /// Reads samples from the internal buffer.
        /// </summary>
        /// <param name="buffer">Buffer into which sample data is written.</param>
        /// <param name="offset">Offset in buffer to start writing.</param>
        /// <param name="length">Number of samples to read.</param>
        public void Read(byte[] buffer, int offset, int length)
        {
            if(this.compression == CompressionLevel.None)
            {
                InternalRead(buffer, offset, length);
                return;
            }

            if(this.decodeBuffer == null || this.decodeBuffer.Length < length * 4)
                this.decodeBuffer = new byte[length * 4];

            while(length > 0 && this.decodeRemainderOffset >= 0)
            {
                buffer[offset] = this.decodeRemainder[this.decodeRemainderOffset];
                offset++;
                length--;
                this.decodeRemainderOffset--;
            }

            if(length <= 0)
                return;

            if(this.referenceByteExpected)
            {
                InternalRead(buffer, offset, 1);
                this.referenceByteExpected = false;
                this.decoder.Reference = decodeBuffer[offset];
                offset++;
                length--;
            }

            if(length <= 0)
                return;

            int blocks = length / this.decoder.CompressionFactor;

            if(blocks > 0)
            {
                InternalRead(this.decodeBuffer, 0, blocks);
                this.decoder.Decode(this.decodeBuffer, 0, blocks, buffer, offset);
            }

            int remainder = length % this.decoder.CompressionFactor;
            if(remainder > 0)
            {
                InternalRead(this.decodeRemainder, 0, remainder);
                Array.Reverse(this.decodeRemainder, 0, remainder);
                this.decodeRemainderOffset = remainder - 1;
            }
        }
        /// <summary>
        /// Writes data from a DMA transfer.
        /// </summary>
        /// <param name="source">Pointer to data in memory.</param>
        /// <param name="count">Number of bytes to write.</param>
        /// <returns>Number of bytes actually written.</returns>
        public int DmaWrite(IntPtr source, int count)
        {
            int actualCount = this.waveBuffer.Write(source, count);

            if(this.AutoInitialize)
            {
                this.autoInitTotal += actualCount;
                if(this.autoInitTotal >= this.BlockTransferSize)
                {
                    this.autoInitTotal -= this.BlockTransferSize;
                    OnAutoInitBufferComplete(EventArgs.Empty);
                }
            }

            return actualCount;
        }
        /// <summary>
        /// Resets the DSP to its initial state.
        /// </summary>
        public void Reset()
        {
            this.SampleRate = 22050;
            this.BlockTransferSize = 65536;
            this.AutoInitialize = false;
            this.is16Bit = false;
            this.isStereo = false;
            this.autoInitTotal = 0;
            this.readIdleCycles = 0;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Reads samples from the internal buffer.
        /// </summary>
        /// <param name="buffer">Buffer into which sample data is written.</param>
        /// <param name="offset">Offset in buffer to start writing.</param>
        /// <param name="length">Number of samples to read.</param>
        private void InternalRead(byte[] buffer, int offset, int length)
        {
            int remaining = length;

            while(remaining > 0)
            {
                int writePos = offset + length - remaining;
                int amt = waveBuffer.Read(buffer, writePos, remaining);

                if(amt == 0)
                {
                    if(!this.IsEnabled || this.readIdleCycles >= 100)
                    {
                        byte zeroValue = this.Is16Bit ? (byte)0 : (byte)128;

                        for(int i = 0; i < remaining; i++)
                            buffer[writePos + i] = zeroValue;

                        return;
                    }

                    this.readIdleCycles++;
                    Thread.Sleep(1);
                }
                else
                    this.readIdleCycles = 0;

                writePos += amt;
                remaining -= amt;
            }
        }
        /// <summary>
        /// Raises the AutoInitBufferComplete event.
        /// </summary>
        /// <param name="e">Unused EventArgs instance.</param>
        private void OnAutoInitBufferComplete(EventArgs e)
        {
            var handler = this.AutoInitBufferComplete;
            if(handler != null)
                handler(this, e);
        }
        #endregion

        #region Private Fields
        /// <summary>
        /// Virtual machine instance which owns the Sound Blaster device.
        /// </summary>
        private readonly VirtualMachine vm;
        /// <summary>
        /// DMA channel used for 8-bit data transfers.
        /// </summary>
        private readonly DmaChannel dmaChannel8;
        /// <summary>
        /// DMA channel used for 16-bit data transfers.
        /// </summary>
        private readonly DmaChannel dmaChannel16;
        /// <summary>
        /// Currently active DMA channel.
        /// </summary>
        private DmaChannel currentChannel;

        /// <summary>
        /// Number of bytes transferred in the current auto-init cycle.
        /// </summary>
        private int autoInitTotal;
        /// <summary>
        /// Number of cycles with no new input data.
        /// </summary>
        private int readIdleCycles;

        /// <summary>
        /// Indicates whether DMA data is 16-bit.
        /// </summary>
        private bool is16Bit;
        /// <summary>
        /// Indicates whether DMA data is in stereo.
        /// </summary>
        private bool isStereo;

        /// <summary>
        /// The current compression level.
        /// </summary>
        private CompressionLevel compression;
        /// <summary>
        /// Indicates whether a reference byte is expected.
        /// </summary>
        private bool referenceByteExpected;
        /// <summary>
        /// Current ADPCM decoder instance.
        /// </summary>
        private ADPCMDecoder decoder;
        /// <summary>
        /// Buffer used for ADPCM decoding.
        /// </summary>
        private byte[] decodeBuffer;
        /// <summary>
        /// Last index of remaining decoded bytes.
        /// </summary>
        private int decodeRemainderOffset;
        /// <summary>
        /// Remaining decoded bytes.
        /// </summary>
        private byte[] decodeRemainder = new byte[4];

        /// <summary>
        /// Contains generated waveform data waiting to be read.
        /// </summary>
        private readonly CircularBuffer waveBuffer = new CircularBuffer(TargetBufferSize);
        #endregion

        #region Private Constants
        /// <summary>
        /// Size of output buffer in samples.
        /// </summary>
        private const int TargetBufferSize = 2048;
        #endregion
    }
}
