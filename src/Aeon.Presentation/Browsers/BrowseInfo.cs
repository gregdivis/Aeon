using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Aeon.Presentation.Browsers
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct BrowseInfo
    {
        public IntPtr hwndOwner;
        public IntPtr pidlRoot;
        public IntPtr pszDisplayName;
        public IntPtr lpszTitle;
        public BrowseFlags ulFlags;
        public IntPtr lpfn;
        public IntPtr lParam;
        public int iImage;
    }

    [Flags]
    internal enum BrowseFlags : uint
    {
        None = 0,
        ReturnOnlyFSDirs = 0x00000001,
        DontGoBelowDomain = 0x00000002,
        StatusText = 0x00000004,
        ReturnFSAncestors = 0x00000008,
        EditBox = 0x00000010,
        Validate = 0x00000020,
        NewDialogStyle = 0x00000040,
        IncludeUrls = 0x00000080,
        UseNewUI = EditBox | NewDialogStyle,
        UAHint = 0x00000100,
        NoNewFolderButton = 0x00000200,
        NoTranslateTargets = 0x00000400,
        BrowseForComputer = 0x00001000,
        BrowseForPrinter = 0x00002000,
        IncludeFiles = 0x00004000,
        Sharable = 0x00008000,
        BrowseFileJunctions = 0x00010000
    }
}
