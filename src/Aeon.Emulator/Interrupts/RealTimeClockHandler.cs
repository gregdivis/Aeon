using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Aeon.Emulator.Interrupts
{
    /// <summary>
    /// Provides real-time-clock services (int 1Ah).
    /// </summary>
    internal sealed class RealTimeClockHandler : IInterruptHandler, IDisposable
    {
        private const byte ReadClock = 0;
        private const byte SetClock = 1;
        private const byte GetDate = 4;

        private VirtualMachine vm;
        private Timer timer;

        IEnumerable<InterruptHandlerInfo> IInterruptHandler.HandledInterrupts => new InterruptHandlerInfo[] { 0x1A };
        void IInterruptHandler.HandleInterrupt(int interrupt)
        {
            var now = DateTime.Now;
            vm.Processor.Flags.Carry = false;

            switch (vm.Processor.AH)
            {
                case ReadClock:
                    // This should be nonzero if timer has run for more than 24 hours.
                    // Ignore it for now.
                    vm.Processor.AL = 0;

                    var nowSpan = now.TimeOfDay;
                    uint dosTicks = (uint)(nowSpan.TotalMilliseconds / 55.0);
                    vm.Processor.DX = (short)(dosTicks & 0xFFFF);
                    vm.Processor.CX = (short)((dosTicks >> 16) & 0xFFFF);
                    break;

                case GetDate:
                    vm.Processor.CX = (short)ConvertToBCD(now.Year);
                    vm.Processor.DL = (byte)ConvertToBCD(now.Day);
                    vm.Processor.DH = (byte)ConvertToBCD(now.Month);
                    break;

                default:
                    System.Diagnostics.Debug.WriteLine($"Timer function {vm.Processor.AH:X2}h not implemented.");
                    break;
            }

            SaveFlags(EFlags.Carry);
        }

        Task IVirtualDevice.PauseAsync()
        {
            this.timer.Change(Timeout.Infinite, Timeout.Infinite);
            return Task.CompletedTask;
        }
        Task IVirtualDevice.ResumeAsync()
        {
            this.timer.Change(0, 55);
            return Task.CompletedTask;
        }

        void IVirtualDevice.DeviceRegistered(VirtualMachine vm)
        {
            this.vm = vm;
            this.timer = new Timer(UpdateClock, null, 0, 55);
        }

        void IDisposable.Dispose() => this.timer.Dispose();

        /// <summary>
        /// Updates the emulated BIOS clock with the number of 55 msec ticks since midnight.
        /// </summary>
        /// <param name="state">Unused state object.</param>
        private void UpdateClock(object state)
        {
            vm.PhysicalMemory.Bios.RealTimeClock = (uint)(DateTime.Now.TimeOfDay.TotalMilliseconds / 55.0);
        }
        private void SaveFlags(EFlags modified)
        {
            var oldFlags = (EFlags)vm.PhysicalMemory.GetUInt16(vm.Processor.SS, (ushort)(vm.Processor.SP + 4));
            oldFlags &= ~modified;
            vm.PhysicalMemory.SetUInt16(vm.Processor.SS, (ushort)(vm.Processor.SP + 4), (ushort)(oldFlags | (vm.Processor.Flags.Value & modified)));
        }
        /// <summary>
        /// Converts an integer value to BCD representation.
        /// </summary>
        /// <param name="value">Integer value to convert.</param>
        /// <returns>BCD representation of the value.</returns>
        private static int ConvertToBCD(int value)
        {
            int result = 0;
            int byteCount = 0;

            while (value != 0)
            {
                int digit = value % 10;
                value /= 10;
                digit |= (value % 10) << 4;
                value /= 10;

                result |= digit << (byteCount * 8);
                byteCount++;
            }

            return result;
        }
    }
}
