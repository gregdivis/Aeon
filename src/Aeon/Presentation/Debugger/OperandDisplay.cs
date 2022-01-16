using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using Aeon.Emulator.DebugSupport;

namespace Aeon.Emulator.Launcher.Presentation.Debugger
{
    /// <summary>
    /// Visual representation of an instruction operand.
    /// </summary>
    internal sealed class OperandDisplay : ContentControl
    {
        /// <summary>
        /// The Operand dependency property definition.
        /// </summary>
        public static readonly DependencyProperty OperandProperty = DependencyProperty.Register("Operand", typeof(CodeOperand), typeof(OperandDisplay));
        /// <summary>
        /// The RegisterSource dependency property definition.
        /// </summary>
        public static readonly DependencyProperty RegisterSourceProperty = DependencyProperty.Register("RegisterSource", typeof(IRegisterContainer), typeof(OperandDisplay));
        /// <summary>
        /// The DebuggerTextFormat dependency property definition.
        /// </summary>
        public static readonly DependencyProperty DebuggerTextFormatProperty = AeonDebug.DebuggerTextFormatProperty.AddOwner(typeof(OperandDisplay));
        /// <summary>
        /// The Instruction dependency property definition.
        /// </summary>
        public static readonly DependencyProperty InstructionProperty = DependencyProperty.Register("Instruction", typeof(Instruction), typeof(OperandDisplay));
        /// <summary>
        /// The IsHexFormat dependency property definition.
        /// </summary>
        public static readonly DependencyProperty IsHexFormatProperty = AeonDebug.IsHexFormatProperty.AddOwner(typeof(OperandDisplay));

        private const PrefixState SegmentPrefixes = PrefixState.CS | PrefixState.DS | PrefixState.ES | PrefixState.FS | PrefixState.GS | PrefixState.SS;

        /// <summary>
        /// Initializes a new instance of the OperandDisplay class.
        /// </summary>
        public OperandDisplay()
        {
        }

        /// <summary>
        /// Gets or sets the operand to display. This is a dependency property.
        /// </summary>
        public CodeOperand Operand
        {
            get { return (CodeOperand)GetValue(OperandProperty); }
            set { SetValue(OperandProperty, value); }
        }
        /// <summary>
        /// Gets or sets the source for displayed register values. This is a dependency property.
        /// </summary>
        public IRegisterContainer RegisterSource
        {
            get { return (IRegisterContainer)GetValue(RegisterSourceProperty); }
            set { SetValue(RegisterSourceProperty, value); }
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
        /// Gets or sets a value indicating whether immediate values should be displayed in hexadecimal. This is a dependency property.
        /// </summary>
        public bool IsHexFormat
        {
            get { return (bool)GetValue(IsHexFormatProperty); }
            set { SetValue(IsHexFormatProperty, value); }
        }
        /// <summary>
        /// Gets or sets the instruction containing the displayed operand. This is a dependency property.
        /// </summary>
        public Instruction Instruction
        {
            get { return (Instruction)GetValue(InstructionProperty); }
            set { SetValue(InstructionProperty, value); }
        }

        /// <summary>
        /// Invoked when a property value has changed.
        /// </summary>
        /// <param name="e">Information about the event.</param>
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if(e.Property == OperandProperty || e.Property == AeonDebug.IsHexFormatProperty || e.Property == InstructionProperty)
                UpdateContent();
        }

        /// <summary>
        /// Returns a string representation of a segment register.
        /// </summary>
        /// <param name="prefixes">Segment register prefix.</param>
        /// <returns>String representation of the segment register.</returns>
        private static string GetSegmentPrefix(PrefixState prefixes)
        {
            if((prefixes & SegmentPrefixes) != 0)
                return (prefixes & SegmentPrefixes).ToString().ToLower();
            else
                return null;
        }

