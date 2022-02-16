using System;
using System.Collections.Generic;
using System.IO;

namespace Aeon.Emulator.Input
{
    /// <summary>
    /// Emulates a joystick or game controller on the game port.
    /// </summary>
    public sealed class JoystickDevice : IInputPort, IOutputPort, IDisposable
    {
        /// <summary>
        /// The number of cycles that represents the maximum value of an axis.
        /// </summary>
        private const int MaximumCount = 300;
        /// <summary>
        /// The value to divide the joystick axis position by to get the number of cycles.
        /// </summary>
        private const int CountFactor = ushort.MaxValue / MaximumCount;

        private readonly DirectInputDevice deviceA;
        private readonly DirectInputDevice deviceB;
        private int xCounterA;
        private int yCounterA;
        private int xCounterB;
        private int yCounterB;
        private bool disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="JoystickDevice"/> class.
        /// </summary>
        /// <param name="hwnd">The handle to the main application window.</param>
        /// <param name="deviceA">The instance ID of the first input device.</param>
        /// <param name="deviceB">The instance ID of the second input device.</param>
        public JoystickDevice(IntPtr hwnd, Guid? deviceA, Guid? deviceB)
        {
            if (deviceA == null && deviceB == null)
                return;

            var input = DirectInput.GetInstance(hwnd);
            if (deviceA != null)
                this.deviceA = input.CreateDevice((Guid)deviceA);
            if (deviceB != null)
                this.deviceB = input.CreateDevice((Guid)deviceB);
        }

        /// <summary>
        /// Gets the input ports implemented by the device.
        /// </summary>
        public IEnumerable<int> InputPorts => new[] { 0x201 };
        /// <summary>
        /// Gets the output ports implemented by the device.
        /// </summary>
        public IEnumerable<int> OutputPorts => new[] { 0x201 };

        /// <summary>
        /// Reads a single byte from one of the device's supported ports.
        /// </summary>
        /// <param name="port">Port from which byte is read.</param>
        /// <returns>Byte read from the specified port.</returns>
        byte IInputPort.ReadByte(int port)
        {
            int portValue = 0;

            if (this.deviceA == null && this.deviceB == null)
                return 0xF0;

            if (this.deviceA != null)
            {
                this.deviceA.Update();
                if (!this.deviceA.Button1)
                    portValue |= 0x10;
                if (!this.deviceA.Button2)
                    portValue |= 0x20;

                if (this.deviceB == null)
                {
                    if (!this.deviceA.Button3)
                        portValue |= 0x40;
                    if (!this.deviceA.Button4)
                        portValue |= 0x80;
                }

                if (this.xCounterA > 0)
                {
                    this.xCounterA--;
                    if (this.xCounterA > 0)
                        portValue |= 0x01;
                }

                if (this.yCounterA > 0)
                {
                    this.yCounterA--;
                    if (this.yCounterA > 0)
                        portValue |= 0x02;
                }
            }

            if (this.deviceB != null)
            {
                this.deviceB.Update();
                if (!this.deviceB.Button1)
                    portValue |= 0x40;
                if (!this.deviceB.Button2)
                    portValue |= 0x80;

                if (this.xCounterB > 0)
                {
                    this.xCounterB--;
                    if (this.xCounterB > 0)
                        portValue |= 0x04;
                }

                if (this.yCounterB > 0)
                {
                    this.yCounterB--;
                    if (this.yCounterB > 0)
                        portValue |= 0x08;
                }

                if (this.deviceA == null)
                {
                    portValue |= 0x30;
                }
            }

            return (byte)portValue;
        }
        /// <summary>
        /// Reads two bytes from one of the device's supported ports.
        /// </summary>
        /// <param name="port">Port from which bytes are read.</param>
        /// <returns>Bytes read from the specified port.</returns>
        ushort IInputPort.ReadWord(int port)
        {
            return ((IInputPort)this).ReadByte(port);
        }
        /// <summary>
        /// Writes a single byte to one of the device's supported ports.
        /// </summary>
        /// <param name="port">Port where byte will be written.</param>
        /// <param name="value">Value to write to the port.</param>
        void IOutputPort.WriteByte(int port, byte value)
        {
            if (this.deviceA != null)
            {
                this.deviceA.Update();
                this.xCounterA = this.deviceA.XAxisPosition / CountFactor;
                this.yCounterA = this.deviceA.YAxisPosition / CountFactor;
            }

            if (this.deviceB != null)
            {
                this.deviceB.Update();
                this.xCounterB = this.deviceB.XAxisPosition / CountFactor;
                this.yCounterB = this.deviceB.YAxisPosition / CountFactor;
            }
        }
        /// <summary>
        /// Writes two bytes to one or two of the device's supported ports.
        /// </summary>
        /// <param name="port">Port where first byte will be written.</param>
        /// <param name="value">Value to write to the ports.</param>
        void IOutputPort.WriteWord(int port, ushort value) => ((IOutputPort)this).WriteByte(port, (byte)value);

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (!this.disposed)
            {
                this.disposed = true;
                this.deviceA?.Dispose();
                this.deviceB?.Dispose();
            }
        }
    }
}
