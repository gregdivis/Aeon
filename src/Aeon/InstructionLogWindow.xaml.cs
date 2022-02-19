using System;
using System.Buffers.Binary;
using System.Globalization;
using System.Windows;
using Aeon.Emulator.Memory;

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

        private bool TryReadAddress(out ushort segment, out uint offset)
        {
            segment = 0;
            offset = 0;

            var parts = this.gotoAddressBox.Text.Trim().Split(':');
            if (parts.Length != 2)
                return false;

            if (!ushort.TryParse(parts[0], NumberStyles.HexNumber, null, out segment))
                return false;

            if (!uint.TryParse(parts[1], NumberStyles.HexNumber, null, out offset))
                return false;

            return true;
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

        private void NextAddress_Click(object sender, RoutedEventArgs e)
        {
            if (!this.TryReadAddress(out ushort segment, out uint offset))
                return;

            var log = (LogAccessor)this.historyList.ItemsSource;
            int i = 0;
            int selectedIndex = this.historyList.SelectedIndex;

            foreach (var item in log)
            {
                if (i > selectedIndex && item.CS == segment && item.EIP == offset)
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
