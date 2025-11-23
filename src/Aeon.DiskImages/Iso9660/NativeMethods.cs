using System.Runtime.InteropServices;

namespace Aeon.DiskImages.Iso9660;

internal static class NativeMethods
{
    public static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1L);

    [DllImport("kernel32.dll", CallingConvention = CallingConvention.Winapi, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool DeviceIoControl(IntPtr hDevice, uint dwIoControlCode, IntPtr lpInBuffer, uint nInBufferSize, IntPtr lpOutBuffer, uint nOutBufferSize, out uint lpBytesReturned, IntPtr lpOverlapped);

    [DllImport("kernel32.dll", EntryPoint= "CreateFileW", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
    public static extern IntPtr CreateFile([MarshalAs(UnmanagedType.LPWStr)] string lpFileName, uint dwDesiredAccess, uint dwShareMode, IntPtr lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile);

    [DllImport("kernel32.dll", CallingConvention = CallingConvention.Winapi, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool CloseHandle(IntPtr hObject);

    [DllImport("kernel32.dll", CallingConvention = CallingConvention.Winapi, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool ReadFile(IntPtr hFile, IntPtr lpBuffer, uint nNumberOfBytesToRead, out uint lpNumberOfBytesRead, IntPtr lpOverlapped);

    [DllImport("kernel32.dll", CallingConvention = CallingConvention.Winapi, SetLastError = true)]
    public static extern uint SetFilePointerEx(IntPtr hFile, long liDistanceToMove, out long lpNewFilePointer, uint dwMoveMethod);
}
