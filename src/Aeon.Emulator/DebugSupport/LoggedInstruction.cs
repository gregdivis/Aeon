using System;

namespace Aeon.Emulator.DebugSupport
{
    /// <summary>
    /// Describes a decoded instruction, its operands, and the state of the emulated
    /// processor before executing the instruction.
    /// </summary>
    public sealed class LoggedInstruction : Instruction, IRegisterContainer
    {
        internal LoggedInstruction()
        {
        }

        /// <summary>
        /// Gets the value of the EAX register.
        /// </summary>
        public uint EAX { get; private set; }
        /// <summary>
        /// Gets the value of the EBX register.
        /// </summary>
        public uint EBX { get; private set; }
        /// <summary>
        /// Gets the value of the ECX register.
        /// </summary>
        public uint ECX { get; private set; }
        /// <summary>
        /// Gets the value of the EDX register.
        /// </summary>
        public uint EDX { get; private set; }
        /// <summary>
        /// Gets the value of the ESI register.
        /// </summary>
        public uint ESI { get; private set; }
        /// <summary>
        /// Gets the value of the EDI register.
        /// </summary>
        public uint EDI { get; private set; }
        /// <summary>
        /// Gets the value of the EBP register.
        /// </summary>
        public uint EBP { get; private set; }
        /// <summary>
        /// Gets the value of the DS register.
        /// </summary>
        public ushort DS { get; private set; }
        /// <summary>
        /// Gets the value of the ES register.
        /// </summary>
        public ushort ES { get; private set; }
        /// <summary>
        /// Gets the value of the FS register.
        /// </summary>
        public ushort FS { get; private set; }
        /// <summary>
        /// Gets the value of the GS register.
        /// </summary>
        public ushort GS { get; private set; }
        /// <summary>
        /// Gets the value of the SS register.
        /// </summary>
        public ushort SS { get; private set; }
        /// <summary>
        /// Gets the value of the ESP register.
        /// </summary>
        public uint ESP { get; private set; }
        /// <summary>
        /// Gets the value of the flags register.
        /// </summary>
        public EFlags Flags { get; private set; }
        /// <summary>
        /// Gets the value of the CR0 register.
        /// </summary>
        public CR0 CR0 { get; private set; }

        internal void Assign(OpcodeInfo opcodeInfo, IntPtr rawCodes, Processor processor, uint ip)
        {
            base.Assign(opcodeInfo, rawCodes, processor.CS, ip, processor.GlobalSize != 0);

            this.EAX = (uint)processor.EAX;
            this.EBX = (uint)processor.EBX;
            this.ECX = (uint)processor.ECX;
            this.EDX = (uint)processor.EDX;
            this.ESI = processor.ESI;
            this.EDI = processor.EDI;
            this.EBP = processor.EBP;
            this.DS = processor.DS;
            this.ES = processor.ES;
            this.FS = processor.FS;
            this.GS = processor.GS;
            this.SS = processor.SS;
            this.ESP = processor.ESP;
            this.Flags = processor.Flags.Value & ~EFlags.Reserved1;
            this.CR0 = processor.CR0;
        }
    }
}
