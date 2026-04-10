using System.Runtime.Versioning;

namespace Aeon.Emulator.Sound.Midi;

/// <summary>
/// Provides access to the Windows MIDI mapper.
/// </summary>
[SupportedOSPlatform("windows")]
internal sealed class WindowsMidiMapper : MidiDevice
{
    private IntPtr midiOutHandle;

    public WindowsMidiMapper()
    {
        _ = WinMM.midiOutOpen(out this.midiOutHandle, WinMM.MIDI_MAPPER, IntPtr.Zero, IntPtr.Zero, 0);
    }
    ~WindowsMidiMapper() => this.Dispose(false);

    public override bool GeneratesWaveformData => false;

    protected override void PlayShortMessage(uint message) => _ = WinMM.midiOutShortMsg(this.midiOutHandle, message);
    protected override void PlaySysex(ReadOnlySpan<byte> data) { }
    public override void Pause() => _ = WinMM.midiOutReset(this.midiOutHandle);
    public override void Resume() { }
    public override void RenderAudio(Span<float> left, Span<float> right)
    {
        left.Clear();
        right.Clear();
    }

    protected override void Dispose(bool disposing)
    {
        if (midiOutHandle != 0)
        {
            _ = WinMM.midiOutClose(midiOutHandle);
            midiOutHandle = 0;
        }
    }
}
