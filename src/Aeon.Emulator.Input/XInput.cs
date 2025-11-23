using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Aeon.Emulator.Input;

public static class XInput
{
    public static void Enable() => NativeMethods.XInputEnable(true);
    public static void Disable() => NativeMethods.XInputEnable(false);

    public static bool TryGetState(int controller, out XInputGamepadState state)
    {
        unsafe
        {
            XINPUT_STATE s;
            if (NativeMethods.XInputGetState((uint)controller, &s) == 0)
            {
                state = s.Gamepad;
                return true;
            }
            else
            {
                state = default;
                return false;
            }
        }
    }

    public static bool TryGetController([MaybeNullWhen(false)] out IGameController controller)
    {
        if (TryGetState(0, out _))
        {
            controller = new Controller(0);
            return true;
        }
        else
        {
            controller = null;
            return false;
        }
    }

    private sealed class Controller : IGameController
    {
        private const long DebouncePeriod = 10;
        private readonly Stopwatch debounce = new();
        private XInputButtons lastButtonState;

        public Controller(int index) => this.Index = index;

        public string Name => $"XInput Controller {this.Index + 1}";
        public int Index { get; }

        public bool TryGetState(out GameControllerState state)
        {
            if (XInput.TryGetState(this.Index, out var s))
            {
                if (s.Buttons != this.lastButtonState)
                {
                    if (!this.debounce.IsRunning)
                    {
                        this.debounce.Restart();
                    }
                    else if (this.debounce.ElapsedMilliseconds >= DebouncePeriod)
                    {
                        this.lastButtonState = s.Buttons;
                        this.debounce.Reset();
                    }
                }
                else if (this.debounce.IsRunning)
                {
                    this.debounce.Reset();
                }

                var f = this.lastButtonState;

                var b = GameControllerButtons.None;
                if (f.HasFlag(XInputButtons.A))
                    b |= GameControllerButtons.Button1;
                if (f.HasFlag(XInputButtons.B))
                    b |= GameControllerButtons.Button2;
                if (f.HasFlag(XInputButtons.X))
                    b |= GameControllerButtons.Button3;
                if (f.HasFlag(XInputButtons.Y))
                    b |= GameControllerButtons.Button4;

                float xAxis;

                if (f.HasFlag(XInputButtons.DPadLeft))
                    xAxis = -1;
                else if (f.HasFlag(XInputButtons.DPadRight))
                    xAxis = 1;
                else
                    xAxis = (float)s.LeftThumbX / short.MaxValue;

                if (MathF.Abs(xAxis) < 0.25f)
                    xAxis = 0;

                float yAxis;

                if (f.HasFlag(XInputButtons.DPadUp))
                    yAxis = -1;
                else if (f.HasFlag(XInputButtons.DPadDown))
                    yAxis = 1;
                else
                    yAxis = (float)s.LeftThumbY / -short.MaxValue;

                if (MathF.Abs(yAxis) < 0.25f)
                    yAxis = 0;

                state = new GameControllerState(xAxis, yAxis, b);
                return true;
            }
            else
            {
                state = default;
                return false;
            }
        }

        void IDisposable.Dispose()
        {
        }
    }
}
