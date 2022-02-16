using System.ComponentModel;
using System.Text;
using Aeon.Emulator.DebugSupport;

namespace Aeon.Emulator.Launcher.Debugger
{
    /// <summary>
    /// Container for formatted register values.
    /// </summary>
    internal sealed class RegisterStringProvider : INotifyPropertyChanged
    {
        private readonly IRegisterContainer source;
        private bool isHex;

        /// <summary>
        /// Initializes a new instance of the <see cref="RegisterStringProvider"/> class.
        /// </summary>
        /// <param name="source">Register value source.</param>
        public RegisterStringProvider(IRegisterContainer source)
        {
            this.source = source;
        }

        /// <summary>
        /// Occurs when a property value has changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets or sets a value indicating whether the registers should be displayed in hexadecimal.
        /// </summary>
        public bool IsHexFormat
        {
            get => this.isHex;
            set
            {
                if (this.isHex != value)
                {
                    this.isHex = value;
                    this.EAX.IsHexFormat = value;
                    this.EBX.IsHexFormat = value;
                    this.ECX.IsHexFormat = value;
                    this.EDX.IsHexFormat = value;
                    this.ESI.IsHexFormat = value;
                    this.EDI.IsHexFormat = value;
                    this.EBP.IsHexFormat = value;
                    this.ESP.IsHexFormat = value;
                    this.DS.IsHexFormat = value;
                    this.ES.IsHexFormat = value;
                    this.FS.IsHexFormat = value;
                    this.GS.IsHexFormat = value;
                    this.SS.IsHexFormat = value;
                    this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(IsHexFormat)));
                }
            }
        }
        /// <summary>
        /// Gets the EAX register value.
        /// </summary>
        public RegisterString EAX { get; } = new();
        /// <summary>
        /// Gets the EBX register value.
        /// </summary>
        public RegisterString EBX { get; } = new();
        /// <summary>
        /// Gets the ECX register value.
        /// </summary>
        public RegisterString ECX { get; } = new();
        /// <summary>
        /// Gets the EDX register value.
        /// </summary>
        public RegisterString EDX { get; } = new();
        /// <summary>
        /// Gets the ESI register value.
        /// </summary>
        public RegisterString ESI { get; } = new();
        /// <summary>
        /// Gets the EDI register value.
        /// </summary>
        public RegisterString EDI { get; } = new();
        /// <summary>
        /// Gets the EBP register value.
        /// </summary>
        public RegisterString EBP { get; } = new();
        /// <summary>
        /// Gets the ESP register value.
        /// </summary>
        public RegisterString ESP { get; } = new();
        /// <summary>
        /// Gets the DS register value.
        /// </summary>
        public RegisterString DS { get; } = new(true);
        /// <summary>
        /// Gets the ES register value.
        /// </summary>
        public RegisterString ES { get; } = new(true);
        /// <summary>
        /// Gets the FS register value.
        /// </summary>
        public RegisterString FS { get; } = new(true);
        /// <summary>
        /// Gets the GS register value.
        /// </summary>
        public RegisterString GS { get; } = new(true);
        /// <summary>
        /// Gets the SS register value.
        /// </summary>
        public RegisterString SS { get; } = new(true);
        /// <summary>
        /// Gets a string for displaying which CPU flags are set.
        /// </summary>
        public string Flags { get; private set; }

        /// <summary>
        /// Updates displayed register values to match the source values.
        /// </summary>
        public void UpdateValues()
        {
            this.EAX.SetValue(this.source.EAX);
            this.EBX.SetValue(this.source.EBX);
            this.ECX.SetValue(this.source.ECX);
            this.EDX.SetValue(this.source.EDX);
            this.ESI.SetValue(this.source.ESI);
            this.EDI.SetValue(this.source.EDI);
            this.EBP.SetValue(this.source.EBP);
            this.ESP.SetValue(this.source.ESP);
            this.DS.SetValue(this.source.DS);
            this.ES.SetValue(this.source.ES);
            this.FS.SetValue(this.source.FS);
            this.GS.SetValue(this.source.GS);
            this.SS.SetValue(this.source.SS);

            var sourceFlags = GetFlagsString(this.source.Flags);
            if (sourceFlags != this.Flags)
            {
                this.Flags = sourceFlags;
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(Flags)));
            }
        }

        /// <summary>
        /// Raises the PropertyChanged event.
        /// </summary>
        /// <param name="e">Information about the event.</param>
        private void OnPropertyChanged(PropertyChangedEventArgs e) => this.PropertyChanged?.Invoke(this, e);
        /// <summary>
        /// Gets a string for displaying set CPU flags.
        /// </summary>
        /// <param name="flags">The CPU flags to format.</param>
        /// <returns>String containing set CPU flags.</returns>
        private static string GetFlagsString(EFlags flags)
        {
            var buffer = new StringBuilder();

            if (flags.HasFlag(EFlags.Carry))
                buffer.Append('C');
            if (flags.HasFlag(EFlags.Parity))
                buffer.Append('P');
            if (flags.HasFlag(EFlags.Auxiliary))
                buffer.Append('A');
            if (flags.HasFlag(EFlags.Zero))
                buffer.Append('Z');
            if (flags.HasFlag(EFlags.Sign))
                buffer.Append('S');
            if (flags.HasFlag(EFlags.Trap))
                buffer.Append('T');
            if (flags.HasFlag(EFlags.InterruptEnable))
                buffer.Append('I');
            if (flags.HasFlag(EFlags.Direction))
                buffer.Append('D');
            if (flags.HasFlag(EFlags.Overflow))
                buffer.Append('O');

            return buffer.ToString();
        }
    }
}
