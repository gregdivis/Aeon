using System.Runtime.InteropServices;

namespace Aeon.Emulator.Launcher
{
    internal static class NativeMethods
    {
        [DllImport("user32.dll", CallingConvention = CallingConvention.Winapi)]
        public static extern uint SetCursorPos(int x, int y);
    }
}
