using System;
using System.Collections.Generic;
using System.Threading;

namespace Aeon.Emulator
{
    /// <summary>
    /// Emulates the Intel 8259 programmable interrupt controller.
    /// </summary>
    public sealed class InterruptController : IInputPort, IOutputPort
    {
        private const int CommandPort1 = 0x20;
        private const int MaskPort1 = 0x21;
        private const int CommandPort2 = 0xA0;
        private const int MaskPort2 = 0xA1;
        private const int InitializeICW1 = 0x10;
        private const int InitializeICW4 = 0x11;

        private int inServiceRegister;
        private int requestRegister;
        private int maskRegister;
        private Command currentCommand1;
        private Command currentCommand2;
        private State state1;
        private State state2;
        private ReaderWriterLockSlim readerWriter = new ReaderWriterLockSlim();

        internal InterruptController()
        {
            this.BaseInterruptVector1 = 0x08;
            this.BaseInterruptVector2 = 0x70;
        }

        IEnumerable<int> IInputPort.InputPorts => new[] { CommandPort1, MaskPort1, CommandPort2, MaskPort2 };
        IEnumerable<int> IOutputPort.OutputPorts => new[] { CommandPort1, MaskPort1, CommandPort2, MaskPort2 };
        /// <summary>
        /// Gets the base interrupt vector for IRQ 0-7.
        /// </summary>
        public int BaseInterruptVector1 { get; private set; }
        /// <summary>
        /// Gets the base interrupt vector for IRQ 8-15.
        /// </summary>
        public int BaseInterruptVector2 { get; private set; }

        /// <summary>
        /// Signals a hardware interrupt request.
        /// </summary>
        /// <param name="irq">Interrupt request from 0 to 15.</param>
        public void RaiseHardwareInterrupt(int irq)
        {
            if (this.state1 != State.Ready && this.state2 != State.Ready)
                return;

            int bit = 1 << irq;

            readerWriter.EnterWriteLock();

            // Only allow the request if not already being serviced.
            if ((inServiceRegister & bit) == 0)
                requestRegister |= bit;

            readerWriter.ExitWriteLock();
        }
        /// <summary>
        /// Acknowledges a pending interrupt request from the processor thread.
        /// </summary>
        /// <returns>Software interrupt number requested, or -1 if none.</returns>
        public int AcknowledgeRequest()
        {
            readerWriter.EnterWriteLock();

            try
            {
                for (int i = 0; i <= 7; i++)
                {
                    int bit = 1 << i;
                    if ((requestRegister & bit) == bit && ((~maskRegister) & bit) == bit)
                    {
                        requestRegister &= ~bit;
                        inServiceRegister |= bit;
                        return this.BaseInterruptVector1 + i;
                    }
                }

                for (int i = 8; i <= 15; i++)
                {
                    int bit = 1 << i;
                    if ((requestRegister & bit) == bit && ((~maskRegister) & bit) == bit)
                    {
                        requestRegister &= ~bit;
                        inServiceRegister |= bit;
                        return this.BaseInterruptVector2 + i;
                    }
                }

                return -1;
            }
            finally
            {
                readerWriter.ExitWriteLock();
            }
        }
        byte IInputPort.ReadByte(int port) => ReadByte(port);
        ushort IInputPort.ReadWord(int port) => ReadByte(port);
        void IOutputPort.WriteByte(int port, byte value) => WriteByte(port, value);
        void IOutputPort.WriteWord(int port, ushort value)
        {
            if (port == 0x20)
            {
                WriteByte(0x20, (byte)value);
                WriteByte(0x21, (byte)(value >> 8));
            }
            else if (port == 0xA0)
            {
                WriteByte(0xA0, (byte)value);
                WriteByte(0xA1, (byte)(value >> 8));
            }
        }
        void IVirtualDevice.Pause()
        {
        }
        void IVirtualDevice.Resume()
        {
        }
        void IVirtualDevice.DeviceRegistered(VirtualMachine vm)
        {
        }
        void IDisposable.Dispose() => readerWriter.Dispose();

