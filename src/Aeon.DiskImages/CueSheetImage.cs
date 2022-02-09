using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Aeon.DiskImages.Iso9660;
using Aeon.Emulator;
using Aeon.Emulator.Dos;
using Aeon.Emulator.Dos.VirtualFileSystem;
using TinyAudio;

namespace Aeon.DiskImages
{
    public sealed class CueSheetImage : IMappedDrive, IAudioCD
    {
        private const int RawSectorSize = 2352;
        private static readonly Regex FileRegex = new(@"^FILE\s+""(?<1>[^""]+)""\s+BINARY$", RegexOptions.ExplicitCapture);
        private static readonly Regex TrackRegex = new(@"^\s+TRACK\s+(?<1>[0-9]+)\s+(?<2>.+)$", RegexOptions.ExplicitCapture);
        private static readonly Regex IndexRegex = new(@"^\s+INDEX\s+(?<1>[0-9]+)\s+(?<2>.+)$", RegexOptions.ExplicitCapture);
        private static readonly Regex GapRegex = new(@"^\s+(?<1>PRE|POST)GAP\s+(?<2>.+)$", RegexOptions.ExplicitCapture);

        private readonly Iso9660Disc disc;
        private readonly string fileImagePath;
        private AudioTrackPlayer player;

        private bool disposed;

