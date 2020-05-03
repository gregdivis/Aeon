using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Aeon.Emulator.Sound.PCSpeaker
{
    /// <summary>
    /// Emulates a PC speaker.
    /// </summary>
    public sealed class InternalSpeaker : IInputPort, IOutputPort
    {
        /// <summary>
        /// Value into which the input frequency is divided to get the frequency in Hz.
        /// </summary>
        private const double FrequencyFactor = 1193180;

        private readonly int outputSampleRate = 44100;
        private readonly int ticksPerSample;
        private readonly LatchedUInt16 frequencyRegister = new LatchedUInt16();
        private readonly Stopwatch durationTimer = new Stopwatch();
        private readonly ConcurrentQueue<QueuedNote> queuedNotes = new ConcurrentQueue<QueuedNote>();
        private readonly object threadStateLock = new object();
        private DirectSound directSound;
        private DirectSoundBuffer soundBuffer;
        private SpeakerControl controlRegister = SpeakerControl.UseTimer;
        private volatile bool isThreadActive;
        private int currentPeriod;

        /// <summary>
        /// Initializes a new instance of the InternalSpeaker class.
        /// </summary>
        /// <param name="hwnd">The parent window handle.</param>
        public InternalSpeaker(IntPtr hwnd)
        {
            this.frequencyRegister.ValueChanged += this.FrequencyChanged;
            this.ticksPerSample = (int)((double)Stopwatch.Frequency / (double)outputSampleRate);
            this.directSound = DirectSound.GetInstance(hwnd);
        }

        IEnumerable<int> IInputPort.InputPorts => new int[] { 0x61 };
        IEnumerable<int> IOutputPort.OutputPorts => new int[] { 0x42, 0x61 };

        /// <summary>
        /// Gets the current frequency in Hz.
        /// </summary>
        private double Frequency => FrequencyFactor / this.frequencyRegister;
        /// <summary>
        /// Gets the current period in samples.
        /// </summary>
        private int PeriodInSamples => (int)(this.outputSampleRate / this.Frequency);

        byte IInputPort.ReadByte(int port)
        {
            if (port == 0x61)
                return (byte)this.controlRegister;

            throw new NotSupportedException();
        }
        ushort IInputPort.ReadWord(int port) => throw new NotImplementedException();
        void IOutputPort.WriteByte(int port, byte value)
        {
            if (port == 0x61)
            {
                var oldValue = this.controlRegister;
                this.controlRegister = (SpeakerControl)value;
                if ((oldValue & SpeakerControl.SpeakerOn) != 0 && (this.controlRegister & SpeakerControl.SpeakerOn) == 0)
                    this.SpeakerDisabled();
            }
            else if (port == 0x42)
            {
                this.frequencyRegister.WriteByte(value);
            }
            else
            {
                throw new NotSupportedException();
            }
        }
        void IOutputPort.WriteWord(int port, ushort value) => throw new NotImplementedException();
        void IVirtualDevice.Pause()
        {
        }
        void IVirtualDevice.Resume()
        {
        }
        void IVirtualDevice.DeviceRegistered(VirtualMachine vm)
        {
        }
        public void Dispose()
        {
            this.frequencyRegister.ValueChanged -= this.FrequencyChanged;
            this.isThreadActive = false;
            this.directSound = null;
        }

        /// <summary>
        /// Fills a buffer with silence.
        /// </summary>
        /// <param name="buffer">Buffer to fill.</param>
        private static void GenerateSilence(byte[] buffer)
        {
            for (int i = 0; i < buffer.Length; i++)
                buffer[i] = 127;
        }

        /// <summary>
        /// Starts the tone generator thread.
        /// </summary>
        private void BeginToneGenerator()
        {
            this.soundBuffer = this.directSound.CreateBuffer(this.outputSampleRate, ChannelMode.Monaural, BitsPerSample.Eight, this.outputSampleRate / 4);
            var thread = new Thread(this.ToneGenerator) { IsBackground = true };
            this.isThreadActive = true;
            thread.Start();
        }
        /// <summary>
        /// Invoked when the speaker has been turned off.
        /// </summary>
        private void SpeakerDisabled()
        {
            this.EnqueueCurrentNote();
            this.currentPeriod = 0;
        }
        /// <summary>
        /// Invoked when the frequency has changed.
        /// </summary>
        /// <param name="source">Source of the event.</param>
        /// <param name="e">Unused EventArgs instance.</param>
        private void FrequencyChanged(object source, EventArgs e)
        {
            this.EnqueueCurrentNote();

            this.durationTimer.Reset();
            this.durationTimer.Start();
            this.currentPeriod = this.PeriodInSamples;
        }
        /// <summary>
        /// Enqueues the current note.
        /// </summary>
        private void EnqueueCurrentNote()
        {
            if (this.durationTimer.IsRunning && this.currentPeriod != 0)
            {
                this.durationTimer.Stop();

                int periodDuration = this.ticksPerSample * this.currentPeriod;
                int repetitions = (int)(this.durationTimer.ElapsedTicks / periodDuration);
                this.queuedNotes.Enqueue(new QueuedNote(this.currentPeriod, repetitions));

                lock (this.threadStateLock)
                {
                    if (!this.isThreadActive)
                        this.BeginToneGenerator();
                }
            }
        }
        /// <summary>
        /// Fills a buffer with a square wave of the current frequency.
        /// </summary>
        /// <param name="buffer">Buffer to fill.</param>
        /// <param name="period">The number of samples in the period.</param>
        /// <returns>Number of bytes written to the buffer.</returns>
        private int GenerateSquareWave(byte[] buffer, int period)
        {
            if (period < 2)
            {
                buffer[0] = 127;
                return 1;
            }

            for (int i = 0; i < period / 2; i++)
                buffer[i] = 96;

            for (int i = period / 2; i < period; i++)
                buffer[i] = 160;

            return period;
        }
        /// <summary>
        /// Generates the PC speaker waveform.
        /// </summary>
        private void ToneGenerator()
        {
            var buffer = new byte[8192];
            GenerateSilence(buffer);

            // Initialize the buffer with an empty waveform.
            while (this.soundBuffer.Write(buffer, 0, buffer.Length)) { }

            this.soundBuffer.Play(PlaybackMode.LoopContinuously);

            int idleCount = 0;

            while (this.isThreadActive)
            {
                if (this.queuedNotes.TryDequeue(out var note))
                {
                    int samples = GenerateSquareWave(buffer, note.Period);
                    int periods = note.PeriodCount;

                    while (periods > 0)
                    {
                        while (!this.soundBuffer.Write(buffer, 0, samples))
                        {
                            Thread.Sleep(1);
                        }

                        periods--;
                    }

                    GenerateSilence(buffer);
                    idleCount = 0;
                }
                else
                {
                    if (idleCount >= 10000)
                    {
                        lock (this.threadStateLock)
                        {
                            this.soundBuffer.Dispose();
                            this.soundBuffer = null;
                            this.isThreadActive = false;
                            return;
                        }
                    }

                    this.soundBuffer.Write(buffer, 0, buffer.Length / 2);
                    Thread.Sleep(5);
                    idleCount++;
                }
            }

            this.soundBuffer.Dispose();
            this.soundBuffer = null;
        }
    }
}
