using System;
using System.Reflection;
using Aeon.Emulator.Decoding;

namespace Aeon.Emulator
{
    /// <summary>
    /// Contains information about a decoded instruction.
    /// </summary>
    public sealed class OpcodeInfo : IEquatable<OpcodeInfo>
    {
        /// <summary>
        /// The instruction's opcode.
        /// </summary>
        public readonly ushort Opcode;
        /// <summary>
        /// The instruction's ModR/M value information.
        /// </summary>
        public readonly ModRmInfo ModRmInfo;
        /// <summary>
        /// Gets performance statistics for the opcode.
        /// </summary>
        public readonly OpcodeInstrumentation Statistics = new();

        internal readonly DecodeAndEmulate[] Emulators;

        private readonly byte extendedOpcode;

        internal OpcodeInfo(InstructionInfo instInfo)
        {
            this.Opcode = instInfo.Opcode;
            this.ModRmInfo = instInfo.ModRmByte;
            this.extendedOpcode = instInfo.ExtendedRmOpcode;
            this.Operands = instInfo.Operands;
            this.Emulators = instInfo.NewEmulators;
            this.Name = instInfo.Name;
            this.IsPrefix = instInfo.IsPrefix;
            this.Length = instInfo.IsMultiByte ? 2 : 1;
            this.EmulateMethods = instInfo.EmulateMethods;
        }

        /// <summary>
        /// Gets the decoded instruction's operands.
        /// </summary>
        public OperandFormat Operands { get; }
        /// <summary>
        /// Gets the name of the instruction.
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// Gets a value indicating whether the instruction is a prefix.
        /// </summary>
        public bool IsPrefix { get; }
        /// <summary>
        /// Gets the length of the opcode in bytes.
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// Gets the descriptors for the method which perform the emulation of this opcode.
        /// </summary>
        public MethodInfo[] EmulateMethods { get; }

        /// <summary>
        /// Tests for equality with another OpcodeInfo instance.
        /// </summary>
        /// <param name="other">Other OpcodeInfo instance to test.</param>
        /// <returns>True if objects are equal; otherwise false.</returns>
        public bool Equals(OpcodeInfo other) => this.Opcode == other.Opcode && this.ModRmInfo == other.ModRmInfo;
        /// <summary>
        /// Tests for equality with another object.
        /// </summary>
        /// <param name="obj">Other object to test.</param>
        /// <returns>True if objects are equal; otherwise false.</returns>
        public override bool Equals(object obj) => obj is OpcodeInfo other && this.Equals(other);
        /// <summary>
        /// Gets a hash code for the OpcodeInfo instance.
        /// </summary>
        /// <returns>Hash code for the OpcodeInfo instance.</returns>
        public override int GetHashCode() => this.Opcode.GetHashCode();
        /// <summary>
        /// Gets a formatted string representation of the OpcodeInfo instance.
        /// </summary>
        /// <returns>Formatted string representation of the OpcodeInfo instance.</returns>
        public override string ToString()
        {
            if (ModRmInfo == ModRmInfo.OnlyRm)
            {
                if (this.Opcode <= 0xFF)
                    return $"{this.Name} ({Opcode:X2}/{extendedOpcode})";
                else
                    return $"{this.Name} ({Opcode:X4}/{extendedOpcode})";
            }
            else if (this.Opcode <= 0xFF)
                return $"{this.Name} ({Opcode:X2})";
            else
                return $"{this.Name} ({this.Opcode:X4})";
        }
        /// <summary>
        /// Gets the flow direction of an operand.
        /// </summary>
        /// <param name="operandIndex">Index of the operand.</param>
        /// <returns>Flow direction of the operand.</returns>
        public CodeOperandFlow GetOperandFlowDirection(int operandIndex)
        {
            var info = this.EmulateMethods[0] ?? this.EmulateMethods[1] ?? this.EmulateMethods[2] ?? this.EmulateMethods[3];
            var args = info.GetParameters();
            var i = operandIndex + 1;
            if (i < args.Length)
            {
                if (args[i].IsOut)
                    return CodeOperandFlow.Out;
                else if (args[i].ParameterType.IsByRef)
                    return CodeOperandFlow.InOut;
                else
                    return CodeOperandFlow.In;
            }

            throw new ArgumentException("Invalid operand index.");
        }
    }
}
