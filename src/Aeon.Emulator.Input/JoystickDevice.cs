using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Aeon.Emulator.Input
{
    /// <summary>
    /// Emulates a joystick or game controller on the game port.
    /// </summary>
    public sealed class JoystickDevice : IInputPort, IOutputPort, IDisposable
    {
        /// <summary>
        /// 100 kilohms when the value is at its maximum.
        /// </summary>
        private const double MaxResistance = 100_000;
        /// <summary>
        /// Delay is always at least 24.2 microseconds.
        /// </summary>
        private const double MinTime = 24.2;
        /// <summary>
        /// Multiplied with the variable resistance to get the delay.
        /// </summary>
        private const double ResistanceFactor = 0.011;
        /// <summary>
        /// Value of <see cref="ResistanceFactor"/> * <see cref="MaxResistance"/>.
        /// </summary>
        private const double Factor = ResistanceFactor * MaxResistance;

        private VirtualMachine vm;
        private readonly IGameController controller = IGameController.GetDefault();
        private long xAxisDischargeTicks;
        private long yAxisDischargeTicks;
        private readonly Stopwatch dischargeStart = new();
        private string controllerName;

        private bool disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="JoystickDevice"/> class.
        /// </summary>
        public JoystickDevice()
        {
        }

        public IEnumerable<int> InputPorts => new[] { 0x201 };
        public IEnumerable<int> OutputPorts => new[] { 0x201 };

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
            if (ticks < this.xAxisDischargeTicks)
                portValue |= 0x01;

            if (ticks < this.yAxisDischargeTicks)
                portValue |= 0x02;

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
                    this.vm.WriteMessage($"Using {name} as joystick 1.", MessageLevel.Info);
                }

                this.xAxisDischargeTicks = ComputeDischargeTime(((double)state.XAxis + 1) / 2);
                this.yAxisDischargeTicks = ComputeDischargeTime(((double)state.YAxis + 1) / 2);
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

        /// <summary>
        /// Returns the amount of time required for <paramref name="value"/> to discharge in <see cref="TimeSpan"/> ticks.
        /// </summary>
        /// <param name="value">Value from 0 to 1.</param>
        /// <returns><see cref="TimeSpan"/> ticks to wait for discharge to complete.</returns>
        private static long ComputeDischargeTime(double value) => (long)(Math.FusedMultiplyAdd(Factor, value, MinTime) * 10);
    }
}
