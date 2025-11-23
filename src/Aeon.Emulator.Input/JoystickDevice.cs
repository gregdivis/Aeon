using System.Diagnostics;

namespace Aeon.Emulator.Input;

/// <summary>
/// Emulates a joystick or game controller on the game port.
/// </summary>
public sealed class JoystickDevice : IInputPort, IOutputPort, IDisposable
{
    private const uint PositionCount = 20000;

    private VirtualMachine? vm;
    private readonly IGameController controller = IGameController.GetDefault();
    private uint xDischargeCount;
    private uint yDischargeCount;
    private readonly Stopwatch dischargeStart = new();
    private string? controllerName;
    private bool disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="JoystickDevice"/> class.
    /// </summary>
    public JoystickDevice()
    {
    }

    public IEnumerable<int> InputPorts => [0x201];
    public IEnumerable<int> OutputPorts => [0x201];

    byte IInputPort.ReadByte(int port)
    {
        int portValue = 0;

        if (!this.controller.TryGetState(out var state))
            return 0xF0;

        if (!state.Buttons.HasFlag(GameControllerButtons.Button1))
            portValue |= 0x10;
        if (!state.Buttons.HasFlag(GameControllerButtons.Button2))
            portValue |= 0x20;

        if (!state.Buttons.HasFlag(GameControllerButtons.Button3))
            portValue |= 0x40;
        if (!state.Buttons.HasFlag(GameControllerButtons.Button4))
            portValue |= 0x80;

        long ticks = this.dischargeStart.ElapsedTicks;
        if (ticks >= TimeSpan.TicksPerMillisecond * 1000)
        {
            this.xDischargeCount = 0;
            this.yDischargeCount = 0;
        }

        if (this.xDischargeCount > 0)
        {
            this.xDischargeCount--;
            portValue |= 0x01;
        }

        if (this.yDischargeCount > 0)
        {
            this.yDischargeCount--;
            portValue |= 0x02;
        }

        return (byte)portValue;
    }
    ushort IInputPort.ReadWord(int port) => ((IInputPort)this).ReadByte(port);
    void IOutputPort.WriteByte(int port, byte value)
    {
        if (this.controller.TryGetState(out var state))
        {
            var name = this.controller.Name;
            if (this.controllerName != name)
            {
                this.controllerName = name;
                this.vm!.WriteMessage($"Using {name} as joystick 1.", MessageLevel.Info);
            }

            this.xDischargeCount = (uint)(((double)state.XAxis + 1) / 2 * PositionCount);
            this.yDischargeCount = (uint)(((double)state.YAxis + 1) / 2 * PositionCount);

            this.dischargeStart.Restart();
        }
    }
    void IOutputPort.WriteWord(int port, ushort value) => ((IOutputPort)this).WriteByte(port, (byte)value);

    Task IVirtualDevice.PauseAsync()
    {
        this.dischargeStart.Stop();
        return Task.CompletedTask;
    }
    Task IVirtualDevice.ResumeAsync()
    {
        this.dischargeStart.Start();
        return Task.CompletedTask;
    }
    void IVirtualDevice.DeviceRegistered(VirtualMachine vm)
    {
        this.vm = vm;
    }

    void IDisposable.Dispose()
    {
        if (!this.disposed)
        {
            this.disposed = true;
            this.controller.Dispose();
        }
    }
}
