using System.Runtime.CompilerServices;

namespace Aeon.Emulator.Video.Modes
{
    /// <summary>
    /// Implements functionality for 4-plane video modes.
    /// </summary>
    internal abstract class Planar4 : VideoMode
    {
        private readonly unsafe uint* videoRam;
        private uint latches;
        private readonly Graphics graphics;
        private readonly Sequencer sequencer;

        public Planar4(int width, int height, int bpp, int fontHeight, VideoModeType modeType, VideoHandler video)
            : base(width, height, bpp, true, fontHeight, modeType, video)
        {
            unsafe
            {
                this.videoRam = (uint*)video.VideoRam.ToPointer();
            }

            this.graphics = video.Graphics;
            this.sequencer = video.Sequencer;
        }

        internal override byte GetVramByte(uint offset)
        {
            offset %= 65536u;

            unsafe
            {
                this.latches = this.videoRam[offset];

                if ((graphics.GraphicsMode & (1 << 3)) == 0)
                {
                    return ReadByte(this.latches, graphics.ReadMapSelect & 0x3u);
                }
                else
                {
                    uint colorDontCare = graphics.ColorDontCare.Expanded;
                    uint colorCompare = graphics.ColorCompare * 0x01010101u;
                    uint results = Intrinsics.AndNot(colorDontCare, this.latches ^ colorCompare);
                    byte* bytes = (byte*)&results;
                    return (byte)(bytes[0] | bytes[1] | bytes[2] | bytes[3]);
                }
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        internal override void SetVramByte(uint offset, byte value)
        {
            offset %= 65536u;

            uint writeMode = this.graphics.GraphicsMode & 0x3u;
            if (writeMode == 0)
            {
                this.SetByteMode0(offset, value);
            }
            else if (writeMode == 1)
            {
                // when mapMask = 0 keep value in vram
                // whem mapMask = 1 take value from latches
                // input value is not used at all

                uint mapMask = this.sequencer.MapMask.Expanded;

                unsafe
                {
                    uint current = Intrinsics.AndNot(this.videoRam[offset], mapMask); // read value and clear mask bits
                    current |= this.latches & mapMask; // set latch bits
                    this.videoRam[offset] = current;
                }
            }
            else if (writeMode == 2)
            {
                this.SetByteMode2(offset, value);
            }
            else
            {
                this.SetByteMode3(offset, value);
            }
        }
        internal override ushort GetVramWord(uint offset)
        {
            return (ushort)(this.GetVramByte(offset) | (this.GetVramByte(offset + 1u) << 8));
        }
        internal override void SetVramWord(uint offset, ushort value)
        {
            this.SetVramByte(offset, (byte)value);
            this.SetVramByte(offset + 1u, (byte)(value >> 8));
        }
        internal override uint GetVramDWord(uint offset)
        {
            return (uint)(this.GetVramByte(offset) | (this.GetVramByte(offset + 1u) << 8) | (this.GetVramByte(offset + 2u) << 16) | (this.GetVramByte(offset + 3u) << 24));
        }
        internal override void SetVramDWord(uint offset, uint value)
        {
            this.SetVramByte(offset, (byte)value);
            this.SetVramByte(offset + 1u, (byte)(value >> 8));
            this.SetVramByte(offset + 2u, (byte)(value >> 16));
            this.SetVramByte(offset + 3u, (byte)(value >> 24));
        }
        internal override void WriteCharacter(int x, int y, int index, byte foreground, byte background)
        {
            unsafe
            {
                uint fg = new MaskValue(foreground).Expanded;
                //uint bg = VideoComponent.ExpandRegister(background);

                int stride = this.Stride;
                int startPos = y * stride * 16 + x;
                byte[] font = this.Font;

                for (int row = 0; row < 16; row++)
                {
                    uint fgMask = font[index * 16 + row] * 0x01010101u;
                    //uint bgMask = ~fgMask;
                    uint value = fg & fgMask;

                    if ((background & 0x08) == 0)
                        this.videoRam[startPos + (row * stride)] = value;
                    else
                        this.videoRam[startPos + (row * stride)] ^= value;
                }
            }
        }

        /// <summary>
        /// Writes a byte to video RAM using the rules for write mode 0.
        /// </summary>
        /// <param name="offset">Video RAM offset to write byte.</param>
        /// <param name="input">Byte to write to video RAM.</param>
        private void SetByteMode0(uint offset, byte input)
        {
            unsafe
            {
                if (graphics.DataRotate == 0)
                {
                    uint source = (uint)input * 0x01010101;
                    uint mask = (uint)graphics.BitMask * 0x01010101;

                    // when mapMask is set, use computed value; otherwise keep vram value
                    uint mapMask = this.sequencer.MapMask.Expanded;

                    uint original = this.videoRam[offset];

                    uint setResetEnabled = this.graphics.EnableSetReset.Expanded;
                    uint setReset = this.graphics.SetReset.Expanded;

                    source = Intrinsics.AndNot(source, setResetEnabled);
                    source |= setReset & setResetEnabled;
                    source &= mask;
                    source |= Intrinsics.AndNot(this.latches, mask);

                    this.videoRam[offset] = (source & mapMask) | Intrinsics.AndNot(original, mapMask);
                }
                else
                {
                    this.SetByteMode0_Extended(offset, input);
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
                uint source = (uint)input * 0x01010101;
                uint mask = (uint)graphics.BitMask * 0x01010101;

                // when mapMask is set, use computed value; otherwise keep vram value
                uint mapMask = sequencer.MapMask.Expanded;
                uint original = this.videoRam[offset];

                uint setResetEnabled = this.graphics.EnableSetReset.Expanded;
                uint setReset = this.graphics.SetReset.Expanded;

                source = Intrinsics.AndNot(source, setResetEnabled);
                source |= setReset & setResetEnabled;

                int rotateCount = graphics.DataRotate & 0x07;
                source = RotateBytes(source, rotateCount);

                uint logicalOp = Intrinsics.ExtractBits(graphics.DataRotate, 3, 2, 0b11000);

                if (logicalOp == 0)
                {
                }
                else if (logicalOp == 1)
                {
                    source &= this.latches;
                }
                else if (logicalOp == 2)
                {
                    source |= this.latches;
                }
                else
                {
                    source ^= this.latches;
                }

                source &= mask;
                source |= Intrinsics.AndNot(this.latches, mask);

                this.videoRam[offset] = (source & mapMask) | Intrinsics.AndNot(original, mapMask);
            }

        }
        /// <summary>
        /// Writes a byte to video RAM using the rules for write mode 2.
        /// </summary>
        /// <param name="offset">Video RAM offset to write byte.</param>
        /// <param name="input">Byte to write to video RAM.</param>
        private void SetByteMode2(uint offset, byte input)
        {
            unsafe
            {
                uint values = new MaskValue(input).Expanded;

                uint logicalOp = Intrinsics.ExtractBits(graphics.DataRotate, 3, 2, 0b11000);

                if (logicalOp == 0)
                {
                }
                else if (logicalOp == 1)
                {
                    values &= this.latches;
                }
                else if (logicalOp == 2)
                {
                    values |= this.latches;
                }
                else
                {
                    values ^= this.latches;
                }

                uint mask = (uint)graphics.BitMask * 0x01010101;

                values &= mask;
                values |= Intrinsics.AndNot(this.latches, mask);

                // when mapMask = 0 keep value in vram
                // whem mapMask = 1 take value from latches
                // input value is not used at all

                uint mapMask = this.sequencer.MapMask.Expanded;
                unsafe
                {
                    uint current = Intrinsics.AndNot(this.videoRam[offset], mapMask); // read value and clear mask bits
                    current |= values & mapMask; // set value bits
                    this.videoRam[offset] = current;
                }
            }
        }
        private void SetByteMode3(uint offset, byte input)
        {
            unsafe
            {
                int rotateCount = graphics.DataRotate & 0x07;
                uint source = (byte)(((uint)input >> rotateCount) | ((uint)input << (8 - rotateCount)));
                source &= graphics.BitMask;
                source *= 0x01010101;

                uint result = source & this.graphics.SetReset.Expanded;
                result |= Intrinsics.AndNot(this.latches, source);

                this.videoRam[offset] = result;
            }
        }
        private static uint RotateBytes(uint value, int count)
        {
            unsafe
            {
                byte* v = (byte*)&value;
                int count2 = 8 - count;
                v[0] = (byte)((v[0] >> count) | (v[0] << count2));
                v[1] = (byte)((v[1] >> count) | (v[1] << count2));
                v[2] = (byte)((v[2] >> count) | (v[2] << count2));
                v[3] = (byte)((v[3] >> count) | (v[3] << count2));
                return value;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static byte ReadByte(uint value, uint index)
        {
            return (byte)Intrinsics.ExtractBits(value, (byte)(index * 8u), 8, 0xFFu << ((int)index * 8));
        }
    }
}