        /// <summary>
        /// Ends the highest-priority in-service interrupt on controller 1.
        /// </summary>
        private void EndCurrentInterrupt1()
        {
            for (int i = 0; i < 8; i++)
            {
                int bit = 1 << i;
                if ((inServiceRegister & bit) == bit)
                {
                    inServiceRegister &= ~bit;
                    return;
                }
            }
        }
        /// <summary>
        /// Ends the highest-priority in-service interrupt on controller 2.
        /// </summary>
        private void EndCurrentInterrupt2()
        {
            for (int i = 8; i < 16; i++)
            {
                int bit = 1 << i;
                if ((inServiceRegister & bit) == bit)
                {
                    inServiceRegister &= ~bit;
                    return;
                }
            }
        }
        /// <summary>
        /// Reads a byte from one of the interrupt controller's ports.
        /// </summary>
        /// <param name="port">Port to read from.</param>
        /// <returns>Value read from the port.</returns>
        private byte ReadByte(int port)
        {
            readerWriter.EnterReadLock();

            try
            {
                switch (port)
                {
                    case CommandPort1:
                        switch (currentCommand1)
                        {
                            case Command.ReadISR:
                                return (byte)inServiceRegister;

                            case Command.ReadIRR:
                                return (byte)requestRegister;
                        }
                        break;

                    case MaskPort1:
                        return (byte)maskRegister;

                    case CommandPort2:
                        switch (currentCommand2)
                        {
                            case Command.ReadISR:
                                return (byte)(inServiceRegister >> 8);

                            case Command.ReadIRR:
                                return (byte)(requestRegister >> 8);
                        }
                        break;

                    case MaskPort2:
                        return (byte)(maskRegister >> 8);
                }

                return 0;
            }
            finally
            {
                readerWriter.ExitReadLock();
            }
        }
        /// <summary>
        /// Writes a byte to one of the interrupt controller's ports.
        /// </summary>
        /// <param name="port">Port to write byte to.</param>
        /// <param name="value">Byte to write to the port.</param>
        private void WriteByte(int port, byte value)
        {
            readerWriter.EnterWriteLock();

            if (value == 0x67)
            {
            }

            try
            {
                int registerValue = maskRegister;

                switch (port)
                {
                    case CommandPort1:
                        if (value == (int)Command.EndOfInterrupt)
                            EndCurrentInterrupt1();
                        else if (value == (int)Command.ReadIRR || value == (int)Command.ReadISR)
                            currentCommand1 = (Command)value;
                        else if ((value & 0x10) != 0) // ICW1
                        {
                            if (value == InitializeICW1 || value == InitializeICW4)
                            {
                                this.requestRegister = 0;
                                this.inServiceRegister = 0;
                                this.maskRegister = 0;
                                this.state1 = State.Initialization_NeedVector;
                            }
                            else
                                throw new NotImplementedException();
                        }
                        else if ((value & 0x18) == 0) // OCW2
                        {
                            if ((value & 0xE0) == 0x60) // Specific EOI
                                this.inServiceRegister &= ~(1 << (value & 0x07));
                            else
                                throw new NotImplementedException();
                        }
                        else
                            throw new NotImplementedException();

                        //currentCommand1 = (Command)value;
                        //switch(currentCommand1)
                        //{
                        //case Command.Initialize:
                        //case Command.InitializeICW4:
                        //    Initialize1();
                        //    break;

                        //case Command.EndOfInterrupt:
                        //    EndCurrentInterrupt1();
                        //    break;

                        //case Command.ReadIRR:
                        //case Command.ReadISR:
                        //    break;

                        //default:
                        //    throw new NotImplementedException();
                        //}
                        break;

                    case MaskPort1:
                        switch (this.state1)
                        {
                            case State.Initialization_NeedVector:
                                this.BaseInterruptVector1 = value;
                                this.state1 = State.Initialization_NeedInt;
                                break;

                            case State.Initialization_NeedInt:
                                this.state1 = State.Initialization_Need1;
                                break;

                            case State.Initialization_Need1:
                                if (value != 1)
                                    throw new InvalidOperationException();
                                this.state1 = State.Ready;
                                break;

                            case State.Ready:
                                registerValue &= 0xFF00;
                                registerValue |= value;
                                maskRegister = registerValue;
                                break;
                        }
                        break;

                    case CommandPort2:
                        currentCommand2 = (Command)value;
                        switch (currentCommand2)
                        {
                            case Command.Initialize:
                            case Command.InitializeICW4:
                                this.state2 = State.Initialization_NeedVector;
                                break;

                            case Command.EndOfInterrupt:
                                EndCurrentInterrupt2();
                                break;
                        }
                        break;

                    case MaskPort2:
                        switch (this.state2)
                        {
                            case State.Initialization_NeedVector:
                                this.BaseInterruptVector2 = value;
                                this.state2 = State.Initialization_NeedInt;
                                break;

                            case State.Initialization_NeedInt:
                                this.state2 = State.Initialization_Need1;
                                break;

                            case State.Initialization_Need1:
                                if (value != 1)
                                    throw new InvalidOperationException();
                                this.state2 = State.Ready;
                                break;

                            case State.Ready:
                                registerValue &= 0x00FF;
                                registerValue |= (value << 8);
                                maskRegister = registerValue;
                                break;
                        }
                        break;
                }
            }
            finally
            {
                readerWriter.ExitWriteLock();
            }
        }

        private enum Command
        {
            None = 0,
            ReadISR = 0x0B,
            ReadIRR = 0x0A,
            Initialize = 0x10,
            InitializeICW4 = 0x11,
            EndOfInterrupt = 0x20
        }

        private enum State
        {
            Ready,
            Initialization_NeedVector,
            Initialization_NeedInt,
            Initialization_Need1
        }
    }
}
