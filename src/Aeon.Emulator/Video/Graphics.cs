using System;

namespace Aeon.Emulator.Video
{
    /// <summary>
    /// Emulates the VGA Graphics registers.
    /// </summary>
    internal sealed class Graphics : VideoComponent
    {
        public unsafe readonly byte* ExpandedColorCompare;
        public unsafe readonly byte* ExpandedColorDontCare;
        public unsafe readonly byte* ExpandedSetReset;
        public unsafe readonly bool* ExpandedEnableSetReset;

        /// <summary>
        /// Initializes a new instance of the Graphics class.
        /// </summary>
        /// <param name="heap">Heap used to allocate unmanaged memory.</param>
        public Graphics(NativeHeap heap)
        {
            unsafe
            {
                this.ExpandedColorCompare = (byte*)heap.Allocate(4, 1).ToPointer();
                this.ExpandedColorDontCare = (byte*)heap.Allocate(4, 1).ToPointer();
                this.ExpandedSetReset = (byte*)heap.Allocate(4, 1).ToPointer();
                this.ExpandedEnableSetReset = (bool*)heap.Allocate(4, 1).ToPointer();
            }
        }

        /// <summary>
        /// Gets or sets the Set/Reset register.
        /// </summary>
        public byte SetReset { get; private set; }
        /// <summary>
        /// Gets or sets the Enable Set/Reset register.
        /// </summary>
        public byte EnableSetReset { get; private set; }
        /// <summary>
        /// Gets or sets the Color Compare register.
        /// </summary>
        public byte ColorCompare { get; private set; }
        /// <summary>
        /// Gets or sets the Data Rotate register.
        /// </summary>
        public byte DataRotate { get; private set; }
        /// <summary>
        /// Gets or sets the Read Map Select register.
        /// </summary>
        public byte ReadMapSelect { get; private set; }
        /// <summary>
        /// Gets or sets the Graphics Mode register.
        /// </summary>
        public byte GraphicsMode { get; set; }
        /// <summary>
        /// Gets or sets the Miscellaneous Graphics register.
        /// </summary>
        public byte MiscellaneousGraphics { get; set; }
        /// <summary>
        /// Gets or sets the Color Don't Care register.
        /// </summary>
        public byte ColorDontCare { get; private set; }
        /// <summary>
        /// Gets or sets the Bit Mask register.
        /// </summary>
        public byte BitMask { get; set; }

        /// <summary>
        /// Returns the current value of a graphics register.
        /// </summary>
        /// <param name="address">Address of register to read.</param>
        /// <returns>Current value of the register.</returns>
        public byte ReadRegister(GraphicsRegister address)
        {
            switch (address)
            {
                case GraphicsRegister.SetReset:
                    return this.SetReset;

                case GraphicsRegister.EnableSetReset:
                    return this.EnableSetReset;

                case GraphicsRegister.ColorCompare:
                    return this.ColorCompare;

                case GraphicsRegister.DataRotate:
                    return this.DataRotate;

                case GraphicsRegister.ReadMapSelect:
                    return ReadMapSelect;

                case GraphicsRegister.GraphicsMode:
                    return this.GraphicsMode;

                case GraphicsRegister.MiscellaneousGraphics:
                    return this.MiscellaneousGraphics;

                case GraphicsRegister.ColorDontCare:
                    return this.ColorDontCare;

                case GraphicsRegister.BitMask:
                    return this.BitMask;

                default:
                    return 0;
            }
        }
        /// <summary>
        /// Writes to a graphics register.
        /// </summary>
        /// <param name="address">Address of register to write.</param>
        /// <param name="value">Value to write to register.</param>
        public void WriteRegister(GraphicsRegister address, byte value)
        {
            unsafe
            {
                switch (address)
                {
                    case GraphicsRegister.SetReset:
                        this.SetReset = value;
                        ExpandRegister(value, new Span<byte>(this.ExpandedSetReset, 4));
                        break;

                    case GraphicsRegister.EnableSetReset:
                        this.EnableSetReset = value;
                        ExpandRegister(value, new Span<byte>(this.ExpandedEnableSetReset, 4));
                        break;

                    case GraphicsRegister.ColorCompare:
                        this.ColorCompare = value;
                        ExpandRegister(value, new Span<byte>(this.ExpandedColorCompare, 4));
                        break;

                    case GraphicsRegister.DataRotate:
                        this.DataRotate = value;
                        break;

                    case GraphicsRegister.ReadMapSelect:
                        this.ReadMapSelect = value;
                        break;

                    case GraphicsRegister.GraphicsMode:
                        this.GraphicsMode = value;
                        break;

                    case GraphicsRegister.MiscellaneousGraphics:
                        this.MiscellaneousGraphics = value;
                        break;

                    case GraphicsRegister.ColorDontCare:
                        this.ColorDontCare = value;
                        ExpandRegister(value, new Span<byte>(this.ExpandedColorDontCare, 4));
                        break;

                    case GraphicsRegister.BitMask:
                        this.BitMask = value;
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
