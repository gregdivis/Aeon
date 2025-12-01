using TinyAudio;
using Ymf262Emu;

namespace Aeon.Emulator.Sound.FM;

/// <summary>
/// Virtual device which emulates OPL3 FM sound.
/// </summary>
public sealed class FmSoundCard : IInputPort, IOutputPort, IDisposable
{
    private const byte Timer1Mask = 0xC0;
    private const byte Timer2Mask = 0xA0;

    private readonly AudioPlayer audioPlayer = Audio.CreatePlayer();
    private int currentAddress;
    private readonly FmSynthesizer synth;
    private Task? generateTask;
    private CancellationTokenSource cancelPlayback = new();
    private byte timer1Data;
    private byte timer2Data;
    private byte timerControlByte;
    private byte statusByte;
    private bool initialized;
    private bool paused;

    public FmSoundCard()
    {
        this.synth = new FmSynthesizer(this.audioPlayer.Format.SampleRate);
    }

    ReadOnlySpan<ushort> IInputPort.InputPorts => [0x388];
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

    ReadOnlySpan<ushort> IOutputPort.OutputPorts => [0x388, 0x389];
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

    async Task IVirtualDevice.PauseAsync()
    {
        if (this.initialized && !this.paused)
        {
            this.cancelPlayback.Cancel();
            await this.generateTask!.ConfigureAwait(false);
            this.paused = true;
        }
    }
    Task IVirtualDevice.ResumeAsync()
    {
        if (paused)
        {
            this.cancelPlayback?.Dispose();
            this.cancelPlayback = new();
            this.generateTask = Task.Run(this.GenerateWaveformsAsync);
            this.paused = false;
        }

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        if (this.initialized)
        {
            if (!paused)
            {
                this.cancelPlayback.Cancel();
                this.generateTask!.GetAwaiter().GetResult();
            }

            this.audioPlayer.Dispose();
            this.cancelPlayback.Dispose();
            this.initialized = false;
        }
    }

    private async Task GenerateWaveformsAsync()
    {
        var buffer = new float[1024];
        float[] playBuffer;

        bool expandToStereo = this.audioPlayer.Format.Channels == 2;
        if (expandToStereo)
            playBuffer = new float[buffer.Length * 2];
        else
            playBuffer = buffer;

        this.audioPlayer.BeginPlayback();
        fillBuffer();
        try
        {
            while (!cancelPlayback.IsCancellationRequested)
            {
                await this.audioPlayer.WriteDataAsync(playBuffer, this.cancelPlayback.Token).ConfigureAwait(false);
                fillBuffer();
            }
        }
        catch (OperationCanceledException)
        {
        }

        this.audioPlayer.StopPlayback();

        void fillBuffer()
        {
            this.synth.GetData(buffer);
            if (expandToStereo)
                ChannelAdapter.MonoToStereo(buffer.AsSpan(), playBuffer.AsSpan());
        }
    }
    private void Initialize()
    {
        this.generateTask = Task.Run(this.GenerateWaveformsAsync);
        this.initialized = true;
    }
}
