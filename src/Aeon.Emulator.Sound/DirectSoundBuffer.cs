using System;
using System.Runtime.InteropServices;

namespace Aeon.Emulator.Sound
{
    /// <summary>
    /// Represents a DirectSound buffer which can be played in the background.
    /// </summary>
    public sealed class DirectSoundBuffer : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the DirectSoundBuffer class.
        /// </summary>
        /// <param name="dsb">Pointer to the native IDirectSoundBuffer8 instance.</param>
        /// <param name="sound">DirectSound instance used to create the buffer.</param>
        internal unsafe DirectSoundBuffer(IntPtr dsb, DirectSound sound)
        {
            this.soundBuffer = dsb;
            this.sound = sound;

            unsafe
            {
                DirectSoundBuffer8Inst* inst = (DirectSoundBuffer8Inst*)dsb.ToPointer();
                this.release = (NoParamProc)Marshal.GetDelegateForFunctionPointer(inst->Vtbl->Release, typeof(NoParamProc));
                this.getFrequency = (GetValueProc)Marshal.GetDelegateForFunctionPointer(inst->Vtbl->GetFrequency, typeof(GetValueProc));
                this.setFrequency = (SetValueProc)Marshal.GetDelegateForFunctionPointer(inst->Vtbl->SetFrequency, typeof(SetValueProc));
                this.getPan = (GetValueProc)Marshal.GetDelegateForFunctionPointer(inst->Vtbl->GetPan, typeof(GetValueProc));
                this.setPan = (SetValueProc)Marshal.GetDelegateForFunctionPointer(inst->Vtbl->SetPan, typeof(SetValueProc));
                this.getVolume = (GetValueProc)Marshal.GetDelegateForFunctionPointer(inst->Vtbl->GetVolume, typeof(GetValueProc));
                this.setVolume = (SetValueProc)Marshal.GetDelegateForFunctionPointer(inst->Vtbl->SetVolume, typeof(SetValueProc));
                this.getStatus = (GetValueProc)Marshal.GetDelegateForFunctionPointer(inst->Vtbl->GetStatus, typeof(GetValueProc));
                this.getPosition = (GetPositionProc)Marshal.GetDelegateForFunctionPointer(inst->Vtbl->GetCurrentPosition, typeof(GetPositionProc));
                this.lockBuffer = (LockProc)Marshal.GetDelegateForFunctionPointer(inst->Vtbl->Lock, typeof(LockProc));
                this.play = (PlayProc)Marshal.GetDelegateForFunctionPointer(inst->Vtbl->Play, typeof(PlayProc));
                this.setPosition = (SetPositionProc)Marshal.GetDelegateForFunctionPointer(inst->Vtbl->SetCurrentPosition, typeof(SetPositionProc));
                this.stop = (NoParamProc)Marshal.GetDelegateForFunctionPointer(inst->Vtbl->Stop, typeof(NoParamProc));
                this.unlockBuffer = (UnlockProc)Marshal.GetDelegateForFunctionPointer(inst->Vtbl->Unlock, typeof(UnlockProc));

                var getCaps = (GetBufferCapsProc)Marshal.GetDelegateForFunctionPointer(inst->Vtbl->GetCaps, typeof(GetBufferCapsProc));
                DSBCAPS caps = new DSBCAPS();
                caps.dwSize = (uint)sizeof(DSBCAPS);
                uint res = getCaps(soundBuffer, &caps);
                this.bufferSize = caps.dwBufferBytes;
            }

            this.releaseHandle = GCHandle.Alloc(release, GCHandleType.Normal);
        }
        ~DirectSoundBuffer()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Gets or sets the playback frequency of the buffer.
        /// </summary>
        public int Frequency
        {
            get
            {
                if (this.disposed)
                    return 0;

                this.getFrequency(this.soundBuffer, out int frequency);
                return frequency;
            }
            set
            {
                if (!this.disposed)
                    this.setFrequency(soundBuffer, value);
            }
        }
        /// <summary>
        /// Gets or sets the playback speaker pan of the buffer.
        /// </summary>
        public int Pan
        {
            get
            {
                if (this.disposed)
                    return 0;

                this.getPan(this.soundBuffer, out int pan);
                return pan;
            }
            set
            {
                if (!this.disposed)
                    this.setPan(this.soundBuffer, value);
            }
        }
        /// <summary>
        /// Gets or sets the playback volume of the buffer.
        /// </summary>
        public int Volume
        {
            get
            {
                if (this.disposed)
                    return 0;

                this.getVolume(this.soundBuffer, out int volume);
                return volume;
            }
            set
            {
                if (!this.disposed)
                    this.setVolume(this.soundBuffer, value);
            }
        }
        /// <summary>
        /// Gets a value indicating whether the buffer is currently playing.
        /// </summary>
        public bool IsPlaying
        {
            get
            {
                if (this.disposed)
                    return false;

                this.getStatus(this.soundBuffer, out int status);

                return (status & 0x01) != 0;
            }
        }
        /// <summary>
        /// Gets the current playback position in bytes.
        /// </summary>
        public int Position
        {
            get
            {
                if (this.disposed)
                    return 0;

                this.getPosition(this.soundBuffer, out uint playPos, out uint writePos);

                return (int)playPos;
            }
        }
        /// <summary>
        /// Gets the size of the buffer in bytes.
        /// </summary>
        public int BufferSize => (int)this.bufferSize;