        /// <summary>
        /// Rebuilds the displayed content.
        /// </summary>
        private void UpdateContent()
        {
            if(this.Instruction != null)
                this.Content = BuildContent(this.Operand, this.DebuggerTextFormat);
            else
                this.Content = null;
        }
        /// <summary>
        /// Rebuilds the displayed content.
        /// </summary>
        /// <param name="operand">Operand to display.</param>
        /// <param name="format">Formatting information.</param>
        /// <returns>FrameworkElement to display as content.</returns>
        private FrameworkElement BuildContent(CodeOperand operand, IDebuggerTextSettings format)
        {
            switch(operand.Type)
            {
            case CodeOperandType.Immediate:
                return BuildImmediateContent(operand.ImmediateValue);

            case CodeOperandType.Register:
                return BuildRegisterContent(operand.RegisterValue);

            case CodeOperandType.MemoryAddress:
                return BuildAddress16Content(operand);

            case CodeOperandType.RelativeJumpAddress:
                return BuildJumpTargetContent((uint)(this.Instruction.EIP + this.Instruction.Length + (int)operand.ImmediateValue));
            }

            return null;
        }
        private FrameworkElement BuildJumpTargetContent(uint offset)
        {
            var textBlock = new TextBlock();
            var run = new Run(offset.ToString("X8"));
            run.Tag = new TargetAddress(QualifiedAddress.FromRealModeAddress(this.Instruction.CS, (ushort)offset), TargetAddressType.Code);

            var binding = new Binding("DebuggerTextFormat.Address") { Source = this, Mode = BindingMode.OneWay };
            run.SetBinding(TextElement.ForegroundProperty, binding);

            var link = new Hyperlink(run);
            textBlock.Inlines.Add(link);
            return textBlock;
        }
        private FrameworkElement BuildAddress16Content(CodeOperand operand)
        {
            bool includeDisplacement = true;
            var textBlock = new TextBlock();

            var prefixes = this.Instruction.Prefixes;
            if(operand.OperandSize == CodeOperandSize.Byte)
                textBlock.Inlines.Add("byte ptr ");
            else if(operand.OperandSize == CodeOperandSize.Word && (prefixes & PrefixState.OperandSize) == 0)
                textBlock.Inlines.Add("word ptr ");
            else
                textBlock.Inlines.Add("dword ptr ");

            var segmentOverride = GetSegmentPrefix(prefixes);
            if(segmentOverride != null)
            {
                textBlock.Inlines.Add(NewRegisterRun(segmentOverride));
                textBlock.Inlines.Add(":");
            }

            textBlock.Inlines.Add("[");

            switch(operand.EffectiveAddress)
            {
            case CodeMemoryBase.DisplacementOnly:
                textBlock.Inlines.Add(NewImmediateRun(operand.ImmediateValue.ToString("X4")));
                includeDisplacement = false;
                break;

            case CodeMemoryBase.BX_plus_SI:
                textBlock.Inlines.Add(NewRegisterRun("bx"));
                textBlock.Inlines.Add("+");
                textBlock.Inlines.Add(NewRegisterRun("si"));
                break;

            case CodeMemoryBase.BX_plus_DI:
                textBlock.Inlines.Add(NewRegisterRun("bx"));
                textBlock.Inlines.Add("+");
                textBlock.Inlines.Add(NewRegisterRun("di"));
                break;

            case CodeMemoryBase.BP_plus_SI:
                textBlock.Inlines.Add(NewRegisterRun("bp"));
                textBlock.Inlines.Add("+");
                textBlock.Inlines.Add(NewRegisterRun("si"));
                break;

            case CodeMemoryBase.BP_plus_DI:
                textBlock.Inlines.Add(NewRegisterRun("bp"));
                textBlock.Inlines.Add("+");
                textBlock.Inlines.Add(NewRegisterRun("di"));
                break;

            case CodeMemoryBase.SI:
                textBlock.Inlines.Add(NewRegisterRun("si"));
                break;

            case CodeMemoryBase.DI:
                textBlock.Inlines.Add(NewRegisterRun("di"));
                break;

            case CodeMemoryBase.BX:
                textBlock.Inlines.Add(NewRegisterRun("bx"));
                break;

            case CodeMemoryBase.BP:
                textBlock.Inlines.Add(NewRegisterRun("bp"));
                break;
            }

            if(includeDisplacement && operand.ImmediateValue != 0)
            {
                int offset = (short)(ushort)operand.ImmediateValue;
                textBlock.Inlines.Add(offset >= 0 ? "+" : "-");
                textBlock.Inlines.Add(NewDisplacementRun(Math.Abs(offset).ToString()));
            }

            textBlock.Inlines.Add("]");

            return textBlock;
        }
        private Run NewImmediateRun(string text)
        {
            var run = new Run(text);

            var binding = new Binding("DebuggerTextFormat.Immediate") { Source = this, Mode = BindingMode.OneWay };
            run.SetBinding(TextElement.ForegroundProperty, binding);

            return run;
        }
        private Run NewDisplacementRun(string text)
        {
            var run = new Run(text);

            var binding = new Binding("DebuggerTextFormat.Address") { Source = this, Mode = BindingMode.OneWay };
            run.SetBinding(TextElement.ForegroundProperty, binding);

            return run;
        }
        private Run NewRegisterRun(string text)
        {
            var run = new Run(text);

            var binding = new Binding("DebuggerTextFormat.Register") { Source = this, Mode = BindingMode.OneWay };
            run.SetBinding(TextElement.ForegroundProperty, binding);

            return run;
        }
        private FrameworkElement BuildImmediateContent(uint value)
        {
            var textBlock = new TextBlock();
            if(this.IsHexFormat)
                textBlock.Text = value.ToString("X");
            else
                textBlock.Text = value.ToString();

            var binding = new Binding("DebuggerTextFormat.Immediate") { Source = this, Mode = BindingMode.OneWay };
            textBlock.SetBinding(TextBlock.ForegroundProperty, binding);

            return textBlock;
        }
        private FrameworkElement BuildRegisterContent(CodeRegister register)
        {
            var textBlock = new TextBlock() { Text = register.ToString().ToLower() };

            var binding = new Binding("DebuggerTextFormat.Register") { Source = this, Mode = BindingMode.OneWay };
            textBlock.SetBinding(TextBlock.ForegroundProperty, binding);

            return textBlock;
        }
    }
}
