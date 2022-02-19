using System;

namespace Aeon.Emulator.Input
{
    public interface IGameController : IDisposable
    {
        string Name { get; }

        bool TryGetState(out GameControllerState state);

        public static IGameController GetDefault() => new DefaultController();
    }

    public readonly record struct GameControllerState(float XAxis, float YAxis, GameControllerButtons Buttons);

    [Flags]
    public enum GameControllerButtons : byte
    {
        None = 0,
        Button1 = 0x01,
        Button2 = 0x02,
        Button3 = 0x04,
        Button4 = 0x08
    }
}
