using Aeon.Emulator.Memory;

namespace Aeon.Emulator
{
    /// <summary>
    /// Defines a virtual device which is accessed via a callback function.
    /// </summary>
    public interface ICallbackProvider : IVirtualDevice
    {
        /// <summary>
        /// Gets a value indicating whether the callback is hookable.
        /// </summary>
        bool IsHookable { get; }
        /// <summary>
        /// Sets the address of the callback function.
        /// </summary>
        RealModeAddress CallbackAddress { set; }

        /// <summary>
        /// Performs the callback action.
        /// </summary>
        void InvokeCallback();
    }
}
