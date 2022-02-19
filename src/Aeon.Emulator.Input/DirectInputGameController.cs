namespace Aeon.Emulator.Input
{
    internal sealed class DirectInputGameController : IGameController
    {
        private readonly DirectInputDevice device;
        private bool disposed;

        public DirectInputGameController(DirectInputDevice device) => this.device = device;

        public string Name => this.device.Info?.Name ?? "DirectInput Device";

        public bool TryGetState(out GameControllerState state)
        {
            if (this.device.Update())
            {
                var buttons = GameControllerButtons.None;
                if (this.device.Button1)
                    buttons |= GameControllerButtons.Button1;
                if (this.device.Button2)
                    buttons |= GameControllerButtons.Button2;
                if (this.device.Button3)
                    buttons |= GameControllerButtons.Button3;
                if (this.device.Button4)
                    buttons |= GameControllerButtons.Button4;

                state = new GameControllerState(ConvertPosition(this.device.XAxisPosition), ConvertPosition(this.device.YAxisPosition), buttons);

                return true;
            }
            else
            {
                state = default;
                return false;
            }
        }
        public void Dispose()
        {
            if (!this.disposed)
            {
                this.device.Dispose();
                this.disposed = true;
            }
        }

        private static float ConvertPosition(int p) => (float)(p - short.MaxValue) / short.MaxValue;
    }
}
