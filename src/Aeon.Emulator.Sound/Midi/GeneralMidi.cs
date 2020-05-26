using System;
using System.Collections.Generic;

namespace Aeon.Emulator.Sound
{
    /// <summary>
    /// Virtual device which emulates general midi playback.
    /// </summary>
    public sealed class GeneralMidi : IInputPort, IOutputPort
    {
        private MidiMapper midiMapper;
        private readonly Queue<byte> dataBytes = new Queue<byte>();

        private const int DataPort = 0x330;
        private const int StatusPort = 0x331;
        private const byte ResetCommand = 0xFF;
        private const byte EnterUartModeCommand = 0x3F;
        private const byte CommandAcknowledge = 0xFE;

        /// <summary>
        /// Initializes a new instance of the GeneralMidi class.
        /// </summary>
        public GeneralMidi()
        {
        }

        /// <summary>
        /// Gets the current state of the General MIDI device.
        /// </summary>
        public GeneralMidiState State { get; private set; }

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

        IEnumerable<int> IInputPort.InputPorts => new int[] { DataPort, StatusPort };
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

        IEnumerable<int> IOutputPort.OutputPorts => new int[] { 0x330, 0x331 };
        void IOutputPort.WriteByte(int port, byte value)
        {
            switch (port)
            {
                case DataPort:
                    if (this.midiMapper == null)
                        this.midiMapper = new MidiMapper();
                    this.midiMapper.SendByte(value);
                    break;

                case StatusPort:
                    switch (value)
                    {
                        case ResetCommand:
                            State = GeneralMidiState.NormalMode;
                            this.dataBytes.Clear();
                            this.dataBytes.Enqueue(CommandAcknowledge);
                            if (this.midiMapper != null)
                            {
                                this.midiMapper.Dispose();
                                this.midiMapper = null;
                            }
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

        void IVirtualDevice.Pause()
        {
            if (this.midiMapper != null)
                this.midiMapper.Reset();
        }
        void IVirtualDevice.Resume()
        {
        }
        void IVirtualDevice.DeviceRegistered(VirtualMachine vm)
        {
        }

        public void Dispose()
        {
            if (this.midiMapper != null)
            {
                this.midiMapper.Dispose();
                this.midiMapper = null;
            }
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

    /// <summary>
    /// Specifies the current state of the General MIDI device.
    /// </summary>
    public enum GeneralMidiState
    {
        /// <summary>
        /// The device is in normal mode.
        /// </summary>
        NormalMode,
        /// <summary>
        /// The device is in UART mode.
        /// </summary>
        UartMode
    }
}
