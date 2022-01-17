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

        private readonly UnsafeBuffer<byte> colorCompareBuffer = new(4);
        private readonly UnsafeBuffer<byte> colorDontCareBuffer = new(4);
        private readonly UnsafeBuffer<byte> setResetBuffer = new(4);
        private readonly UnsafeBuffer<bool> enableSetResetBuffer = new(4);

        /// <summary>
        /// Initializes a new instance of the <see cref="Graphics"/> class.
        /// </summary>
        public Graphics()
        {
            unsafe
            {
                this.ExpandedColorCompare = this.colorCompareBuffer.ToPointer();
                this.ExpandedColorDontCare = this.colorDontCareBuffer.ToPointer();
                this.ExpandedSetReset = this.setResetBuffer.ToPointer();
                this.ExpandedEnableSetReset = this.enableSetResetBuffer.ToPointer();
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
            return address switch
            {
                GraphicsRegister.SetReset => this.SetReset,
                GraphicsRegister.EnableSetReset => this.EnableSetReset,
                GraphicsRegister.ColorCompare => this.ColorCompare,
                GraphicsRegister.DataRotate => this.DataRotate,
                GraphicsRegister.ReadMapSelect => ReadMapSelect,
                GraphicsRegister.GraphicsMode => this.GraphicsMode,
                GraphicsRegister.MiscellaneousGraphics => this.MiscellaneousGraphics,
                GraphicsRegister.ColorDontCare => this.ColorDontCare,
                GraphicsRegister.BitMask => this.BitMask,
                _ => 0
            };
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
