using System;

namespace Aeon.Emulator.Sound
{
    /// <summary>
    /// Provides access to the Windows MIDI mapper.
    /// </summary>
    internal sealed class WindowsMidiMapper : MidiDevice
    {
        private IntPtr midiOutHandle;

        public WindowsMidiMapper()
        {
            _ = NativeMethods.midiOutOpen(out this.midiOutHandle, NativeMethods.MIDI_MAPPER, IntPtr.Zero, IntPtr.Zero, 0);
        }
        ~WindowsMidiMapper() => this.Dispose(false);

        protected override void PlayShortMessage(uint message) => _ = NativeMethods.midiOutShortMsg(this.midiOutHandle, message);
        protected override void PlaySysex(ReadOnlySpan<byte> data) { }
        public override void Pause() => _ = NativeMethods.midiOutReset(this.midiOutHandle);
        public override void Resume() { }

        protected override void Dispose(bool disposing)
        {
            if (midiOutHandle != IntPtr.Zero)
            {
                _ = NativeMethods.midiOutClose(midiOutHandle);
                midiOutHandle = IntPtr.Zero;
            }
        }
    }
}
