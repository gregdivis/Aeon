using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Aeon.DiskImages.Archives;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aeon.Test
{
    [TestClass]
    public class Archives
    {
        [TestMethod]
        public void WriteChunkedStream()
        {
            using var srcStream = File.OpenRead(@"C:\DOS\32\DAGGER\ARENA2\MAPS.BSA");
            using var buffer = new MemoryStream();
            ChunkedCompressor.Compress(srcStream, buffer);
            //using (var writer = new ChunkedStreamWriter())
            //{
            //    srcStream.CopyTo(writer);
            //    writer.Save(buffer);
            //}

            buffer.Position = 0;
            using (var reader = new ChunkedStreamReader(buffer, srcStream.Length))
            {
                srcStream.Position = 0;
                Assert.IsTrue(StreamsEqual(srcStream, reader));
            }
        }

        [TestMethod]
        public void BuildArchive()
        {
            using var builder = new ArchiveBuilder();
            foreach (var fileName in Directory.EnumerateFiles(@"C:\DOS\16\KEEN4"))
                builder.AddFile(fileName, Path.GetFileName(fileName));

            using var outputStream = new MemoryStream();
            builder.Write(outputStream);

            outputStream.Position = 0;
            using var reader = new ArchiveFile(outputStream);
            foreach (var fileName in Directory.EnumerateFiles(@"C:\DOS\16\KEEN4"))
            {
                using (var f = File.OpenRead(fileName))
                using (var a = reader.OpenItem(Path.GetFileName(fileName)))
                {
                    Assert.IsTrue(StreamsEqual(f, a));
                }
            }
        }

        private static bool StreamsEqual(Stream stream1, Stream stream2)
        {
            int a = stream1.ReadByte();
            int b = stream2.ReadByte();

            while (a == b && a != -1 && b != -1)
            {
                a = stream1.ReadByte();
                b = stream2.ReadByte();
            }

            return a == b;
        }
    }
}
