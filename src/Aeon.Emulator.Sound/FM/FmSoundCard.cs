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

    private readonly AudioPlayer audioPlayer = AudioPlayer.CreateDefault(TimeSpan.FromSeconds(0.25), true, new AudioFormat(44100, 1, SampleFormat.IeeeFloat32));
    private int currentAddress;
    private readonly FmSynthesizer synth;
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

    Task IVirtualDevice.PauseAsync()
    {
        if (this.initialized && !this.paused)
        {
            this.audioPlayer.StopPlayback();
            this.paused = true;
        }

        return Task.CompletedTask;
    }
    Task IVirtualDevice.ResumeAsync()
    {
        if (paused)
        {
            this.audioPlayer.BeginPlayback(this.GetAudioData);
            this.paused = false;
        }

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        if (this.initialized)
        {
            this.audioPlayer.Dispose();
            this.initialized = false;
        }
    }

    private void GetAudioData(Span<float> buffer, out int samplesWritten)
    {
        this.synth.GetData(buffer);
        samplesWritten = buffer.Length;
    }
    private void Initialize()
    {
        this.audioPlayer.BeginPlayback(this.GetAudioData);
        // this.generateTask = Task.Run(this.GenerateWaveformsAsync);
        this.initialized = true;
    }
}
