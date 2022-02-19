using System;
using System.Runtime.InteropServices;

namespace Aeon.Emulator.Input
{
    internal static class NativeMethods
    {
        /// <summary>
        /// Interface ID for the DirectInput8 (Unicode) interface.
        /// </summary>
        public static readonly Guid IID_IDirectInput8W = new(0xBF798031, 0x483A, 0x4DA2, 0xAA, 0x99, 0x5D, 0x64, 0xED, 0x36, 0x97, 0x00);
        /// <summary>
        /// Class ID for the DirectInput8 class.
        /// </summary>
        public static readonly Guid CLSID_DirectInput8 = new(0x25E609E4, 0xB259, 0x11CF, 0xBF, 0xC7, 0x44, 0x45, 0x53, 0x54, 0x00, 0x00);

        [DllImport("ole32.dll", CallingConvention = CallingConvention.Winapi)]
        public static extern unsafe uint CoCreateInstance(Guid* rclsid, void* pUnkOuter, uint dwClsContext, Guid* riid, void** ppv);

        [DllImport("kernel32.dll", CallingConvention = CallingConvention.Winapi)]
        public static extern IntPtr GetModuleHandleW(IntPtr moduleName);

        [DllImport("Xinput1_4.dll")]
        public static extern void XInputEnable([MarshalAs(UnmanagedType.Bool)] bool enable);
        [DllImport("Xinput1_4.dll")]
        public static extern unsafe uint XInputGetState(uint dwUserIndex, XINPUT_STATE* pState);
    }
}
