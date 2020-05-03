using System;
using System.Runtime.InteropServices;

namespace Aeon.Emulator.Sound
{
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    internal delegate uint GetValueProc(IntPtr pThis, out int value);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    internal delegate uint SetValueProc(IntPtr pThis, int value);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    internal delegate uint GetPositionProc(IntPtr pThis, out uint pdwCurrentPlayCursor, out uint pdwCurrentWriteCursor);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    internal delegate uint LockProc(IntPtr pThis, uint dwOffset, uint dwBytes, out IntPtr ppvAudioPtr1, out uint pdwAudioBytes1, out IntPtr ppvAudioPtr2, out uint pdwAudioBytes2, uint dwFlags);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    internal delegate uint PlayProc(IntPtr pThis, uint dwReserved1, uint dwPriority, uint dwFlags);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    internal delegate uint SetPositionProc(IntPtr pThis, uint dwNewPosition);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    internal delegate uint UnlockProc(IntPtr pThis, IntPtr pvAudioPtr1, uint dwAudioBytes1, IntPtr pvAudioPtr2, uint dwAudioBytes2);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    internal unsafe delegate uint GetBufferCapsProc(IntPtr pThis, DSBCAPS* pDSBufferCaps);

    [StructLayout(LayoutKind.Sequential)]
    internal struct DirectSoundBuffer8Inst
    {
        public unsafe DirectSoundBuffer8V* Vtbl;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct DirectSoundBuffer8V
    {
        public IntPtr QueryInterface;
        public IntPtr AddRef;
        public IntPtr Release;

        public IntPtr GetCaps;
        public IntPtr GetCurrentPosition;
        public IntPtr GetFormat;
        public IntPtr GetVolume;
        public IntPtr GetPan;
        public IntPtr GetFrequency;
        public IntPtr GetStatus;
        public IntPtr Initialize;
        public IntPtr Lock;
        public IntPtr Play;
        public IntPtr SetCurrentPosition;
        public IntPtr SetFormat;
        public IntPtr SetVolume;
        public IntPtr SetPan;
        public IntPtr SetFrequency;
        public IntPtr Stop;
        public IntPtr Unlock;
        public IntPtr Restore;
        public IntPtr SetFX;
        public IntPtr AcquireResources;
        public IntPtr GetObjectInPath;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct DSBCAPS
    {
        public uint dwSize;
        public uint dwFlags;
        public uint dwBufferBytes;
        public uint dwUnlockTransferRate;
        public uint dwPlayCpuOverhead;
    }
}
