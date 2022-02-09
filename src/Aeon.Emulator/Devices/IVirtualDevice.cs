using System.Threading.Tasks;

namespace Aeon.Emulator
{
    /// <summary>
    /// Defines a virtual device for an emulated machine.
    /// </summary>
    public interface IVirtualDevice
    {
        /// <summary>
        /// Invoked when the emulator enters a paused state.
        /// </summary>
        Task PauseAsync() => Task.CompletedTask;
        /// <summary>
        /// Invoked when the emulator resumes from a paused state.
        /// </summary>
        Task ResumeAsync() => Task.CompletedTask;
        /// <summary>
        /// Invoked when the virtual device has been added to a VirtualMachine.
        /// </summary>
        /// <param name="vm">VirtualMachine which owns the device.</param>
        void DeviceRegistered(VirtualMachine vm)
        {
        }
    }
}
