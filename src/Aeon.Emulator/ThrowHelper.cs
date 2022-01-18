using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Aeon.Emulator.RuntimeExceptions;

namespace Aeon.Emulator
{
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
    }
}
