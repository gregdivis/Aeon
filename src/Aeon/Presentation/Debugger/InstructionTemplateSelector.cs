using System.Windows;
using System.Windows.Controls;
using Aeon.Emulator.DebugSupport;

namespace Aeon.Emulator.Launcher.Presentation.Debugger
{
    internal sealed class InstructionTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var element = (FrameworkElement)container;

            if(item is LoggedInstruction)
                return element.FindResource("loggedInstructionTemplate") as DataTemplate;
            else
                return element.FindResource("instructionTemplate") as DataTemplate;
        }
    }
}
