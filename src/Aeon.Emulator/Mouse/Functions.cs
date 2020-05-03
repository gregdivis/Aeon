namespace Aeon.Emulator.Mouse
{
    internal static class Functions
    {
        public const ushort Reset = 0x0000;
        public const ushort ShowCursor = 0x0001;
        public const ushort HideCursor = 0x0002;
        public const ushort GetPositionAndStatus = 0x0003;
        public const ushort SetCursorPosition = 0x0004;
        public const ushort GetButtonPressData = 0x0005;
        public const ushort GetButtonReleaseData = 0x0006;
        public const ushort SetHorizontalRange = 0x0007;
        public const ushort SetVerticalRange = 0x0008;
        public const ushort SetGraphicsCursor = 0x0009;
        public const ushort SetTextCursor = 0x000A;
        public const ushort GetMotionCounters = 0x000B;
        public const ushort SetCallbackParameters = 0x000C;
        public const ushort SetMickeyPixelRatio = 0x000F;
        public const ushort SetScreenUpdateRegion = 0x0010;
        public const ushort GetDriverStateStorageSize = 0x0015;
        public const ushort SaveDriverState = 0x0016;
        public const ushort RestoreDriverState = 0x0017;
        public const ushort EnableMouseDriver = 0x0020;
        public const ushort ExchangeCallbacks = 0x0014;
        public const ushort SoftwareReset = 0x0021;
    }
}
