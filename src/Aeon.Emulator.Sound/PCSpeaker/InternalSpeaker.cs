namespace Aeon.Emulator.Sound.PCSpeaker;

/// <summary>
/// Emulates a PC speaker.
/// </summary>
public sealed class InternalSpeaker : IInputPort, IOutputPort, IAudioSource
{
    /// <summary>
    /// Value into which the input frequency is divided to get the frequency in Hz.
    /// </summary>
    private const double FrequencyFactor = 1193180;
    private const float Amplitude = 0.5f;
    #warning sample rate
    private const int OutputSampleRate = 44100;

    private volatile uint frequencyRegister;
    private byte? nextFrequencyRegisterByte;
    private volatile SpeakerControl controlRegister = SpeakerControl.UseTimer;
    private double phase;

    ReadOnlySpan<ushort> IInputPort.InputPorts => [0x61];
    ReadOnlySpan<ushort> IOutputPort.OutputPorts => [0x42, 0x61];

    bool IAudioSource.Active => this.controlRegister.HasFlag(SpeakerControl.SpeakerOn);

    byte IInputPort.ReadByte(int port) => (byte)this.controlRegister;
    ushort IInputPort.ReadWord(int port) => throw new NotImplementedException();
    void IOutputPort.WriteByte(int port, byte value)
    {
        if (port == 0x61)
        {
            this.controlRegister = (SpeakerControl)value;
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

    private void WriteAudioData(Span<float> buffer)
    {
        bool isOn = this.controlRegister.HasFlag(SpeakerControl.SpeakerOn);
        var frequency = FrequencyFactor / this.frequencyRegister;

        if (!isOn || frequency <= 0)
        {
            buffer.Clear();
            return;
        }

        var phaseIncrement = frequency / OutputSampleRate;

        for (int i = 0; i < buffer.Length; i++)
        {
            var normalizedPhase = this.phase % 1.0;
            buffer[i] = normalizedPhase < 0.5 ? Amplitude : -Amplitude;
            this.phase += phaseIncrement;
        }
    }

    void IAudioSource.RenderAudio(Span<float> left, Span<float> right)
    {
        this.WriteAudioData(left);
        left.CopyTo(right);
    }
}
