using System.Windows;

namespace Aeon.Presentation
{
    /// <summary>
    /// Contains information about an emulation error.
    /// </summary>
    public sealed class EmulationErrorRoutedEventArgs : RoutedEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the EmulationErrorRoutedEventArgs class.
        /// </summary>
        /// <param name="routedEvent">RoutedEvent identifier.</param>
        /// <param name="message">Message describing the error.</param>
        public EmulationErrorRoutedEventArgs(RoutedEvent routedEvent, string message)
            : base(routedEvent)
        {
            this.Message = message;
        }

        /// <summary>
        /// Gets a message which describes the error.
        /// </summary>
        public string Message { get; }
    }
}
