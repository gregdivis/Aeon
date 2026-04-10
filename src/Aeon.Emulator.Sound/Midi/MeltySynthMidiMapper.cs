using MeltySynth;

namespace Aeon.Emulator.Sound;

internal sealed class MeltySynthMidiMapper : MidiDevice
{
    private readonly Synthesizer synthesizer;

    public MeltySynthMidiMapper(string soundFontPath)
    {
        ArgumentException.ThrowIfNullOrEmpty(soundFontPath);

        #warning sample rate
        this.synthesizer = new Synthesizer(soundFontPath, 44100);
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
