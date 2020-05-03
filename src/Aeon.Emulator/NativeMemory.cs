using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Aeon.Emulator
{
    /// <summary>
    /// Manages an expandable block of virtual memory.
    /// </summary>
    internal sealed class NativeMemory : IDisposable
    {
        private readonly IntPtr blockStart;
        private readonly int bytesReserved;
        private int bytesCommitted;
        private bool disposed;

        /// <summary>
        /// Initializes a new instance of the NativeMemory class.
        /// </summary>
        /// <param name="size">Number of bytes to reserve and commit.</param>
        public NativeMemory(int size)
        {
            if (size <= 0)
                throw new ArgumentOutOfRangeException(nameof(size));

            var ptr = SafeNativeMethods.VirtualAlloc(IntPtr.Zero, new IntPtr(size), SafeNativeMethods.MEM_RESERVE | SafeNativeMethods.MEM_COMMIT, 0x04);
            if (ptr == IntPtr.Zero)
                throw new Win32Exception();

            GC.AddMemoryPressure(size);

            this.blockStart = ptr;
            this.bytesReserved = size;
            this.bytesCommitted = size;
        }
        /// <summary>
        /// Initializes a new instance of the NativeMemory class.
        /// </summary>
        /// <param name="size">Number of bytes to reserve.</param>
        /// <param name="committed">Number of reserved bytes to commit.</param>
        public NativeMemory(int size, int committed)
        {
            if (size <= 0)
                throw new ArgumentOutOfRangeException(nameof(size));
            if (committed < 0 || committed > size)
                throw new ArgumentOutOfRangeException(nameof(committed));

            var ptr = SafeNativeMethods.VirtualAlloc(IntPtr.Zero, new IntPtr(size), SafeNativeMethods.MEM_RESERVE, 0x04);
            if (ptr == IntPtr.Zero)
                throw new Win32Exception();

            this.blockStart = ptr;
            this.bytesReserved = size;

            Commit(committed);
        }
        ~NativeMemory()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Gets a pointer to the block of virtual memory.
        /// </summary>
        public IntPtr Pointer => this.blockStart;
        /// <summary>
        /// Gets the number of bytes reserved.
        /// </summary>
        public int ReservedBytes => this.bytesReserved;
        /// <summary>
        /// Gets the number of bytes committed.
        /// </summary>
        public int CommittedBytes => this.bytesCommitted;

        /// <summary>
        /// Commits reserved memory.
        /// </summary>
        /// <param name="amount">Number of bytes to commit.</param>
        public void Commit(int amount)
        {
            if (amount < 0)
                throw new ArgumentOutOfRangeException(nameof(amount));
            if (amount == 0)
                return;
            if (amount > this.bytesReserved - this.bytesCommitted)
                throw new ArgumentOutOfRangeException(nameof(amount));

            var startPtr = new IntPtr(this.blockStart.ToInt64() + this.bytesCommitted);
            var ptr = SafeNativeMethods.VirtualAlloc(startPtr, new IntPtr(amount), SafeNativeMethods.MEM_COMMIT, 0x04);
            if (ptr == IntPtr.Zero)
                throw new Win32Exception();

            GC.AddMemoryPressure(amount);

            this.bytesCommitted += amount;
        }
        /// <summary>
        /// Commits all remaining reserved memory.
        /// </summary>
        public void Commit()
        {
            Commit(this.bytesReserved - this.bytesCommitted);
        }
        /// <summary>
        /// Fills in all committed memory with zeros.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public void Clear()
        {
            unsafe
            {
                var span = new Span<byte>(this.blockStart.ToPointer(), this.bytesCommitted);
                span.Clear();
            }
        }
        /// <summary>
        /// Releases resources used by the instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Frees any reserved or committed memory.
        /// </summary>
        /// <param name="disposing">Value indicating whether method was invoked from Dispose.</param>
        private void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                this.disposed = true;
                if (this.blockStart != IntPtr.Zero)
                    SafeNativeMethods.VirtualFree(this.blockStart, IntPtr.Zero, SafeNativeMethods.MEM_RELEASE);

                if (this.bytesCommitted > 0)
                    GC.RemoveMemoryPressure(this.bytesCommitted);
            }
        }

        private static class SafeNativeMethods
        {
            [DllImport("kernel32.dll", CallingConvention = CallingConvention.Winapi, SetLastError = true)]
            public static extern IntPtr VirtualAlloc(IntPtr lpAddress, IntPtr dwSize, uint flAllocationType, uint flProtect);
            [DllImport("kernel32.dll", CallingConvention = CallingConvention.Winapi, SetLastError = true)]
            public static extern uint VirtualFree(IntPtr lpAddress, IntPtr dwSize, uint dwFreeType);

            public const uint MEM_COMMIT = 0x1000;
            public const uint MEM_RESERVE = 0x2000;
            public const uint MEM_RESET = 0x80000;
            public const uint MEM_DECOMMIT = 0x4000;
            public const uint MEM_RELEASE = 0x8000;
        }
    }
}