        public CueSheetImage(string fileName)
        {
            using var reader = File.OpenText(fileName);
            var line = reader.ReadLine();
            if (line == null || FileRegex.Match(line) is not Match m)
                throw new ArgumentException($"{fileName} is not a valid cue sheet.");

            var binFileName = m.Groups[1].Value;

            var tracks = new List<TrackInfo>();
            var indexes = new List<TrackIndex>();

            int? currentTrackIndex = null;
            TrackFormat? currentTrackType = null;
            CDTimeSpan preGap = default;
            CDTimeSpan postGap = default;

            while ((line = reader.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var match = TrackRegex.Match(line);
                if (match.Success)
                {
                    if (currentTrackIndex.HasValue)
                    {
                        tracks.Add(new TrackInfo(currentTrackType.GetValueOrDefault(), indexes.ToArray(), preGap, postGap));
                        indexes.Clear();
                        preGap = default;
                        postGap = default;
                    }

                    currentTrackIndex = int.Parse(match.Groups[1].ValueSpan);
                    currentTrackType = ParseFormat(match.Groups[2].Value);
                    continue;
                }

                match = GapRegex.Match(line);
                if (match.Success)
                {
                    var pos = CDTimeSpan.Parse(match.Groups[2].ValueSpan);
                    if (match.Groups[1].Value == "PRE")
                        preGap = pos;
                    else
                        postGap = pos;

                    continue;
                }

                match = IndexRegex.Match(line);
                if (match.Success)
                {
                    int indexNumber = int.Parse(match.Groups[1].ValueSpan);
                    var indexPosition = CDTimeSpan.Parse(match.Groups[2].ValueSpan);
                    indexes.Add(new TrackIndex(indexNumber, indexPosition));
                }
            }

            if (currentTrackIndex.HasValue)
                tracks.Add(new TrackInfo(currentTrackType.GetValueOrDefault(), indexes.ToArray(), preGap, postGap));

            this.Tracks = Array.AsReadOnly(tracks.ToArray());

            this.fileImagePath = Path.Combine(Path.GetDirectoryName(fileName), binFileName);

            if (tracks.Count < 1)
                throw new InvalidOperationException("Cuesheet has no tracks.");

            this.TotalSectors = (int)(new FileInfo(this.fileImagePath).Length / RawSectorSize);

            if (tracks[0].Format == TrackFormat.Mode1)
            {
                var fileStream = new FileStream(this.fileImagePath, new FileStreamOptions { Options = FileOptions.RandomAccess, Access = FileAccess.Read });
                try
                {
                    int dataSectors;
                    if (tracks.Count > 1)
                        dataSectors = tracks[1].Indexes[0].Position.TotalSectors;
                    else
                        dataSectors = (int)(fileStream.Length / RawSectorSize);

                    this.disc = new Iso9660Disc(new Mode1Stream(fileStream, dataSectors));
                }
                catch
                {
                    fileStream?.Dispose();
                    throw;
                }
            }
        }

        public string VolumeLabel => this.disc?.PrimaryVolumeDescriptor.VolumeIdentifier ?? string.Empty;

        long IMappedDrive.FreeSpace => 0;

        public IReadOnlyList<TrackInfo> Tracks { get; }

        public int PlaybackSector
        {
            get => (int)((this.player?.Position ?? 0) / RawSectorSize);
            set
            {
                this.EnsureAudioPlayer();
                this.player.Position = value * RawSectorSize;
            }
        }

        public bool Playing => this.player?.Playing ?? false;

        public int TotalSectors { get; }

        public void Dispose()
        {
            if (!this.disposed)
            {
                this.player?.Dispose();
                this.disc?.Dispose();
                this.disposed = true;
            }
        }

        public ErrorCodeResult<Stream> OpenRead(VirtualPath path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            var entry = this.disc?.GetDirectoryEntry(path.Elements);
            if (entry == null)
                return ExtendedErrorCode.FileNotFound;

            return this.disc.Open(entry);
        }
        public ErrorCodeResult<IEnumerable<VirtualFileInfo>> GetDirectory(VirtualPath path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            var entry = this.disc?.GetDirectoryEntry(path.Elements);
            if (entry == null)
                return ExtendedErrorCode.PathNotFound;

            return new ErrorCodeResult<IEnumerable<VirtualFileInfo>>(entry.Children);
        }
        public ErrorCodeResult<VirtualFileInfo> GetFileInfo(VirtualPath path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            var info = this.disc?.GetDirectoryEntry(path.Elements);
            if (info != null)
                return info;

            return ExtendedErrorCode.FileNotFound;
        }

        private static TrackFormat ParseFormat(string s)
        {
            return s switch
            {
                "MODE1" or "MODE1/2352" => TrackFormat.Mode1,
                "AUDIO" => TrackFormat.Audio,
                _ => throw new NotSupportedException($"Track format {s} not supported.")
            };
        }

        private void EnsureAudioPlayer()
        {
            if (this.player == null)
            {
                this.player = new AudioTrackPlayer(this.fileImagePath)
                {
                    Position = this.PlaybackSector * RawSectorSize
                };
            }
        }

        public void Play(int? sectors = null)
        {
            this.EnsureAudioPlayer();
            this.player.StopPosition = sectors.HasValue ? (this.PlaybackSector + sectors.GetValueOrDefault()) * 2352 : -1;
            this.player.Start();
        }
        public void Stop()
        {
            this.player?.Stop();
        }

        private sealed class Mode1Stream : Stream
        {
            private const int DataSectorSize = 2048;
            private const int HeaderSize = 16;
            private const long StartOffset = 16 * RawSectorSize;
            private readonly Stream baseStream;
            private readonly int sectors;
            private int currentSector;
            private int currentOffset;

            public Mode1Stream(Stream baseStream, int sectors)
            {
                this.baseStream = baseStream;
                this.sectors = sectors;
            }

            public override bool CanRead => true;
            public override bool CanSeek => true;
            public override bool CanWrite => false;
            public override long Length => this.sectors * DataSectorSize;
            public override long Position
            {
                get => (this.currentSector * DataSectorSize) + this.currentOffset;
                set
                {
                    var (sector, offset) = Math.DivRem(value, DataSectorSize);
                    this.currentSector = (int)sector;
                    this.currentOffset = (int)offset;
                }
            }
            public override int Read(Span<byte> buffer)
            {
                if (buffer.IsEmpty)
                    return 0;

                int totalBytesRead = 0;
                var currentBuffer = buffer;

                while (!currentBuffer.IsEmpty)
                {
                    int bytesRead = this.ReadInternal(currentBuffer);
                    if (bytesRead == 0)
                        break;
                    totalBytesRead += bytesRead;
                    currentBuffer = currentBuffer[bytesRead..];
                }

                return totalBytesRead;
            }
            public override int Read(byte[] buffer, int offset, int count) => this.Read(buffer.AsSpan(offset, count));
            public override int ReadByte()
            {
                Span<byte> oneByte = stackalloc byte[1];
                return this.Read(oneByte) == 1 ? oneByte[0] : -1;
            }
            public override long Seek(long offset, SeekOrigin origin)
            {
                return this.Position = origin switch
                {
                    SeekOrigin.Begin => offset,
                    SeekOrigin.Current => this.Position + offset,
                    SeekOrigin.End => this.Length + offset,
                    _ => throw new ArgumentOutOfRangeException(nameof(origin))
                };
            }
            public override void SetLength(long value) => throw new NotSupportedException();
            public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
            public override void Flush()
            {
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                    this.baseStream.Dispose();

                base.Dispose(disposing);
            }

            private int ReadInternal(Span<byte> buffer)
            {
                int bytesToRead = Math.Min(buffer.Length, DataSectorSize - this.currentOffset);
                if (bytesToRead == 0)
                {
                    if (this.currentSector >= this.sectors - 1)
                        return 0;

                    this.currentSector++;
                    this.currentOffset = 0;
                }

                this.baseStream.Position = (this.currentSector * RawSectorSize) + this.currentOffset + HeaderSize;
                int n = this.baseStream.Read(buffer[..bytesToRead]);
                this.currentOffset += n;
                if (this.currentOffset >= DataSectorSize)
                {
                    this.currentSector++;
                    this.currentOffset = 0;
                }

                return n;
            }
        }

        private sealed class AudioTrackPlayer : IDisposable
        {
            private const double BufferSeconds = 0.1;
            private const int SourceRate = 44100;
            private readonly AudioPlayer audioPlayer = WasapiAudioPlayer.Create(TimeSpan.FromSeconds(BufferSeconds));
            private readonly Stream audioStream;
            private readonly SemaphoreSlim syncLock = new(1, 1);
            private readonly Stopwatch playbackTimer = new();
            private CancellationTokenSource stopTokenSource = new();
            private Task readTask;
            private int sectorsRead;
            private volatile bool playing;
            private bool disposed;

            public AudioTrackPlayer(string fileName)
            {
                this.audioStream = File.Open(fileName, new FileStreamOptions { Options = FileOptions.Asynchronous });
            }

            public bool Playing => this.playing;

            public long Position
            {
                get => this.sectorsRead * RawSectorSize;
                set
                {
                    this.syncLock.Wait();
                    try
                    {
                        this.audioStream.Position = value;
                        this.sectorsRead = (int)(value % RawSectorSize);
                    }
                    finally
                    {
                        this.syncLock.Release();
                    }
                }
            }
            public long StopPosition { get; set; }

            public void Start()
            {
                if (this.playing)
                    return;

                this.playing = true;
                this.playbackTimer.Start();
                this.readTask = Task.Run(this.ReadAndResampleAsync);
                this.audioPlayer.BeginPlayback();
            }
            public void Stop()
            {
                if (!this.playing)
                    return;

                this.playing = false;
                this.audioPlayer.StopPlayback();
                this.stopTokenSource.Cancel();
                this.readTask.Wait();
                this.stopTokenSource.Dispose();
                this.stopTokenSource = new CancellationTokenSource();
            }

            public void Dispose()
            {
                if (!this.disposed)
                {
                    this.Stop();
                    this.audioPlayer.Dispose();
                    this.audioStream.Dispose();
                    this.stopTokenSource.Dispose();
                    this.disposed = true;
                }
            }

            private async Task ReadAndResampleAsync()
            {
                try
                {
                    var tempBuffer = new byte[2352];
                    int minTargetSize = (int)((double)this.audioPlayer.Format.SampleRate / SourceRate * tempBuffer.Length) + 16;
                    var resampleBuffer = new byte[minTargetSize];

                    while (this.playing)
                    {
                        int bytesRead = 0;
                        while (bytesRead < tempBuffer.Length)
                        {
                            await this.syncLock.WaitAsync().ConfigureAwait(false);
                            try
                            {
                                int n = await this.audioStream.ReadAsync(tempBuffer.AsMemory(bytesRead, tempBuffer.Length - bytesRead), this.stopTokenSource.Token).ConfigureAwait(false);
                                bytesRead += n;
                                if (n == 0)
                                    break;
                            }
                            finally
                            {
                                this.syncLock.Release();
                            }
                        }

                        if (bytesRead > 0)
                        {
                            this.sectorsRead++;

                            int sampleCount = Resample16Stereo(
                                SourceRate,
                                this.audioPlayer.Format.SampleRate,
                                MemoryMarshal.Cast<byte, short>(tempBuffer.AsSpan(0, bytesRead)),
                                MemoryMarshal.Cast<byte, short>(resampleBuffer)
                            );

                            await this.audioPlayer.WriteDataRawAsync<short>(resampleBuffer.AsMemory(0, sampleCount * 2), this.stopTokenSource.Token).ConfigureAwait(false);
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                }
            }

            private static int Resample16Stereo(int sourceRate, int destRate, ReadOnlySpan<short> source, Span<short> dest)
            {
                double src2Dest = (double)destRate / (double)sourceRate;
                double dest2Src = (double)sourceRate / (double)destRate;

                int length = (int)(src2Dest * source.Length) / 2;

                for (int i = 0; i < length; i++)
                {
                    int srcIndex = (int)(i * dest2Src) << 1;

                    var value1Left = source[srcIndex];
                    var value1Right = source[srcIndex + 1];
                    if (srcIndex < source.Length - 3)
                    {
                        var remainder = (i * dest2Src) % 1;
                        var value2Left = source[srcIndex + 2];
                        var value2Right = source[srcIndex + 3];

                        dest[i << 1] = Interpolate(value1Left, value2Left, remainder);
                        dest[(i << 1) + 1] = Interpolate(value1Right, value2Right, remainder);
                    }
                    else
                    {
                        dest[i << 1] = value1Left;
                        dest[(i << 1) + 1] = value1Right;
                    }
                }

                return length * 2;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static short Interpolate(short a, short b, double factor) => (short)(((b - a) * factor) + a);
        }
    }
}
