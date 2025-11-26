namespace Aeon.Emulator;

/// <summary>
/// Implements a null IO port which stores a single value.
/// </summary>
internal sealed class DefaultPortHandler : IInputPort, IOutputPort
{
    private readonly SortedList<int, ushort> values = [];

    ReadOnlySpan<ushort> IInputPort.InputPorts => [];
    public byte ReadByte(int port) => 0xFF;
    public ushort ReadWord(int port) => 0xFFFF;

    ReadOnlySpan<ushort> IOutputPort.OutputPorts => [];
    public void WriteByte(int port, byte value) => values[port] = value;
    public void WriteWord(int port, ushort value) => values[port] = value;
}
