using System;

namespace Aeon.Emulator.Sound
{
    /// <summary>
    /// Provides access to the Windows midi mapper.
    /// </summary>
    internal sealed class MidiMapper : IDisposable
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the MidiMapper class.
        /// </summary>
        public MidiMapper()
        {
            SafeNativeMethods.midiOutOpen(out midiOutHandle, SafeNativeMethods.MIDI_MAPPER, IntPtr.Zero, IntPtr.Zero, 0);
        }
        ~MidiMapper()
        {
            Dispose(false);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Writes the next byte of a midi message to the midi mapper.
        /// </summary>
        /// <param name="value">Byte to write.</param>
        public void SendByte(byte value)
        {
            if((value & 0x80) == 0x80)
            {
                currentMessage = value;
                bytesReceived = 1;
                bytesExpected = messageLength[(value & 0x70) >> 4];
            }
            else
            {
                if(bytesReceived < bytesExpected)
                {
                    currentMessage |= (uint)(value << (int)(bytesReceived * 8u));
                    bytesReceived++;
                }
            }

            if(bytesReceived >= bytesExpected)
            {
                SafeNativeMethods.midiOutShortMsg(midiOutHandle, currentMessage);
                bytesReceived = 0;
                bytesExpected = 0;
            }
        }
        /// <summary>
        /// Turns off all of the notes on the midi device.
        /// </summary>
        public void Reset()
        {
            SafeNativeMethods.midiOutReset(midiOutHandle);
        }
        /// <summary>
        /// Releases resources used by the midi mapper.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Releases resources used by the midi mapper.
        /// </summary>
        /// <param name="disposing">True indicates that the method was called from Dispose; false indicates that it was called from the finalizer.</param>
        private void Dispose(bool disposing)
        {
            if(midiOutHandle != IntPtr.Zero)
            {
                SafeNativeMethods.midiOutClose(midiOutHandle);
                midiOutHandle = IntPtr.Zero;
            }
        }
        #endregion

        #region Private Fields
        private IntPtr midiOutHandle;
        private uint currentMessage;
        private uint bytesReceived;
        private uint bytesExpected;
        private static readonly uint[] messageLength = { 3, 3, 3, 3, 2, 2, 3, 1 };
        #endregion
    }
}
