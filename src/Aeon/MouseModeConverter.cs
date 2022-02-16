using System;
using System.Windows.Data;

namespace Aeon.Emulator.Launcher
{
    internal sealed class MouseModeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var mouseMode = (MouseInputMode)value;
            return mouseMode == MouseInputMode.Absolute;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool b)
                return b ? MouseInputMode.Absolute : MouseInputMode.Relative;
            else
                return MouseInputMode.Relative;
        }
    }
}
