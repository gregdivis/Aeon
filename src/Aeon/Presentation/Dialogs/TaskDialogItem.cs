using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Aeon.Emulator.Launcher.Presentation.Dialogs
{
    /// <summary>
    /// Represents an item in a task dialog.
    /// </summary>
    public class TaskDialogItem : Button
    {
        /// <summary>
        /// The Icon dependency property definition.
        /// </summary>
        public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(ImageSource), typeof(TaskDialogItem));
        /// <summary>
        /// The Text depdendency property definition.
        /// </summary>
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(TaskDialogItem));
        /// <summary>
        /// The Description dependency property definition.
        /// </summary>
        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register("Description", typeof(string), typeof(TaskDialogItem));

        /// <summary>
        /// Initializes a new instance of the TaskDialogItem class.
        /// </summary>
        public TaskDialogItem()
        {
            this.BeginInit();
            //this.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri("pack://application:,,,/Aeon.Presentation;component/Dialogs/TaskDialogTemplates.xaml", UriKind.Absolute) });
            //this.Style = (Style)FindResource("taskDialogItemStyle");
            this.EndInit();
        }
        /// <summary>
        /// Initializes a new instance of the TaskDialogItem class.
        /// </summary>
        /// <param name="text">The text to display.</param>
        /// <param name="description">The description to display.</param>
        public TaskDialogItem(string text, string description)
            : this()
        {
            this.Text = text;
            this.Description = description;
        }

        /// <summary>
        /// Gets or sets the icon to display. This is a dependency property.
        /// </summary>
        public ImageSource Icon
        {
            get { return (ImageSource)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }
        /// <summary>
        /// Gets or sets the text to display. This is a dependency property.
        /// </summary>
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
        /// <summary>
        /// Gets or sets the description to display. This is a dependency property.
        /// </summary>
        public string Description
        {
            get { return (string)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }
    }
}
