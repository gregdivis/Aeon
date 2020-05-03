using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Aeon.Presentation.Dialogs
{
    /// <summary>
    /// Displays the current color palette.
    /// </summary>
    public partial class PaletteDialog : Window
    {
        /// <summary>
        /// The EmulatorDisplay dependency property definition.
        /// </summary>
        public static readonly DependencyProperty EmulatorDisplayProperty = DependencyProperty.Register("EmulatorDisplay", typeof(EmulatorDisplay), typeof(PaletteDialog));

        private DispatcherTimer timer;

        /// <summary>
        /// Initializes a new instance of the PaletteDialog class.
        /// </summary>
        public PaletteDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the current EmulatorDisplay control. This is a dependency property.
        /// </summary>
        public EmulatorDisplay EmulatorDisplay
        {
            get { return (EmulatorDisplay)GetValue(EmulatorDisplayProperty); }
            set { SetValue(EmulatorDisplayProperty, value); }
        }

        /// <summary>
        /// Gets the palette to display. This is a dependency property.
        /// </summary>
        public uint[] Palette
        {
            get
            {
                var display = this.EmulatorDisplay;
                if(display != null && display.EmulatorHost != null && display.EmulatorHost.VirtualMachine != null && display.EmulatorHost.VirtualMachine.VideoMode != null)
                    return display.EmulatorHost.VirtualMachine.VideoMode.Palette.ToArray();
                else
                    return null;
            }
        }

        /// <summary>
        /// Invoked when the window is initialized.
        /// </summary>
        /// <param name="e">Unused EventArgs instance.</param>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            for(int i = 0; i < 256; i++)
                this.grid.Children.Add(new Rectangle() { Fill = new SolidColorBrush() });

            this.timer = new DispatcherTimer(TimeSpan.FromSeconds(1.0 / 30.0), DispatcherPriority.Normal, UpdateColors, this.Dispatcher);
            this.timer.Start();
        }

        /// <summary>
        /// Invoked by the timer to update the displayed colors.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Unused EventArgs instance.</param>
        private void UpdateColors(object sender, EventArgs e)
        {
            var palette = this.Palette;
            if(palette == null)
                return;

            for(int i = 0; i < palette.Length; i++)
                ((SolidColorBrush)((Rectangle)this.grid.Children[i]).Fill).Color = Color.FromRgb((byte)(palette[i] >> 16), (byte)(palette[i] >> 8), (byte)palette[i]);
        }
    }
}
