namespace Aeon.Emulator.Dos;

/// <summary>
/// Manages conventional memory allocations.
/// </summary>
internal sealed class MemoryBlockList : IEnumerable<MemoryControlBlock>
{
    public const ushort FirstSegment = 0x0102;
    private const ushort EnvironmentVariableSegment = 0x011A;
    private const ushort EnvironmentBlockSize = 0x0B;
    private const ushort ConventionalMemorySize = 0xA000 - 1;

    private readonly PhysicalMemory memory;

    public MemoryBlockList(PhysicalMemory memory)
    {
        this.memory = memory;
        Initialize();
    }

    /// <summary>
    /// Gets the used blocks in the allocation list.
    /// </summary>
    public IEnumerable<MemoryControlBlock> UsedBlocks
    {
        get
        {
            return from mcb in this
                   where mcb.IsInUse
                   select mcb;
        }
    }
    /// <summary>
    /// Gets the free blocks in the allocation list.
    /// </summary>
    public IEnumerable<MemoryControlBlock> FreeBlocks
    {
        get
        {
            return from mcb in this
                   where !mcb.IsInUse
                   select mcb;
        }
    }
    /// <summary>
    /// Gets the size of the largest block of available paragraphs.
    /// </summary>
    public ushort LargestFreeBlock
    {
        get
        {
            // Find the largest block in the free list.
            var block = (from mcb in this.FreeBlocks
                         orderby mcb.Length
                         select mcb).LastOrDefault();

            if(block == null)
                return 0;
            else
                return block.Length;
        }
    }
    /// <summary>
    /// Gets the segment where a new PSP should be placed.
    /// </summary>
    public ushort NextPspSegment
    {
        get
        {
            var lastBlock = this.Last();
            return (ushort)(lastBlock.Segment + 1);
        }
    }
    /// <summary>
    /// Gets or sets the memory allocation strategy.
    /// </summary>
    public AllocationStrategy AllocationStrategy { get; set; }

    /// <summary>
    /// Attempts to allocate a block of memory of a specified size.
    /// </summary>
    /// <param name="requestedSize">Size of allocation in 16-byte paragraphs.</param>
    /// <param name="process">Process that is making the allocation.</param>
    /// <returns>Segment of allocated memory if successful; zero if there is insufficient memory available.</returns>
    public ushort Allocate(ushort requestedSize, DosProcess process)
    {
        // First check for free blocks in the list.
        var block = FindNextAllocationBlock(requestedSize);

        if(block != null)
        {
            // Special case for if the block is exactly the right size.
            if(block.Length == requestedSize)
                return MarkBlock(block, process);

            ushort address;

            // Otherwise split the block.
            if(this.AllocationStrategy == AllocationStrategy.LowLastFit || this.AllocationStrategy == AllocationStrategy.HighLowLastFit)
            {
                var newBlock = SplitFreeBlockAtEnd(block, requestedSize);
                address = MarkBlock(newBlock, process);
            }
            else
            {
                SplitFreeBlock(block, requestedSize);
                address = MarkBlock(block, process);
            }

            Consolidate();
            return address;
        }

        // Return 0 if there is not enough memory available.
        return 0;
    }
    /// <summary>
    /// Frees a previously allocated block of memory.
    /// </summary>
    /// <param name="segment">Segment of allocated memory to free.</param>
    /// <returns>True if memory was freed; false if segment was not valid.</returns>
    public bool Free(ushort segment)
    {
        var block = FindBlock(segment);
        if(block == null)
            return false;

        block.Free();
        block.Write(memory);
        Consolidate();

        return true;
    }
    /// <summary>
    /// Attempts to resize an existing allocation.
    /// </summary>
    /// <param name="segment">Segment of allocated memory to resize.</param>
    /// <param name="newSize">New size of allocation in 16-byte paragraphs.</param>
    /// <returns>True if resize was successful, false if there was not enough free space, or null if the segment was invalid.</returns>
    public bool? Reallocate(ushort segment, ushort newSize)
    {
        var block = FindBlock(segment);
        if(block == null)
            return null;

        // Make sure nothing happens if sizes are the same.
        if(newSize == block.Length)
            return true;

        // Handle last block as special case.
        if(block.IsLast)
        {
            if(newSize < block.Length)
            {
                block.Length = newSize;
                block.Write(memory);
                NormalizeTail();
                return true;
            }
            else
                return false;
        }

        // If the new size is smaller, we need a new empty block after it.
        if(newSize < block.Length)
        {
            var newBlock = new MemoryControlBlock((ushort)(block.Segment + newSize + 1), 0, string.Empty)
            {
                IsLast = false,
                Length = (ushort)(block.Length - newSize - 1)
            };

            newBlock.Write(memory);
            block.Length = newSize;
            block.Write(memory);
            Consolidate();
            return true;
        }

        // Otherwise the new size is larger, so we need to check for an empty block after it.
        var nextBlock = new MemoryControlBlock(memory, (ushort)(block.Segment + block.Length + 1));
        // Can't expand block - not enough space.
        if(nextBlock.IsInUse || nextBlock.Length + block.Length + 1 < newSize)
            return false;

        // If sizes add to match exactly what's needed, include next block's MCB.
        if(nextBlock.Length + block.Length + 1 == newSize)
        {
            block.Length = newSize;
            block.IsLast = nextBlock.IsLast;
            block.Write(memory);
            return true;
        }

        // Finally handle splitting of free block.
        nextBlock.Segment += (ushort)(newSize - block.Length);
        nextBlock.Length -= (ushort)(newSize - block.Length);
        nextBlock.Write(memory);
        block.Length = newSize;
        block.IsLast = false;
        block.Write(memory);
        Consolidate();
        return true;
    }
    /// <summary>
    /// Returns the maximum number of paragraphs a block may expand to.
    /// </summary>
    /// <param name="segment">Segment of block to check; this method does not check this value for validity.</param>
    /// <returns>Maximum number of paragraphs the block may expand to.</returns>
    public ushort GetMaximumBlockSize(ushort segment)
    {
        var block = new MemoryControlBlock(memory, (ushort)(segment - 1));
        ushort length = block.Length;
        var nextBlock = new MemoryControlBlock(memory, (ushort)(block.Segment + block.Length + 1));

        if(!nextBlock.IsInUse)
        {
            length += nextBlock.Length;
            length++;
        }

        return length;
    }
    /// <summary>
    /// Reassignes ownership of an allocation to a new process.
    /// </summary>
    /// <param name="segment">Segment of allocation to change ownership on.</param>
    /// <param name="newOwner">New owner process of the allocation.</param>
    public void Reassign(ushort segment, DosProcess newOwner)
    {
        var block = FindBlock(segment) ?? throw new ArgumentException("Invalid segment address.");
        block.PspSegment = newOwner.PrefixSegment;
        block.ImageName = newOwner.ImageName;
        block.Write(memory);
    }
    /// <summary>
    /// Gets an enumerator for the blocks in the list.
    /// </summary>
    /// <returns>Enumerator for the blocks in the list.</returns>
    public IEnumerator<MemoryControlBlock> GetEnumerator()
    {
        int count = 0;

        var mcb = new MemoryControlBlock(memory, FirstSegment);
        while(!mcb.IsLast)
        {
            yield return mcb;
            mcb = new MemoryControlBlock(memory, (ushort)(mcb.Segment + mcb.Length + 1u));
            count++;
            if(count > 1000)
                throw new InvalidOperationException();
        }

        yield return mcb;
    }

