using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Aeon.Emulator.Launcher.Presentation.Dialogs
{
    /// <summary>
    /// A dialog which presents choices to the user.
    /// </summary>
    public partial class TaskDialog : Window
    {
        /// <summary>
        /// The Caption dependency property definition.
        /// </summary>
        public static readonly DependencyProperty CaptionProperty = DependencyProperty.Register("Caption", typeof(string), typeof(TaskDialog));
        /// <summary>
        /// The Items dependency property definition.
        /// </summary>
        public static readonly DependencyProperty ItemsProperty = DependencyProperty.Register("Items", typeof(IEnumerable<TaskDialogItem>), typeof(TaskDialog));

        /// <summary>
        /// Initializes a new instance of the TaskDialog class.
        /// </summary>
        public TaskDialog()
        {
            InitializeComponent();
            AddHandler(Button.ClickEvent, new RoutedEventHandler(this.Item_Click));
        }

        /// <summary>
        /// Gets or sets the caption text to display in the dialog. This is a dependency property.
        /// </summary>
        public string Caption
        {
            get { return (string)GetValue(CaptionProperty); }
            set { SetValue(CaptionProperty, value); }
        }
        /// <summary>
        /// Gets or sets the choices to display in the dialog. This is a dependency property.
        /// </summary>
        public IEnumerable<TaskDialogItem> Items
        {
            get { return (IEnumerable<TaskDialogItem>)GetValue(ItemsProperty); }
            set { SetValue(ItemsProperty, value); }
        }
        /// <summary>
        /// Gets the item that has been selected in the dialog.
        /// </summary>
        public TaskDialogItem SelectedItem { get; private set; }

        /// <summary>
        /// Invoked when a choice is clicked.
        /// </summary>
        /// <param name="source">Source of the event.</param>
        /// <param name="e">Information about the event.</param>
        private void Item_Click(object source, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.SelectedItem = e.OriginalSource as TaskDialogItem;
            this.Close();
        }
    }
}
