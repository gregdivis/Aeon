using System;
using System.Collections.Generic;
using System.ComponentModel;
using Aeon.Emulator.Decoding;

namespace Aeon.Emulator.DebugSupport
{
    /// <summary>
    /// Generates x86 disassembly from an arbitrary source.
    /// </summary>
    public sealed class Disassembler : INotifyPropertyChanged, IDataErrorInfo
    {
        private readonly IMachineCodeSource source;
        private ushort cs;
        private uint? csBase;
        private uint eip;
        private int maximum;

        /// <summary>
        /// Initializes a new instance of the <see cref="Disassembler"/> class.
        /// </summary>
        /// <param name="memory"><see cref="IMachineCodeSource"/> containing instructions to decode.</param>
        public Disassembler(IMachineCodeSource source) => this.source = source ?? throw new ArgumentNullException(nameof(source));

        /// <summary>
        /// Occurs when a property value has changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets or sets the starting code segment for instruction decoding.
        /// </summary>
        public ushort StartSegment
        {
            get => this.cs;
            set
            {
                if (this.cs != value)
                {
                    this.cs = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(StartSegment)));
                }

                this.SegmentBaseAddress = this.source.GetBaseAddress(this.cs);
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(Instructions)));
            }
        }
        /// <summary>
        /// Gets or sets the starting code offset for instruction decoding.
        /// </summary>
        public uint StartOffset
        {
            get => this.eip;
            set
            {
                if (this.eip != value)
                {
                    this.eip = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(StartOffset)));
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(Instructions)));
                }
            }
        }
        /// <summary>
        /// Gets the base address calculated from the segment.
        /// </summary>
        public uint? SegmentBaseAddress
        {
            get => this.csBase;
            private set
            {
                if (this.csBase != value)
                {
                    this.csBase = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(SegmentBaseAddress)));
                }
            }
        }
        /// <summary>
        /// Gets or sets the maximum number of instructions to decode.
        /// </summary>
        public int MaximumInstructions
        {
            get => this.maximum;
            set
            {
                if (this.maximum != value)
                {
                    this.maximum = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(MaximumInstructions)));
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(Instructions)));
                }
            }
        }
        /// <summary>
        /// Gets the disassembled instructions.
        /// </summary>
        public IEnumerable<Instruction> Instructions => Disassemble();
        string IDataErrorInfo.Error
        {
            get
            {
                if (((IDataErrorInfo)this)[nameof(StartSegment)] != string.Empty)
                    return ((IDataErrorInfo)this)[nameof(StartSegment)];
                if (((IDataErrorInfo)this)[nameof(MaximumInstructions)] != string.Empty)
                    return ((IDataErrorInfo)this)[nameof(MaximumInstructions)];

                return string.Empty;
            }
        }

        string IDataErrorInfo.this[string columnName]
        {
            get
            {
                if (columnName == nameof(StartSegment))
                {
                    if (this.SegmentBaseAddress == null)
                        return "Invalid code segment.";
                }
                else if (columnName == nameof(MaximumInstructions))
                {
                    if (this.MaximumInstructions <= 0)
                        return "Maximum instructions must be greater than zero.";
                }

                return string.Empty;
            }
        }

        private static PrefixState GetPrefix(OpcodeInfo opcode)
        {
            switch (opcode.Opcode)
            {
                case 0x2E:
                    return PrefixState.CS;

                case 0x36:
                    return PrefixState.SS;

                case 0x3E:
                    return PrefixState.DS;

                case 0x26:
                    return PrefixState.ES;

                case 0x64:
                    return PrefixState.FS;

                case 0x65:
                    return PrefixState.GS;

                case 0x66:
                    return PrefixState.OperandSize;

                case 0x67:
                    return PrefixState.AddressSize;

                case 0xF2:
                    return PrefixState.Repne;

                case 0xF3:
                    return PrefixState.Repe;
            }

            return PrefixState.None;
        }

        private IEnumerable<Instruction> Disassemble()
        {
            if (this.csBase == null)
                yield break;

            byte[] instBuffer = new byte[16];
            uint baseAddress = (uint)this.csBase;
            uint offset = this.eip;
            var prefixes = PrefixState.None;

            for (int i = 0; i < this.maximum; i++)
            {
                this.source.ReadInstruction(instBuffer, baseAddress + offset);
                var opcode = InstructionSet.Decode(instBuffer);
                var inst = new Instruction(opcode, instBuffer, this.cs, offset, false) { Prefixes = prefixes };
                if (opcode != null && opcode.IsPrefix)
                {
                    prefixes |= GetPrefix(opcode);
                    i--;
                }
                else
                {
                    prefixes = PrefixState.None;
                    yield return inst;
                }

                offset += (uint)inst.UnprefixedLength;
            }
        }
        private void OnPropertyChanged(PropertyChangedEventArgs e) => this.PropertyChanged?.Invoke(this, e);
    }
}
