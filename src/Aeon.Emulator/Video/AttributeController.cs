namespace Aeon.Emulator.Video
{
    /// <summary>
    /// Emulates the VGA Attribute Controller registers.
    /// </summary>
    internal sealed class AttributeController
    {
        /// <summary>
        /// Initializes a new instance of the AttributeController class.
        /// </summary>
        public AttributeController()
        {
            for (int i = 0; i < this.InternalPalette.Length; i++)
                this.InternalPalette[i] = (byte)i;
        }

        /// <summary>
        /// Gets the internal palette.
        /// </summary>
        public byte[] InternalPalette { get; } = new byte[16];
        /// <summary>
        /// Gets or sets the Attribute Mode Control register.
        /// </summary>
        public byte AttributeModeControl { get; set; }
        /// <summary>
        /// Gets or sets the Overscan Color register.
        /// </summary>
        public byte OverscanColor { get; set; }
        /// <summary>
        /// Gets or sets the Color Plane Enable register.
        /// </summary>
        public byte ColorPlaneEnable { get; set; }
        /// <summary>
        /// Gets or sets the Horizontal Pixel Panning register.
        /// </summary>
        public byte HorizontalPixelPanning { get; set; }
        /// <summary>
        /// Gets or sets the Color Select register.
        /// </summary>
        public byte ColorSelect { get; set; }

        /// <summary>
        /// Returns the current value of an attribute controller register.
        /// </summary>
        /// <param name="address">Address of register to read.</param>
        /// <returns>Current value of the register.</returns>
        public byte ReadRegister(AttributeControllerRegister address)
        {
            if (address >= AttributeControllerRegister.FirstPaletteEntry && address <= AttributeControllerRegister.LastPaletteEntry)
                return this.InternalPalette[(byte)address];

            return address switch
            {
                AttributeControllerRegister.AttributeModeControl => this.AttributeModeControl,
                AttributeControllerRegister.OverscanColor => this.OverscanColor,
                AttributeControllerRegister.ColorPlaneEnable => this.ColorPlaneEnable,
                AttributeControllerRegister.HorizontalPixelPanning => this.HorizontalPixelPanning,
                AttributeControllerRegister.ColorSelect => this.ColorSelect,
                _ => 0
            };
        }
        /// <summary>
        /// Writes to an attribute controller register.
        /// </summary>
        /// <param name="address">Address of register to write.</param>
        /// <param name="value">Value to write to register.</param>
        public void WriteRegister(AttributeControllerRegister address, byte value)
        {
            if (address >= AttributeControllerRegister.FirstPaletteEntry && address <= AttributeControllerRegister.LastPaletteEntry)
            {
                this.InternalPalette[(byte)address] = value;
            }
            else
            {
                switch (address)
                {
                    case AttributeControllerRegister.AttributeModeControl:
                        this.AttributeModeControl = value;
                        break;

                    case AttributeControllerRegister.OverscanColor:
                        this.OverscanColor = value;
                        break;

                    case AttributeControllerRegister.ColorPlaneEnable:
                        this.ColorPlaneEnable = value;
                        break;

                    case AttributeControllerRegister.HorizontalPixelPanning:
                        this.HorizontalPixelPanning = value;
                        break;

                    case AttributeControllerRegister.ColorSelect:
                        this.ColorSelect = value;
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
