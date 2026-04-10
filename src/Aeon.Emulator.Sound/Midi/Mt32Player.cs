using System.IO.Compression;
using Mt32emu;

namespace Aeon.Emulator.Sound;

internal sealed class Mt32Player : IDisposable
{
    private readonly Mt32Context context = new();
    private bool disposed;

    public Mt32Player(string romsPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(romsPath);

        this.LoadRoms(romsPath);

        #warning sample rate
        int sampleRate = 44100;
        var analogMode = Mt32GlobalState.GetBestAnalogOutputMode(sampleRate);
        this.context.AnalogOutputMode = analogMode;
        this.context.SetSampleRate(sampleRate);

        this.context.OpenSynth();
    }

    public void PlayShortMessage(uint message) => this.context.PlayMessage(message);
    public void PlaySysex(ReadOnlySpan<byte> data) => this.context.PlaySysex(data);
    public void Dispose()
    {
        if (!this.disposed)
        {
            this.context.Dispose();
            this.disposed = true;
        }
    }

    private void FillBuffer(Span<float> buffer, out int samplesWritten)
    {
        try
        {
            this.context.Render(buffer);
            samplesWritten = buffer.Length;
        }
        catch (ObjectDisposedException)
        {
            buffer.Clear();
            samplesWritten = buffer.Length;
        }
    }
    private void LoadRoms(string path)
    {
        if (path.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
        {
            using var zip = new ZipArchive(File.OpenRead(path), ZipArchiveMode.Read);
            foreach (var entry in zip.Entries)
            {
                if (entry.FullName.EndsWith(".ROM", StringComparison.OrdinalIgnoreCase))
                {
                    using var stream = entry.Open();
                    this.context.AddRom(stream);
                }
            }
        }
        else if (Directory.Exists(path))
        {
            foreach (var fileName in Directory.EnumerateFiles(path, "*.ROM"))
                this.context.AddRom(fileName);
        }
    }
}
