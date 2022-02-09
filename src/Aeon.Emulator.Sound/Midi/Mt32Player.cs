using System;
using System.IO;
using System.IO.Compression;
using Mt32emu;
using TinyAudio;

namespace Aeon.Emulator.Sound
{
    internal sealed class Mt32Player : IDisposable
    {
        private readonly Mt32Context context = new();
        private readonly AudioPlayer audioPlayer = Audio.CreatePlayer(true);
        private bool disposed;

        public Mt32Player(string romsPath)
        {
            if (string.IsNullOrWhiteSpace(romsPath))
                throw new ArgumentNullException(nameof(romsPath));

            this.LoadRoms(romsPath);

            var analogMode = Mt32GlobalState.GetBestAnalogOutputMode(this.audioPlayer.Format.SampleRate);
            this.context.AnalogOutputMode = analogMode;
            this.context.SetSampleRate(this.audioPlayer.Format.SampleRate);

            this.context.OpenSynth();
            this.audioPlayer.BeginPlayback(this.FillBuffer);
        }

        public void PlayShortMessage(uint message) => this.context.PlayMessage(message);
        public void PlaySysex(ReadOnlySpan<byte> data) => this.context.PlaySysex(data);
        public void Pause() => this.audioPlayer.StopPlayback();
        public void Resume() => this.audioPlayer.BeginPlayback(this.FillBuffer);
        public void Dispose()
        {
            if (!this.disposed)
            {
                this.context.Dispose();
                this.audioPlayer.Dispose();
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
}
