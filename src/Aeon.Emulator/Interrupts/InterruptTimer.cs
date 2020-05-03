using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security;

namespace Aeon.Emulator.Interrupts
{
    /// <summary>
    /// Emulates the Intel 8253/8254 programmable interval timer.
    /// </summary>
    public sealed class InterruptTimer : IInputPort, IOutputPort
    {
        /// <summary>
        /// The number of <see cref="System.Diagnostics.Stopwatch"/> timer ticks per millisecond.
        /// </summary>
        public static readonly long StopwatchTicksPerMillisecond = Stopwatch.Frequency / 1000;

        private static readonly double stopwatchTickDuration = 1000.0 / (double)Stopwatch.Frequency;
        private static readonly double pitToStopwatchMultiplier = pitTickDuration / stopwatchTickDuration;
        private static readonly double TimeSpanToStopwatchMultiplier = (double)StopwatchTicksPerMillisecond / (double)TimeSpan.TicksPerMillisecond;

        private int initialValue = 65536;
        private int inLatch;
        private int outLatch;
        private bool wroteLowByte;
        private bool readLowByte;
        private int stopwatchTickPeriod = (int)(65536 * pitToStopwatchMultiplier);
        private Stopwatch pitStopwatch = new Stopwatch();
        private const double pitTickDuration = 8.3809651519468982047972644529744e-4;

        internal InterruptTimer()
        {
        }

        /// <summary>
        /// Gets the current value of the system performance counter.
        /// </summary>
        public static long GlobalTimerTicks
        {
            get
            {
                SafeNativeMethods.QueryPerformanceCounter(out long value);
                return value;
            }
        }

        /// <summary>
        /// Gets the period of the interrupt timer.
        /// </summary>
        public TimeSpan Period => TimeSpan.FromMilliseconds(stopwatchTickPeriod * stopwatchTickDuration);
        /// <summary>
        /// Gets the period of the interrupt timer in stopwatch ticks.
        /// </summary>
        public int TickPeriod => stopwatchTickPeriod;
        /// <summary>
        /// Gets a value indicating whether the interrupt timer has completed a cycle.
        /// </summary>
        public bool IsIntervalComplete => pitStopwatch.ElapsedTicks >= stopwatchTickPeriod;

        /// <summary>
        /// Returns a value indicating whether the real time clock is in the specified state.
        /// </summary>
        /// <param name="interval">Interval within the period in stopwatch ticks.</param>
        /// <param name="period">Length of the period in stopwatch ticks.</param>
        /// <returns>True if the clock is in the specified state; otherwise false.</returns>
        public static bool IsInRealtimeInterval(long interval, long period)
        {
            long ticks = GlobalTimerTicks;
            return (ticks % period) < interval;
        }
        /// <summary>
        /// Returns a value indicating whether the real time clock is in the specified state.
        /// </summary>
        /// <param name="interval">Interval within the period.</param>
        /// <param name="period">Length of the period.</param>
        /// <returns>True if the clock is in the specified state; otherwise false.</returns>
        public static bool IsInRealtimeInterval(TimeSpan interval, TimeSpan period)
        {
            long ticks = GlobalTimerTicks;
            long intervalTicks = (long)(TimeSpanToStopwatchMultiplier * interval.Ticks);
            long periodTicks = (long)(TimeSpanToStopwatchMultiplier * period.Ticks);
            return (ticks % periodTicks) < intervalTicks;
        }

        /// <summary>
        /// Resets the timer to begin a new interval.
        /// </summary>
        public void Reset()
        {
            pitStopwatch.Reset();
            pitStopwatch.Start();
        }
        IEnumerable<int> IInputPort.InputPorts => new int[] { 0x40 };
        byte IInputPort.ReadByte(int port)
        {
            if (!readLowByte)
            {
                readLowByte = true;
                outLatch = (int)(pitStopwatch.ElapsedTicks / InterruptTimer.pitToStopwatchMultiplier);
                return (byte)(outLatch & 0xFF);
            }
            else
            {
                readLowByte = false;
                return (byte)((outLatch >> 8) & 0xFF);
            }
        }
        ushort IInputPort.ReadWord(int port) => (ushort)(pitStopwatch.ElapsedTicks / pitToStopwatchMultiplier);
        IEnumerable<int> IOutputPort.OutputPorts => new[] { 0x40, 0x43 };
        void IOutputPort.WriteByte(int port, byte value)
        {
            if (port == 0x040)
            {
                if (!wroteLowByte)
                {
                    inLatch = value;
                    wroteLowByte = true;
                }
                else
                {
                    int newValue = inLatch | (value << 8);
                    wroteLowByte = false;
                    if (newValue != 0)
                        SetInitialValue(newValue);
                    else
                        SetInitialValue(65536);
                }
            }
            else
            {
                //if(value != 0x36)
                //    throw new NotImplementedException();
            }
        }
        void IOutputPort.WriteWord(int port, ushort value)
        {
            wroteLowByte = false;
            if (value != 0)
                SetInitialValue(value);
            else
                SetInitialValue(65536);
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
        void IDisposable.Dispose()
        {
        }

        /// <summary>
        /// Changes the timer's initial value.
        /// </summary>
        /// <param name="value">New initial value for the timer from 1 to 65536.</param>
        private void SetInitialValue(int value)
        {
            initialValue = value;
            stopwatchTickPeriod = (int)(initialValue * pitToStopwatchMultiplier);
        }

        [SuppressUnmanagedCodeSecurity]
        internal static class SafeNativeMethods
        {
            [DllImport("kernel32.dll")]
            public static extern bool QueryPerformanceCounter(out long value);
        }
    }
}
