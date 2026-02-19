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
    private const float Amplitude = 0.5f;
    private const int OutputSampleRate = 44100;

    private volatile uint frequencyRegister;
    private byte? nextFrequencyRegisterByte;
    private volatile SpeakerControl controlRegister = SpeakerControl.UseTimer;
    private double phase;
    private AudioPlayer? audioPlayer;

    ReadOnlySpan<ushort> IInputPort.InputPorts => [0x61];
    ReadOnlySpan<ushort> IOutputPort.OutputPorts => [0x42, 0x61];

    byte IInputPort.ReadByte(int port) => (byte)this.controlRegister;
    ushort IInputPort.ReadWord(int port) => throw new NotImplementedException();
    void IOutputPort.WriteByte(int port, byte value)
    {
        if (port == 0x61)
        {
            var oldValue = this.controlRegister;
            this.controlRegister = (SpeakerControl)value;
            if (!oldValue.HasFlag(SpeakerControl.SpeakerOn) && this.controlRegister.HasFlag(SpeakerControl.SpeakerOn))
            {
                this.audioPlayer ??= AudioPlayer.CreateDefault(TimeSpan.FromSeconds(0.1), true, new AudioFormat(OutputSampleRate, 1, SampleFormat.IeeeFloat32));
                if (!this.audioPlayer.Playing)
                    this.audioPlayer.BeginPlayback(this.WriteAudioData);
            }
            else if (oldValue.HasFlag(SpeakerControl.SpeakerOn) && !this.controlRegister.HasFlag(SpeakerControl.SpeakerOn))
            {
                this.audioPlayer?.StopPlayback();
            }
        }
        else
        {
            if (!this.nextFrequencyRegisterByte.HasValue)
            {
                this.nextFrequencyRegisterByte = value;
            }
            else
            {
                this.frequencyRegister = this.nextFrequencyRegisterByte.GetValueOrDefault() | ((uint)value << 8);
                this.nextFrequencyRegisterByte = null;
                this.phase = 0;
            }
        }
    }

    Task IVirtualDevice.PauseAsync()
    {
        this.audioPlayer?.StopPlayback();
        return Task.CompletedTask;
    }
    Task IVirtualDevice.ResumeAsync()
    {
        this.audioPlayer?.BeginPlayback(this.WriteAudioData);
        return Task.CompletedTask;
    }
    void IDisposable.Dispose()
    {
        this.audioPlayer?.Dispose();
        this.audioPlayer = null;
    }

    private void WriteAudioData(Span<float> buffer, out int samplesWritten)
    {
        bool isOn = this.controlRegister.HasFlag(SpeakerControl.SpeakerOn);
        var frequency = FrequencyFactor / this.frequencyRegister;

        if (!isOn || frequency <= 0)
        {
            buffer.Clear();
            samplesWritten = buffer.Length;
            return;
        }

        var phaseIncrement = frequency / OutputSampleRate;

        for (int i = 0; i < buffer.Length; i++)
        {
            var normalizedPhase = this.phase % 1.0;
            buffer[i] = normalizedPhase < 0.5 ? Amplitude : -Amplitude;
            this.phase += phaseIncrement;
        }

        samplesWritten = buffer.Length;
    }
}
