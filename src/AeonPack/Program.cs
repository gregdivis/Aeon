using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Aeon.DiskImages.Archives;
using Aeon.Emulator.Dos.VirtualFileSystem;
using Aeon.Emulator.Launcher.Configuration;

namespace Aeon.Pack
{
    public static class Program
    {
        private static readonly Regex Valid83PathRegex = new Regex(@"^[^\.]{1,8}(\.[^\.]{0,3})?$");

        public static int Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Usage: aeonpack <AeonConfig> <PackageName>");
                return 1;
            }

            var archiveBuilder = new ArchiveBuilder();

            var config = AeonConfiguration.Load(args[0]);

            archiveBuilder.AddFile(GetArchiveConfigStream(config), "Archive.AeonConfig");

            int isoIndex = 1;

            foreach (var drive in config.Drives)
            {
                var driveConfig = drive.Value;
                if (!string.IsNullOrEmpty(driveConfig.HostPath))
                {
                    int hostPathLength = driveConfig.HostPath.Length;
                    if (!driveConfig.HostPath.EndsWith('\\') && !driveConfig.HostPath.EndsWith('/'))
                        hostPathLength++;

                    var drivePrefix = drive.Key.ToUpperInvariant() + ":\\";

                    foreach (var sourceFileName in Directory.EnumerateFiles(driveConfig.HostPath, "*", SearchOption.AllDirectories))
                    {
                        var destPath = getArchivePath(sourceFileName);
                        if (destPath != null)
                        {
                            Console.WriteLine($"Adding {sourceFileName} => {destPath}...");
                            archiveBuilder.AddFile(sourceFileName, destPath);
                        }
                    }

                    string getArchivePath(string srcPath)
                    {
                        var relativePath = srcPath.Substring(hostPathLength).Trim('\\', '/');
                        var pathParts = relativePath.Split(Path.DirectorySeparatorChar);
                        if (!pathParts.All(Valid83PathRegex.IsMatch))
                            return null;

                        return drivePrefix + relativePath.ToUpperInvariant();
                    }
                }
                else if (!string.IsNullOrWhiteSpace(drive.Value.ImagePath))
                {
                    archiveBuilder.AddFile(drive.Value.ImagePath, $"Image{isoIndex}.iso");
                    isoIndex++;
                }
                else
                {
                    throw new InvalidDataException();
                }
            }

            Console.WriteLine($"Writing {args[1]}...");
            using var outputStream = File.Create(args[1]);
            Console.CursorVisible = false;
            archiveBuilder.Write(outputStream, new BuilderProgress(archiveBuilder.DataCount));
            Console.CursorVisible = true;

            return 0;
        }

        private sealed class BuilderProgress : IArchiveBuilderProgress
        {
            private readonly object streamLock = new object();
            private readonly int dataCount;
            private int streamX;
            private int streamY;
            private long itemLength;

            public BuilderProgress(int dataCount) => this.dataCount = dataCount;

            public void ItemStart(int index, string name, long size)
            {
                Console.Write($"[{index + 1}/{this.dataCount}] {name}: ");
                this.streamX = Console.CursorLeft;
                this.streamY = Console.CursorTop;
                this.itemLength = size;
            }
            public void ItemDataProcessed(long completed)
            {
                lock (this.streamLock)
                {
                    Console.SetCursorPosition(this.streamX, this.streamY);
                    Console.Write($"{FormatSize(completed)}/{FormatSize(this.itemLength)}");
                }
            }
            public void ItemComplete(long outputSize, bool compressed)
            {
                lock (this.streamLock)
                {
                    Console.SetCursorPosition(this.streamX, this.streamY);
                    if (compressed)
                    {
                        int ratio = (int)Math.Round((double)outputSize / this.itemLength * 100);
                        Console.WriteLine($"{FormatSize(this.itemLength)} => {FormatSize(outputSize)} ({ratio}%)");
                    }
                    else
                    {
                        Console.WriteLine($"{FormatSize(outputSize)} (not compressed)");
                    }
                }
            }

            private static SizeUnit GetUnit(long size)
            {
                if (size < 4096)
                    return SizeUnit.Bytes;
                else if (size < 10 * 1024 * 1024)
                    return SizeUnit.Kilobytes;
                else
                    return SizeUnit.Megabytes;
            }
            private static string FormatSize(long size, SizeUnit unit, bool showUnit = true)
            {
                return unit switch
                {
                    SizeUnit.Bytes => size.ToString("G") + (showUnit ? " bytes" : string.Empty),
                    SizeUnit.Kilobytes => (size / 1024).ToString("G") + (showUnit ? " kb" : string.Empty),
                    _ => (size / (1024 * 1024)).ToString("G") + (showUnit ? " mb" : string.Empty)
                };
            }
            private static string FormatSize(long size, bool showUnit = true) => FormatSize(size, GetUnit(size), showUnit);

            private enum SizeUnit
            {
                Bytes,
                Kilobytes,
                Megabytes
            }
        }

        private static Stream GetArchiveConfigStream(AeonConfiguration config)
        {
            var serializer = new YamlDotNet.Serialization.Serializer();
            var deserializer = new YamlDotNet.Serialization.Deserializer();
            var config2 = deserializer.Deserialize<AeonConfiguration>(serializer.Serialize(config));
            int isoIndex = 1;
            foreach (var d in config2.Drives)
            {
                d.Value.HostPath = null;
                if (!string.IsNullOrWhiteSpace(d.Value.ImagePath))
                {
                    d.Value.ImagePath = $"Image{isoIndex}.iso";
                    isoIndex++;
                }
            }

            var stream = new MemoryStream();
            using (var writer = new StreamWriter(stream, new UTF8Encoding(false), leaveOpen: true))
            {
                serializer.Serialize(writer, config2);
            }

            stream.Position = 0;
            return stream;
        }
    }
}
