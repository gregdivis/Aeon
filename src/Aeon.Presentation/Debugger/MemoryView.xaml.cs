using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Aeon.Emulator.DebugSupport;

namespace Aeon.Presentation.Debugger
{
    /// <summary>
    /// Provides a view of a region of memory.
    /// </summary>
    public sealed partial class MemoryView : UserControl
    {
        #region Private Static Fields
        /// <summary>
        /// The MemorySource dependency property definition.
        /// </summary>
        public static readonly DependencyProperty MemorySourceProperty = DependencyProperty.Register("MemorySource", typeof(IMemorySource), typeof(MemoryView));
        /// <summary>
        /// The StartAddress dependency property definition.
        /// </summary>
        public static readonly DependencyProperty StartAddressProperty = DependencyProperty.Register("StartAddress", typeof(QualifiedAddress), typeof(MemoryView), new PropertyMetadata(QualifiedAddress.FromRealModeAddress(0, 0)));
        #endregion

        #region Private Fields
        private const double RowHeight = 14;
        private readonly List<RowControls> rows = new List<RowControls>();
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the MemoryView class.
        /// </summary>
        public MemoryView()
        {
            InitializeComponent();
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets or sets the source memory to display. This is a dependency property.
        /// </summary>
        public IMemorySource MemorySource
        {
            get { return (IMemorySource)GetValue(MemorySourceProperty); }
            set { SetValue(MemorySourceProperty, value); }
        }
        /// <summary>
        /// Gets or sets the initial address offset. This is a dependency property.
        /// </summary>
        public QualifiedAddress StartAddress
        {
            get { return (QualifiedAddress)GetValue(StartAddressProperty); }
            set { SetValue(StartAddressProperty, value); }
        }
        #endregion

        #region Protected Methods
        /// <summary>
        /// Invoked when a property value has changed.
        /// </summary>
        /// <param name="e">Information about the event.</param>
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if(e.Property == ActualHeightProperty)
            {
                if(rows.Count != CalculateVisibleRows())
                {
                    RebuildRows();
                    UpdateValues();
                }
            }
            else if(e.Property == MemorySourceProperty || e.Property == StartAddressProperty)
            {
                UpdateValues();
                if(e.Property == StartAddressProperty)
                    this.scrollBar.Value = this.StartAddress.Offset;
            }
        }
        /// <summary>
        /// Invoked when the mouse wheel has changed position.
        /// </summary>
        /// <param name="e">Information about the event.</param>
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);

            var newValue = this.scrollBar.Value - e.Delta;
            if(newValue < 0)
                newValue = 0;
            if(newValue > this.scrollBar.Maximum)
                newValue = this.scrollBar.Maximum;

            this.scrollBar.Value = newValue;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Updates the values displayed to match the memory source.
        /// </summary>
        public void UpdateValues()
        {
            var source = this.MemorySource;
            if(source != null)
            {
                if(rows.Count == 0)
                    RebuildRows();
                UpdateValues(source, this.StartAddress);
            }
            else
                ClearValues();
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Resets the display to empty.
        /// </summary>
        private void ClearValues()
        {
            foreach(var row in this.rows)
            {
                row.Address.Text = "????????";

                foreach(var text in row.HexValues)
                    text.Text = "??";

                row.ByteValues.Text = "................";
            }
        }
        /// <summary>
        /// Updates the values displayed to match the memory source.
        /// </summary>
        /// <param name="source">Memory where values are read from.</param>
        /// <param name="address">Initial address in memory to read from.</param>
        private void UpdateValues(IMemorySource source, QualifiedAddress address)
        {
            var buffer = new byte[this.rows.Count * 16];
            source.ReadBytes(buffer, 0, address, buffer.Length);
            var textBuffer = new StringBuilder(16);

            for(int i = 0; i < rows.Count; i++)
            {
                textBuffer.Length = 0;
                rows[i].Address.Text = (address + (i * 16)).ToString();
                for(int c = 0; c < 16; c++)
                {
                    byte b = buffer[(i * 16) + c];
                    rows[i].HexValues[c].Text = b.ToString("X2");

                    if(b == '\n' || b == '\r' || b == '\t')
                        textBuffer.Append(' ');
                    else if(b < '!')
                        textBuffer.Append('.');
                    else
                        textBuffer.Append((char)b);
                }

                rows[i].ByteValues.Text = textBuffer.ToString();
            }
        }
        /// <summary>
        /// Removes previously generated controls then rebuilds the correct number of them.
        /// </summary>
        private void RebuildRows()
        {
            foreach(var row in this.rows)
            {
                this.mainGrid.Children.Remove(row.Address);

                foreach(var text in row.HexValues)
                    this.mainGrid.Children.Remove(text);

                this.mainGrid.Children.Remove(row.ByteValues);
            }

            this.mainGrid.RowDefinitions.Clear();
            this.rows.Clear();

            int count = CalculateVisibleRows();
            this.scrollBar.Maximum = ushort.MaxValue - count * 16;
            for(int i = 0; i < count; i++)
            {
                var row = new RowControls();
                this.mainGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                this.mainGrid.Children.Add(row.Address);
                Grid.SetColumn(row.Address, 0);
                Grid.SetRow(row.Address, i);

                for(int c = 0; c < 16; c++)
                {
                    this.mainGrid.Children.Add(row.HexValues[c]);
                    Grid.SetColumn(row.HexValues[c], c + 1);
                    Grid.SetRow(row.HexValues[c], i);
                }

                this.mainGrid.Children.Add(row.ByteValues);
                Grid.SetColumn(row.ByteValues, 18);
                Grid.SetRow(row.ByteValues, i);

                this.rows.Add(row);
            }
        }
        /// <summary>
        /// Returns the number of currently visible whole rows.
        /// </summary>
        /// <returns>Number of whole rows visible.</returns>
        private int CalculateVisibleRows()
        {
            return (int)(this.ActualHeight / RowHeight);
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// Invoked when the scroll bar's value has changed.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Information about the event.</param>
        private void ScrollBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var current = this.StartAddress;
            if((uint)e.NewValue != this.StartAddress.Offset)
                SetValue(StartAddressProperty, new QualifiedAddress(current.AddressType, current.Segment ?? 0, (uint)e.NewValue));
        }
        #endregion

        #region Private RowControls Class
        /// <summary>
        /// Contains generated controls for a row.
        /// </summary>
        private sealed class RowControls
        {
            private static readonly FontFamily NumberFont = new FontFamily("Courier New");
            private static readonly FontFamily ConsoleFont = new FontFamily("Lucida Console");

            /// <summary>
            /// Initializes a new instance of the RowControls class.
            /// </summary>
            public RowControls()
            {
                for(int i = 0; i < HexValues.Length; i++)
                    this.HexValues[i] = new TextBlock() { FontFamily = NumberFont, HorizontalAlignment = HorizontalAlignment.Center };
            }

            /// <summary>
            /// The TextBlock where the address is displayed.
            /// </summary>
            public readonly TextBlock Address = new TextBlock() { FontFamily = NumberFont, Margin = new Thickness(0, 0, 10, 0) };
            /// <summary>
            /// The TextBlocks where the hex values are displayed.
            /// </summary>
            public readonly TextBlock[] HexValues = new TextBlock[16];
            /// <summary>
            /// The TextBlock where the ASCII values are displayed.
            /// </summary>
            public readonly TextBlock ByteValues = new TextBlock() { FontFamily = ConsoleFont };
        }
        #endregion
    }
}
