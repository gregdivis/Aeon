using System;
using System.Runtime.InteropServices;
using System.Security;

namespace Aeon.Emulator.Input
{
    [SuppressUnmanagedCodeSecurity]
    internal static class SafeNativeMethods
    {
        /// <summary>
        /// Interface ID for the DirectInput8 (Unicode) interface.
        /// </summary>
        public static readonly Guid IID_IDirectInput8W = new Guid(0xBF798031, 0x483A, 0x4DA2, 0xAA, 0x99, 0x5D, 0x64, 0xED, 0x36, 0x97, 0x00);
        /// <summary>
        /// Class ID for the DirectInput8 class.
        /// </summary>
        public static readonly Guid CLSID_DirectInput8 = new Guid(0x25E609E4, 0xB259, 0x11CF, 0xBF, 0xC7, 0x44, 0x45, 0x53, 0x54, 0x00, 0x00);

        [DllImport("ole32.dll", CallingConvention = CallingConvention.Winapi)]
        public static extern uint CoCreateInstance(ref Guid rclsid, IntPtr pUnkOuter, uint dwClsContext, ref Guid riid, out IntPtr ppv);

        [DllImport("kernel32.dll", CallingConvention = CallingConvention.Winapi)]
        public static extern IntPtr GetModuleHandleW(IntPtr moduleName);
    }
}
