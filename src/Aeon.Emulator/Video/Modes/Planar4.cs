using System;
using System.Runtime.CompilerServices;

namespace Aeon.Emulator.Video.Modes
{
    /// <summary>
    /// Implements functionality for 4-plane video modes.
    /// </summary>
    internal abstract class Planar4 : VideoMode
    {
        private readonly UnsafeBuffer<nint> planesBuffer = new UnsafeBuffer<nint>(4);
        private readonly UnsafeBuffer<byte> latchesBuffer = new UnsafeBuffer<byte>(4);
        private readonly UnsafeBuffer<byte> expandedBuffer = new UnsafeBuffer<byte>(8);
        private readonly unsafe byte** planes;
        private readonly unsafe byte* latches;
        private readonly Graphics graphics;
        private readonly Sequencer sequencer;
        private readonly unsafe byte* expandedForeground;
        private readonly unsafe byte* expandedBackground;

        public Planar4(int width, int height, int bpp, int fontHeight, VideoModeType modeType, VideoHandler video)
            : base(width, height, bpp, true, fontHeight, modeType, video)
        {
            unsafe
            {
                this.planes = (byte**)this.planesBuffer.ToPointer();
                byte* vram = (byte*)video.VideoRam.ToPointer();
                this.planes[0] = vram + PlaneSize * 0;
                this.planes[1] = vram + PlaneSize * 1;
                this.planes[2] = vram + PlaneSize * 2;
                this.planes[3] = vram + PlaneSize * 3;
            }

            this.graphics = video.Graphics;
            this.sequencer = video.Sequencer;
            unsafe
            {
                this.latches = this.latchesBuffer.ToPointer();
                this.expandedForeground = this.latchesBuffer.ToPointer();
                this.expandedBackground = this.expandedForeground + 4;
            }
        }

