using System.Text;

namespace Aeon.SourceGenerator
{
    internal sealed class InstructionInfo : IEquatable<InstructionInfo>
    {
        private static readonly Dictionary<int, OperandFormat> operandFormats = new();
        private static readonly char[] Pipe = new[] { '|' };
        private static readonly char[] Space = new[] { ' ' };
        private static readonly char[] Slash = new[] { '/' };
        private static readonly char[] Comma = new[] { ',' };

        private InstructionInfo()
        {
        }
        private InstructionInfo(InstructionInfo src)
        {
            this.Opcode = src.Opcode;
            this.Operands = src.Operands;
            this.ModRmByte = src.ModRmByte;
            this.ExtendedRmOpcode = src.ExtendedRmOpcode;
            this.IsMultiByte = src.IsMultiByte;
        }
        private InstructionInfo(ushort opcode, OperandFormat operands, ModRmInfo modRm, byte extendedRmOpcode, bool isMultiByte)
        {
            this.Opcode = opcode;
            this.Operands = operands;
            this.ModRmByte = modRm;
            this.ExtendedRmOpcode = extendedRmOpcode;
            this.IsMultiByte = isMultiByte;
        }

        public ushort Opcode { get; private set; }
        public OperandFormat Operands { get; private set; }
        public ModRmInfo ModRmByte { get; private set; }
        public byte ExtendedRmOpcode { get; private set; }
        public int RPlusIndex
        {
            get
            {
                if (ModRmByte == ModRmInfo.RegisterPlus)
                    return this.Operands.IndexOfAny(new[] { OperandType.RegisterST, OperandType.RegisterByte, OperandType.RegisterWord });

                return -1;
            }
        }
        public bool IsMultiByte { get; set; }
        public bool IsAffectedByOperandSize => this.Operands.IndexOfAny(new[] { OperandType.ImmediateFarPointer, OperandType.ImmediateRelativeWord, OperandType.ImmediateWord, OperandType.IndirectFarPointer, OperandType.MemoryOffsetWord, OperandType.RegisterOrMemoryWord, OperandType.RegisterOrMemoryWordNearPointer, OperandType.RegisterWord, OperandType.EffectiveAddress, OperandType.FullLinearAddress }) >= 0;
        public bool IsAffectedByAddressSize => this.Operands.IndexOfAny(new[] { OperandType.IndirectFarPointer, OperandType.MemoryInt16, OperandType.MemoryInt32, OperandType.MemoryFloat32, OperandType.MemoryFloat64, OperandType.MemoryFloat80, OperandType.MemoryInt64, OperandType.MemoryOffsetByte, OperandType.MemoryOffsetWord, OperandType.RegisterOrMemory16, OperandType.RegisterOrMemory32, OperandType.RegisterOrMemoryByte, OperandType.RegisterOrMemoryWord, OperandType.RegisterOrMemoryWordNearPointer, OperandType.EffectiveAddress, OperandType.FullLinearAddress }) >= 0;

        public bool TryGetOperandSize(bool operand32, bool address32, out int size)
        {
            size = 0;

            foreach (var operand in this.Operands)
            {
                switch (operand)
                {
                    case OperandType.None:
                        return true;

                    case OperandType.ImmediateByte or OperandType.ImmediateByteExtend or OperandType.ImmediateRelativeByte:
                        size++;
                        break;

                    case OperandType.ImmediateInt16:
                        size += 2;
                        break;

                    case OperandType.ImmediateInt32:
                        size += 4;
                        break;

                    case OperandType.ImmediateInt64:
                        size += 8;
                        break;

                    case OperandType.ImmediateWord or OperandType.ImmediateRelativeWord:
                        size += operand32 ? 4 : 2;
                        break;

                    case OperandType.MemoryOffsetByte or OperandType.MemoryOffsetWord:
                        size += address32 ? 4 : 2;
                        break;

                    case OperandType.ImmediateFarPointer:
                    case OperandType.RegisterOrMemoryByte:
                    case OperandType.RegisterOrMemoryWord:
                    case OperandType.RegisterOrMemoryWordNearPointer:
                    case >= OperandType.IndirectFarPointer and <= OperandType.MemoryFloat80:
                        return false;
                }
            }

            return true;
        }

