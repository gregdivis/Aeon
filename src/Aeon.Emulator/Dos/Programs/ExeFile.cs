using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.InteropServices;
using Aeon.Emulator.Dos.VirtualFileSystem;
using Aeon.Emulator.Memory;

namespace Aeon.Emulator.Dos.Programs
{
    internal sealed class ExeFile : ProgramImage
    {
        private byte[] imageData;
        private readonly List<RealModeAddress> relocationTable = new List<RealModeAddress>();

        public ExeFile(VirtualPath path, Stream stream)
            : base(path, stream)
        {
        }

        /// <summary>
        /// Gets the minimum required memory specified by the executable header.
        /// </summary>
        public int MinRequiredMemory { get; private set; }
        /// <summary>
        /// Gets the maximum required memory specified by the executable header.
        /// </summary>
        public int MaxRequiredMemory { get; private set; }
        /// <summary>
        /// Gets the initial stack segment value specified by the executable header.
        /// </summary>
        public int StackSegmentOffset { get; private set; }
        /// <summary>
        /// Gets the initial stack pointer value specified by the executable header.
        /// </summary>
        public int InitialSP { get; private set; }
        /// <summary>
        /// Gets the checksum value specified by the executable header.
        /// </summary>
        public int Checksum { get; private set; }
        /// <summary>
        /// Gets the initial instruction pointer value specified by the executable header.
        /// </summary>
        public int InitialIP { get; private set; }
        /// <summary>
        /// Gets the initial code segment offset value specified by the executable header.
        /// </summary>
        public int CSOffset { get; private set; }
        /// <summary>
        /// Gets the image size specified by the executable header.
        /// </summary>
        public int ImageSize { get; private set; }
        /// <summary>
        /// Gets the overlay number of the executable header.
        /// </summary>
        public int Overlay { get; private set; }
        /// <summary>
        /// Gets the collection of relocation entries in the executable file header.
        /// </summary>
        public ReadOnlyCollection<RealModeAddress> RelocationEntries => relocationTable.AsReadOnly();

        internal override ushort MaximumParagraphs
        {
            get
            {
                int size = this.ImageSize / 16;
                if ((this.ImageSize % 16) != 0)
                    size++;

                return (ushort)Math.Min(size + MaxRequiredMemory, 0xFFFF);
            }
        }

        internal override void Load(VirtualMachine vm, ushort dataSegment)
        {
            if (vm == null)
                throw new ArgumentNullException(nameof(vm));

            ushort codeSegment = (ushort)(dataSegment + 0x10u);

            vm.WriteSegmentRegister(SegmentIndex.CS, (ushort)(this.CSOffset + codeSegment));
            vm.WriteSegmentRegister(SegmentIndex.SS, (ushort)(this.StackSegmentOffset + codeSegment));
            vm.WriteSegmentRegister(SegmentIndex.DS, dataSegment);
            vm.WriteSegmentRegister(SegmentIndex.ES, dataSegment);

            vm.Processor.IP = (ushort)this.InitialIP;
            vm.Processor.SP = (ushort)this.InitialSP;

            vm.Processor.AX = 0;
            vm.Processor.BX = 0;
            vm.Processor.SI = vm.Processor.IP;
            vm.Processor.DI = vm.Processor.SP;
            vm.Processor.BP = 0x091C;
            vm.Processor.CX = 0x00FF;
            vm.Processor.DX = (short)dataSegment;
            vm.Processor.Flags.Clear(~(EFlags.InterruptEnable | EFlags.Virtual8086Mode | EFlags.IOPrivilege1 | EFlags.IOPrivilege2 | EFlags.NestedTask));

            var ptr = vm.PhysicalMemory.GetPointer(codeSegment, 0);
            Marshal.Copy(imageData, 0, ptr, Math.Min(this.ImageSize, imageData.Length));

            foreach (var relocationEntry in this.RelocationEntries)
            {
                ushort value = vm.PhysicalMemory.GetUInt16((ushort)(codeSegment + relocationEntry.Segment), relocationEntry.Offset);
                vm.PhysicalMemory.SetUInt16((ushort)(codeSegment + relocationEntry.Segment), relocationEntry.Offset, (ushort)(value + codeSegment));
            }
        }
        internal override void LoadOverlay(VirtualMachine vm, ushort overlaySegment, int relocationFactor)
        {
            var ptr = vm.PhysicalMemory.GetPointer(overlaySegment, 0);
            Marshal.Copy(imageData, 0, ptr, Math.Min(this.ImageSize, imageData.Length));

            foreach (var relocationEntry in this.RelocationEntries)
            {
                ushort value = vm.PhysicalMemory.GetUInt16((ushort)(overlaySegment + relocationEntry.Segment), relocationEntry.Offset);
                vm.PhysicalMemory.SetUInt16((ushort)(overlaySegment + relocationEntry.Segment), relocationEntry.Offset, (ushort)(value + relocationFactor));
            }
        }
        internal override void Read(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream);
            if (reader.ReadUInt16() != 0x5A4D)
                throw new InvalidDataException();

            ushort imgSizeMod = reader.ReadUInt16();
            ushort imgSizePages = reader.ReadUInt16();

            if (imgSizeMod == 0)
                this.ImageSize = imgSizePages * 512 + imgSizeMod;
            else if (imgSizePages == 0)
                this.ImageSize = imgSizeMod;
            else
                this.ImageSize = (imgSizePages - 1) * 512 + imgSizeMod;

            ushort relocationEntries = reader.ReadUInt16();
            ushort headerParagraphs = reader.ReadUInt16();

            int headerSize = headerParagraphs * 16;

            this.MinRequiredMemory = reader.ReadUInt16();
            this.MaxRequiredMemory = reader.ReadUInt16();
            this.StackSegmentOffset = reader.ReadUInt16();
            this.InitialSP = reader.ReadUInt16();
            this.Checksum = reader.ReadUInt16();
            this.InitialIP = reader.ReadUInt16();
            this.CSOffset = reader.ReadUInt16();

            ushort relocationOffset = reader.ReadUInt16();
            this.Overlay = reader.ReadUInt16();

            stream.Position = relocationOffset;
            for (int i = 0; i < relocationEntries; i++)
            {
                ushort relOffset = reader.ReadUInt16();
                ushort relSegment = reader.ReadUInt16();
                relocationTable.Add(new RealModeAddress(relSegment, relOffset));
            }

            stream.Position = headerSize;
            imageData = reader.ReadBytes((int)(stream.Length - stream.Position));
        }
    }
}
