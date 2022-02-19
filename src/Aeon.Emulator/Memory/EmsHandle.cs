using System;
using System.Collections.Generic;
using System.Linq;

namespace Aeon.Emulator.Memory
{
    /// <summary>
    /// Represents a handle for allocated EMS memory.
    /// </summary>
    internal sealed class EmsHandle
    {
        private readonly List<ushort> pages;
        private readonly int[] savedPageMap = new int[] { -1, -1, -1, -1 };
        private static readonly string nullHandleName = new((char)0, 8);

        public EmsHandle()
        {
            pages = new List<ushort>();
        }
        public EmsHandle(IEnumerable<ushort> pages)
        {
            this.pages = pages.ToList();
        }

        /// <summary>
        /// Gets the number of pages currently allocated to the handle.
        /// </summary>
        public int PagesAllocated => this.pages.Count;
        /// <summary>
        /// Gets the logical pages allocated to the handle.
        /// </summary>
        public List<ushort> LogicalPages => this.pages;
        /// <summary>
        /// Gets or sets the handle name.
        /// </summary>
        public string Name { get; set; } = nullHandleName;
        /// <summary>
        /// Gets or sets the saved page map for the handle.
        /// </summary>
        public Span<int> SavedPageMap => this.savedPageMap;

        /// <summary>
        /// Returns a string containing the handle name.
        /// </summary>
        /// <returns>String containing the handle name.</returns>
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(this.Name) && this.Name != nullHandleName)
                return this.Name;
            else
                return "Untitled";
        }
    }
}
