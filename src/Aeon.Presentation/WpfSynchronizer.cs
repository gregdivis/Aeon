using System;
using System.Windows.Threading;
using Aeon.Emulator;

namespace Aeon.Presentation
{
    /// <summary>
    /// Allows Aeon to raise events via a Dispatcher object.
    /// </summary>
    internal sealed class WpfSynchronizer : IEventSynchronizer
    {
        /// <summary>
        /// The WPF dispatcher to use.
        /// </summary>
        private readonly Dispatcher dispatcher;

        /// <summary>
        /// Initializes a new instance of the WpfSynchronizer class.
        /// </summary>
        /// <param name="dispatcher">WPF dispatcher object for syncrhonization.</param>
        public WpfSynchronizer(Dispatcher dispatcher)
        {
            this.dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        }

        /// <summary>
        /// Invokes a method on another thread asynchronously.
        /// </summary>
        /// <param name="method">Method to invoke.</param>
        /// <param name="source">The object which raised the event.</param>
        /// <param name="e">Arguments to pass to the method.</param>
        public void BeginInvoke(Delegate method, object source, EventArgs e)
        {
            this.dispatcher.BeginInvoke(method, source, e);
        }
    }
}
