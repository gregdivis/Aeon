using System.ComponentModel;
using System.Windows.Media;

namespace Aeon.Emulator.Launcher.Debugger
{
    /// <summary>
    /// Dynamically formats a register for display using WPF.
    /// </summary>
    internal sealed class RegisterString : INotifyPropertyChanged
    {
        /// <summary>
        /// The default value color.
        /// </summary>
        private static readonly SolidColorBrush DefaultColor = new(Colors.Black);
        /// <summary>
        /// The changed value color.
        /// </summary>
        private static readonly SolidColorBrush ChangedColor = new(Colors.Red);

        private uint currentValue;
        private bool isHex;
        private bool hasChanged;
        private readonly bool isShort;

        /// <summary>
        /// Initializes a new instance of the RegisterString class.
        /// </summary>
        public RegisterString()
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="RegisterString"/> class.
        /// </summary>
        /// <param name="isShort">Value indicating whether the register is 16 or 32 bit.</param>
        public RegisterString(bool isShort) => this.isShort = isShort;

        /// <summary>
        /// Occurs when a property value has changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets or sets a value indicating whether the value is displayed in hexadecimal.
        /// </summary>
        public bool IsHexFormat
        {
            get => this.isHex;
            set
            {
                if (this.isHex != value)
                {
                    this.isHex = value;
                    this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(IsHexFormat)));
                    this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(Value)));
                }
            }
        }
        /// <summary>
        /// Gets a value indicating whether the value has changed.
        /// </summary>
        public bool HasValueChanged
        {
            get => this.hasChanged;
            private set
            {
                if (this.hasChanged != value)
                {
                    this.hasChanged = value;
                    this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(HasValueChanged)));
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.Color)));
                }
            }
        }
        /// <summary>
        /// Gets the current color of the value.
        /// </summary>
        public Brush Color => this.hasChanged ? ChangedColor : DefaultColor;
        /// <summary>
        /// Gets the current value.
        /// </summary>
        public string Value => this.isHex ? this.currentValue.ToString(this.isShort ? "X4" : "X8") : this.currentValue.ToString();

        /// <summary>
        /// Sets the current value to display.
        /// </summary>
        /// <param name="value">New value to display.</param>
        public void SetValue(uint value)
        {
            if (this.currentValue != value)
            {
                this.currentValue = value;
                this.HasValueChanged = true;
            }
            else
            {
                this.HasValueChanged = false;
            }
        }

        /// <summary>
        /// Raises the PropertyChanged event.
        /// </summary>
        /// <param name="e">Information about the event.</param>
        private void OnPropertyChanged(PropertyChangedEventArgs e) => this.PropertyChanged?.Invoke(this, e);
    }
}
