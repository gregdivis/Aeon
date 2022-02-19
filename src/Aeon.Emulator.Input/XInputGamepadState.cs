using System.Runtime.InteropServices;

namespace Aeon.Emulator.Input
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct XInputGamepadState
    {
        private readonly XInputButtons wButtons;
        private readonly byte bLeftTrigger;
        private readonly byte bRightTrigger;
        private readonly short sThumbLX;
        private readonly short sThumbLY;
        private readonly short sThumbRX;
        private readonly short sThumbRY;

        public XInputButtons Buttons => this.wButtons;
        public byte LeftTrigger => this.bLeftTrigger;
        public byte RightTrigger => this.bRightTrigger;
        public short LeftThumbX => this.sThumbLX;
        public short LeftThumbY => this.sThumbLY;
        public short RightThumbX => this.sThumbRX;
        public short RightThumbY => this.sThumbRY;
    }
}
