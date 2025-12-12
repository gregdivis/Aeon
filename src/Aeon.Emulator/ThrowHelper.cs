using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Aeon.Emulator.RuntimeExceptions;

namespace Aeon.Emulator;

[StackTraceHidden]
internal static class ThrowHelper
{
    [DoesNotReturn]
    internal static void ThrowEmulatedDivideByZeroException() => throw new EmulatedDivideByZeroException();
    [DoesNotReturn]
    internal static void ThrowEnableInstuctionTrapException() => throw new EnableInstructionTrapException();
    [DoesNotReturn]
    internal static void ThrowGeneralProtectionFaultException(ushort errorCode) => throw new GeneralProtectionFaultException(errorCode);
    [DoesNotReturn]
    internal static void ThrowSegmentNotPresentException(ushort value)
    {
        if ((value & 0x4u) == 0)
            throw new GDTSegmentNotPresentException((uint)value >> 3);
        else
            throw new LDTSegmentNotPresentException((uint)value >> 3);
    }
    [DoesNotReturn]
    internal static void ThrowNotImplementedException() => throw new NotImplementedException();
    [DoesNotReturn]
    internal static void ThrowCplGreaterThanRplException() => throw new InvalidOperationException("iret not supported when cpl > rpl.");
    [DoesNotReturn]
    internal static void ThrowCplLessThanDplException() => throw new InvalidOperationException("call not supported when cpl < dpl.");
    [DoesNotReturn]
    internal static void ThrowNoInterruptHandlerException(byte interrupt) => throw new ArgumentException($"There is no handler associated with interrupt {interrupt:X2}h.");
    [DoesNotReturn]
    internal static void ThrowNoCallbackHandlerException(byte id) => throw new ArgumentException($"There is no handler associated with callback #{id}.");
    [DoesNotReturn]
    internal static void ThrowInvalidTaskSegmentSelectorException() => throw new InvalidOperationException("Invalid task segment selector.");
    [DoesNotReturn]
    internal static void ThrowNullCallException() => throw new InvalidOperationException("Attempted to call function at address 0.");
    [DoesNotReturn]
    internal static void ThrowHaltException() => throw new HaltException();
}
