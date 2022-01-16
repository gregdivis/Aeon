using System.Windows;
using Aeon.Emulator.DebugSupport;
using Aeon.Emulator.Launcher.Presentation.Debugger;

namespace Aeon.Emulator.Launcher
{
    public partial class DebuggerWindow : Window
    {
        public static readonly DependencyProperty EmulatorHostProperty = DependencyProperty.Register("EmulatorHost", typeof(EmulatorHost), typeof(DebuggerWindow));
        public static readonly DependencyProperty IsHexFormatProperty = AeonDebug.IsHexFormatProperty.AddOwner(typeof(DebuggerWindow));
        public static readonly DependencyProperty InstructionLogProperty = DependencyProperty.Register("InstructionLog", typeof(InstructionLog), typeof(DebuggerWindow));

        private Disassembler disassembler;

        public DebuggerWindow() => this.InitializeComponent();

        public EmulatorHost EmulatorHost
        {
            get => (EmulatorHost)this.GetValue(EmulatorHostProperty);
            set => this.SetValue(EmulatorHostProperty, value);
        }
        public bool IsHexFormat
        {
            get => (bool)this.GetValue(IsHexFormatProperty);
            set => this.SetValue(IsHexFormatProperty, value);
        }
        public InstructionLog InstructionLog
        {
            get => (InstructionLog)this.GetValue(InstructionLogProperty);
            set => this.SetValue(InstructionLogProperty, value);
        }

        public void UpdateDebugger()
        {
            var vm = this.EmulatorHost.VirtualMachine;

            this.disassembler = new Disassembler(this.EmulatorHost.VirtualMachine)
            {
                StartSegment = vm.Processor.CS,
                StartOffset = vm.Processor.EIP,
                MaximumInstructions = 1000
            };

            var disasm = this.disassembler.Instructions;
            this.disassemblyView.InstructionsSource = disasm;
            this.registerView.RegisterSource = vm.Processor;
            this.memoryView.MemorySource = vm.PhysicalMemory;
        }
    }
}