        /// <summary>
        /// Begins playback of the sound buffer.
        /// </summary>
        /// <param name="playbackMode">Specifies the buffer playback behavior.</param>
        public void Play(PlaybackMode playbackMode)
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(DirectSoundBuffer));

            uint res = 0;
            switch (playbackMode)
            {
                case PlaybackMode.PlayOnce:
                    res = this.play(this.soundBuffer, 0, 0, 0);
                    break;

                case PlaybackMode.LoopContinuously:
                    res = this.play(this.soundBuffer, 0, 0, 1);
                    break;
            }

            if (res != 0)
                throw new InvalidOperationException("Unable to play DirectSound buffer.");
        }
        /// <summary>
        /// Stops playback of the sound buffer.
        /// </summary>
        public void Stop()
        {
            if (!this.disposed)
                this.stop(this.soundBuffer);
        }
        /// <summary>
        /// Writes waveform data to the buffer.
        /// </summary>
        /// <param name="source">Array containing waveform data to write to the buffer.</param>
        /// <param name="offset">Offset in source array from which to start copying.</param>
        /// <param name="length">Number of elements in source array to copy.</param>
        /// <returns>True if data was written to the buffer; false if data does not yet fit.</returns>
        public bool Write(byte[] source, int offset, int length)
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(DirectSoundBuffer));
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (offset < 0 || offset > source.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (length < 0 || offset + length > source.Length || length > bufferSize)
                throw new ArgumentOutOfRangeException(nameof(length));

            if (length == 0)
                return true;

            uint playPos;
            uint safeWritePos;
            this.getPosition(soundBuffer, out playPos, out safeWritePos);

            uint maxBytes;
            if (this.isEmpty)
                maxBytes = bufferSize;
            else if (writePos > playPos)
                maxBytes = bufferSize - writePos + playPos;
            else if (writePos < playPos)
                maxBytes = playPos - writePos;
            else
                return false;

            if (length > maxBytes)
                return false;

            uint res = this.lockBuffer(this.soundBuffer, this.writePos, (uint)length, out var ptr1, out uint length1, out var ptr2, out uint length2, 0);
            if (res != 0)
                throw new InvalidOperationException("Unable to lock DirectSound buffer.");

            if (ptr1 == IntPtr.Zero)
                throw new InvalidOperationException();

            try
            {
                if (ptr2 == IntPtr.Zero)
                {
                    Marshal.Copy(source, offset, ptr1, length);
                    writePos += (uint)length;
                    if (writePos >= bufferSize)
                        writePos = 0;
                }
                else
                {
                    Marshal.Copy(source, offset, ptr1, (int)length1);
                    Marshal.Copy(source, offset + (int)length1, ptr2, (int)length2);
                    writePos = length2;
                }

                this.isEmpty = false;
                return true;
            }
            finally
            {
                this.unlockBuffer(soundBuffer, ptr1, length1, ptr2, length2);
            }
        }
        /// <summary>
        /// Writes waveform data to the buffer.
        /// </summary>
        /// <param name="source">Array containing waveform data to write to the buffer.</param>
        /// <param name="offset">Offset in source array from which to start copying.</param>
        /// <param name="length">Number of elements in source array to copy.</param>
        /// <returns>True if data was written to the buffer; false if data does not yet fit.</returns>
        public bool Write(short[] source, int offset, int length)
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(DirectSoundBuffer));
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (offset < 0 || offset > source.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (length < 0 || offset + length > source.Length || length > bufferSize / 2)
                throw new ArgumentOutOfRangeException(nameof(length));

            if (length == 0)
                return true;

            this.getPosition(this.soundBuffer, out uint playPos, out uint safeWritePos);

            uint maxBytes;
            if (this.isEmpty)
                maxBytes = bufferSize;
            else if (writePos > playPos)
                maxBytes = bufferSize - writePos + playPos;
            else if (writePos < playPos)
                maxBytes = playPos - writePos;
            else
                return false;

            if (length > maxBytes / 2)
                return false;

            uint res = this.lockBuffer(this.soundBuffer, this.writePos, (uint)length * 2, out var ptr1, out uint length1, out var ptr2, out uint length2, 0);
            if (res != 0)
                throw new InvalidOperationException("Unable to lock DirectSound buffer.");

            if (ptr1 == IntPtr.Zero)
                throw new InvalidOperationException();

            try
            {
                if (ptr2 == IntPtr.Zero)
                {
                    Marshal.Copy(source, offset, ptr1, length);
                    writePos += (uint)length * 2u;
                    if (writePos >= bufferSize)
                        writePos = 0;
                }
                else
                {
                    Marshal.Copy(source, offset, ptr1, (int)length1 / 2);
                    Marshal.Copy(source, offset + (int)length1 / 2, ptr2, (int)length2 / 2);
                    writePos = length2;
                }

                this.isEmpty = false;
                return true;
            }
            finally
            {
                this.unlockBuffer(this.soundBuffer, ptr1, length1, ptr2, length2);
            }
        }
        /// <summary>
        /// Returns the current playback position indicatiors.
        /// </summary>
        /// <param name="writePos">Current position of the write pointer.</param>
        /// <param name="playPos">Current position of the playback pointer.</param>
        public void GetPositions(out int writePos, out int playPos)
        {
            this.getPosition(this.soundBuffer, out uint tempPlayPos, out uint tempSafeWritePos);
            writePos = (int)this.writePos;
            playPos = (int)tempPlayPos;
        }
        /// <summary>
        /// Releases resources used by the buffer.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                this.disposed = true;
                this.release(this.soundBuffer);
                this.releaseHandle.Free();
            }
        }

        private bool disposed;
        private bool isEmpty = true;
        private uint writePos;
        private GCHandle releaseHandle;
        private readonly uint bufferSize;
        private readonly DirectSound sound;
        private readonly IntPtr soundBuffer;
        private readonly NoParamProc release;
        private readonly GetValueProc getFrequency;
        private readonly SetValueProc setFrequency;
        private readonly GetValueProc getPan;
        private readonly SetValueProc setPan;
        private readonly GetValueProc getVolume;
        private readonly SetValueProc setVolume;
        private readonly GetValueProc getStatus;
        private readonly GetPositionProc getPosition;
        private readonly LockProc lockBuffer;
        private readonly PlayProc play;
        private readonly SetPositionProc setPosition;
        private readonly NoParamProc stop;
        private readonly UnlockProc unlockBuffer;
    }

    /// <summary>
    /// Specifies sound buffer playback behavior.
    /// </summary>
    public enum PlaybackMode
    {
        /// <summary>
        /// The buffer plays once and then stops.
        /// </summary>
        PlayOnce,
        /// <summary>
        /// The buffer plays repeatedly until it is explicitly stopped.
        /// </summary>
        LoopContinuously
    }
}
