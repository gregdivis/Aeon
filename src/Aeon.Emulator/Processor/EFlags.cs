using System;

namespace Aeon.Emulator
{
    /// <summary>
    /// Represents the allowable values of the EFLAGS register.
    /// </summary>
    [Flags]
    public enum EFlags
    {
        /// <summary>
        /// The EFLAGS register is clear.
        /// </summary>
        Clear = 0,
        /// <summary>
        /// The carry flag (CF) is set.
        /// </summary>
        Carry = (1 << 0),
        /// <summary>
        /// Reserved. Must be set to 1.
        /// </summary>
        Reserved1 = (1 << 1),
        /// <summary>
        /// The parity flag (PF) is set.
        /// </summary>
        Parity = (1 << 2),
        /// <summary>
        /// The auxiliary flag (AF) is set.
        /// </summary>
        Auxiliary = (1 << 4),
        /// <summary>
        /// The zero flag (ZF) is set.
        /// </summary>
        Zero = (1 << 6),
        /// <summary>
        /// The sign flag (SF) is set.
        /// </summary>
        Sign = (1 << 7),
        /// <summary>
        /// The trap flag is set.
        /// </summary>
        Trap = (1 << 8),
        /// <summary>
        /// The interrupt enable flag is set.
        /// </summary>
        InterruptEnable = (1 << 9),
        /// <summary>
        /// The direction flag is set.
        /// </summary>
        Direction = (1 << 10),
        /// <summary>
        /// The overflow flag (OF) is set.
        /// </summary>
        Overflow = (1 << 11),
        /// <summary>
        /// The first bit of the I/O privilege level is set.
        /// </summary>
        IOPrivilege1 = (1 << 12),
        /// <summary>
        /// The second bit of the I/O privilege level is set.
        /// </summary>
        IOPrivilege2 = (1 << 13),
        /// <summary>
        /// The nested task flag is set.
        /// </summary>
        NestedTask = (1 << 14),
        /// <summary>
        /// The resume flag is set.
        /// </summary>
        Resume = (1 << 16),
        /// <summary>
        /// The virtual 8086 mode flag is set.
        /// </summary>
        Virtual8086Mode = (1 << 17),
        /// <summary>
        /// The alignment check flag is set.
        /// </summary>
        AlignmentCheck = (1 << 18),
        /// <summary>
        /// The virtual interrupt flag is set.
        /// </summary>
        VirtualInterrupt = (1 << 19),
        /// <summary>
        /// The virtual interrupt pending flag is set.
        /// </summary>
        VirtualInterruptPending = (1 << 20),
        /// <summary>
        /// The identification flag is set.
        /// </summary>
        Identification = (1 << 21)
    }
}
