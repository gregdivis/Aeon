using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Text;

namespace Aeon.Emulator.Launcher.Presentation.Browsers
{
    public class FolderBrowserDialog
    {
        public FolderBrowserDialog()
        {
        }

        public string Title { get; set; }
        public string Path { get; set; }
        public bool ShowNewFolderButton { get; set; }

        public bool ShowDialog(Window owner)
        {
            var hwnd = new WindowInteropHelper(owner).Handle;

            IntPtr pidl = IntPtr.Zero;
            var browseInfo = new BrowseInfo() { hwndOwner = hwnd, ulFlags = BrowseFlags.NewDialogStyle | BrowseFlags.ReturnOnlyFSDirs };
            if(!this.ShowNewFolderButton)
                browseInfo.ulFlags |= BrowseFlags.NoNewFolderButton;

            try
            {
                browseInfo.pszDisplayName = Marshal.AllocCoTaskMem(300 * 2);

                string title = this.Title;
                if(!string.IsNullOrEmpty(title))
                    browseInfo.lpszTitle = Marshal.StringToCoTaskMemUni(title);

                string path = this.Path ?? string.Empty;
                if(path.Length > 259)
                    path = path.Substring(0, 259);
                Marshal.Copy(path.ToCharArray(), 0, browseInfo.pszDisplayName, path.Length);
                Marshal.WriteInt16(browseInfo.pszDisplayName, path.Length, 0);

                pidl = SafeNativeMethods.SHBrowseForFolder(ref browseInfo);

                if(pidl != IntPtr.Zero)
                {
                    var buffer = new StringBuilder(300);
                    SafeNativeMethods.SHGetPathFromIDList(pidl, buffer);
                    this.Path = buffer.ToString();

                    return true;
                }

                return false;
            }
            finally
            {
                if(pidl != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(pidl);
                if(browseInfo.lpszTitle != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(browseInfo.lpszTitle);
                if(browseInfo.pszDisplayName != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(browseInfo.pszDisplayName);
            }

        }
    }
}
