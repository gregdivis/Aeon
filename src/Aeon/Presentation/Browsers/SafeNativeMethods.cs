using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace Aeon.Emulator.Launcher.Presentation.Browsers
{
    [SuppressUnmanagedCodeSecurity]
    internal static class SafeNativeMethods
    {
        [DllImport("shell32.dll", EntryPoint = "SHBrowseForFolderW", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        public static extern IntPtr SHBrowseForFolder(ref BrowseInfo lpbi);

        [DllImport("shell32.dll", EntryPoint = "SHGetPathFromIDListW", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SHGetPathFromIDList(IntPtr pidl, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszPath);
    }
}
