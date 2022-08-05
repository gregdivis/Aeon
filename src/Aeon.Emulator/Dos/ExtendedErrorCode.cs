namespace Aeon.Emulator.Dos
{
    public enum ExtendedErrorCode : byte
    {
        NoError,
        FunctionNumberInvalid,
        FileNotFound,
        PathNotFound,
        TooManyOpenFiles,
        AccessDenied,
        InvalidHandle,
        MemoryControlBlockDestroyed,
        InsufficientMemory,
        MemoryBlockAddressInvalid,
        EnvironmentInvalid,
        FormatInvalid,
        AccessCodeInvalid,
        DataInvalid,
        Reserved,
        InvalidDrive,
        AttemptedToRemoveCurrentDirectory,
        NotSameDevice,
        NoMoreFiles
    }
}
