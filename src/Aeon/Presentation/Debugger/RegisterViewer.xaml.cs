using System.Windows;
using System.Windows.Controls;
using Aeon.Emulator.DebugSupport;

namespace Aeon.Emulator.Launcher.Presentation.Debugger
{
    /// <summary>
    /// Displays the values of registers.
    /// </summary>
    public partial class RegisterViewer : UserControl
    {
        /// <summary>
        /// The RegisterProvider dependency property key definition.
        /// </summary>
        private static readonly DependencyPropertyKey RegisterProviderPropertyKey = DependencyProperty.RegisterReadOnly("RegisterProvider", typeof(RegisterStringProvider), typeof(RegisterViewer), new PropertyMetadata(null));

        /// <summary>
        /// The RegisterSource dependency property definition.
        /// </summary>
        public static readonly DependencyProperty RegisterSourceProperty = DependencyProperty.Register("RegisterSource", typeof(IRegisterContainer), typeof(RegisterViewer));
        /// <summary>
        /// The IsHexFormat dependency property definition.
        /// </summary>
        public static readonly DependencyProperty IsHexFormatProperty = AeonDebug.IsHexFormatProperty.AddOwner(typeof(RegisterViewer));

        /// <summary>
        /// The RegisterProvider dependency property definition.
        /// </summary>
        internal static DependencyProperty RegisterProviderProperty = RegisterProviderPropertyKey.DependencyProperty;

        /// <summary>
        /// Initializes a new instance of the RegisterViewer class.
        /// </summary>
        public RegisterViewer()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the register container to display. This is a dependency property.
        /// </summary>
        public IRegisterContainer RegisterSource
        {
            get { return (IRegisterContainer)GetValue(RegisterSourceProperty); }
            set { SetValue(RegisterSourceProperty, value); }
        }
        /// <summary>
        /// Gets or sets a value indicating whether values should be displayed in hexadecimal format. This is a dependency property.
        /// </summary>
        public bool IsHexFormat
        {
            get { return (bool)GetValue(IsHexFormatProperty); }
            set { SetValue(IsHexFormatProperty, value); }
        }

        /// <summary>
        /// Gets the register value provider. This is a dependency property.
        /// </summary>
        internal RegisterStringProvider RegisterProvider
        {
            get { return (RegisterStringProvider)GetValue(RegisterProviderProperty); }
        }

        /// <summary>
        /// Updates the displayed values to match the source values.
        /// </summary>
        public void UpdateValues()
        {
            var provider = this.RegisterProvider;
            if(provider != null)
                provider.UpdateValues();
        }

        /// <summary>
        /// Invoked when a property value has changed.
        /// </summary>
        /// <param name="e">Information about the event.</param>
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if(e.Property == RegisterSourceProperty)
            {
                if(e.NewValue != null)
                {
                    SetValue(RegisterProviderPropertyKey, new RegisterStringProvider((IRegisterContainer)e.NewValue) { IsHexFormat = this.IsHexFormat });
                    UpdateValues();
                }
                else
                    SetValue(RegisterProviderPropertyKey, null);
            }
            else if(e.Property == AeonDebug.IsHexFormatProperty)
            {
                var provider = this.RegisterProvider;
                if(provider != null)
                    provider.IsHexFormat = (bool)e.NewValue;
            }
        }
    }
}