        public string GetDecodeAndEmulateMethodName(bool operand32, bool address32)
        {
            int operandSize = operand32 ? 32 : 16;
            int addressSize = address32 ? 32 : 16;
            if (this.ModRmByte == ModRmInfo.OnlyRm)
                return $"Op_{this.Opcode:X4}_R{this.ExtendedRmOpcode}_O{operandSize}_A{addressSize}";
            else
                return $"Op_{this.Opcode:X4}_O{operandSize}_A{addressSize}";
        }

        public static bool TryParse(string instructionFormat, out IReadOnlyCollection<InstructionInfo> values)
        {
            values = null;
            if (string.IsNullOrWhiteSpace(instructionFormat))
                return false;

            var infos = new List<InstructionInfo>();

            foreach (var format in instructionFormat.Split(Pipe))
            {
                if (!TryParseOne(format, infos))
                    return false;
            }

            if (infos.Count == 0)
                return false;

            values = infos;
            return true;
        }

        private IEnumerable<InstructionInfo> Expand()
        {
            if (this.ModRmByte != ModRmInfo.RegisterPlus)
            {
                yield return this;
                yield break;
            }

            for (int r = 0; r < 8; r++)
            {
                var subInst = new InstructionInfo(this) { ModRmByte = ModRmInfo.None };
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
        public bool Equals(InstructionInfo other)
        {
            return this.Opcode == other.Opcode
                && this.Operands.Equals(other.Operands)
                && this.ModRmByte == other.ModRmByte
                && this.ExtendedRmOpcode == other.ExtendedRmOpcode
                && this.IsMultiByte == other.IsMultiByte;
        }
        public override bool Equals(object obj) => this.Equals(obj as InstructionInfo);
        public override int GetHashCode() => this.Opcode | this.ExtendedRmOpcode << 16;

        private static bool TryParseOne(string s, List<InstructionInfo> values)
        {
            if (string.IsNullOrWhiteSpace(s))
                return false;

            var outer = s.Split(Space, StringSplitOptions.RemoveEmptyEntries);
            bool isMultiByte = false;

            string[] innerCode;
            if (!outer[0].Contains("+"))
                innerCode = outer[0].Split(Slash, StringSplitOptions.RemoveEmptyEntries);
            else
                innerCode = new string[2] { outer[0].TrimEnd('+'), "+" };
            ushort opcode = ushort.Parse(innerCode[0], System.Globalization.NumberStyles.HexNumber);
            if (opcode > 0xFF)
            {
                opcode = (ushort)(((opcode & 0xFF) << 8) | ((opcode >> 8) & 0xFF));
                isMultiByte = true;
            }

            var modRm = ModRmInfo.None;
            byte extendedRmOpcode = 0;

            if (innerCode.Length > 1)
            {
                if (innerCode[1][0] == 'r')
                {
                    modRm = ModRmInfo.All;
                }
                else if (innerCode[1][0] == '+')
                {
                    modRm = ModRmInfo.RegisterPlus;
                }
                else
                {
                    modRm = ModRmInfo.OnlyRm;
                    extendedRmOpcode = byte.Parse(new string(innerCode[1][0], 1));
                }
            }

            OperandFormat format;

            if (outer.Length > 1)
            {
                string[] innerOperands = outer[1].Split(Comma, StringSplitOptions.RemoveEmptyEntries);
                int operandCode = (int)ParseOperand(innerOperands[0]);

                if (innerOperands.Length > 1)
                {
                    operandCode |= (int)ParseOperand(innerOperands[1]) << 8;

                    if (innerOperands.Length > 2)
                        operandCode |= (int)ParseOperand(innerOperands[2]) << 16;
                }

                if (!operandFormats.TryGetValue(operandCode, out format))
                {
                    format = new OperandFormat(operandCode);
                    operandFormats.Add(operandCode, format);
                }
            }
            else
            {
                if (!operandFormats.TryGetValue(0, out format))
                {
                    format = new OperandFormat(0);
                    operandFormats.Add(0, format);
                }
            }

            var info = new InstructionInfo(opcode, format, modRm, extendedRmOpcode, isMultiByte);
            foreach (var subInfo in info.Expand())
                values.Add(subInfo);

            return true;
        }
        private static OperandType ParseOperand(string s)
        {
            return s switch
            {
                "rb" => OperandType.RegisterByte,
                "ib" => OperandType.ImmediateByte,
                "ibx" => OperandType.ImmediateByteExtend,
                "rw" => OperandType.RegisterWord,
                "iw" => OperandType.ImmediateWord,
                "rmb" => OperandType.RegisterOrMemoryByte,
                "rmw" => OperandType.RegisterOrMemoryWord,
                "moffsb" => OperandType.MemoryOffsetByte,
                "moffsw" => OperandType.MemoryOffsetWord,
                "sreg" => OperandType.SegmentRegister,
                "al" => OperandType.RegisterAL,
                "ah" => OperandType.RegisterAH,
                "ax" => OperandType.RegisterAX,
                "bl" => OperandType.RegisterBL,
                "bh" => OperandType.RegisterBH,
                "bx" => OperandType.RegisterBX,
                "cl" => OperandType.RegisterCL,
                "ch" => OperandType.RegisterCH,
                "cx" => OperandType.RegisterCX,
                "dl" => OperandType.RegisterDL,
                "dh" => OperandType.RegisterDH,
                "dx" => OperandType.RegisterDX,
                "sp" => OperandType.RegisterSP,
                "bp" => OperandType.RegisterBP,
                "si" => OperandType.RegisterSI,
                "di" => OperandType.RegisterDI,
                "cs" => OperandType.RegisterCS,
                "ss" => OperandType.RegisterSS,
                "ds" => OperandType.RegisterDS,
                "es" => OperandType.RegisterES,
                "fs" => OperandType.RegisterFS,
                "gs" => OperandType.RegisterGS,
                "irelb" => OperandType.ImmediateRelativeByte,
                "irelw" => OperandType.ImmediateRelativeWord,
                "jmprmw" => OperandType.RegisterOrMemoryWordNearPointer,
                "iptr" => OperandType.ImmediateFarPointer,
                "mptr" => OperandType.IndirectFarPointer,
                "addr:rmw" => OperandType.EffectiveAddress,
                "fulladdr:rmw" => OperandType.FullLinearAddress,
                "i16" => OperandType.ImmediateInt16,
                "i32" => OperandType.ImmediateInt32,
                "i64" => OperandType.ImmediateInt64,
                "m16" => OperandType.MemoryInt16,
                "m32" => OperandType.MemoryInt32,
                "m64" => OperandType.MemoryInt64,
                "rm16" => OperandType.RegisterOrMemory16,
                "rm32" => OperandType.RegisterOrMemory32,
                "mf32" => OperandType.MemoryFloat32,
                "mf64" => OperandType.MemoryFloat64,
                "mf80" => OperandType.MemoryFloat80,
                "st" => OperandType.RegisterST,
                "st0" => OperandType.RegisterST0,
                "st1" => OperandType.RegisterST1,
                "st2" => OperandType.RegisterST2,
                "st3" => OperandType.RegisterST3,
                "st4" => OperandType.RegisterST4,
                "st5" => OperandType.RegisterST5,
                "st6" => OperandType.RegisterST6,
                "st7" => OperandType.RegisterST7,
                "dr" => OperandType.DebugRegister,
                _ => OperandType.None
            };
        }
        private static OperandType GetRegisterIndex(int index, OperandType operandType)
        {
            if (operandType == OperandType.RegisterByte)
            {
                return index switch
                {
                    0 => OperandType.RegisterAL,
                    1 => OperandType.RegisterCL,
                    2 => OperandType.RegisterDL,
                    3 => OperandType.RegisterBL,
                    4 => OperandType.RegisterAH,
                    5 => OperandType.RegisterCH,
                    6 => OperandType.RegisterDH,
                    7 => OperandType.RegisterBH,
                    _ => throw new ArgumentException()
                };
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
}
