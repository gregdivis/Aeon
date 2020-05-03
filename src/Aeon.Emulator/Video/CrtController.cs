
namespace Aeon.Emulator.Video
{
    /// <summary>
    /// Emulates the VGA CRT Controller registers.
    /// </summary>
    internal sealed class CrtController
    {
        /// <summary>
        /// Initializes a new instance of the CrtController class.
        /// </summary>
        public CrtController()
        {
        }

        /// <summary>
        /// Gets or sets the Horizontal Total register.
        /// </summary>
        public byte HorizontalTotal { get; set; }
        /// <summary>
        /// Gets or sets the End Horizontal Display register.
        /// </summary>
        public byte EndHorizontalDisplay { get; set; }
        /// <summary>
        /// Gets or sets the Start Horizontal Blanking register.
        /// </summary>
        public byte StartHorizontalBlanking { get; set; }
        /// <summary>
        /// Gets or sets the End Horizontal Blanking register.
        /// </summary>
        public byte EndHorizontalBlanking { get; set; }
        /// <summary>
        /// Gets or sets the Start Horizontal Retrace register.
        /// </summary>
        public byte StartHorizontalRetrace { get; set; }
        /// <summary>
        /// Gets or sets the End Horizontal Retrace register.
        /// </summary>
        public byte EndHorizontalRetrace { get; set; }
        /// <summary>
        /// Gets or sets the Vertical Total register.
        /// </summary>
        public byte VerticalTotal { get; set; }
        /// <summary>
        /// Gets or sets the Overflow register.
        /// </summary>
        public byte Overflow { get; set; }
        /// <summary>
        /// Gets or sets the Preset Row Scan register.
        /// </summary>
        public byte PresetRowScan { get; set; }
        /// <summary>
        /// Gets or sets the Maximum Scan Line register.
        /// </summary>
        public byte MaximumScanLine { get; set; }
        /// <summary>
        /// Gets or sets the Cursor Start register.
        /// </summary>
        public byte CursorStart { get; set; }
        /// <summary>
        /// Gets or sets the Cursor End register.
        /// </summary>
        public byte CursorEnd { get; set; }
        /// <summary>
        /// Gets or sets the Start Address register.
        /// </summary>
        public ushort StartAddress { get; set; }
        /// <summary>
        /// Gets or sets the Cursor Location register.
        /// </summary>
        public ushort CursorLocation { get; set; }
        /// <summary>
        /// Gets or sets the Vertical Retrace Start register.
        /// </summary>
        public byte VerticalRetraceStart { get; set; }
        /// <summary>
        /// Gets or sets the Vertical Retrace End register.
        /// </summary>
        public byte VerticalRetraceEnd { get; set; }
        /// <summary>
        /// Gets or sets the Vertical Display End register.
        /// </summary>
        public byte VerticalDisplayEnd { get; set; }
        /// <summary>
        /// Gets or sets the Offset register.
        /// </summary>
        public byte Offset { get; set; }
        /// <summary>
        /// Gets or sets the Underline Location register.
        /// </summary>
        public byte UnderlineLocation { get; set; }
        /// <summary>
        /// Gets or sets the Start Vertical Blanking register.
        /// </summary>
        public byte StartVerticalBlanking { get; set; }
        /// <summary>
        /// Gets or sets the End Vertical Blanking register.
        /// </summary>
        public byte EndVerticalBlanking { get; set; }
        /// <summary>
        /// Gets or sets the CRT Mode Control register.
        /// </summary>
        public byte CrtModeControl { get; set; }
        /// <summary>
        /// Gets or sets the Line Compare register.
        /// </summary>
        public byte LineCompare { get; set; }

