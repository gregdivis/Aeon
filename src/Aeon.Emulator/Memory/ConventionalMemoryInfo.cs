using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Aeon.Emulator
{
    /// <summary>
    /// Contains information about conventional memory usage.
    /// </summary>
    public sealed class ConventionalMemoryInfo
    {
        /// <summary>
        /// Size of conventional memory in bytes.
        /// </summary>
        public const int ConventionalMemorySize = 0xA0000;

        private readonly ReadOnlyCollection<ProcessAllocation> processes;
        private readonly int totalUsed;
        private readonly int largestFreeBlock;

        internal ConventionalMemoryInfo(IEnumerable<ProcessAllocation> processes, int largestFreeBlock)
        {
            this.processes = new List<ProcessAllocation>(processes).AsReadOnly();
            this.totalUsed = (from p in this.processes
                              select p.AllocationSize).Sum();
            this.largestFreeBlock = largestFreeBlock;
        }

        /// <summary>
        /// Gets the collection of process allocations.
        /// </summary>
        public IEnumerable<ProcessAllocation> Processes => processes;
        /// <summary>
        /// Gets the number of conventional memory bytes allocated.
        /// </summary>
        public int MemoryUsed => totalUsed;
        /// <summary>
        /// Gets the number of available conventional memory bytes.
        /// </summary>
        public int MemoryFree => ConventionalMemorySize - totalUsed;
        /// <summary>
        /// Gets the size of the largest free block of contiguous conventional memory.
        /// </summary>
        public int LargestFreeBlock => largestFreeBlock;
    }

    /// <summary>
    /// Contains information about an allocation in conventional memory.
    /// </summary>
    public readonly struct ProcessAllocation : IEquatable<ProcessAllocation>
    {
        /// <summary>
        /// Initializes a new <see cref="ProcessAllocation"/> struct.
        /// </summary>
        /// <param name="processName">Name of the process which owns the allocation.</param>
        /// <param name="size">Size of the allocation in bytes.</param>
        public ProcessAllocation(string processName, int size)
        {
            this.ProcessName = processName;
            this.AllocationSize = size;
        }

        public static bool operator ==(ProcessAllocation value1, ProcessAllocation value2) => value1.Equals(value2);
        public static bool operator !=(ProcessAllocation value1, ProcessAllocation value2) => !value1.Equals(value2);

        /// <summary>
        /// Gets the name of the process which owns the allocation.
        /// </summary>
        public string ProcessName { get; }
        /// <summary>
        /// Gets the size of the allocation in bytes.
        /// </summary>
        public int AllocationSize { get; }

        /// <summary>
        /// Returns a string representation of the ProcessAllocation struct.
        /// </summary>
        /// <returns>String representation of the ProcessAllocation struct.</returns>
        public override string ToString() => $"{this.ProcessName}: {this.AllocationSize}";
        /// <summary>
        /// Returns a code used for hashing algorithms.
        /// </summary>
        /// <returns>Hash code for the struct.</returns>
        public override int GetHashCode()
        {
            if (string.IsNullOrEmpty(this.ProcessName))
                return this.AllocationSize.GetHashCode();
            else
                return this.ProcessName.GetHashCode() ^ this.AllocationSize.GetHashCode();
        }
        /// <summary>
        /// Tests for equality with another object.
        /// </summary>
        /// <param name="obj">Other object to test.</param>
        /// <returns>True if objects are equal; otherwise false.</returns>
        public override bool Equals(object obj) => obj is ProcessAllocation a && this.Equals(a);
        /// <summary>
        /// Tests for equality with another ProcessAllocation struct.
        /// </summary>
        /// <param name="other">Other ProcessAllocation struct to test.</param>
        /// <returns>True if structs are equal; otherwise false.</returns>
        public bool Equals(ProcessAllocation other) => this.ProcessName == other.ProcessName && this.AllocationSize == other.AllocationSize;
    }
}
