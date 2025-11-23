using Aeon.Emulator.Memory;

namespace Aeon.Emulator;

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
    /// <summary>
    /// Notifies the handler what machine code can be used to invoke itself.
    /// </summary>
    /// <param name="machineCode">Machine code sequence used to invoke the handler.</param>
    void SetRaiseCallbackInstruction(ReadOnlySpan<byte> machineCode)
    {
    }
}
