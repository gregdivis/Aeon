#nullable disable

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

        public static implicit operator ErrorCodeResult<TResult>(TResult result) => new(result);
        public static implicit operator ErrorCodeResult<TResult>(ExtendedErrorCode errorCode) => new(errorCode);

        public TResult Result { get; }
        public ExtendedErrorCode ErrorCode { get; }
    }
}
