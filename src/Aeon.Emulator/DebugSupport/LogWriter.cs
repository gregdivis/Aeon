using System;
using System.IO;
using System.IO.Compression;

namespace Aeon.Emulator.DebugSupport
{
    internal sealed class LogWriter : IDisposable
    {
        private readonly ZipArchive zip;
        private int currentIndex;
        private Stream currentStream;

        public LogWriter(Stream stream)
        {
            this.zip = new ZipArchive(stream, ZipArchiveMode.Create);
            this.OpenNextFile();
        }

        public int EntrySize { get; }
        public int EntriesPerFile => 100000;

        public static LogWriter Create(string fileName) => new LogWriter(File.Create(fileName));

        public void Write(ReadOnlySpan<byte> data1, ReadOnlySpan<byte> data2)
        {
            this.currentStream.Write(data1);
            this.currentStream.Write(data2);
            this.currentIndex++;
            if ((this.currentIndex % this.EntriesPerFile) == 0)
            {
                this.currentStream.Dispose();
                this.OpenNextFile();
            }
        }

        public void Dispose()
        {
            this.currentStream.Dispose();
            this.zip.Dispose();
        }

        private void OpenNextFile()
        {
            var entry = this.zip.CreateEntry(this.currentIndex.ToString(), CompressionLevel.Fastest);
            this.currentStream = new BufferedStream(entry.Open(), 32 * 1024);
        }
    }
}
