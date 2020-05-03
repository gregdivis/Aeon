using System;

namespace Aeon.Emulator.DebugSupport
{
    /// <summary>
    /// Specifies the current state of processor override prefixes.
    /// </summary>
    [Flags]
    public enum PrefixState
    {
        /// <summary>
        /// No prefix is in effect.
        /// </summary>
        None = 0,
        /// <summary>
        /// The DS segment override prefix is in effect.
        /// </summary>
        DS = 1 << 0,
        /// <summary>
        /// The ES segment override prefix is in effect.
        /// </summary>
        ES = 1 << 1,
        /// <summary>
        /// The CS segment override prefix is in effect.
        /// </summary>
        CS = 1 << 2,
        /// <summary>
        /// The SS segment override prefix is in effect.
        /// </summary>
        SS = 1 << 3,
        /// <summary>
        /// The FS segment override prefix is in effect.
        /// </summary>
        FS = 1 << 4,
        /// <summary>
        /// The GS segment override prefix is in effect.
        /// </summary>
        GS = 1 << 5,

        /// <summary>
        /// The operand-size override prefix is in effect.
        /// </summary>
        OperandSize = 1 << 6,
        /// <summary>
        /// The address-size override prefix is in effect.
        /// </summary>
        AddressSize = 1 << 7,

        /// <summary>
        /// The REPNE prefix is in effect.
        /// </summary>
        Repne = 1 << 8,
        /// <summary>
        /// The REPE prefix is in effect.
        /// </summary>
        Repe = 1 << 9
    }
}
