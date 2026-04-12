using Ymf262Emu;

namespace Aeon.Emulator.Sound.FM;

/// <summary>
/// Virtual device which emulates OPL3 FM sound.
/// </summary>
public sealed class FmSoundCard : IInputPort, IOutputPort, IAudioSource
{
    private const byte Timer1Mask = 0xC0;
    private const byte Timer2Mask = 0xA0;

    private int currentAddress;
    private FmSynthesizer synth = new();
    private byte timer1Data;
    private byte timer2Data;
    private byte timerControlByte;
    private byte statusByte;
    private bool initialized;

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

    bool IAudioSource.Active => this.initialized;

    public int OutputSampleRate
    {
        get => this.synth.SampleRate;
        set
        {
            if (this.synth.SampleRate != value)
                this.synth = new FmSynthesizer(value);
        }
    }

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
                this.initialized = true;

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

    void IVirtualDevice.DeviceRegistered(VirtualMachine vm) => vm.AudioMixer.RegisterSource(this);

    void IAudioSource.RenderAudio(Span<float> left, Span<float> right)
    {
        this.synth.GetData(left);
        left.CopyTo(right);
    }
}
