using System;
using System.Runtime.InteropServices;

namespace Aeon.Emulator.Sound
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct DirectSound8Inst
    {
        public unsafe DirectSound8V* Vtbl;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct DirectSound8V
    {
        public IntPtr QueryInterface;
        public IntPtr AddRef;
        public IntPtr Release;

        public IntPtr CreateSoundBuffer;
        public IntPtr GetCaps;
        public IntPtr DuplicateSoundBuffer;
        public IntPtr SetCooperativeLevel;

        public IntPtr Compact;
        public IntPtr GetSpeakerConfig;
        public IntPtr SetSpeakerConfig;
        public IntPtr Initialize;
        public IntPtr VerifyCertification;
    }

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    internal delegate uint NoParamProc(IntPtr pThis);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    internal unsafe delegate uint CreateBufferProc(IntPtr pThis, DSBUFFERDESC* pcDSBufferDesc, out IntPtr ppDSBuffer, IntPtr pUnkOuter);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    internal unsafe delegate uint GetCapsProc(DirectSound8Inst* pThis, DSCAPS* pDSCaps);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    internal unsafe delegate uint DuplicateSoundBufferProc(DirectSound8Inst* pThis, IntPtr pDSBufferOriginal, out IntPtr ppDSBufferDuplicate);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    internal unsafe delegate uint SetCooperativeLevelProc(IntPtr pThis, IntPtr hwnd, uint dwLevel);

    [StructLayout(LayoutKind.Sequential)]
    internal struct DSBUFFERDESC
    {
        public uint dwSize;
        public uint dwFlags;
        public uint dwBufferBytes;
        public uint dwReserved;
        public unsafe WAVEFORMATEX* lpwfxFormat;
        public unsafe fixed byte guid3DAlgorithm[16];
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct WAVEFORMATEX
    {
        public ushort wFormatTag;
        public ushort nChannels;
        public uint nSamplesPerSec;
        public uint nAvgBytesPerSec;
        public ushort nBlockAlign;
        public ushort wBitsPerSample;
        public ushort cbSize;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct DSCAPS
    {
        public uint dwSize;
        public uint dwFlags;
        public uint dwMinSecondarySampleRate;
        public uint dwMaxSecondarySampleRate;
        public uint dwPrimaryBuffers;
        public uint dwMaxHwMixingAllBuffers;
        public uint dwMaxHwMixingStaticBuffers;
        public uint dwMaxHwMixingStreamingBuffers;
        public uint dwFreeHwMixingAllBuffers;
        public uint dwFreeHwMixingStaticBuffers;
        public uint dwFreeHwMixingStreamingBuffers;
        public uint dwMaxHw3DAllBuffers;
        public uint dwMaxHw3DStaticBuffers;
        public uint dwMaxHw3DStreamingBuffers;
        public uint dwFreeHw3DAllBuffers;
        public uint dwFreeHw3DStaticBuffers;
        public uint dwFreeHw3DStreamingBuffers;
        public uint dwTotalHwMemBytes;
        public uint dwFreeHwMemBytes;
        public uint dwMaxContigFreeHwMemBytes;
        public uint dwUnlockTransferRateHwBuffers;
        public uint dwPlayCpuOverheadSwBuffers;
        public uint dwReserved1;
        public uint dwReserved2;
    }
}
