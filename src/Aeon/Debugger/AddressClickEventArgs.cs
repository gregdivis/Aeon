using System.Windows;

namespace Aeon.Emulator.Launcher.Debugger
{
    /// <summary>
    /// Contains information about a clicked address.
    /// </summary>
    public sealed class AddressClickEventArgs : RoutedEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the AddressClickEventArgs class.
        /// </summary>
        /// <param name="address">Target address clicked.</param>
        /// <param name="routedEvent">The owner event.</param>
        public AddressClickEventArgs(TargetAddress address, RoutedEvent routedEvent)
            : base(routedEvent)
        {
            this.Target = address;
        }

        /// <summary>
        /// Gets the target address.
        /// </summary>
        public TargetAddress Target { get; private set; }
    }
}
