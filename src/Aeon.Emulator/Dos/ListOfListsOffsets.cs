namespace Aeon.Emulator.Dos
{
    /// <summary>
    /// Offsets of relevant data inside the DOS list of lists.
    /// </summary>
    internal static class ListOfListsOffsets
    {
        /// <summary>
        /// Size: word
        /// </summary>
        public const int SegmentOfFirstMCB = -2;
        /// <summary>
        /// Size: dword (pointer)
        /// </summary>
        public const int FirstDriveParamaterBlock = 0;
        /// <summary>
        /// Size: dword (pointer)
        /// </summary>
        public const int FirstSystemFileTable = 4;
        /// <summary>
        /// Size: word
        /// </summary>
        public const int BytesPerSectorOfAnyBlockDevice = 0x10;
        /// <summary>
        /// Size: dword (pointer)
        /// </summary>
        public const int DiskBufferInfoRecord = 0x12;
        /// <summary>
        /// Size: dword (pointer)
        /// </summary>
        public const int CurrentDirectoryStructures = 0x16;
        /// <summary>
        /// Size: dword (pointer)
        /// </summary>
        public const int RoutineForResidentIFSUtilityFuncctions = 0x37;
        /// <summary>
        /// Size: dword (pointer)
        /// </summary>
        public const int ChainOfIFSDrivers = 0x3B;
        /// <summary>
        /// Size: word
        /// </summary>
        public const int NumberOfBuffers = 0x3F;
        /// <summary>
        /// Size: word
        /// </summary>
        public const int NumberOfLookaheadBuffers = 0x41;
        /// <summary>
        /// Size: byte (1=A:)
        /// </summary>
        public const int BootDrive = 0x43;
        /// <summary>
        /// Size: word (kilobytes)
        /// </summary>
        public const int ExtendedMemorySize = 0x45;
    }
}
