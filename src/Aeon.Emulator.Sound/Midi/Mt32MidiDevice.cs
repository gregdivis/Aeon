namespace Aeon.Emulator.Sound;

internal sealed class Mt32MidiDevice : MidiDevice
{
    private readonly Lazy<Mt32Player> player;
    private bool disposed;

    public Mt32MidiDevice(string romsPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(romsPath);

        this.player = new Lazy<Mt32Player>(() => new(romsPath));
    }

    public override void Pause()
    {
    }

    public override void Resume()
    {
    }

    protected override void PlayShortMessage(uint message) => this.player.Value.PlayShortMessage(message);
    protected override void PlaySysex(ReadOnlySpan<byte> data) => this.player.Value.PlaySysex(data);

    protected override void Dispose(bool disposing)
    {
        if (!this.disposed)
        {
            if (disposing && this.player.IsValueCreated)
                this.player.Value.Dispose();

            this.disposed = true;
        }
    }

    public override void RenderAudio(Span<float> left, Span<float> right)
    {
        #warning implement
        throw new NotImplementedException();
    }
}
