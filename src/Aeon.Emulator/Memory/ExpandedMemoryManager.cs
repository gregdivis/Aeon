namespace Aeon.Emulator.Memory;

/// <summary>
/// Provides DOS applications with EMS memory.
/// </summary>
internal sealed class ExpandedMemoryManager(VirtualMachine vm) : IInterruptHandler
{
    /// <summary>
    /// Size of each EMS page in bytes.
    /// </summary>
    public const int PageSize = 16384;
    /// <summary>
    /// Maximum number of mappable pages.
    /// </summary>
    public const int MaximumPhysicalPages = 4;
    /// <summary>
    /// Maximum number of logical pages.
    /// </summary>
    public const int MaximumLogicalPages = 1024;

    private const ushort PageFrameSegment = 0xE000;
    private const int FirstHandle = 1;
    private const int LastHandle = 254;
    private const int SegmentsPerPage = PageSize / 16;

    private readonly VirtualMachine vm = vm;
    private readonly SortedList<int, EmsHandle> handles = [];
    private readonly byte[]?[] mappedPages = new byte[MaximumPhysicalPages][];

    /// <summary>
    /// Gets the total number of allocated EMS pages.
    /// </summary>
    public int AllocatedPages => this.handles.Values.Sum(p => p.PagesAllocated);

    IEnumerable<InterruptHandlerInfo> IInterruptHandler.HandledInterrupts => [0x67];

    void IInterruptHandler.HandleInterrupt(int interrupt)
    {
        switch (vm.Processor.AH)
        {
            case EmsFunctions.GetStatus:
                // Return good status in AH.
                vm.Processor.AH = 0;
                break;

            case EmsFunctions.GetPageFrameAddress:
                // Return page frame segment in BX.
                vm.Processor.BX = unchecked((short)PageFrameSegment);
                // Set good status.
                vm.Processor.AH = 0;
                break;

            case EmsFunctions.GetUnallocatedPageCount:
                // Return number of pages available in BX.
                vm.Processor.BX = (short)(MaximumLogicalPages - this.AllocatedPages);
                // Return total number of pages in DX.
                vm.Processor.DX = (short)MaximumLogicalPages;
                // Set good status.
                vm.Processor.AH = 0;
                break;

            case EmsFunctions.AllocatePages:
                AllocatePages();
                break;

            case EmsFunctions.ReallocatePages:
                ReallocatePages();
                break;

            case EmsFunctions.MapUnmapHandlePage:
                MapUnmapHandlePage();
                break;

            case EmsFunctions.DeallocatePages:
                DeallocatePages();
                break;

            case EmsFunctions.GetVersion:
                // Return EMS version 4.0.
                vm.Processor.AL = 0x40;
                // Return good status.
                vm.Processor.AH = 0;
                break;

            case EmsFunctions.GetHandleCount:
                // Return the number of EMM handles (plus 1 for the OS handle).
                vm.Processor.BX = (short)(handles.Count + 1);
                // Return good status.
                vm.Processor.AH = 0;
                break;

            case EmsFunctions.GetHandlePages:
                GetHandlePages();
                break;

            case EmsFunctions.SavePageMap:
                SavePageMap();
                break;

            case EmsFunctions.RestorePageMap:
                RestorePageMap();
                break;

            case EmsFunctions.AdvancedMap:
                switch (vm.Processor.AL)
                {
                    case EmsFunctions.AdvancedMap_MapUnmapPages:
                        MapUnmapMultiplePages();
                        break;

                    default:
                        throw new InvalidOperationException();
                }
                break;

            case EmsFunctions.HandleName:
                switch (vm.Processor.AL)
                {
                    case EmsFunctions.HandleName_Get:
                        GetHandleName();
                        break;

                    case EmsFunctions.HandleName_Set:
                        SetHandleName();
                        break;

                    default:
                        throw new InvalidOperationException();
                }
                break;

            case EmsFunctions.GetHardwareInformation:
                switch (vm.Processor.AL)
                {
                    case EmsFunctions.GetHardwareInformation_UnallocatedRawPages:
                        // Return number of pages available in BX.
                        vm.Processor.BX = (short)(MaximumLogicalPages - this.AllocatedPages);
                        // Return total number of pages in DX.
                        vm.Processor.DX = (short)MaximumLogicalPages;
                        // Set good status.
                        vm.Processor.AH = 0;
                        break;

                    default:
                        throw new InvalidOperationException();
                }
                break;

            case EmsFunctions.MoveExchange:
                switch (vm.Processor.AL)
                {
                    case EmsFunctions.MoveExchange_Move:
                        Move();
                        break;

                    default:
                        throw new NotImplementedException(string.Format("EMM function 57{0:X2}h not implemented.", vm.Processor.AL));
                }
                break;

            default:
                System.Diagnostics.Debug.WriteLine(string.Format("EMM function {0:X2}h not implemented.", vm.Processor.AH));
                vm.Processor.AH = 0x84;
                break;
        }
    }
    void IVirtualDevice.DeviceRegistered(VirtualMachine vm)
    {
        vm.PhysicalMemory.SetString(0xF100, 0x000A, "EMMXXXX0");
        vm.PhysicalMemory.Reserve(PageFrameSegment, PageSize * MaximumPhysicalPages);
    }

