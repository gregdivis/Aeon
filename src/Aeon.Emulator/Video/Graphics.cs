namespace Aeon.Emulator.Video
{
    /// <summary>
    /// Emulates the VGA Graphics registers.
    /// </summary>
    internal sealed class Graphics : VideoComponent
    {
        /// <summary>
        /// Gets the Set/Reset register.
        /// </summary>
        public MaskValue SetReset { get; private set; }
        /// <summary>
        /// Gets the Enable Set/Reset register.
        /// </summary>
        public MaskValue EnableSetReset { get; private set; }
        /// <summary>
        /// Gets the Color Compare register.
        /// </summary>
        public byte ColorCompare { get; private set; }
        /// <summary>
        /// Gets the Data Rotate register.
        /// </summary>
        public byte DataRotate { get; private set; }
        /// <summary>
        /// Gets the Read Map Select register.
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
        /// Gets the Color Don't Care register.
        /// </summary>
        public MaskValue ColorDontCare { get; private set; }
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
                GraphicsRegister.SetReset => this.SetReset.Packed,
                GraphicsRegister.EnableSetReset => this.EnableSetReset.Packed,
                GraphicsRegister.ColorCompare => this.ColorCompare,
                GraphicsRegister.DataRotate => this.DataRotate,
                GraphicsRegister.ReadMapSelect => ReadMapSelect,
                GraphicsRegister.GraphicsMode => this.GraphicsMode,
                GraphicsRegister.MiscellaneousGraphics => this.MiscellaneousGraphics,
                GraphicsRegister.ColorDontCare => this.ColorDontCare.Packed,
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
            switch (address)
            {
                case GraphicsRegister.SetReset:
                    this.SetReset = value;
                    break;

                case GraphicsRegister.EnableSetReset:
                    this.EnableSetReset = value;
                    break;

                case GraphicsRegister.ColorCompare:
                    this.ColorCompare = value;
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
