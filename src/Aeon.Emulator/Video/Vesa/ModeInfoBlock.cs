using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Aeon.Emulator.Video.Vesa
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct ModeInfoBlock
    {
        public ModeAttributes ModeAttributes;
        public WindowAttributes WinAAtrributes;
        public WindowAttributes WinBAttributes;
        public ushort WinGranularity;
        public ushort WinSize;
        public ushort WinASegment;
        public ushort WinBSegment;
        public uint WinFuncPtr;
        public ushort BytesPerScanLine;

        public ushort XResolution;
        public ushort YResolution;
        public byte XCharSize;
        public byte YCharSize;
        public byte NumberOfPlanes;
        public byte BitsPerPixel;
        public byte NumberOfBanks;
        public MemoryModel MemoryModel;
        public byte BankSize;
        public byte NumberOfImagePages;
        public byte Reserved1;

        public byte RedMaskSize;
        public byte RedFieldPosition;
        public byte GreenMaskSize;
        public byte GreenFieldPosition;
        public byte BlueMaskSize;
        public byte BlueFieldPosition;
        public byte ReservedMaskSize;
        public byte ReservedFieldPosition;
        public byte DirectColorModeInfo;

        public uint PhysicalBasePointer;
        public uint OffscreenMemoryOffset;
        public ushort OffscreenMemorySize;
    }

    [Flags]
    internal enum ModeAttributes : ushort
    {
        None = 0,
        Supported = (1 << 0),
        Reserved1 = (1 << 1),
        BiosSupport = (1 << 2),
        Color = (1 << 3),
        Graphics = (1 << 4),
        VGACompatible = (1 << 5),
        VGACompatibleWindow = (1 << 6),
        LinearFrameBuffer = (1 << 7)
    }

    [Flags]
    internal enum WindowAttributes : byte
    {
        None = 0,
        Supported = (1 << 0),
        Readable = (1 << 1),
        Writeable = (1 << 2)
    }

    internal enum MemoryModel : byte
    {
        Text,
        CGA,
        Hercules,
        Planar4,
        PackedPixel,
        Unchained256,
        DirectColor,
        YUV
    }
}
