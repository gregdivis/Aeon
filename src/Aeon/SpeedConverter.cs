using System;
using System.Globalization;
using System.Windows.Data;

namespace Aeon.Emulator.Launcher
{
    internal sealed class SpeedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int d)
            {
                var mhz = (decimal)d / 1_000_000;
                return mhz.ToString("0.#") + "MHz";
            }
            else
            {
                return string.Empty;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();
    }
}