        internal override byte GetVramByte(uint offset)
        {
            offset %= 65536u;

            unsafe
            {
                latches[0] = planes[0][offset];
                latches[1] = planes[1][offset];
                latches[2] = planes[2][offset];
                latches[3] = planes[3][offset];

                if ((graphics.GraphicsMode & (1 << 3)) == 0)
                {
                    return latches[graphics.ReadMapSelect & 0x3];
                }
                else
                {
                    int result1 = graphics.ExpandedColorDontCare[0] & ~(latches[0] ^ graphics.ExpandedColorCompare[0]);
                    int result2 = graphics.ExpandedColorDontCare[1] & ~(latches[1] ^ graphics.ExpandedColorCompare[1]);
                    int result3 = graphics.ExpandedColorDontCare[2] & ~(latches[2] ^ graphics.ExpandedColorCompare[2]);
                    int result4 = graphics.ExpandedColorDontCare[3] & ~(latches[3] ^ graphics.ExpandedColorCompare[3]);
                    return (byte)(result1 | result2 | result3 | result4);
                }
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        internal override void SetVramByte(uint offset, byte value)
        {
            offset %= 65536u;

            uint writeMode = graphics.GraphicsMode & 0x3u;
            if (writeMode == 0)
            {
                SetByteMode0(offset, value);
            }
            else if (writeMode == 1)
            {
                uint mapMask = sequencer.MapMask;
                unsafe
                {
                    if ((sequencer.MapMask & 0x01) != 0)
                        planes[0][offset] = latches[0];
                    if ((sequencer.MapMask & 0x02) != 0)
                        planes[1][offset] = latches[1];
                    if ((sequencer.MapMask & 0x04) != 0)
                        planes[2][offset] = latches[2];
                    if ((sequencer.MapMask & 0x08) != 0)
                        planes[3][offset] = latches[3];
                }
            }
            else
            {
                SetByteMode2(offset, value);
            }
        }
        internal override ushort GetVramWord(uint offset)
        {
            offset %= 65536u;

            uint latchOffset = offset + 1u;
            unsafe
            {
                latches[0] = planes[0][latchOffset];
                latches[1] = planes[1][latchOffset];
                latches[2] = planes[2][latchOffset];
                latches[3] = planes[3][latchOffset];
                return *(ushort*)(planes[graphics.ReadMapSelect & 0x3] + offset);
            }
        }
        internal override void SetVramWord(uint offset, ushort value)
        {
            SetVramByte(offset, (byte)value);
            SetVramByte(offset + 1u, (byte)(value >> 8));
        }
        internal override uint GetVramDWord(uint offset)
        {
            offset %= 65536u;

            uint latchOffset = offset + 3u;
            unsafe
            {
                latches[0] = planes[0][latchOffset];
                latches[1] = planes[1][latchOffset];
                latches[2] = planes[2][latchOffset];
                latches[3] = planes[3][latchOffset];
                return *(uint*)(planes[graphics.ReadMapSelect & 0x3] + offset);
            }
        }
        internal override void SetVramDWord(uint offset, uint value)
        {
            SetVramByte(offset, (byte)value);
            SetVramByte(offset + 1u, (byte)(value >> 8));
            SetVramByte(offset + 2u, (byte)(value >> 16));
            SetVramByte(offset + 3u, (byte)(value >> 24));
        }
        internal override void WriteCharacter(int x, int y, int index, byte foreground, byte background)
        {
            unsafe
            {
                VideoComponent.ExpandRegister(foreground, new Span<byte>(expandedForeground, 4));
                VideoComponent.ExpandRegister(background, new Span<byte>(expandedBackground, 4));

                int stride = this.Stride;
                int startPos = y * stride * 16 + x;
                byte[] font = this.Font;

                for (int row = 0; row < 16; row++)
                {
                    uint fgMask = font[index * 16 + row];
                    uint bgMask = ~fgMask;
                    uint value1 = expandedForeground[0] & fgMask;
                    uint value2 = expandedForeground[1] & fgMask;
                    uint value3 = expandedForeground[2] & fgMask;
                    uint value4 = expandedForeground[3] & fgMask;

                    if ((background & 0x08) == 0)
                    {
                        planes[0][startPos + row * stride] = (byte)value1;
                        planes[1][startPos + row * stride] = (byte)value2;
                        planes[2][startPos + row * stride] = (byte)value3;
                        planes[3][startPos + row * stride] = (byte)value4;
                    }
                    else
                    {
                        planes[0][startPos + row * stride] ^= (byte)value1;
                        planes[1][startPos + row * stride] ^= (byte)value2;
                        planes[2][startPos + row * stride] ^= (byte)value3;
                        planes[3][startPos + row * stride] ^= (byte)value4;
                    }
                }
            }
        }

        /// <summary>
        /// Writes a byte to video RAM using the rules for write mode 0.
        /// </summary>
        /// <param name="offset">Video RAM offset to write byte.</param>
        /// <param name="input">Byte to write to video RAM.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetByteMode0(uint offset, byte input)
        {
            unsafe
            {
                if (graphics.DataRotate == 0)
                {
                    byte mask = graphics.BitMask;

                    for (int p = 0; p < 4; p++)
                    {
                        if (sequencer.ExpandedMapMask[p])
                        {
                            byte source = input;

                            if (graphics.ExpandedEnableSetReset[p])
                                source = graphics.ExpandedSetReset[p];

                            source &= mask;
                            source |= (byte)(latches[p] & ~mask);

                            planes[p][offset] = source;
                        }
                    }
                }
                else
                {
                    SetByteMode0_Extended(offset, input);
                }
            }
        }
        /// <summary>
        /// Writes a byte to video RAM using the rules for write mode 0.
        /// </summary>
        /// <param name="offset">Video RAM offset to write byte.</param>
        /// <param name="input">Byte to write to video RAM.</param>
        /// <remarks>
        /// This method handles the uncommon case when DataRotate is not 0.
        /// </remarks>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void SetByteMode0_Extended(uint offset, byte input)
        {
            unsafe
            {
                byte source;
                int rotateCount = graphics.DataRotate & 0x07;
                int logicalOp = (graphics.DataRotate >> 3) & 0x03;

                for (int p = 0; p < 4; p++)
                {
                    int planeBit = 1 << p;
                    if ((sequencer.MapMask & planeBit) != 0)
                    {
                        if ((graphics.EnableSetReset & planeBit) != 0)
                            source = ((graphics.SetReset & planeBit) != 0) ? (byte)0xFF : (byte)0;
                        else
                            source = input;

                        uint a = (uint)(source >> rotateCount);
                        uint b = (uint)(source << (8 - rotateCount));
                        source = (byte)(a | b);
                        switch (logicalOp)
                        {
                            case 1:
                                source &= (byte)latches[p];
                                break;

                            case 2:
                                source |= (byte)latches[p];
                                break;

                            case 3:
                                source ^= (byte)latches[p];
                                break;
                        }

                        byte mask = graphics.BitMask;
                        source &= mask;
                        source |= (byte)(latches[p] & ~mask);

                        planes[p][offset] = source;
                    }
                }
            }
        }
        /// <summary>
        /// Writes a byte to video RAM using the rules for write mode 2.
        /// </summary>
        /// <param name="offset">Video RAM offset to write byte.</param>
        /// <param name="input">Byte to write to video RAM.</param>
        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        private void SetByteMode2(uint offset, byte input)
        {
            unsafe
            {
                var values = stackalloc byte[4];

                if ((input & 0x1) == 0x1)
                    values[0] = 0xFF;
                if ((input & 0x2) == 0x2)
                    values[1] = 0xFF;
                if ((input & 0x4) == 0x4)
                    values[2] = 0xFF;
                if ((input & 0x8) == 0x8)
                    values[3] = 0xFF;

                uint logicalOp = ((uint)graphics.DataRotate >> 3) & 0x03u;
                if (logicalOp == 0)
                {
                }
                else if (logicalOp == 1)
                {
                    *(uint*)values &= *(uint*)latches;
                }
                else if (logicalOp == 2)
                {
                    *(uint*)values |= *(uint*)latches;
                }
                else
                {
                    *(uint*)values ^= *(uint*)latches;
                }

                var mask = stackalloc byte[4];
                byte bm = graphics.BitMask;
                mask[0] = bm;
                mask[1] = bm;
                mask[2] = bm;
                mask[3] = bm;

                *(uint*)values &= *(uint*)mask;
                *(uint*)values |= *(uint*)latches & ~*(uint*)mask;

                byte mapMask = this.sequencer.MapMask;

                if ((mapMask & 0x01) == 0x01)
                    planes[0][offset] = values[0];
                if ((mapMask & 0x02) == 0x02)
                    planes[1][offset] = values[1];
                if ((mapMask & 0x04) == 0x04)
                    planes[2][offset] = values[2];
                if ((mapMask & 0x08) == 0x08)
                    planes[3][offset] = values[3];
            }
        }
    }
}
