using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Ymf262Emu;

namespace Aeon.Emulator.Sound.FM
{
    /// <summary>
    /// Virtual device which emulates OPL3 FM sound.
    /// </summary>
    public sealed class FmSoundCard : IInputPort, IOutputPort
    {
        private const byte Timer1Mask = 0xC0;
        private const byte Timer2Mask = 0xA0;
        private const int DefaultSampleRate = 44100;

        private readonly IntPtr hwnd;
        private DirectSound directSound;
        private DirectSoundBuffer soundBuffer;
        private int currentAddress;
        private readonly FmSynthesizer synth;
        private System.Threading.Thread generateThread;
        private volatile bool endThread;
        private byte timer1Data;
        private byte timer2Data;
        private byte timerControlByte;
        private byte statusByte;
        private bool initialized;
        private bool paused;
        private readonly int sampleRate;

        /// <summary>
        /// Initializes a new instance of the FmSoundCard class.
        /// </summary>
        /// <param name="hwnd">Main application window handle.</param>
        public FmSoundCard(IntPtr hwnd)
            : this(DefaultSampleRate, hwnd)
        {
        }
        /// <summary>
        /// Initializes a new instance of the FmSoundCard class.
        /// </summary>
        /// <param name="sampleRate">Sample rate of generated PCM data.</param>
        /// <param name="hwnd">Main application window handle.</param>
        public FmSoundCard(int sampleRate, IntPtr hwnd)
        {
            this.sampleRate = sampleRate;
            this.hwnd = hwnd;
            this.synth = new FmSynthesizer(sampleRate);
            this.generateThread = new System.Threading.Thread(this.GenerateWaveforms)
            {
                IsBackground = true,
                Priority = System.Threading.ThreadPriority.AboveNormal
            };
        }

        IEnumerable<int> IInputPort.InputPorts => new int[] { 0x388 };
        byte IInputPort.ReadByte(int port)
        {
            if ((this.timerControlByte & 0x01) != 0x00 && (this.statusByte & Timer1Mask) == 0)
            {
                this.timer1Data++;
                if (this.timer1Data == 0)
                    this.statusByte |= Timer1Mask;
            }

            if ((this.timerControlByte & 0x02) != 0x00 && (this.statusByte & Timer2Mask) == 0)
            {
                this.timer2Data++;
                if (this.timer2Data == 0)
                    this.statusByte |= Timer2Mask;
            }

            return this.statusByte;
        }
        ushort IInputPort.ReadWord(int port) => this.statusByte;

        IEnumerable<int> IOutputPort.OutputPorts => new int[] { 0x388, 0x389 };
        void IOutputPort.WriteByte(int port, byte value)
        {
            if (port == 0x388)
            {
                currentAddress = value;
            }
            else if (port == 0x389)
            {
                if (currentAddress == 0x02)
                {
                    this.timer1Data = value;
                }
                else if (currentAddress == 0x03)
                {
                    this.timer2Data = value;
                }
                else if (currentAddress == 0x04)
                {
                    this.timerControlByte = value;
                    if ((value & 0x80) == 0x80)
                        this.statusByte = 0;
                }
                else
                {
                    if (!this.initialized)
                        this.Initialize();

                    this.synth.SetRegisterValue(0, currentAddress, value);
                }
            }
        }
        void IOutputPort.WriteWord(int port, ushort value)
        {
            if (port == 0x388)
            {
                ((IOutputPort)this).WriteByte(0x388, (byte)value);
                ((IOutputPort)this).WriteByte(0x389, (byte)(value >> 8));
            }
        }

        void IVirtualDevice.Pause()
        {
            if (this.initialized && !this.paused)
            {
                this.endThread = true;
                this.generateThread.Join();
                this.paused = true;
            }
        }
        void IVirtualDevice.Resume()
        {
            if (paused)
            {
                this.endThread = false;
                this.generateThread = new System.Threading.Thread(this.GenerateWaveforms) { IsBackground = true };
                this.generateThread.Start();
                this.paused = false;
            }
        }
        void IVirtualDevice.DeviceRegistered(VirtualMachine vm)
        {
        }

        public void Dispose()
        {
            if (this.initialized)
            {
                if (!paused)
                {
                    this.endThread = true;
                    this.generateThread.Join();
                }

                this.soundBuffer.Dispose();
                this.initialized = false;
            }
        }

        /// <summary>
        /// Generates and plays back output waveform data.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private void GenerateWaveforms()
        {
            var buffer = new short[1024];
            synth.GetData(buffer);
            soundBuffer.Play(PlaybackMode.LoopContinuously);
            while (!endThread)
            {
                while (soundBuffer.Write(buffer, 0, buffer.Length))
                {
                    synth.GetData(buffer);
                }

                System.Threading.Thread.Sleep(5);
            }

            soundBuffer.Stop();
        }
        /// <summary>
        /// Performs DirectSound initialization.
        /// </summary>
        private void Initialize()
        {
            this.directSound = DirectSound.GetInstance(hwnd);
            this.soundBuffer = directSound.CreateBuffer(sampleRate, ChannelMode.Monaural, BitsPerSample.Sixteen, TimeSpan.FromSeconds(0.25));
            this.generateThread.Start();
            this.initialized = true;
        }
    }
}
