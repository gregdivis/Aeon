namespace Aeon.Emulator.Dos
{
    public readonly struct ErrorCodeResult<TResult>
    {
        public ErrorCodeResult(TResult result)
        {
            this.Result = result;
            this.ErrorCode = ExtendedErrorCode.NoError;
        }
        public ErrorCodeResult(ExtendedErrorCode errorCode)
        {
            this.Result = default;
            this.ErrorCode = errorCode;
        }

        public static implicit operator ErrorCodeResult<TResult>(TResult result) => new ErrorCodeResult<TResult>(result);
        public static implicit operator ErrorCodeResult<TResult>(ExtendedErrorCode errorCode) => new ErrorCodeResult<TResult>(errorCode);

        public TResult Result { get; }
        public ExtendedErrorCode ErrorCode { get; }
    }

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
