using System.Windows;
using System.Windows.Controls;
using Aeon.Emulator.DebugSupport;

namespace Aeon.Presentation.Debugger
{
    /// <summary>
    /// Selects the appropriate style for an instruction.
    /// </summary>
    internal sealed class InstructionStyleSelector : StyleSelector
    {
        /// <summary>
        /// When overridden in a derived class, returns a <see cref="T:System.Windows.Style"/> based on custom logic.
        /// </summary>
        /// <param name="item">The content.</param>
        /// <param name="container">The element to which the style will be applied.</param>
        /// <returns>
        /// Returns an application-specific style to apply; otherwise, null.
        /// </returns>
        public override Style SelectStyle(object item, DependencyObject container)
        {
            if(item is LoggedInstruction)
                return (Style)((FrameworkElement)container).FindResource("loggedInstructionStyle");
            else
                return (Style)((FrameworkElement)container).FindResource("listBoxItemStyle");
        }
    }
}
