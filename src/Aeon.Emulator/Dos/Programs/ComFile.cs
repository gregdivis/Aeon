using System;
using System.IO;
using System.Runtime.InteropServices;
using Aeon.Emulator.Dos.VirtualFileSystem;

#nullable disable

namespace Aeon.Emulator.Dos.Programs
{
    internal sealed class ComFile : ProgramImage
    {
        public static readonly VirtualPath CommandPath = new("COMMAND.COM");

        private byte[] imageData;

        public ComFile(VirtualPath path, Stream stream)
            : base(path, stream)
        {
        }

        internal override ushort MaximumParagraphs => 0xFFFF;

        internal override void Load(VirtualMachine vm, ushort dataSegment)
        {
            ArgumentNullException.ThrowIfNull(vm);

            vm.WriteSegmentRegister(SegmentIndex.CS, dataSegment);
            vm.WriteSegmentRegister(SegmentIndex.DS, dataSegment);
            vm.WriteSegmentRegister(SegmentIndex.ES, dataSegment);
            vm.WriteSegmentRegister(SegmentIndex.SS, dataSegment);
            vm.Processor.SP = 0xFFFE;
            vm.Processor.DI = 0xFFFE;
            vm.Processor.BP = 0x091C;
            vm.Processor.IP = 0x0100;
            vm.Processor.SI = 0x0100;
            vm.Processor.AX = 0;
            vm.Processor.BX = 0;
            vm.Processor.CX = 0x00FF;
            vm.Processor.DX = (short)dataSegment;

            var ptr = vm.PhysicalMemory.GetPointer(vm.Processor.CS, vm.Processor.IP);
            Marshal.Copy(imageData, 0, ptr, imageData.Length);
        }
        internal override void LoadOverlay(VirtualMachine vm, ushort overlaySegment, int relocationFactor) => throw new NotSupportedException();
        internal override void Read(Stream stream)
        {
            int length = (int)stream.Length;
            imageData = new byte[length];
            stream.ReadExactly(imageData);
        }
    }
}
