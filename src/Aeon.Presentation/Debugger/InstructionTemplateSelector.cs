using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using Aeon.Emulator.DebugSupport;

namespace Aeon.Presentation.Debugger
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
