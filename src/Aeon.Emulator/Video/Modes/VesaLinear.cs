using System;

namespace Aeon.Emulator.Video.Modes
{
    /// <summary>
    /// A linear VESA video mode.
    /// </summary>
    internal sealed class VesaLinear : VideoMode
    {
        public static readonly uint BaseAddress = PhysicalMemory.ConvMemorySize + 65536 + 0x4000;

        public VesaLinear(int width, int height, int bpp, VideoHandler video)
            : base(width, height, bpp, false, 16, VideoModeType.Graphics, video)
        {
        }

        internal override byte GetVramByte(uint offset) => throw new NotImplementedException();
        internal override void SetVramByte(uint offset, byte value) => throw new NotImplementedException();
        internal override ushort GetVramWord(uint offset) => throw new NotImplementedException();
        internal override void SetVramWord(uint offset, ushort value) => throw new NotImplementedException();
        internal override uint GetVramDWord(uint offset) => throw new NotImplementedException();
        internal override void SetVramDWord(uint offset, uint value) => throw new NotImplementedException();
        internal override void WriteCharacter(int x, int y, int index, byte foreground, byte background) => throw new NotImplementedException();
        internal override IntPtr GetVideoRamPointer(VideoHandler video) => video.VirtualMachine.PhysicalMemory.GetPointer((int)BaseAddress);
    }
}
