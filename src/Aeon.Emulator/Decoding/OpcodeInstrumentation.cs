using System.Diagnostics;
using Aeon.Emulator.Interrupts;

namespace Aeon.Emulator.Decoding
{
    /// <summary>
    /// Provides performance statistics for an instruction.
    /// </summary>
    public sealed class OpcodeInstrumentation
    {
        private readonly Stopwatch enterTime = new Stopwatch();
        private readonly long[] recentTicks = new long[16];
        private long totalCalls;
        private int currentPos;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpcodeInstrumentation"/> class.
        /// </summary>
        public OpcodeInstrumentation()
        {
        }

        /// <summary>
        /// Gets the total number of calls to this instruction.
        /// </summary>
        public long TotalCalls => this.totalCalls;
        /// <summary>
        /// Gets the average amount of time it took to run the instruction in milliseconds.
        /// </summary>
        public double AverageTime
        {
            get
            {
                long total = 0;
                for (int i = 0; i < this.recentTicks.Length; i++)
                    total += this.recentTicks[i];

                total /= 16;
                return total / (double)InterruptTimer.StopwatchTicksPerMillisecond;
            }
        }
        /// <summary>
        /// Gets a value indicating whether this instruction should be counted.
        /// </summary>
        public bool Include => this.totalCalls >= this.recentTicks.Length;

        internal void Enter()
        {
            this.totalCalls++;
            this.enterTime.Start();
        }
        internal void Exit()
        {
            this.enterTime.Stop();
            this.recentTicks[this.currentPos] = this.enterTime.ElapsedTicks;
            this.currentPos = (this.currentPos + 1) % 16;
            this.enterTime.Reset();
        }
        internal void Reset()
        {
            this.enterTime.Reset();
            this.totalCalls = 0;
            for (int i = 0; i < this.recentTicks.Length; i++)
                this.recentTicks[i] = 0;
        }
    }
}
