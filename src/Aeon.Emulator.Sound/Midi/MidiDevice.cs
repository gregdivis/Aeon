using System;

namespace Aeon.Emulator.Sound
{
    internal abstract class MidiDevice : IDisposable
    {
        private uint currentMessage;
        private uint bytesReceived;
        private uint bytesExpected;
        private byte[] currentSysex = new byte[128];
        private int sysexIndex = -1;
        private static readonly uint[] messageLength = { 3, 3, 3, 3, 2, 2, 3, 1 };

        protected MidiDevice()
        {
        }

        public void SendByte(byte value)
        {
            if (this.sysexIndex == -1)
            {
                if (value == 0xF0 && this.bytesExpected == 0)
                {
                    this.currentSysex[0] = 0xF0;
                    this.sysexIndex = 1;
                    return;
                }
                else if ((value & 0x80) != 0)
                {
                    this.currentMessage = value;
                    this.bytesReceived = 1;
                    this.bytesExpected = messageLength[(value & 0x70) >> 4];
                }
                else
                {
                    if (this.bytesReceived < this.bytesExpected)
                    {
                        this.currentMessage |= (uint)(value << (int)(this.bytesReceived * 8u));
                        this.bytesReceived++;
                    }
                }

                if (bytesReceived >= bytesExpected)
                {
                    this.PlayShortMessage(this.currentMessage);
                    this.bytesReceived = 0;
                    this.bytesExpected = 0;
                }
            }
            else
            {
                if (this.sysexIndex >= this.currentSysex.Length)
                    Array.Resize(ref this.currentSysex, this.currentSysex.Length * 2);

                this.currentSysex[this.sysexIndex++] = value;

                if (value == 0xF7)
                {
                    // do nothing for general midi
                    this.PlaySysex(this.currentSysex.AsSpan(0, this.sysexIndex));
                    this.sysexIndex = -1;
                }
            }
        }
        public abstract void Pause();
        public abstract void Resume();
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected abstract void PlayShortMessage(uint message);
        protected abstract void PlaySysex(ReadOnlySpan<byte> data);

        protected virtual void Dispose(bool disposing)
        {
        }
    }
}
