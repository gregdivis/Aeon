namespace Aeon.Emulator;

/// <summary>
/// Allows events to be raised on arbitrary threads.
/// </summary>
public interface IEventSynchronizer
{
    /// <summary>
    /// Invokes a method on another thread asynchronously.
    /// </summary>
    /// <param name="method">Method to invoke.</param>
    /// <param name="source">The object which raised the event.</param>
    /// <param name="e">Arguments to pass to the method.</param>
    void BeginInvoke(Delegate method, object source, EventArgs e);
}
