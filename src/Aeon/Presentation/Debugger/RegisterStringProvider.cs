using System.ComponentModel;
using System.Text;
using System.Windows.Media;
using Aeon.Emulator.DebugSupport;

namespace Aeon.Emulator.Launcher.Presentation.Debugger
{
    /// <summary>
    /// Container for formatted register values.
    /// </summary>
    internal sealed class RegisterStringProvider : INotifyPropertyChanged
    {
        #region Private Fields
        private readonly IRegisterContainer source;
        private bool isHex;
        /// <summary>
        /// The EAX register value.
        /// </summary>
        private readonly RegisterString eax = new RegisterString();
        /// <summary>
        /// The EBX register value.
        /// </summary>
        private readonly RegisterString ebx = new RegisterString();
        /// <summary>
        /// The ECX register value.
        /// </summary>
        private readonly RegisterString ecx = new RegisterString();
        /// <summary>
        /// The EDX register value.
        /// </summary>
        private readonly RegisterString edx = new RegisterString();
        /// <summary>
        /// The ESI register value.
        /// </summary>
        private readonly RegisterString esi = new RegisterString();
        /// <summary>
        /// The EDI register value.
        /// </summary>
        private readonly RegisterString edi = new RegisterString();
        /// <summary>
        /// The EBP register value.
        /// </summary>
        private readonly RegisterString ebp = new RegisterString();
        /// <summary>
        /// The ESP register value.
        /// </summary>
        private readonly RegisterString esp = new RegisterString();
        /// <summary>
        /// The DS register value.
        /// </summary>
        private readonly RegisterString ds = new RegisterString(true);
        /// <summary>
        /// The ES register value.
        /// </summary>
        private readonly RegisterString es = new RegisterString(true);
        /// <summary>
        /// The FS register value.
        /// </summary>
        private readonly RegisterString fs = new RegisterString(true);
        /// <summary>
        /// The GS register value.
        /// </summary>
        private readonly RegisterString gs = new RegisterString(true);
        /// <summary>
        /// The SS register value.
        /// </summary>
        private readonly RegisterString ss = new RegisterString(true);
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the RegisterStringProvider class.
        /// </summary>
        /// <param name="source">Register value source.</param>
        public RegisterStringProvider(IRegisterContainer source)
        {
            this.source = source;
        }
        #endregion

        #region Public Events
        /// <summary>
        /// Occurs when a property value has changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets or sets a value indicating whether the registers should be displayed in hexadecimal.
        /// </summary>
        public bool IsHexFormat
        {
            get { return this.isHex; }
            set
            {
                if(this.isHex != value)
                {
                    this.isHex = value;
                    this.eax.IsHexFormat = value;
                    this.ebx.IsHexFormat = value;
                    this.ecx.IsHexFormat = value;
                    this.edx.IsHexFormat = value;
                    this.esi.IsHexFormat = value;
                    this.edi.IsHexFormat = value;
                    this.ebp.IsHexFormat = value;
                    this.esp.IsHexFormat = value;
                    this.ds.IsHexFormat = value;
                    this.es.IsHexFormat = value;
                    this.fs.IsHexFormat = value;
                    this.gs.IsHexFormat = value;
                    this.ss.IsHexFormat = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("IsHexFormat"));
                }
            }
        }
        /// <summary>
        /// Gets the EAX register value.
        /// </summary>
        public RegisterString EAX
        {
            get { return this.eax; }
        }
        /// <summary>
        /// Gets the EBX register value.
        /// </summary>
        public RegisterString EBX
        {
            get { return this.ebx; }
        }
        /// <summary>
        /// Gets the ECX register value.
        /// </summary>
        public RegisterString ECX
        {
            get { return this.ecx; }
        }
        /// <summary>
        /// Gets the EDX register value.
        /// </summary>
        public RegisterString EDX
        {
            get { return this.edx; }
        }
        /// <summary>
        /// Gets the ESI register value.
        /// </summary>
        public RegisterString ESI
        {
            get { return this.esi; }
        }
        /// <summary>
        /// Gets the EDI register value.
        /// </summary>
        public RegisterString EDI
        {
            get { return this.edi; }
        }
        /// <summary>
        /// Gets the EBP register value.
        /// </summary>
        public RegisterString EBP
        {
            get { return this.ebp; }
        }
        /// <summary>
        /// Gets the ESP register value.
        /// </summary>
        public RegisterString ESP
        {
            get { return this.esp; }
        }
        /// <summary>
        /// Gets the DS register value.
        /// </summary>
        public RegisterString DS
        {
            get { return this.ds; }
        }
        /// <summary>
        /// Gets the ES register value.
        /// </summary>
        public RegisterString ES
        {
            get { return this.es; }
        }
        /// <summary>
        /// Gets the FS register value.
        /// </summary>
        public RegisterString FS
        {
            get { return this.fs; }
        }
        /// <summary>
        /// Gets the GS register value.
        /// </summary>
        public RegisterString GS
        {
            get { return this.gs; }
        }
        /// <summary>
        /// Gets the SS register value.
        /// </summary>
        public RegisterString SS
        {
            get { return this.ss; }
        }
        /// <summary>
        /// Gets a string for displaying which CPU flags are set.
        /// </summary>
        public string Flags { get; private set; }
        #endregion

