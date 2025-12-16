using System.Collections.Concurrent;
using System.Diagnostics;
using TinyAudio;

namespace Aeon.Emulator.Sound.PCSpeaker;

/// <summary>
/// Emulates a PC speaker.
/// </summary>
public sealed class InternalSpeaker : IInputPort, IOutputPort, IDisposable
{
    /// <summary>
    /// Value into which the input frequency is divided to get the frequency in Hz.
    /// </summary>
    private const double FrequencyFactor = 1193180;

    private readonly int outputSampleRate = 48000;
    private readonly int ticksPerSample;
    private readonly LatchedUInt16 frequencyRegister = new();
    private readonly Stopwatch durationTimer = new();
    private readonly ConcurrentQueue<QueuedNote> queuedNotes = new();
    private readonly Lock threadStateLock = new();
    private readonly float[] emptyBuffer = new float[1024];
    private SpeakerControl controlRegister = SpeakerControl.UseTimer;
    private Task? generateWaveformTask;
    private readonly CancellationTokenSource cancelGenerateWaveform = new();
    private int currentPeriod;

    /// <summary>
    /// Initializes a new instance of the InternalSpeaker class.
    /// </summary>
    public InternalSpeaker()
    {
        this.frequencyRegister.ValueChanged += this.FrequencyChanged;
        this.ticksPerSample = (int)(Stopwatch.Frequency / (double)this.outputSampleRate);
    }

    ReadOnlySpan<ushort> IInputPort.InputPorts => [0x61];
    ReadOnlySpan<ushort> IOutputPort.OutputPorts => [0x42, 0x61];

    /// <summary>
    /// Gets the current frequency in Hz.
    /// </summary>
    private double Frequency => FrequencyFactor / this.frequencyRegister;
    /// <summary>
    /// Gets the current period in samples.
    /// </summary>
    private int PeriodInSamples => (int)(this.outputSampleRate / this.Frequency);

    byte IInputPort.ReadByte(int port) => (byte)this.controlRegister;
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
        else
        {
            this.frequencyRegister.WriteByte(value);
        }
    }
    void IOutputPort.WriteWord(int port, ushort value) => throw new NotImplementedException();
    public void Dispose()
    {
        this.frequencyRegister.ValueChanged -= this.FrequencyChanged;
        lock (this.threadStateLock)
        {
            this.cancelGenerateWaveform.Cancel();
        }
    }

    /// <summary>
    /// Fills a buffer with silence.
    /// </summary>
    /// <param name="buffer">Buffer to fill.</param>
    private static void GenerateSilence(Span<byte> buffer) => buffer.Fill(127);

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
    private void FrequencyChanged(object? source, EventArgs e)
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
                if (this.generateWaveformTask == null || this.generateWaveformTask.IsCompleted)
                    this.generateWaveformTask = Task.Run(this.GenerateWaveformAsync);
            }
        }
    }
    /// <summary>
    /// Fills a buffer with a square wave of the current frequency.
    /// </summary>
    /// <param name="buffer">Buffer to fill.</param>
    /// <param name="period">The number of samples in the period.</param>
    /// <returns>Number of bytes written to the buffer.</returns>
    private static int GenerateSquareWave(Span<byte> buffer, int period)
    {
        if (period < 2)
        {
            buffer[0] = 127;
            return 1;
        }

        int halfPeriod = period / 2;
        buffer[..halfPeriod].Fill(96);
        buffer.Slice(halfPeriod, halfPeriod).Fill(120);

        return period;
    }
    /// <summary>
    /// Generates the PC speaker waveform.
    /// </summary>
    private async Task GenerateWaveformAsync()
    {
        using var player = Audio.CreatePlayer();

        this.FillWithSilence(player);

        var buffer = new byte[4096];
        var writeBuffer = buffer;
        bool expandToStereo = false;
        if (player.Format.Channels == 2)
        {
            writeBuffer = new byte[buffer.Length * 2];
            expandToStereo = true;
        }

        player.BeginPlayback();

        int idleCount = 0;

        while (idleCount < 10000)
        {
            if (this.queuedNotes.TryDequeue(out var note))
            {
                int samples = GenerateSquareWave(buffer, note.Period);
                int periods = note.PeriodCount;

                if (expandToStereo)
                {
                    ChannelAdapter.MonoToStereo(buffer.AsSpan(0, samples), writeBuffer.AsSpan(0, samples * 2));
                    samples *= 2;
                }

                while (periods > 0)
                {
                    Audio.WriteFullBuffer(player, writeBuffer.AsSpan(0, samples));
                    periods--;
                }

                GenerateSilence(buffer);
                idleCount = 0;
            }
            else
            {
                this.FillWithSilence(player);

                await Task.Delay(5, this.cancelGenerateWaveform.Token);
                idleCount++;
            }

            this.cancelGenerateWaveform.Token.ThrowIfCancellationRequested();
        }
    }

    private void FillWithSilence(AudioPlayer player)
    {
        while (player.WriteData(this.emptyBuffer) > 0)
        {
        }
    }
}
