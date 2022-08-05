using System;

#nullable disable

namespace Aeon.Emulator.DebugSupport
{
    /// <summary>
    /// Describes a decoded instruction and its operands.
    /// </summary>
    public class Instruction
    {
        private readonly byte[] operandCodes = new byte[12];
        private string formattedValue;
        private uint offset;

        internal Instruction()
        {
        }
        /// <summary>
        /// Initializes a new instance of the Instruction class.
        /// </summary>
        /// <param name="opcodeInfo">Information about the instruction opcode.</param>
        /// <param name="rawCodes">Machine code for the instruction.</param>
        /// <param name="cs">Segment where instruction was decoded from.</param>
        /// <param name="ip">Offset where instruction was decoded from.</param>
        /// <param name="bigMode">Indicates whether the instruction should be decoded in big (32-bit) mode.</param>
        internal Instruction(OpcodeInfo opcodeInfo, byte[] rawCodes, ushort cs, uint ip, bool bigMode)
        {
            if (opcodeInfo != null)
            {
                for (int i = 0; i < 12; i++)
                    this.operandCodes[i] = rawCodes[opcodeInfo.Length + i];
            }

            this.Opcode = opcodeInfo;
            this.CS = cs;
            this.offset = ip;
            this.BigMode = bigMode;
        }

        /// <summary>
        /// Assigns new values to the instruction instance.
        /// </summary>
        /// <param name="opcodeInfo">Information about the instruction opcode.</param>
        /// <param name="rawCodes">Operand data starting with the first byte after the opcode.</param>
        /// <param name="cs">Code segment of the instruction.</param>
        /// <param name="ip">Offset in the current code segment of the instruction.</param>
        /// <param name="bigMode">Indicates whether the instruction should be decoded in big (32-bit) mode.</param>
        internal void Assign(OpcodeInfo opcodeInfo, IntPtr rawCodes, ushort cs, uint ip, bool bigMode)
        {
            unsafe
            {
                byte* ptr = (byte*)rawCodes.ToPointer();
                for (int i = 0; i < this.operandCodes.Length; i++)
                    this.operandCodes[i] = ptr[i];
            }

            this.Opcode = opcodeInfo;
            this.CS = cs;
            this.offset = ip;
            this.BigMode = bigMode;
            this.formattedValue = null;
        }

        /// <summary>
        /// Gets the decoded operands of the instruction.
        /// </summary>
        public DecodedOperands Operands
        {
            get
            {
                if (this.Opcode != null)
                    return InstructionDecoder.Decode(this.Opcode, this.operandCodes, this.ComplementedPrefixes);
                else
                    return new DecodedOperands();
            }
        }
        /// <summary>
        /// Gets additional information about the instruction opcode.
        /// </summary>
        public OpcodeInfo Opcode { get; private set; }
        /// <summary>
        /// Gets the segment where the instruction was found.
        /// </summary>
        public ushort CS { get; private set; }
        /// <summary>
        /// Gets the offset where the instruction was found.
        /// </summary>
        public uint EIP
        {
            get
            {
                uint size = 0;
                if ((this.Prefixes & PrefixState.OperandSize) != 0)
                    size++;
                if ((this.Prefixes & PrefixState.AddressSize) != 0)
                    size++;
                if ((this.Prefixes & AddressFormatter.SegmentOverrideMask) != 0)
                    size++;
                if ((this.Prefixes & (PrefixState.Repe | PrefixState.Repne)) != 0)
                    size++;

                return this.offset - size;
            }
        }
        /// <summary>
        /// Gets the prefixes in effect for the instruction.
        /// </summary>
        public PrefixState Prefixes { get; internal set; }
        /// <summary>
        /// Gets a value indicating whether the instruction is in big mode.
        /// </summary>
        public bool BigMode { get; private set; }
        /// <summary>
        /// Gets the length of the instruction in bytes.
        /// </summary>
        public int Length
        {
            get
            {
                try
                {
                    return CalculateLength(true);
                }
                catch (ArgumentException)
                {
                    return 1;
                }
            }
        }
        /// <summary>
        /// Gets the length of the instruction in bytes (not including prefixes).
        /// </summary>
        public int UnprefixedLength
        {
            get
            {
                try
                {
                    return CalculateLength(false);
                }
                catch (ArgumentException)
                {
                    return 1;
                }
            }
        }

        /// <summary>
        /// Gets the prefixes in effect for the instruction, complementing operand size and address size first for big mode.
        /// </summary>
        protected PrefixState ComplementedPrefixes
        {
            get
            {
                if (this.BigMode)
                    return this.Prefixes ^ (PrefixState.OperandSize | PrefixState.AddressSize);
                else
                    return this.Prefixes;
            }
        }

        /// <summary>
        /// Gets a string representation of the instruction.
        /// </summary>
        /// <returns>String representation of the instruction.</returns>
        public override string ToString()
        {
            if (this.Opcode != null)
            {
                if (this.formattedValue == null)
                    this.formattedValue = string.Format("{0:X4}:{1:X8} {2} {3}", this.CS, this.EIP, this.Opcode.Name, this.Operands.ToString((int)this.EIP + this.Length, this.ComplementedPrefixes));

                return this.formattedValue;
            }
            else
            {
                return "???";
            }
        }

        private int CalculateLength(bool includePrefixes)
        {
            if (this.Opcode == null)
                return 1;

            int size = 0;
            if (includePrefixes)
            {
                if ((this.Prefixes & PrefixState.OperandSize) != 0)
                    size++;
                if ((this.Prefixes & PrefixState.AddressSize) != 0)
                    size++;
                if ((this.Prefixes & AddressFormatter.SegmentOverrideMask) != 0)
                    size++;
                if ((this.Prefixes & (PrefixState.Repe | PrefixState.Repne)) != 0)
                    size++;
            }

            return this.Opcode.Length + InstructionDecoder.CalculateOperandLength(this.Opcode, this.operandCodes, this.ComplementedPrefixes) + size;
        }
    }
}
