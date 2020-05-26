using System;
using System.Runtime.InteropServices;

namespace Aeon.Emulator.Sound
{
    /// <summary>
    /// Provides access to a DirectSound device.
    /// </summary>
    public sealed class DirectSound : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the DirectSound class.
        /// </summary>
        /// <param name="hwnd">Main application window handle.</param>
        private DirectSound(IntPtr hwnd)
        {
            NativeMethods.DirectSoundCreate8(IntPtr.Zero, out var ds8, IntPtr.Zero);

            this.directSound = ds8;

            unsafe
            {
                DirectSound8Inst* inst = (DirectSound8Inst*)ds8.ToPointer();
                var setCoopLevel = (SetCooperativeLevelProc)Marshal.GetDelegateForFunctionPointer(inst->Vtbl->SetCooperativeLevel, typeof(SetCooperativeLevelProc));

                this.createBuffer = (CreateBufferProc)Marshal.GetDelegateForFunctionPointer(inst->Vtbl->CreateSoundBuffer, typeof(CreateBufferProc));
                this.release = (NoParamProc)Marshal.GetDelegateForFunctionPointer(inst->Vtbl->Release, typeof(NoParamProc));

                uint res = setCoopLevel(directSound, hwnd, 2);
                if (res != 0)
                    throw new InvalidOperationException("Unable to set DirectSound cooperative level.");
            }

            this.releaseHandle = GCHandle.Alloc(this.release, GCHandleType.Normal);
        }
        ~DirectSound()
        {
            Dispose(false);
        }

        /// <summary>
        /// Creates a new DirectSound buffer.
        /// </summary>
        /// <param name="sampleRate">Sampling rate of the buffer data.</param>
        /// <param name="channelMode">Specifies the number of channels in the buffer data.</param>
        /// <param name="bitsPerSample">Specifies the number of bits for each sample in the buffer data.</param>
        /// <param name="bufferLength">The length of the buffer.</param>
        /// <returns>New DirectSound buffer instance.</returns>
        public DirectSoundBuffer CreateBuffer(int sampleRate, ChannelMode channelMode, BitsPerSample bitsPerSample, TimeSpan bufferLength)
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(DirectSound));
            if (bufferLength < new TimeSpan(0, 0, 0, 0, 5) || bufferLength > new TimeSpan(1, 0, 0))
                throw new ArgumentOutOfRangeException(nameof(bufferLength));

            double bytesPerSec = sampleRate;
            if (channelMode == ChannelMode.Stereo)
                bytesPerSec *= 2;
            if (bitsPerSample == BitsPerSample.Sixteen)
                bytesPerSec *= 2;

            return CreateBuffer(sampleRate, channelMode, bitsPerSample, (int)(bufferLength.TotalSeconds * bytesPerSec));
        }
        /// <summary>
        /// Creates a new DirectSound buffer.
        /// </summary>
        /// <param name="sampleRate">Sampling rate of the buffer data.</param>
        /// <param name="channelMode">Specifies the number of channels in the buffer data.</param>
        /// <param name="bitsPerSample">Specifies the number of bits for each sample in the buffer data.</param>
        /// <param name="bufferSize">The size of the buffer in bytes.</param>
        /// <returns>New DirectSound buffer instance.</returns>
        public DirectSoundBuffer CreateBuffer(int sampleRate, ChannelMode channelMode, BitsPerSample bitsPerSample, int bufferSize)
        {
            if (disposed)
                throw new ObjectDisposedException("DirectSound");
            if (bufferSize <= 0)
                throw new ArgumentOutOfRangeException("bufferSize");

            DSBUFFERDESC dsbd = new DSBUFFERDESC();
            dsbd.dwSize = (uint)Marshal.SizeOf(typeof(DSBUFFERDESC));
            dsbd.dwFlags = bufferFlags;

            WAVEFORMATEX wfx = new WAVEFORMATEX();
            wfx.nChannels = (channelMode == ChannelMode.Stereo) ? (ushort)2u : (ushort)1u;
            wfx.wBitsPerSample = (bitsPerSample == BitsPerSample.Sixteen) ? (ushort)16u : (ushort)8u;
            wfx.wFormatTag = 1;
            wfx.nSamplesPerSec = (uint)sampleRate;
            wfx.nBlockAlign = (ushort)(wfx.nChannels * (wfx.wBitsPerSample / 8u));
            wfx.nAvgBytesPerSec = wfx.nSamplesPerSec * wfx.nBlockAlign;

            dsbd.dwBufferBytes = (uint)bufferSize;

            unsafe
            {
                IntPtr dsbuf = new IntPtr();
                dsbd.lpwfxFormat = &wfx;
                uint res = this.createBuffer(directSound, &dsbd, out dsbuf, IntPtr.Zero);

                if (res != 0)
                    throw new InvalidOperationException("Unable to create DirectSound buffer.");

                return new DirectSoundBuffer(dsbuf, this);
            }
        }

        /// <summary>
        /// Releases resources used by the DirectSound instance.
        /// </summary>
        void IDisposable.Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Returns the current DirectSound instance or creates a new one if necessary.
        /// </summary>
        /// <param name="hwnd">Main application window handle.</param>
        /// <returns>Current DirectSound instance.</returns>
        public static DirectSound GetInstance(IntPtr hwnd)
        {
            lock (getInstanceLock)
            {
                DirectSound directSound;
                if (instance != null)
                {
                    directSound = instance.Target as DirectSound;
                    if (directSound != null)
                        return directSound;
                }

                directSound = new DirectSound(hwnd);
                instance = new WeakReference(directSound);
                return directSound;
            }
        }

        private void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                this.disposed = true;
                this.release(this.directSound);
                this.releaseHandle.Free();
            }
        }

        private bool disposed;
        private GCHandle releaseHandle;
        private readonly IntPtr directSound;
        private unsafe readonly CreateBufferProc createBuffer;
        private readonly NoParamProc release;

        private static WeakReference instance;
        private static readonly object getInstanceLock = new object();

        private const uint bufferFlags = 0x00000008u | 0x00000020u | 0x00000080u | 0x00008000u;
    }

    /// <summary>
    /// Used to specify the number of channels in a sound buffer.
    /// </summary>
    public enum ChannelMode
    {
        /// <summary>
        /// There is one channel.
        /// </summary>
        Monaural,
        /// <summary>
        /// There are two channels.
        /// </summary>
        Stereo
    }

    /// <summary>
    /// Used to specify the number of bits per sample in a sound buffer.
    /// </summary>
    public enum BitsPerSample
    {
        /// <summary>
        /// There are eight bits per sample.
        /// </summary>
        Eight,
        /// <summary>
        /// There are sixteen bits per sample.
        /// </summary>
        Sixteen
    }
}
