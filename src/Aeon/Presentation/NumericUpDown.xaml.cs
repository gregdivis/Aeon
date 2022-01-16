using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;

namespace Aeon.Emulator.Launcher.Presentation
{
    /// <summary>
    /// A simple integer numeric up/down control.
    /// </summary>
    [ContentProperty("Value")]
    public partial class NumericUpDown : UserControl
    {
        /// <summary>
        /// Defines the Value dependency property.
        /// </summary>
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(int), typeof(NumericUpDown), new PropertyMetadata(0, Value_PropertyChanged, Value_CoerceValue));
        /// <summary>
        /// Defines the MinimumValue dependency property.
        /// </summary>
        public static readonly DependencyProperty MinimumValueProperty = DependencyProperty.Register("MinimumValue", typeof(int), typeof(NumericUpDown), new PropertyMetadata(0, MinimumValue_PropertyChanged));
        /// <summary>
        /// Defines the MaximumValue dependency property.
        /// </summary>
        public static readonly DependencyProperty MaximumValueProperty = DependencyProperty.Register("MaximumValue", typeof(int), typeof(NumericUpDown), new PropertyMetadata(100, MaximumValue_PropertyChanged));
        /// <summary>
        /// Defines the StepValue dependency property.
        /// </summary>
        public static readonly DependencyProperty StepValueProperty = DependencyProperty.Register("StepValue", typeof(int), typeof(NumericUpDown), new PropertyMetadata(1));
        /// <summary>
        /// Defines the IsReadOnly dependency property.
        /// </summary>
        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(NumericUpDown), new PropertyMetadata(false));

        /// <summary>
        /// Initializes a new instance of the NumericUpDown class.
        /// </summary>
        public NumericUpDown() => this.InitializeComponent();

        /// <summary>
        /// Gets or sets the current value. This is a dependency property.
        /// </summary>
        public int Value
        {
            get => (int)this.GetValue(ValueProperty);
            set => this.SetValue(ValueProperty, value);
        }
        /// <summary>
        /// Gets or sets the minimum value. This is a dependency property.
        /// </summary>
        public int MinimumValue
        {
            get => (int)this.GetValue(MinimumValueProperty);
            set => this.SetValue(MinimumValueProperty, value);
        }
        /// <summary>
        /// Gets or sets the maximum value. This is a dependency property.
        /// </summary>
        public int MaximumValue
        {
            get => (int)this.GetValue(MaximumValueProperty);
            set => this.SetValue(MaximumValueProperty, value);
        }
        /// <summary>
        /// Gets or sets the increment/decrement value. This is a dependency property.
        /// </summary>
        public int StepValue
        {
            get => (int)this.GetValue(StepValueProperty);
            set => this.SetValue(StepValueProperty, value);
        }
        /// <summary>
        /// Gets or sets a value indicating whether the text box part of the control is read-only. This is a dependency property.
        /// </summary>
        public bool IsReadOnly
        {
            get => (bool)this.GetValue(IsReadOnlyProperty);
            set => this.SetValue(IsReadOnlyProperty, value);
        }

        private void upButton_Click(object sender, RoutedEventArgs e)
        {
            SetCurrentValue(ValueProperty, Math.Min(this.Value + this.StepValue, this.MaximumValue));
        }
        private void downButton_Click(object sender, RoutedEventArgs e)
        {
            SetCurrentValue(ValueProperty, Math.Max(this.Value - this.StepValue, this.MinimumValue));
        }
        private static void MinimumValue_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NumericUpDown control = (NumericUpDown)d;

            int newValue = (int)e.NewValue;
            if (newValue > control.Value)
                control.SetCurrentValue(ValueProperty, newValue);
        }
        private static void MaximumValue_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NumericUpDown control = (NumericUpDown)d;

            int newValue = (int)e.NewValue;
            if (newValue < control.Value)
                control.SetCurrentValue(ValueProperty, newValue);
        }
        private static void Value_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NumericUpDown control = (NumericUpDown)d;

            string text = control.valueText.Text;
            if (!string.IsNullOrEmpty(text))
            {
                int value;
                if (int.TryParse(text, out value) && value == (int)e.NewValue)
                    return;
            }

            control.valueText.Text = e.NewValue.ToString();
        }
        private static object Value_CoerceValue(DependencyObject d, object baseValue)
        {
            NumericUpDown control = (NumericUpDown)d;

            int value = (int)baseValue;
            if (value < control.MinimumValue)
                value = control.MinimumValue;
            if (value > control.MaximumValue)
                value = control.MaximumValue;

            return value;
        }
        private void valueText_KeyDown(object sender, KeyEventArgs e)
        {
            if (!(e.Key >= Key.D0 && e.Key <= Key.D9))
                e.Handled = true;
        }
        private void valueText_TextChanged(object sender, TextChangedEventArgs e)
        {
            string text = this.valueText.Text;
            if (!string.IsNullOrEmpty(text))
            {
                int value;
                if (int.TryParse(text, out value))
                {
                    if (this.Value != value)
                        SetCurrentValue(ValueProperty, value);
                }
            }
        }
    }
}