    /// <summary>
    /// Verifies that any free space at the end of the list is covered with a block.
    /// </summary>
    private void NormalizeTail()
    {
        ushort spaceRemaining = this.GetUnallocatedSpace();
        if(spaceRemaining > 0)
        {
            var lastBlock = this.Last();
            if(!lastBlock.IsInUse)
            {
                lastBlock.Length += spaceRemaining;
                lastBlock.Write(memory);
            }
            else
            {
                var newBlock = new MemoryControlBlock((ushort)(lastBlock.Segment + lastBlock.Length + 1), 0, string.Empty)
                {
                    Length = (ushort)(spaceRemaining - 1),
                    IsLast = true
                };

                newBlock.Write(memory);
                lastBlock.IsLast = false;
                lastBlock.Write(memory);
            }
        }
    }
    /// <summary>
    /// Replaces contiguous empty blocks with one empty block.
    /// </summary>
    private void Consolidate()
    {
        // Keep trying to consolidate free blocks until nothing is left to consolidate.
        bool done;
        do
        {
            done = true;
            var freeList = new List<MemoryControlBlock>(this.FreeBlocks);
            foreach(var block in freeList)
            {
                if(ConsolidateFreeBlocks(block))
                {
                    block.Write(memory);
                    done = false;
                    break;
                }
            }
        }
        while(!done);

        NormalizeTail();
    }
    /// <summary>
    /// Consolidates a series of contiguous free memory blocks.
    /// </summary>
    /// <param name="initialBlock">First block of free run to consolidate into.</param>
    /// <returns>True if blocks were consolidated; false if no change was made.</returns>
    private bool ConsolidateFreeBlocks(MemoryControlBlock initialBlock)
    {
        if(initialBlock.IsInUse || initialBlock.IsLast)
            return false;

        var nextBlock = new MemoryControlBlock(memory, (ushort)(initialBlock.Segment + initialBlock.Length + 1));
        if(nextBlock.IsInUse)
            return false;

        while(!nextBlock.IsInUse)
        {
            // Add the size of the next block plus its MCB.
            initialBlock.Length += nextBlock.Length;
            initialBlock.Length++;

            initialBlock.IsLast = nextBlock.IsLast;
            if(nextBlock.IsLast)
                return true;

            nextBlock = new MemoryControlBlock(memory, (ushort)(nextBlock.Segment + nextBlock.Length + 1));
        }
        
        return true;
    }
    /// <summary>
    /// Marks a newly-allocated block as belonging to a process.
    /// </summary>
    /// <param name="block">New block to mark.</param>
    /// <param name="process">Process which allocated the block.</param>
    /// <returns>Address of allocated block.</returns>
    private ushort MarkBlock(MemoryControlBlock block, DosProcess process)
    {
        block.PspSegment = process.PrefixSegment;
        block.ImageName = process.ImageName;
        block.Write(memory);
        return (ushort)(block.Segment + 1);
    }
    /// <summary>
    /// Writes the initial blocks to memory.
    /// </summary>
    private void Initialize()
    {
        var firstBlock = new MemoryControlBlock(FirstSegment, 0x0008, string.Empty)
        {
            Length = 1,
            IsLast = false
        };

        firstBlock.Write(memory);

        ushort freeSize = unchecked(EnvironmentVariableSegment - FirstSegment - 3);
        var freeBlock = new MemoryControlBlock(FirstSegment + 2, 0, string.Empty)
        {
            Length = freeSize,
            IsLast = false
        };

        freeBlock.Write(memory);

        var variableBlock = new MemoryControlBlock(EnvironmentVariableSegment, 0x0128, string.Empty) { Length = EnvironmentBlockSize };
        variableBlock.Write(memory);

        NormalizeTail();
    }
    /// <summary>
    /// Gets the number of paragraphs remaining after the last allocation.
    /// </summary>
    /// <returns>Number of unallocated paragraphs.</returns>
    private ushort GetUnallocatedSpace()
    {
        var block = this.Last();
        int nextSegment = block.Segment + block.Length + 1;
        if(nextSegment < ConventionalMemorySize)
            return (ushort)(ConventionalMemorySize - nextSegment);
        else
            return 0;
    }
    /// <summary>
    /// Splits a free block of memory.
    /// </summary>
    /// <param name="block">Block to split.</param>
    /// <param name="newSize">New size of block.</param>
    private void SplitFreeBlock(MemoryControlBlock block, ushort newSize)
    {
        if(block.Length > newSize)
        {
            var newBlock = new MemoryControlBlock((ushort)(block.Segment + newSize + 1), 0, string.Empty) { Length = (ushort)(block.Length - newSize - 1) };
            block.Length = newSize;
            newBlock.IsLast = block.IsLast;
            block.IsLast = false;
            newBlock.Write(memory);
        }
        else
            throw new InvalidOperationException();
    }
    /// <summary>
    /// Splits a free block of memory from the end.
    /// </summary>
    /// <param name="block">Block to split.</param>
    /// <param name="endSize">Size of new block at the end of the provided block.</param>
    private MemoryControlBlock SplitFreeBlockAtEnd(MemoryControlBlock block, ushort endSize)
    {
        if(block.Length > endSize)
        {
            var newBlock = new MemoryControlBlock((ushort)(block.Segment + block.Length - endSize), 0, string.Empty)
            {
                Length = endSize,
                IsLast = block.IsLast
            };

            block.IsLast = false;
            block.Length = (ushort)(block.Length - endSize - 1);
            block.Write(memory);
            newBlock.Write(memory);
            return newBlock;
        }
        else
            throw new InvalidOperationException();
    }
    /// <summary>
    /// Searches for a block in the used allocation list.
    /// </summary>
    /// <param name="userSegment">Segment of the block to search for.</param>
    /// <returns>Block instance if found; otherwise null.</returns>
    private MemoryControlBlock? FindBlock(ushort userSegment)
    {
        return (from mcb in this.UsedBlocks
                where mcb.Segment + 1 == userSegment
                select mcb).FirstOrDefault();
    }
    /// <summary>
    /// Returns an available memory control block according to the current allocation strategy.
    /// </summary>
    /// <param name="size">Minimum size of the requested block.</param>
    /// <returns>Available memory control block if one was found; otherwise null.</returns>
    private MemoryControlBlock? FindNextAllocationBlock(ushort size)
    {
        return this.AllocationStrategy switch
        {
            AllocationStrategy.LowFirstFit or AllocationStrategy.HighLowFirstFit => (from mcb in this.FreeBlocks
                                                                                     where mcb.Length >= size
                                                                                     select mcb).FirstOrDefault(),
            AllocationStrategy.LowLastFit or AllocationStrategy.HighLowLastFit => (from mcb in this.FreeBlocks
                                                                                   where mcb.Length >= size
                                                                                   select mcb).LastOrDefault(),
            AllocationStrategy.LowBestFit or AllocationStrategy.HighLowBestFit => (from mcb in this.FreeBlocks
                                                                                   where mcb.Length >= size
                                                                                   orderby mcb.Length, mcb.Segment
                                                                                   select mcb).FirstOrDefault(),
            _ => null
        };
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}
