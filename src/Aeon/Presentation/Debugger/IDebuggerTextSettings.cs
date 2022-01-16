using System.Windows.Media;

namespace Aeon.Emulator.Launcher.Presentation.Debugger
{
    /// <summary>
    /// Defines properties which describe how debugger text should be formatted.
    /// </summary>
    public interface IDebuggerTextSettings
    {
        /// <summary>
        /// Gets the brush used for registers.
        /// </summary>
        Brush Register { get; }
        /// <summary>
        /// Gets the brush used for immediates.
        /// </summary>
        Brush Immediate { get; }
        /// <summary>
        /// Gets the brush used for addresses.
        /// </summary>
        Brush Address { get; }
    }
}
