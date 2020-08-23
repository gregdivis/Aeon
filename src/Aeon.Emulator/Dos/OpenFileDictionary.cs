using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Aeon.Emulator.Dos
{
    /// <summary>
    /// Maintains a collection of all open handles.
    /// </summary>
    internal sealed class OpenFileDictionary
    {
        private int maxFiles;
        private int sftFileCount;
        private unsafe SFTHeader* header;
        private unsafe SFTEntry* entries;
        private readonly SortedList<short, DosStream> fileHandles = new SortedList<short, DosStream>();

        /// <summary>
        /// Gets the system file table address.
        /// </summary>
        public IntPtr SystemFileTableAddress
        {
            get
            {
                unsafe { return new IntPtr(this.header); }
            }
            private set
            {
                unsafe
                {
                    this.header = (SFTHeader*)value.ToPointer();
                    this.entries = (SFTEntry*)((byte*)value.ToPointer() + sizeof(SFTHeader));
                }
            }
        }
        /// <summary>
        /// Gets the next handle.
        /// </summary>
        public short NextHandle
        {
            get
            {
                for (int i = 3; i < short.MaxValue; i++)
                {
                    if (!this.fileHandles.ContainsKey((short)i))
                        return (short)i;
                }

                return 0;
            }
        }

        /// <summary>
        /// Initializes the open file dictionary.
        /// </summary>
        /// <param name="sftAddress">Pointer to the SFT in memory.</param>
        /// <param name="maxFiles">Maximum number of open files supported at once.</param>
        public void Initialize(IntPtr sftAddress, int maxFiles)
        {
            this.SystemFileTableAddress = sftAddress;
            this.maxFiles = maxFiles;
            this.sftFileCount = 0;

            unsafe
            {
                this.header->NextFileTable = 0xFFFF;
                this.header->NumberOfFiles = (ushort)maxFiles;
            }
        }

        public void AddSpecial(short handle, DosStream stream)
        {
            this.fileHandles.Add(handle, stream);
        }

        public void Add(short handle, DosStream stream, VirtualFileInfo fileInfo, bool overwrite = false)
        {
            stream.FileInfo = fileInfo;

            if (fileInfo != null)
            {
                unsafe
                {
                    if (stream.SFTIndex == -1)
                    {
                        if (this.sftFileCount >= this.maxFiles)
                            throw new InvalidOperationException("Maximum number of open files exceeded.");

                        stream.SFTIndex = this.sftFileCount;
                        var entry = &this.entries[this.sftFileCount];

                        this.sftFileCount++;

                        entry->RefCount = 1;
                        entry->FileOpenMode = 0;
                        entry->FileAttribute = fileInfo.DosAttributes;
                        entry->DeviceInfo = 0;
                        entry->StartingCluster = 0;
                        entry->FileTime = fileInfo.DosModifyTime;
                        entry->FileDate = fileInfo.DosModifyDate;
                        entry->FileSize = fileInfo.DosLength;
                        entry->CurrentOffset = 0;
                        entry->RelativeClusterLastAccess = 0;
                        entry->DirectoryEntrySector = 0;
                        entry->DirectoryEntry = 0;

                        var fcbName = GetFCBName(fileInfo.Name);
                        for (int i = 0; i < 11; i++)
                            entry->FileName[i] = (byte)fcbName[i];
                    }
                    else
                    {
                        var entry = &this.entries[stream.SFTIndex];
                        entry->RefCount++;
                    }
                }
            }

            if (!overwrite)
                this.fileHandles.Add(handle, stream);
            else
                this.fileHandles[handle] = stream;
        }
        public bool Remove(short handle)
        {
            if (!this.fileHandles.TryGetValue(handle, out var stream))
                return false;

            if (stream.SFTIndex >= 0)
            {
                unsafe
                {
                    var entry = &this.entries[stream.SFTIndex];
                    if (entry->RefCount > 1)
                        entry->RefCount--;
                    else
                    {
                        this.sftFileCount--;

                        for (int i = stream.SFTIndex; i < this.sftFileCount; i++)
                            this.entries[i] = this.entries[i + 1];
                    }
                }
            }

            this.fileHandles.Remove(handle);
            return true;
        }
        public void Clear()
        {
            this.fileHandles.Clear();
            unsafe
            {
                this.sftFileCount = 0;
            }
        }
        public void CloseAll()
        {
            foreach (var stream in this.fileHandles.Values)
                stream.Close();

            this.Clear();
        }
        /// <summary>
        /// Closes all file streams opened by a specified process.
        /// </summary>
        /// <param name="ownerId">ID of process whose files will be closed.</param>
        public void CloseAllFiles(int ownerId)
        {
            var files = from f in this.fileHandles
                        where f.Value.OwnerId == ownerId
                        select f;

            var fileList = new List<KeyValuePair<short, DosStream>>(files);
            foreach (var file in fileList)
            {
                file.Value.Close();
                this.Remove(file.Key);
            }
        }
        public bool TryGetValue(short handle, out DosStream stream)
        {
            return this.fileHandles.TryGetValue(handle, out stream);
        }
        public void Dispose()
        {
            foreach (var stream in this.fileHandles.Values)
                stream.Close();

            this.fileHandles.Clear();
        }

        private static string GetFCBName(string fileName)
        {
            var parts = fileName.Split('.');
            var buffer = new StringBuilder("           ");
            for (int i = 0; i < parts[0].Length; i++)
                buffer[i] = parts[0][i];

            if (parts.Length > 1)
            {
                for (int i = 0; i < parts[1].Length; i++)
                    buffer[i + 8] = parts[1][i];
            }

            return buffer.ToString();
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 6)]
    internal struct SFTHeader
    {
        public uint NextFileTable;
        public ushort NumberOfFiles;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0x3B)]
    internal struct SFTEntry
    {
        public ushort RefCount;
        public ushort FileOpenMode;
        public byte FileAttribute;
        public ushort DeviceInfo;
        public uint DeviceDriverHeader;
        public ushort StartingCluster;
        public ushort FileTime;
        public ushort FileDate;
        public uint FileSize;
        public uint CurrentOffset;
        public ushort RelativeClusterLastAccess;
        public uint DirectoryEntrySector;
        public byte DirectoryEntry;
        public unsafe fixed byte FileName[11];
    }
}
