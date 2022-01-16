using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Aeon.Emulator.DebugSupport;

namespace Aeon.Presentation.Debugger
{
    /// <summary>
    /// Provides a view of disassembled machine code instructions.
    /// </summary>
    public partial class DisassemblyView : UserControl
    {
        /// <summary>
        /// The AddressClick routed event definition.
        /// </summary>
        public static readonly RoutedEvent AddressClickEvent = EventManager.RegisterRoutedEvent("AddressClick", RoutingStrategy.Bubble, typeof(EventHandler<AddressClickEventArgs>), typeof(DisassemblyView));
        /// <summary>
        /// The InstructionsSource dependency property definition.
        /// </summary>
        public static readonly DependencyProperty InstructionsSourceProperty = DependencyProperty.Register("InstructionsSource", typeof(IEnumerable<Instruction>), typeof(DisassemblyView));
        /// <summary>
        /// The IsHexFormat dependency property definition.
        /// </summary>
        public static readonly DependencyProperty IsHexFormatProperty = AeonDebug.IsHexFormatProperty.AddOwner(typeof(DisassemblyView));
        /// <summary>
        /// The DebuggerTextFormat dependency property definition.
        /// </summary>
        public static readonly DependencyProperty DebuggerTextFormatProperty = AeonDebug.DebuggerTextFormatProperty.AddOwner(typeof(DisassemblyView));

        /// <summary>
        /// Initializes a new instance of the DisassemblyView class.
        /// </summary>
        public DisassemblyView()
        {
            InitializeComponent();
            AddHandler(Hyperlink.ClickEvent, new RoutedEventHandler(this.Hyperlink_Click));
        }

        /// <summary>
        /// Occurs when an address has been clicked. This is a dependency property.
        /// </summary>
        public event EventHandler<AddressClickEventArgs> AddressClick
        {
            add { AddHandler(AddressClickEvent, value); }
            remove { RemoveHandler(AddressClickEvent, value); }
        }

        /// <summary>
        /// Gets or sets the instructions used. This is a dependency property.
        /// </summary>
        public IEnumerable<Instruction> InstructionsSource
        {
            get { return (IEnumerable<Instruction>)GetValue(InstructionsSourceProperty); }
            set { SetValue(InstructionsSourceProperty, value); }
        }
        /// <summary>
        /// Gets or sets a value indicating whether immediate values should be displayed in hexadecimal. This is a dependency property.
        /// </summary>
        public bool IsHexFormat
        {
            get { return (bool)GetValue(IsHexFormatProperty); }
            set { SetValue(IsHexFormatProperty, value); }
        }
        /// <summary>
        /// Gets or sets formatting information. This is a dependency property.
        /// </summary>
        public IDebuggerTextSettings DebuggerTextFormat
        {
            get { return (IDebuggerTextSettings)GetValue(DebuggerTextFormatProperty); }
            set { SetValue(DebuggerTextFormatProperty, value); }
        }

        /// <summary>
        /// Raises the AddressClick event.
        /// </summary>
        /// <param name="e">Information about the event.</param>
        protected virtual void OnAddressClick(AddressClickEventArgs e)
        {
            RaiseEvent(e);

            if(!e.Handled)
            {
                if(e.Target.AddressType == TargetAddressType.Code)
                {
                    var disasm = this.InstructionsSource;
                    var inst = disasm.Where(i => i.EIP == e.Target.Address.Offset && i.CS == e.Target.Address.Segment).FirstOrDefault();
                    if(inst != null)
                    {
                        this.listBox.SelectedItem = inst;
                        this.listBox.ScrollIntoView(inst);
                    }
                }
            }
        }

        /// <summary>
        /// Invoked when a hyperlink has been clicked.
        /// </summary>
        /// <param name="source">Source of the event.</param>
        /// <param name="e">Information about the event.</param>
        private void Hyperlink_Click(object source, RoutedEventArgs e)
        {
            var hyperlink = e.OriginalSource as Hyperlink;
            if(hyperlink == null)
                return;

            var address = hyperlink.Tag as TargetAddress;
            if(address == null)
                return;

            e.Handled = true;
            OnAddressClick(new AddressClickEventArgs(address, AddressClickEvent));
        }
    }

    /// <summary>
    /// Contains information about a clicked address.
    /// </summary>
    public sealed class AddressClickEventArgs : RoutedEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the AddressClickEventArgs class.
        /// </summary>
        /// <param name="address">Target address clicked.</param>
        /// <param name="routedEvent">The owner event.</param>
        public AddressClickEventArgs(TargetAddress address, RoutedEvent routedEvent)
            : base(routedEvent)
        {
            this.Target = address;
        }

        /// <summary>
        /// Gets the target address.
        /// </summary>
        public TargetAddress Target { get; private set; }
    }
}