    /// <summary>
    /// Allocates pages for a new handle.
    /// </summary>
    private void AllocatePages()
    {
        uint pagesRequested = (ushort)vm.Processor.BX;
        if (pagesRequested == 0)
        {
            // Return "attempted to allocate zero pages" code.
            vm.Processor.AH = 0x89;
            return;
        }

        if (pagesRequested <= MaximumLogicalPages - this.AllocatedPages)
        {
            // Some programs like to use one more page than they ask for.
            // What a bunch of rubbish.
            int handle = CreateHandle((int)pagesRequested + 1);
            if (handle != 0)
            {
                // Return handle in DX.
                vm.Processor.DX = (short)handle;
                // Return good status.
                vm.Processor.AH = 0;
            }
            else
            {
                // Return "all handles in use" code.
                vm.Processor.AH = 0x85;
            }
        }
        else
        {
            // Return "not enough available pages" code.
            vm.Processor.AH = 0x87;
        }
    }
    /// <summary>
    /// Reallocates pages for a handle.
    /// </summary>
    private void ReallocatePages()
    {
        int pagesRequested = vm.Processor.BX;

        if (pagesRequested < MaximumLogicalPages)
        {
            int handle = vm.Processor.DX;
            if (handles.TryGetValue(handle, out var emsHandle))
            {
                emsHandle.Reallocate(pagesRequested);

                // Return good status.
                vm.Processor.AH = 0;
            }
            else
            {
                // Return "couldn't find specified handle" code.
                vm.Processor.AH = 0x83;
            }
        }
        else
        {
            // Return "not enough available pages" code.
            vm.Processor.AH = 0x87;
        }
    }
    /// <summary>
    /// Attempts to create a new EMS handle.
    /// </summary>
    /// <param name="pagesRequested">Number of pages to allocate to the new handle.</param>
    /// <returns>New EMS handle if created successfully; otherwise null.</returns>
    private int CreateHandle(int pagesRequested)
    {
        for (int i = FirstHandle; i <= LastHandle; i++)
        {
            if (!handles.ContainsKey(i))
            {
                EmsHandle handle = new EmsHandle(pagesRequested);
                handles.Add(i, handle);
                return i;
            }
        }
        return 0;
    }
    /// <summary>
    /// Deallocates a handle and all of its pages.
    /// </summary>
    private void DeallocatePages()
    {
        int handle = vm.Processor.DX;
        if (handles.Remove(handle))
        {
            // Return good status.
            vm.Processor.AH = 0;
        }
        else
        {
            // Return "couldn't find specified handle" code.
            vm.Processor.AH = 0x83;
        }
    }
    /// <summary>
    /// Maps or unmaps a physical page.
    /// </summary>
    private void MapUnmapHandlePage()
    {
        int physicalPage = vm.Processor.AL;
        if (physicalPage < 0 || physicalPage >= MaximumPhysicalPages)
        {
            // Return "physical page out of range" code.
            vm.Processor.AH = 0x8B;
            return;
        }

        int handleIndex = vm.Processor.DX;
        if (!handles.TryGetValue(handleIndex, out var handle))
        {
            // Return "couldn't find specified handle" code.
            vm.Processor.AH = 0x83;
            return;
        }

        int logicalPageIndex = (ushort)vm.Processor.BX;

        if (logicalPageIndex != 0xFFFF)
        {
            var logicalPage = handle.GetLogicalPage(logicalPageIndex);
            if (logicalPage == null)
            {
                // Return "logical page out of range" code.
                vm.Processor.AH = 0x8A;
                return;
            }

            MapPage(logicalPage, physicalPage);
        }
        else
        {
            UnmapPage(physicalPage);
        }

        // Return good status.
        vm.Processor.AH = 0;
    }
    /// <summary>
    /// Copies data from a logical page to a physical page.
    /// </summary>
    /// <param name="logicalPage">Logical page to copy from.</param>
    /// <param name="physicalPageIndex">Index of physical page to copy to.</param>
    private void MapPage(byte[] logicalPage, int physicalPageIndex)
    {
        // If the requested logical page is already mapped, it needs to get unmapped first.
        UnmapLogicalPage(logicalPage);

        // If a page is already mapped, make sure it gets unmapped first.
        UnmapPage(physicalPageIndex);

        ushort segment = (ushort)(PageFrameSegment + SegmentsPerPage * physicalPageIndex);
        IntPtr physicalPtr = vm.PhysicalMemory.GetPointer(segment, 0);
        System.Runtime.InteropServices.Marshal.Copy(logicalPage, 0, physicalPtr, PageSize);
        mappedPages[physicalPageIndex] = logicalPage;
    }
    /// <summary>
    /// Copies data from a physical page to a logical page.
    /// </summary>
    /// <param name="physicalPageIndex">Physical page to copy from.</param>
    private void UnmapPage(int physicalPageIndex)
    {
        var currentPage = mappedPages[physicalPageIndex];
        if (currentPage != null)
        {
            ushort segment = (ushort)(PageFrameSegment + SegmentsPerPage * physicalPageIndex);
            IntPtr physicalPtr = vm.PhysicalMemory.GetPointer(segment, 0);
            System.Runtime.InteropServices.Marshal.Copy(physicalPtr, currentPage, 0, PageSize);
            mappedPages[physicalPageIndex] = null;
        }
    }
    /// <summary>
    /// Unmaps a specific logical page if it is currently mapped.
    /// </summary>
    /// <param name="logicalPage">Logical page to unmap.</param>
    private void UnmapLogicalPage(byte[] logicalPage)
    {
        for (int i = 0; i < this.mappedPages.Length; i++)
        {
            if (this.mappedPages[i] == logicalPage)
                UnmapPage(i);
        }
    }
    /// <summary>
    /// Gets the number of pages allocated to a handle.
    /// </summary>
    private void GetHandlePages()
    {
        int handleIndex = vm.Processor.DX;
        if (handles.TryGetValue(handleIndex, out var handle))
        {
            // Return the number of pages allocated in BX.
            vm.Processor.BX = (short)handle.PagesAllocated;
            // Return good status.
            vm.Processor.AH = 0;
        }
        else
        {
            // Return "couldn't find specified handle" code.
            vm.Processor.AH = 0x83;
        }
    }
    /// <summary>
    /// Gets the name of a handle.
    /// </summary>
    private void GetHandleName()
    {
        int handleIndex = vm.Processor.DX;
        if (handles.TryGetValue(handleIndex, out var handle))
        {
            // Write the handle name to ES:DI.
            vm.PhysicalMemory.SetString(vm.Processor.ES, vm.Processor.DI, handle.Name);
            // Return good status.
            vm.Processor.AH = 0;
        }
        else
        {
            // Return "couldn't find specified handle" code.
            vm.Processor.AH = 0x83;
        }
    }
    /// <summary>
    /// Set the name of a handle.
    /// </summary>
    private void SetHandleName()
    {
        int handleIndex = vm.Processor.DX;
        if (handles.TryGetValue(handleIndex, out var handle))
        {
            // Read the handle name from DS:SI.
            handle.Name = vm.PhysicalMemory.GetString(vm.Processor.DS, vm.Processor.SI, 8);
            // Return good status.
            vm.Processor.AH = 0;
        }
        else
        {
            // Return "couldn't find specified handle" code.
            vm.Processor.AH = 0x83;
        }
    }
    /// <summary>
    /// Maps or unmaps multiple pages.
    /// </summary>
    private void MapUnmapMultiplePages()
    {
        int handleIndex = vm.Processor.DX;
        if (!handles.TryGetValue(handleIndex, out var handle))
        {
            // Return "couldn't find specified handle" code.
            vm.Processor.AH = 0x83;
            return;
        }

        int pageCount = vm.Processor.CX;
        if (pageCount < 0 || pageCount > MaximumPhysicalPages)
        {
            // Return "physical page count out of range" code.
            vm.Processor.AH = 0x8B;
            return;
        }

        uint arraySegment = vm.Processor.DS;
        uint arrayOffset = vm.Processor.SI;
        for (int i = 0; i < pageCount; i++)
        {
            ushort logicalPageIndex = vm.PhysicalMemory.GetUInt16(arraySegment, arrayOffset);
            ushort physicalPageIndex = vm.PhysicalMemory.GetUInt16(arraySegment, arrayOffset + 2u);

            if (physicalPageIndex < 0 || physicalPageIndex >= MaximumPhysicalPages)
            {
                // Return "physical page out of range" code.
                vm.Processor.AH = 0x8B;
                return;
            }

            if (logicalPageIndex != 0xFFFF)
            {
                var logicalPage = handle.GetLogicalPage(logicalPageIndex);
                if (logicalPage == null)
                {
                    // Return "logical page out of range" code.
                    vm.Processor.AH = 0x8A;
                    return;
                }

                MapPage(logicalPage, physicalPageIndex);
            }
            else
            {
                UnmapPage(physicalPageIndex);
            }

            arrayOffset += 4u;
        }

        // Return good status.
        vm.Processor.AH = 0;
    }
    /// <summary>
    /// Saves the current state of page map registers for a handle.
    /// </summary>
    private void SavePageMap()
    {
        int handleIndex = vm.Processor.DX;
        if (!handles.TryGetValue(handleIndex, out var handle))
        {
            // Return "couldn't find specified handle" code.
            vm.Processor.AH = 0x83;
            return;
        }

        handle.SavedPageMap = (byte[][])mappedPages.Clone();

        // Return good status.
        vm.Processor.AH = 0;
    }
    /// <summary>
    /// Restores the state of page map registers for a handle.
    /// </summary>
    private void RestorePageMap()
    {
        int handleIndex = vm.Processor.DX;
        if (!handles.TryGetValue(handleIndex, out var handle))
        {
            // Return "couldn't find specified handle" code.
            vm.Processor.AH = 0x83;
            return;
        }

        if (handle.SavedPageMap != null)
        {
            for (int i = 0; i < MaximumPhysicalPages; i++)
            {
                if (handle.SavedPageMap[i] != null && handle.SavedPageMap[i] != mappedPages[i])
                    MapPage(handle.SavedPageMap[i]!, i);
            }
        }

        // Return good status.
        vm.Processor.AH = 0;
    }
    /// <summary>
    /// Copies a block of memory.
    /// </summary>
    private void Move()
    {
        int length = (int)vm.PhysicalMemory.GetUInt32(vm.Processor.DS, vm.Processor.SI);

        byte sourceType = vm.PhysicalMemory.GetByte(vm.Processor.DS, vm.Processor.SI + 4u);
        int sourceHandleIndex = vm.PhysicalMemory.GetUInt16(vm.Processor.DS, vm.Processor.SI + 5u);
        int sourceOffset = vm.PhysicalMemory.GetUInt16(vm.Processor.DS, vm.Processor.SI + 7u);
        int sourcePage = vm.PhysicalMemory.GetUInt16(vm.Processor.DS, vm.Processor.SI + 9u);

        byte destType = vm.PhysicalMemory.GetByte(vm.Processor.DS, vm.Processor.SI + 11u);
        int destHandleIndex = vm.PhysicalMemory.GetUInt16(vm.Processor.DS, vm.Processor.SI + 12u);
        int destOffset = vm.PhysicalMemory.GetUInt16(vm.Processor.DS, vm.Processor.SI + 14u);
        int destPage = vm.PhysicalMemory.GetUInt16(vm.Processor.DS, vm.Processor.SI + 16u);

        SyncToEms();

        if (sourceType == 0 && destType == 0)
        {
            vm.Processor.AH = EmsCopier.ConvToConv(vm.PhysicalMemory, (uint)((sourcePage << 4) + sourceOffset), (uint)((destPage << 4) + destOffset), length);
        }
        else if (sourceType != 0 && destType == 0)
        {
            if (!handles.TryGetValue(sourceHandleIndex, out var sourceHandle))
            {
                // Return "couldn't find specified handle" code.
                vm.Processor.AH = 0x83;
                return;
            }

            vm.Processor.AH = EmsCopier.EmsToConv(sourceHandle, sourcePage, sourceOffset, vm.PhysicalMemory, (uint)((destPage << 4) + destOffset), length);
        }
        else if (sourceType == 0 && destType != 0)
        {
            if (!handles.TryGetValue(destHandleIndex, out var destHandle))
            {
                // Return "couldn't find specified handle" code.
                vm.Processor.AH = 0x83;
                return;
            }

            vm.Processor.AH = EmsCopier.ConvToEms(vm.PhysicalMemory, (uint)((sourcePage << 4) + sourceOffset), destHandle, destPage, destOffset, length);
        }
        else
        {
            if (!handles.TryGetValue(sourceHandleIndex, out var sourceHandle) || !handles.TryGetValue(destHandleIndex, out var destHandle))
            {
                // Return "couldn't find specified handle" code.
                vm.Processor.AH = 0x83;
                return;
            }

            vm.Processor.AH = EmsCopier.EmsToEms(sourceHandle, sourcePage, sourceOffset, destHandle, destPage, destOffset, length);
        }

        SyncFromEms();
    }
    /// <summary>
    /// Copies data from mapped conventional memory to EMS pages.
    /// </summary>
    private void SyncToEms()
    {
        for (int i = 0; i < MaximumPhysicalPages; i++)
        {
            if (mappedPages[i] != null)
            {
                ushort segment = (ushort)(PageFrameSegment + SegmentsPerPage * i);
                IntPtr physicalPtr = vm.PhysicalMemory.GetPointer(segment, 0);
                System.Runtime.InteropServices.Marshal.Copy(physicalPtr, mappedPages[i]!, 0, PageSize);
            }
        }
    }
    /// <summary>
    /// Copies data from EMS pages to mapped conventional memory.
    /// </summary>
    private void SyncFromEms()
    {
        for (int i = 0; i < MaximumPhysicalPages; i++)
        {
            if (mappedPages[i] != null)
            {
                ushort segment = (ushort)(PageFrameSegment + SegmentsPerPage * i);
                IntPtr physicalPtr = vm.PhysicalMemory.GetPointer(segment, 0);
                System.Runtime.InteropServices.Marshal.Copy(mappedPages[i]!, 0, physicalPtr, PageSize);
            }
        }
    }
}
