using System.Buffers;
using System.IO.Compression;
using Mt32emu;

namespace Aeon.Emulator.Sound.Midi;

internal sealed class Mt32Player : IDisposable
{
    private readonly Mt32Context context = new();
    private bool disposed;

    public Mt32Player(string romsPath, int sampleRate)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(romsPath);

        this.LoadRoms(romsPath);

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
    public void FillBuffer(Span<float> left, Span<float> right)
    {
        var buffer = ArrayPool<float>.Shared.Rent(left.Length * 2);
        try
        {
            var span = buffer.AsSpan(0, left.Length);
            this.context.Render(span);
            for (int i = 0; i < span.Length; i += 2)
            {
                left[i / 2] = span[i];
                right[i / 2] = span[i + 1];
            }
        }
        catch (ObjectDisposedException)
        {
            left.Clear();
            right.Clear();
        }
        finally
        {
            ArrayPool<float>.Shared.Return(buffer);
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
