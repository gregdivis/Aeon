using System;
using System.Collections.Generic;

namespace Aeon.Emulator
{
    /// <summary>
    /// Implements a null IO port which stores a single value.
    /// </summary>
    internal sealed class DefaultPortHandler : IInputPort, IOutputPort
    {
        private readonly SortedList<int, ushort> values = new SortedList<int, ushort>();

        public IEnumerable<int> InputPorts => values.Keys;
        public byte ReadByte(int port) => 0xFF;
        public ushort ReadWord(int port) => 0xFFFF;

        public IEnumerable<int> OutputPorts => values.Keys;
        public void WriteByte(int port, byte value) => values[port] = value;
        public void WriteWord(int port, ushort value) => values[port] = value;

        void IVirtualDevice.Pause()
        {
        }
        void IVirtualDevice.Resume()
        {
        }
        void IVirtualDevice.DeviceRegistered(VirtualMachine vm)
        {
        }
        void IDisposable.Dispose()
        {
        }
    }
}
