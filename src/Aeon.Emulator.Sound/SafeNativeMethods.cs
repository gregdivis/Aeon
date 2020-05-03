using System;
using System.Runtime.InteropServices;
using System.Security;

namespace Aeon.Emulator.Sound
{
    [SuppressUnmanagedCodeSecurity]
    internal static class SafeNativeMethods
    {
        #region MIDI Mapper
        [DllImport("winmm.dll", CallingConvention = CallingConvention.Winapi, SetLastError = false)]
        public static extern uint midiOutOpen(out IntPtr lphmo, uint uDeviceID, IntPtr dwCallback, IntPtr dwCallbackInstance, uint dwFlags);
        [DllImport("winmm.dll", CallingConvention = CallingConvention.Winapi, SetLastError = false)]
        public static extern uint midiOutClose(IntPtr hmo);
        [DllImport("winmm.dll", CallingConvention = CallingConvention.Winapi, SetLastError = false)]
        public static extern uint midiOutShortMsg(IntPtr hmo, uint dwMsg);
        [DllImport("winmm.dll", CallingConvention = CallingConvention.Winapi, SetLastError = false)]
        public static extern uint midiOutReset(IntPtr hmo);

        public const uint MIDI_MAPPER = 0xFFFFFFFF;
        #endregion

        #region DirectSound
        [DllImport("dsound.dll", CallingConvention = CallingConvention.Winapi)]
        public static extern uint DirectSoundCreate8(IntPtr lpcGuidDevice, out IntPtr ppDS8, IntPtr pUnkOuter);
        #endregion
    }
}
