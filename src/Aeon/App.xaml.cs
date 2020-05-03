using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace Aeon.Emulator.Launcher
{
    /// <summary>
    /// The Aeon application class.
    /// </summary>
    public sealed partial class App : Application
    {
        /// <summary>
        /// Gets the current application instance.
        /// </summary>
        public static new App Current => (App)Application.Current;

        /// <summary>
        /// Gets the application command line arguments.
        /// </summary>
        public ReadOnlyCollection<string> Args { get; private set; }

        /// <summary>
        /// Invoked when the application is started.
        /// </summary>
        /// <param name="e">Information about the event.</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            this.Args = new ReadOnlyCollection<string>(e.Args.ToList());

            base.OnStartup(e);
        }
    }
}
