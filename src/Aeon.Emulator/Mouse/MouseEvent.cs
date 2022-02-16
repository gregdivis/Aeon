using Aeon.Emulator.Mouse;

namespace Aeon.Emulator
{
    /// <summary>
    /// Describes a mouse input event.
    /// </summary>
    public abstract class MouseEvent
    {
        private protected MouseEvent()
        {
        }

        /// <summary>
        /// Raises the event on the emulated mouse device.
        /// </summary>
        /// <param name="mouse">Emulated mouse device instance.</param>
        internal abstract void RaiseEvent(MouseHandler mouse);
    }
}
