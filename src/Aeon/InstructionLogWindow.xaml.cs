using System.Globalization;
using System.Windows;

namespace Aeon.Emulator.Launcher
{
    internal sealed partial class InstructionLogWindow : Window
    {
        public InstructionLogWindow() => this.InitializeComponent();

        public static void ShowDialog(LogAccessor log)
        {
            var window = new InstructionLogWindow { Owner = App.Current.MainWindow };
            window.historyList.ItemsSource = log;
            window.Show();
        }

        private void HistoryList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (this.historyList.SelectedItem is DebugLogItem item)
                this.registerText.Text = item.RegisterText;
        }
        private void FindNextError_Click(object sender, RoutedEventArgs e)
        {
            if (this.historyList.ItemsSource is LogAccessor log)
            {
                int index = log.FindNextError(this.historyList.SelectedIndex + 1);
                if (index >= 0)
                {
                    this.historyList.SelectedIndex = index;
                    this.historyList.ScrollIntoView(this.historyList.SelectedItem);
                }
            }
        }

        private void GotoAddressBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            var parts = this.gotoAddressBox.Text.Trim().Split(':');
            if (parts.Length == 2)
            {
                var segment = ushort.Parse(parts[0], NumberStyles.HexNumber);
                uint offset = uint.Parse(parts[1], NumberStyles.HexNumber);
                var log = (LogAccessor)this.historyList.ItemsSource;
                int i = 0;
                foreach (var item in log)
                {
                    if (item.CS == segment && item.EIP == offset)
                    {
                        this.historyList.SelectedIndex = i;
                        this.historyList.ScrollIntoView(item);
                        return;
                    }

                    i++;
                }
            }
        }
    }
}
