using System.Windows;
using System.Windows.Media;

namespace Aeon.Emulator.Launcher.Debugger
{
    /// <summary>
    /// Contains attached dependency properties for Aeon debugging.
    /// </summary>
    public static class AeonDebug
    {
        /// <summary>
        /// The AeonDebug.IsHexFormat dependency property definition.
        /// </summary>
        public static readonly DependencyProperty IsHexFormatProperty = DependencyProperty.RegisterAttached("IsHexFormat", typeof(bool), typeof(AeonDebug), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits));
        /// <summary>
        /// The AeonDebug.DebuggerTextFormat dependency property definition.
        /// </summary>
        public static readonly DependencyProperty DebuggerTextFormatProperty = DependencyProperty.RegisterAttached("DebuggerTextFormat", typeof(IDebuggerTextSettings), typeof(AeonDebug), new FrameworkPropertyMetadata(new DefaultTextFormat(), FrameworkPropertyMetadataOptions.Inherits));

        /// <summary>
        /// Default debugger text format.
        /// </summary>
        private sealed class DefaultTextFormat : IDebuggerTextSettings
        {
            /// <summary>
            /// Initializes a new instance of the DefaultTextFormat class.
            /// </summary>
            public DefaultTextFormat()
            {
            }

            /// <summary>
            /// Gets the brush used for registers.
            /// </summary>
            public Brush Register => Brushes.Blue;
            /// <summary>
            /// Gets the brush used for immediates.
            /// </summary>
            public Brush Immediate => Brushes.Magenta;
            /// <summary>
            /// Gets the brush used for addresses.
            /// </summary>
            public Brush Address => Brushes.Maroon;
        }
    }
}
