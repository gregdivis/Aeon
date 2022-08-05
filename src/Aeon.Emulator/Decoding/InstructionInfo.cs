using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

#nullable disable

namespace Aeon.Emulator.Decoding
{
    internal struct InstructionInfo
    {
        private static readonly Dictionary<string, OperandType> operandStrings = new Dictionary<string, OperandType>();
        private static readonly Dictionary<int, OperandFormat> operandFormats = new Dictionary<int, OperandFormat>();

        static InstructionInfo()
        {
            var fields = typeof(OperandType).GetFields(BindingFlags.Static | BindingFlags.Public);
            foreach (var field in fields)
            {
                var customAttributes = field.GetCustomAttributes(typeof(OperandStringAttribute), false);
                if (customAttributes != null && customAttributes.Length == 1)
                {
                    var operandStringAttribute = (OperandStringAttribute)customAttributes[0];
                    operandStrings.Add(operandStringAttribute.OperandString, (OperandType)field.GetValue(null));
                }
            }
        }

        public ushort Opcode { get; set; }
        public OperandFormat Operands { get; set; }
        public ModRmInfo ModRmByte { get; set; }
        public byte ExtendedRmOpcode { get; set; }
        public int RPlusIndex
        {
            get
            {
                if (ModRmByte == ModRmInfo.RegisterPlus)
                    return this.Operands.IndexOfAny(new[] { OperandType.RegisterST, OperandType.RegisterByte, OperandType.RegisterWord });

                return -1;
            }
        }
        public MethodInfo[] EmulateMethods { get; set; }
        public DecodeAndEmulate[] NewEmulators { get; set; }
        public string Name { get; set; }
        public bool IsPrefix { get; set; }
        public bool IsMultiByte { get; set; }
        public bool IsAffectedByOperandSize => this.Operands.IndexOfAny(new[] { OperandType.ImmediateFarPointer, OperandType.ImmediateRelativeWord, OperandType.ImmediateWord, OperandType.IndirectFarPointer, OperandType.MemoryOffsetWord, OperandType.RegisterOrMemoryWord, OperandType.RegisterOrMemoryWordNearPointer, OperandType.RegisterWord, OperandType.EffectiveAddress, OperandType.FullLinearAddress }) >= 0;
        public bool IsAffectedByAddressSize => this.Operands.IndexOfAny(new[] { OperandType.IndirectFarPointer, OperandType.MemoryInt16, OperandType.MemoryInt32, OperandType.MemoryFloat32, OperandType.MemoryFloat64, OperandType.MemoryFloat80, OperandType.MemoryInt64, OperandType.MemoryOffsetByte, OperandType.MemoryOffsetWord, OperandType.RegisterOrMemory16, OperandType.RegisterOrMemory32, OperandType.RegisterOrMemoryByte, OperandType.RegisterOrMemoryWord, OperandType.RegisterOrMemoryWordNearPointer, OperandType.EffectiveAddress, OperandType.FullLinearAddress }) >= 0;

        public static InstructionInfo Parse(string instructionFormat)
        {
            var info = new InstructionInfo();

            var outer = instructionFormat.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            string[] innerCode;
            if (!outer[0].Contains('+'))
                innerCode = outer[0].Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            else
                innerCode = new string[2] { outer[0].TrimEnd('+'), "+" };
            info.Opcode = ushort.Parse(innerCode[0], System.Globalization.NumberStyles.HexNumber);
            if (info.Opcode > 0xFF)
            {
                info.Opcode = (ushort)(((info.Opcode & 0xFF) << 8) | ((info.Opcode >> 8) & 0xFF));
                info.IsMultiByte = true;
            }

            if (innerCode.Length > 1)
            {
                if (innerCode[1][0] == 'r')
                {
                    info.ModRmByte = ModRmInfo.All;
                }
                else if (innerCode[1][0] == '+')
                {
                    info.ModRmByte = ModRmInfo.RegisterPlus;
                }
                else
                {
                    info.ModRmByte = ModRmInfo.OnlyRm;
                    info.ExtendedRmOpcode = byte.Parse(new string(innerCode[1][0], 1));
                }
            }

            if (outer.Length > 1)
            {
                string[] innerOperands = outer[1].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                int operandCode = 0;

                operandCode = (int)ParseOperand(innerOperands[0]);

                if (innerOperands.Length > 1)
                {
                    operandCode |= (int)ParseOperand(innerOperands[1]) << 8;

                    if (innerOperands.Length > 2)
                        operandCode |= (int)ParseOperand(innerOperands[2]) << 16;
                }

                if (!operandFormats.TryGetValue(operandCode, out var format))
                {
                    format = new OperandFormat(operandCode);
                    operandFormats.Add(operandCode, format);
                }

                info.Operands = format;
            }
            else
            {
                if (!operandFormats.TryGetValue(0, out var format))
                {
                    format = new OperandFormat(0);
                    operandFormats.Add(0, format);
                }

                info.Operands = format;
            }

            return info;
        }

