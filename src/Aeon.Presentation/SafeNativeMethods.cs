using System.Runtime.InteropServices;
using System.Security;

namespace Aeon.Presentation
{
    [SuppressUnmanagedCodeSecurity]
    internal static class SafeNativeMethods
    {
        [DllImport("user32.dll", CallingConvention = CallingConvention.Winapi)]
        public static extern uint SetCursorPos(int x, int y);
    }
}
