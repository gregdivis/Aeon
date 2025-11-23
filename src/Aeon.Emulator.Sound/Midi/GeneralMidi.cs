namespace Aeon.Emulator.Sound;

/// <summary>
/// Virtual device which emulates general midi playback.
/// </summary>
public sealed class GeneralMidi : IInputPort, IOutputPort, IDisposable
{
    private MidiDevice? midiMapper;
    private readonly Queue<byte> dataBytes = new();

    private const int DataPort = 0x330;
    private const int StatusPort = 0x331;
    private const byte ResetCommand = 0xFF;
    private const byte EnterUartModeCommand = 0x3F;
    private const byte CommandAcknowledge = 0xFE;

    /// <summary>
    /// Initializes a new instance of the GeneralMidi class.
    /// </summary>
    /// <param name="options">Options used to initialize MIDI playback.</param>
    public GeneralMidi(GeneralMidiOptions? options = null)
    {
        this.Options = options ?? new GeneralMidiOptions(MidiEngine.MidiMapper);
    }

    /// <summary>
    /// Gets the current state of the General MIDI device.
    /// </summary>
    public GeneralMidiState State { get; private set; }
    /// <summary>
    /// Gets the initialization options.
    /// </summary>
    public GeneralMidiOptions Options { get; }
    /// <summary>
    /// Gets a value indicating whether to emulate an MT-32 device.
    /// </summary>
    public bool UseMT32 => this.Options.Engine == MidiEngine.Mt32 && !string.IsNullOrWhiteSpace(this.Options.Mt32RomsPath);
    /// <summary>
    /// Gets a value indicating whether to use MeltySynth for MIDI playback.
    /// </summary>
    public bool UseMeltySynth => this.Options.Engine == MidiEngine.MeltySynth && !string.IsNullOrWhiteSpace(this.Options.SoundFontPath);

    /// <summary>
    /// Gets the current value of the MIDI status port.
    /// </summary>
    private GeneralMidiStatus Status
    {
        get
        {
            var status = GeneralMidiStatus.OutputReady;

            if (this.dataBytes.Count > 0)
                status |= GeneralMidiStatus.InputReady;

            return status;
        }
    }

    IEnumerable<int> IInputPort.InputPorts => [DataPort, StatusPort];
    byte IInputPort.ReadByte(int port)
    {
        switch (port)
        {
            case DataPort:
                if (this.dataBytes.Count > 0)
                    return this.dataBytes.Dequeue();
                else
                    return 0;

            case StatusPort:
                return (byte)(~(byte)this.Status & 0xC0);

            default:
                throw new ArgumentException("Invalid MIDI port.");
        }
    }
    ushort IInputPort.ReadWord(int port) => ((IInputPort)this).ReadByte(port);

    IEnumerable<int> IOutputPort.OutputPorts => [0x330, 0x331];
    void IOutputPort.WriteByte(int port, byte value)
    {
        switch (port)
        {
            case DataPort:
                this.TryCreateMidiMapper();
                this.midiMapper?.SendByte(value);
                break;

            case StatusPort:
                switch (value)
                {
                    case ResetCommand:
                        State = GeneralMidiState.NormalMode;
                        this.dataBytes.Clear();
                        this.dataBytes.Enqueue(CommandAcknowledge);
                        this.midiMapper?.Dispose();
                        this.midiMapper = null;
                        break;

                    case EnterUartModeCommand:
                        this.State = GeneralMidiState.UartMode;
                        this.dataBytes.Enqueue(CommandAcknowledge);
                        break;
                }
                break;
        }
    }
    void IOutputPort.WriteWord(int port, ushort value) => ((IOutputPort)this).WriteByte(port, (byte)value);

    Task IVirtualDevice.PauseAsync()
    {
        this.midiMapper?.Pause();
        return Task.CompletedTask;
    }
    Task IVirtualDevice.ResumeAsync()
    {
        this.midiMapper?.Resume();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        this.midiMapper?.Dispose();
        this.midiMapper = null;
    }

    private void TryCreateMidiMapper()
    {
        this.midiMapper ??= this.Options.Engine switch
        {
            MidiEngine.MeltySynth when !string.IsNullOrWhiteSpace(this.Options.SoundFontPath) => new MeltySynthMidiMapper(this.Options.SoundFontPath),
            MidiEngine.Mt32 when !string.IsNullOrWhiteSpace(this.Options.Mt32RomsPath) => new Mt32MidiDevice(this.Options.Mt32RomsPath),
            _ => OperatingSystem.IsWindows() ? new WindowsMidiMapper() : null
        };
    }

    [Flags]
    private enum GeneralMidiStatus : byte
    {
        /// <summary>
        /// The status of the device is unknown.
        /// </summary>
        None = 0,
        /// <summary>
        /// The command port may be written to.
        /// </summary>
        OutputReady = (1 << 6),
        /// <summary>
        /// The data port may be read from.
        /// </summary>
        InputReady = (1 << 7)
    }
}
