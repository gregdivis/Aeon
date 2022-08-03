namespace Aeon.Emulator.Dos
{
    internal static class Functions
    {
        public const byte GetVersionNumber = 0x30;
        public const byte GlobalCodePageTable = 0x66;
        public const byte SetHandleCount = 0x67;
        public const byte CloseFile = 0x3E;
        public const byte TerminateProgram = 0x4C;
        public const byte TerminateAndStayResident = 0x31;
        public const byte OpenFile = 0x3D;
        public const byte CreateFile = 0x3C;
        public const byte WriteToFile = 0x40;
        public const byte Ioctl = 0x44;
        public const byte WriteToStdOut = 0x09;
        public const byte AllocateMemory = 0x48;
        public const byte DeallocateMemory = 0x49;
        public const byte ModifyMemoryAllocation = 0x4A;
        public const byte GetDefaultDrive = 0x19;
        public const byte ParseToFcb = 0x29;
        public const byte Seek = 0x42;
        public const byte ReadFromFile = 0x3F;
        public const byte ConsoleIO = 0x06;
        public const byte ConsoleInput = 0x07;
        public const byte ConsoleInputNoEcho = 0x08;
        public const byte CheckStandardInput = 0x0B;
        public const byte GetSystemTime = 0x2C;
        public const byte SetSystemTime = 0x2D;
        public const byte GetSystemDate = 0x2A;
        public const byte SetSystemDate = 0x2B;
        public const byte ConsoleOutput = 0x02;
        public const byte ConsoleReadLine = 0x0A;
        public const byte GetInterruptVector = 0x35;
        public const byte SetInterruptVector = 0x25;
        public const byte FileAttributes = 0x43;
        public const byte FileAttributes_GetFileAttributes = 0x00;
        public const byte SetDiskTransferAreaAddress = 0x1A;
        public const byte GetDiskTransferAreaAddress = 0x2F;
        public const byte FindFirstFile = 0x4E;
        public const byte FindNextFile = 0x4F;
        public const byte ResetDisk = 0x0D;
        public const byte GetCurrentDirectory = 0x47;
        public const byte GetFreeSpace = 0x36;
        public const byte SelectDefaultDrive = 0x0E;
        public const byte DuplicateFileHandle = 0x45;
        public const byte GetCurrentPsp = 0x62;
        public const byte ChangeDirectory = 0x3B;
        public const byte DeleteFile = 0x41;
        public const byte GetListOfLists = 0x52;
        public const byte ExecuteProgram = 0x4B;
        public const byte ExecuteProgram_LoadAndRun = 0;
        public const byte ExecuteProgram_LoadOverlay = 3;
        public const byte GetInDosPointer = 0x34;
        public const byte GetReturnCode = 0x4D;
        public const byte GetCountryInfo = 0x38;
        public const byte SetCurrentProcessId = 0x50;
        public const byte GetCurrentProcessId = 0x51;
        public const byte CreateTemporaryFile = 0x5A;
        public const byte CreateDirectory = 0x39;
        public const byte GetDriveInfo = 0x1C;

        public const byte AllocationStrategy = 0x58;
        public const byte AllocationStrategy_Get = 0x00;
        public const byte AllocationStrategy_Set = 0x01;
        public const byte AllocationStrategy_GetUMB = 0x02;
        public const byte AllocationStrategy_SetUMB = 0x03;

        public const byte Function33 = 0x33;
        public const byte Function33_GetExtendedBreakChecking = 0x00;
        public const byte Function33_SetExtendedBreakChecking = 0x01;
        public const byte Function33_GetBootDrive = 0x05;
        public const byte Function33_GetTrueVersionNumber = 0x06;

        public const byte Internal = 0x5D;
        public const byte Internal_GetSwappableDataArea = 0x06;

        public const byte FileInfo = 0x57;
        public const byte FileInfo_GetDateTime = 0x00;
        public const byte FileInfo_SetDateTime = 0x01;

        public const byte CanonicalizePath = 0x60;
        public const byte CreateChildPsp = 0x55;
        public const byte RenameFile = 0x56;

        public const byte Function37 = 0x37;
        public const byte Function37_GetSwitchCharacter = 0x00;
    }
}
