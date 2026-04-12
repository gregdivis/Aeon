using MeltySynth;

namespace Aeon.Emulator.Sound.Midi;

internal sealed class MeltySynthMidiMapper : MidiDevice
{
    private readonly Synthesizer synthesizer;

    public MeltySynthMidiMapper(string soundFontPath, int sampleRate)
    {
        ArgumentException.ThrowIfNullOrEmpty(soundFontPath);
        this.synthesizer = new Synthesizer(soundFontPath, sampleRate);
    }

    public override void Pause()
    {
    }
    public override void Resume()
    {
    }
    public override void RenderAudio(Span<float> left, Span<float> right) => this.synthesizer.Render(left, right);

    protected override void PlayShortMessage(uint message)
    {
        this.synthesizer.ProcessMidiMessage((int)message & 0xF, (int)message & 0xF0, (byte)(message >>> 8), (byte)(message >>> 16));
    }
    protected override void PlaySysex(ReadOnlySpan<byte> data)
    {
    }
}
