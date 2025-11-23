using System.Diagnostics;

namespace Aeon.Emulator.Input;

internal sealed class DefaultController : IGameController
{
    private IGameController? current;
    private readonly Stopwatch lastAttempt = new();

    public string? Name => this.current?.Name;

    public bool TryGetState(out GameControllerState state)
    {
        if (this.current == null || !this.current.TryGetState(out state))
        {
            this.current?.Dispose();
            this.current = null;

            if (!this.lastAttempt.IsRunning || this.lastAttempt.Elapsed >= new TimeSpan(0, 0, 5))
            {
                this.current = GetDefaultController();
                this.lastAttempt.Restart();

                if (this.current != null)
                    return this.current.TryGetState(out state);
            }

            state = default;
            return false;
        }

        return true;
    }

    public void Dispose() => this.current?.Dispose();

    private static IGameController? GetDefaultController()
    {
        // first check for an XInput compatible controller
        if (XInput.TryGetController(out var controller))
            return controller;

        IntPtr hwnd;

        using (var p = Process.GetCurrentProcess())
        {
            hwnd = p.MainWindowHandle;
        }

        var dinput = DirectInput.GetInstance(hwnd);

        // if none found, try for the first DirectInput device
        var d = dinput.GetDevices(DeviceClass.GameController, DeviceEnumFlags.All).FirstOrDefault();
        if (d != null)
            return new DirectInputGameController(dinput.CreateDevice(d.InstanceId));

        return null;
    }
}
