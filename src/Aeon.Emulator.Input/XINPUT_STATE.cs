using System.Runtime.InteropServices;

namespace Aeon.Emulator.Input;

[StructLayout(LayoutKind.Sequential)]
internal readonly struct XINPUT_STATE
{
    public readonly uint dwPacketNumber;
    public readonly XInputGamepadState Gamepad;
}