        /// <summary>
        /// Returns the current value of a CRT controller register.
        /// </summary>
        /// <param name="address">Address of register to read.</param>
        /// <returns>Current value of the register.</returns>
        public byte ReadRegister(CrtControllerRegister address)
        {
            switch (address)
            {
                case CrtControllerRegister.HorizontalTotal:
                    return this.HorizontalTotal;

                case CrtControllerRegister.EndHorizontalDisplay:
                    return this.EndHorizontalDisplay;

                case CrtControllerRegister.StartHorizontalBlanking:
                    return this.StartHorizontalBlanking;

                case CrtControllerRegister.EndHorizontalBlanking:
                    return this.EndHorizontalBlanking;

                case CrtControllerRegister.StartHorizontalRetrace:
                    return this.StartHorizontalRetrace;

                case CrtControllerRegister.EndHorizontalRetrace:
                    return this.EndHorizontalRetrace;

                case CrtControllerRegister.VerticalTotal:
                    return this.VerticalTotal;

                case CrtControllerRegister.Overflow:
                    return this.Overflow;

                case CrtControllerRegister.PresetRowScan:
                    return this.PresetRowScan;

                case CrtControllerRegister.MaximumScanLine:
                    return this.MaximumScanLine;

                case CrtControllerRegister.CursorStart:
                    return this.CursorStart;

                case CrtControllerRegister.CursorEnd:
                    return this.CursorEnd;

                case CrtControllerRegister.StartAddressHigh:
                    return (byte)(this.StartAddress >> 8);

                case CrtControllerRegister.StartAddressLow:
                    return (byte)this.StartAddress;

                case CrtControllerRegister.CursorLocationHigh:
                    return (byte)(this.CursorLocation >> 8);

                case CrtControllerRegister.CursorLocationLow:
                    return (byte)this.CursorLocation;

                case CrtControllerRegister.VerticalRetraceStart:
                    return this.VerticalRetraceStart;

                case CrtControllerRegister.VerticalRetraceEnd:
                    return this.VerticalRetraceEnd;

                case CrtControllerRegister.VerticalDisplayEnd:
                    return this.VerticalDisplayEnd;

                case CrtControllerRegister.Offset:
                    return this.Offset;

                case CrtControllerRegister.UnderlineLocation:
                    return this.UnderlineLocation;

                case CrtControllerRegister.StartVerticalBlanking:
                    return this.StartVerticalBlanking;

                case CrtControllerRegister.EndVerticalBlanking:
                    return this.EndVerticalBlanking;

                case CrtControllerRegister.CrtModeControl:
                    return this.CrtModeControl;

                case CrtControllerRegister.LineCompare:
                    return this.LineCompare;

                default:
                    return 0;
            }
        }
        /// <summary>
        /// Writes to a CRT controller register.
        /// </summary>
        /// <param name="address">Address of register to write.</param>
        /// <param name="value">Value to write to register.</param>
        public void WriteRegister(CrtControllerRegister address, byte value)
        {
            switch (address)
            {
                case CrtControllerRegister.HorizontalTotal:
                    this.HorizontalTotal = value;
                    break;

                case CrtControllerRegister.EndHorizontalDisplay:
                    this.EndHorizontalDisplay = value;
                    break;

                case CrtControllerRegister.StartHorizontalBlanking:
                    this.StartHorizontalBlanking = value;
                    break;

                case CrtControllerRegister.EndHorizontalBlanking:
                    this.EndHorizontalBlanking = value;
                    break;

                case CrtControllerRegister.StartHorizontalRetrace:
                    this.StartHorizontalRetrace = value;
                    break;

                case CrtControllerRegister.EndHorizontalRetrace:
                    this.EndHorizontalRetrace = value;
                    break;

                case CrtControllerRegister.VerticalTotal:
                    this.VerticalTotal = value;
                    break;

                case CrtControllerRegister.Overflow:
                    this.Overflow = value;
                    break;

                case CrtControllerRegister.PresetRowScan:
                    this.PresetRowScan = value;
                    break;

                case CrtControllerRegister.MaximumScanLine:
                    this.MaximumScanLine = value;
                    break;

                case CrtControllerRegister.CursorStart:
                    this.CursorStart = value;
                    break;

                case CrtControllerRegister.CursorEnd:
                    this.CursorEnd = value;
                    break;

                case CrtControllerRegister.StartAddressHigh:
                    this.StartAddress &= 0x000000FF;
                    this.StartAddress |= (ushort)(value << 8);
                    break;

                case CrtControllerRegister.StartAddressLow:
                    this.StartAddress &= 0x0000FF00;
                    this.StartAddress |= value;
                    break;

                case CrtControllerRegister.CursorLocationHigh:
                    this.CursorLocation &= 0x000000FF;
                    this.CursorLocation |= (ushort)(value << 8);
                    break;

                case CrtControllerRegister.CursorLocationLow:
                    this.CursorLocation &= 0x0000FF00;
                    this.CursorLocation |= value;
                    break;

                case CrtControllerRegister.VerticalRetraceStart:
                    this.VerticalRetraceStart = value;
                    break;

                case CrtControllerRegister.VerticalRetraceEnd:
                    this.VerticalRetraceEnd = value;
                    break;

                case CrtControllerRegister.VerticalDisplayEnd:
                    this.VerticalDisplayEnd = value;
                    break;

                case CrtControllerRegister.Offset:
                    this.Offset = value;
                    break;

                case CrtControllerRegister.UnderlineLocation:
                    this.UnderlineLocation = value;
                    break;

                case CrtControllerRegister.StartVerticalBlanking:
                    this.StartVerticalBlanking = value;
                    break;

                case CrtControllerRegister.EndVerticalBlanking:
                    this.EndVerticalBlanking = value;
                    break;

                case CrtControllerRegister.CrtModeControl:
                    this.CrtModeControl = value;
                    break;

                case CrtControllerRegister.LineCompare:
                    this.LineCompare = value;
                    break;

                default:
                    break;
            }
        }
    }
}