        public IEnumerable<InstructionInfo> Expand()
        {
            if (this.ModRmByte != ModRmInfo.RegisterPlus)
            {
                yield return this;
                yield break;
            }

            for (int r = 0; r < 8; r++)
            {
                var subInst = this;
                subInst.ModRmByte = ModRmInfo.None;
                if (!subInst.IsMultiByte)
                    subInst.Opcode += (ushort)r;
                else
                    subInst.Opcode += (ushort)(r << 8);

                int codes = this.Operands.PackedCode & ~(0xFF << (this.RPlusIndex * 8));
                codes |= (int)GetRegisterIndex(r, this.Operands[this.RPlusIndex]) << (this.RPlusIndex * 8);
                if (!operandFormats.TryGetValue(codes, out var operands))
                {
                    operands = new OperandFormat(codes);
                    operandFormats.Add(codes, operands);
                }

                subInst.Operands = operands;

                yield return subInst;
            }
        }
        public override string ToString()
        {
            var sb = new StringBuilder();

            if (Opcode <= 0xFF)
                sb.Append(Opcode.ToString("X2"));
            else
                sb.Append(Opcode.ToString("X4"));

            switch (ModRmByte)
            {
                case ModRmInfo.All:
                    sb.Append("/r");
                    break;

                case ModRmInfo.OnlyRm:
                    sb.AppendFormat("/{0}", ExtendedRmOpcode);
                    break;
            }

            if (Operands.Count > 0)
            {
                sb.Append(' ');
                sb.Append(Operands.ToString());
            }

            return sb.ToString();
        }
        public override bool Equals(object obj)
        {
            if (obj is InstructionInfo other)
                return this.Opcode == other.Opcode && this.Operands == other.Operands && this.ModRmByte == other.ModRmByte && this.ExtendedRmOpcode == other.ExtendedRmOpcode;
            else
                return false;
        }
        public override int GetHashCode() => Opcode | ExtendedRmOpcode << 16;

        private static OperandType ParseOperand(string s) => operandStrings[s];
        private static OperandType GetRegisterIndex(int index, OperandType operandType)
        {
            if (operandType == OperandType.RegisterByte)
            {
                switch (index)
                {
                    case 0:
                        return OperandType.RegisterAL;

                    case 1:
                        return OperandType.RegisterCL;

                    case 2:
                        return OperandType.RegisterDL;

                    case 3:
                        return OperandType.RegisterBL;

                    case 4:
                        return OperandType.RegisterAH;

                    case 5:
                        return OperandType.RegisterCH;

                    case 6:
                        return OperandType.RegisterDH;

                    case 7:
                        return OperandType.RegisterBH;
                }
            }
            else if (operandType == OperandType.RegisterWord)
            {
                switch (index)
                {
                    case 0:
                        return OperandType.RegisterAX;

                    case 1:
                        return OperandType.RegisterCX;

                    case 2:
                        return OperandType.RegisterDX;

                    case 3:
                        return OperandType.RegisterBX;

                    case 4:
                        return OperandType.RegisterSP;

                    case 5:
                        return OperandType.RegisterBP;

                    case 6:
                        return OperandType.RegisterSI;

                    case 7:
                        return OperandType.RegisterDI;
                }
            }
            else if (operandType == OperandType.RegisterST)
            {
                switch (index)
                {
                    case 0:
                        return OperandType.RegisterST0;

                    case 1:
                        return OperandType.RegisterST1;

                    case 2:
                        return OperandType.RegisterST2;

                    case 3:
                        return OperandType.RegisterST3;

                    case 4:
                        return OperandType.RegisterST4;

                    case 5:
                        return OperandType.RegisterST5;

                    case 6:
                        return OperandType.RegisterST6;

                    case 7:
                        return OperandType.RegisterST7;
                }
            }

            return operandType;
        }
    }

    /// <summary>
    /// Describes the ModR/M byte of an instruction.
    /// </summary>
    public enum ModRmInfo : byte
    {
        /// <summary>
        /// The ModR/M byte is not present.
        /// </summary>
        None,
        /// <summary>
        /// Only the R/M field in the ModR/M byte is used.
        /// </summary>
        OnlyRm,
        /// <summary>
        /// The entire ModR/M byte is used.
        /// </summary>
        All,
        /// <summary>
        /// A register code is added to the opcode.
        /// </summary>
        RegisterPlus
    }

    /// <summary>
    /// Describes any instruction prefixes present.
    /// </summary>
    [Flags]
    public enum PrefixFlags : ushort
    {
        /// <summary>
        /// The instruction has no prefixes.
        /// </summary>
        None = 0,
        /// <summary>
        /// The CS segment register should be used.
        /// </summary>
        CSOverride = (1 << 0),
        /// <summary>
        /// The SS segment register should be used.
        /// </summary>
        SSOverride = (1 << 1),
        /// <summary>
        /// The DS segment register should be used.
        /// </summary>
        DSOverride = (1 << 2),
        /// <summary>
        /// The ES segment register should be used.
        /// </summary>
        ESOverride = (1 << 3),
        /// <summary>
        /// The FS segment register should be used.
        /// </summary>
        FSOverride = (1 << 4),
        /// <summary>
        /// The GS segment register should be used.
        /// </summary>
        GSOverride = (1 << 5),
        /// <summary>
        /// Specifies the LOCK prefix.
        /// </summary>
        Lock = (1 << 6),
        /// <summary>
        /// Specifies the REPN prefix for string instructions.
        /// </summary>
        REPN = (1 << 7),
        /// <summary>
        /// Specifies the REP prefix for string instructions.
        /// </summary>
        REP = (1 << 8),
        /// <summary>
        /// Specifies the Operand-Size prefix.
        /// </summary>
        OperandSize = (1 << 9),
        /// <summary>
        /// Specifies the Address-Size prefix.
        /// </summary>
        AddressSize = (1 << 10)
    }
}