        #region Public Methods
        /// <summary>
        /// Updates displayed register values to match the source values.
        /// </summary>
        public void UpdateValues()
        {
            this.eax.SetValue(this.source.EAX);
            this.ebx.SetValue(this.source.EBX);
            this.ecx.SetValue(this.source.ECX);
            this.edx.SetValue(this.source.EDX);
            this.esi.SetValue(this.source.ESI);
            this.edi.SetValue(this.source.EDI);
            this.ebp.SetValue(this.source.EBP);
            this.esp.SetValue(this.source.ESP);
            this.ds.SetValue(this.source.DS);
            this.es.SetValue(this.source.ES);
            this.fs.SetValue(this.source.FS);
            this.gs.SetValue(this.source.GS);
            this.ss.SetValue(this.source.SS);

            var sourceFlags = GetFlagsString(this.source.Flags);
            if(sourceFlags != this.Flags)
            {
                this.Flags = sourceFlags;
                OnPropertyChanged(new PropertyChangedEventArgs("Flags"));
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Raises the PropertyChanged event.
        /// </summary>
        /// <param name="e">Information about the event.</param>
        private void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            var handler = this.PropertyChanged;
            if(handler != null)
                handler(this, e);
        }
        /// <summary>
        /// Gets a string for displaying set CPU flags.
        /// </summary>
        /// <param name="flags">The CPU flags to format.</param>
        /// <returns>String containing set CPU flags.</returns>
        private string GetFlagsString(EFlags flags)
        {
            var buffer = new StringBuilder();
            
            if((flags & EFlags.Carry) != 0)
                buffer.Append('C');
            if((flags & EFlags.Parity) != 0)
                buffer.Append('P');
            if((flags & EFlags.Auxiliary) != 0)
                buffer.Append('A');
            if((flags & EFlags.Zero) != 0)
                buffer.Append('Z');
            if((flags & EFlags.Sign) != 0)
                buffer.Append('S');
            if((flags & EFlags.Trap) != 0)
                buffer.Append('T');
            if((flags & EFlags.InterruptEnable) != 0)
                buffer.Append('I');
            if((flags & EFlags.Direction) != 0)
                buffer.Append('D');
            if((flags & EFlags.Overflow) != 0)
                buffer.Append('O');

            return buffer.ToString();
        }
        #endregion
    }

    /// <summary>
    /// Dynamically formats a register for display using WPF.
    /// </summary>
    internal sealed class RegisterString : INotifyPropertyChanged
    {
        #region Private Static Fields
        /// <summary>
        /// The default value color.
        /// </summary>
        private static readonly SolidColorBrush DefaultColor = new SolidColorBrush(Colors.Black);
        /// <summary>
        /// The changed value color.
        /// </summary>
        private static readonly SolidColorBrush ChangedColor = new SolidColorBrush(Colors.Red);
        #endregion

        #region Private Fields
        private uint currentValue;
        private bool isHex;
        private bool hasChanged;
        private bool isShort;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the RegisterString class.
        /// </summary>
        public RegisterString()
        {
        }
        /// <summary>
        /// Initializes a new instance of the RegisterString class.
        /// </summary>
        /// <param name="isShort">Value indicating whether the register is 16 or 32 bit.</param>
        public RegisterString(bool isShort)
        {
            this.isShort = isShort;
        }
        #endregion

        #region Public Events
        /// <summary>
        /// Occurs when a property value has changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets or sets a value indicating whether the value is displayed in hexadecimal.
        /// </summary>
        public bool IsHexFormat
        {
            get { return this.isHex; }
            set
            {
                if(this.isHex != value)
                {
                    this.isHex = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("IsHexFormat"));
                    OnPropertyChanged(new PropertyChangedEventArgs("Value"));
                }
            }
        }
        /// <summary>
        /// Gets a value indicating whether the value has changed.
        /// </summary>
        public bool HasValueChanged
        {
            get { return this.hasChanged; }
            private set
            {
                if(this.hasChanged != value)
                {
                    this.hasChanged = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("HasValueChanged"));
                    OnPropertyChanged(new PropertyChangedEventArgs("Color"));
                }
            }
        }
        /// <summary>
        /// Gets the current color of the value.
        /// </summary>
        public Brush Color
        {
            get { return this.hasChanged ? ChangedColor : DefaultColor; }
        }
        /// <summary>
        /// Gets the current value.
        /// </summary>
        public string Value
        {
            get
            {
                if(this.isHex)
                    return this.currentValue.ToString(this.isShort ? "X4" : "X8");
                else
                    return this.currentValue.ToString();
            }
        }

        /// <summary>
        /// Sets the current value to display.
        /// </summary>
        /// <param name="value">New value to display.</param>
        public void SetValue(uint value)
        {
            if(this.currentValue != value)
            {
                this.currentValue = value;
                this.HasValueChanged = true;
            }
            else
                this.HasValueChanged = false;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Raises the PropertyChanged event.
        /// </summary>
        /// <param name="e">Information about the event.</param>
        private void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            var handler = this.PropertyChanged;
            if(handler != null)
                handler(this, e);
        }
        #endregion
    }
}
