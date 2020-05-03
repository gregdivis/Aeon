using System;

namespace Aeon.Emulator
{
    [Flags]
    public enum FPUStatus
    {
        Clear = 0,
        InvalidOperation = (1 << 0),
        Denormalized = (1 << 1),
        ZeroDivide = (1 << 2),
        Overflow = (1 << 3),
        Underflow = (1 << 4),
        Precision = (1 << 5),
        StackFault = (1 << 6),
        InterruptRequest = (1 << 7),
        C0 = (1 << 8),
        C1 = (1 << 9),
        C2 = (1 << 10),
        C3 = (1 << 14),
        Busy = (1 << 15)
    }
}
