using System;
using Aeon.Emulator.Decoding;

namespace Aeon.Emulator.DebugSupport
{
    public static class InstructionDecoder
    {
        public static DecodedOperands Decode(OpcodeInfo opcode, ReadOnlySpan<byte> rawData, PrefixState prefixes) => Decode(opcode, rawData, prefixes, out _);
        public static DecodedOperands? TryDecode(OpcodeInfo opcode, ReadOnlySpan<byte> rawData, PrefixState prefixes)
        {
            try
            {
                return Decode(opcode, rawData, prefixes, out _);
            }
            catch
            {
                return null;
            }
        }

        public static int CalculateOperandLength(OpcodeInfo opcode, ReadOnlySpan<byte> rawData, PrefixState prefixes)
        {
            Decode(opcode, rawData, prefixes, out int length);
            return length;
        }

        private static DecodedOperands Decode(OpcodeInfo opcode, ReadOnlySpan<byte> rawData, PrefixState prefixes, out int length)
        {
            var reader = new OperandReader(rawData);
            var operands = new DecodedOperands();

            length = 0;

            if (opcode.Operands.Count == 0)
                return operands;

            if (opcode.ModRmInfo != ModRmInfo.None)
            {
                byte modRmByte = reader.ReadByte();
                int mod = (modRmByte & 0xC0) >> 6;
                int rm = modRmByte & 0x07;

                for (int i = 0; i < opcode.Operands.Count; i++)
                {
                    var type = opcode.Operands[i];
                    if (type == OperandType.RegisterOrMemoryByte)
                    {
                        operands.SetOperand(i, DecodeRmbw(mod, rm, true, prefixes, ref reader));
                    }
                    else if (IsPointerOperand(type))
                    {
                        var operand = DecodeRmbw(mod, rm, false, prefixes, ref reader);
                        if (type == OperandType.EffectiveAddress)
                            operand.Type = CodeOperandType.EffectiveAddress;
                        else if (type == OperandType.FullLinearAddress)
                            operand.Type = CodeOperandType.FullLinearAddress;
                        else if (type == OperandType.RegisterOrMemoryWordNearPointer)
                            operand.Type = CodeOperandType.AbsoluteJumpAddress;
                        else if (type == OperandType.IndirectFarPointer)
                            operand.Type = CodeOperandType.IndirectFarMemoryAddress;
                        else if (type == OperandType.MemoryInt16 || type == OperandType.RegisterOrMemory16)
                            operand.OperandSize = CodeOperandSize.Word;
                        else if (type == OperandType.MemoryInt32 || type == OperandType.RegisterOrMemory32)
                            operand.OperandSize = CodeOperandSize.DoubleWord;
                        else if (type == OperandType.MemoryInt64)
                            operand.OperandSize = CodeOperandSize.QuadWord;
                        else if (type == OperandType.MemoryFloat32)
                            operand.OperandSize = CodeOperandSize.Single;
                        else if (type == OperandType.MemoryFloat64)
                            operand.OperandSize = CodeOperandSize.Double;
                        else if (type == OperandType.MemoryFloat80)
                            operand.OperandSize = CodeOperandSize.LongDouble;

                        operands.SetOperand(i, operand);
                    }
                }

                if (opcode.ModRmInfo == ModRmInfo.All)
                {
                    int reg = (modRmByte & 0x38) >> 3;
                    for (int i = 0; i < opcode.Operands.Count; i++)
                    {
                        var type = opcode.Operands[i];
                        if (type == OperandType.RegisterByte)
                            operands.SetOperand(i, DecodeRb(reg));
                        else if (type == OperandType.RegisterWord)
                            operands.SetOperand(i, DecodeRw(reg, prefixes));
                        else if (type == OperandType.SegmentRegister)
                            operands.SetOperand(i, DecodeSreg(reg));
                    }
                }
            }

            for (int i = 0; i < opcode.Operands.Count; i++)
            {
                var type = opcode.Operands[i];
                if (IsKnownRegister(type))
                {
                    operands.SetOperand(i, new CodeOperand(DecodeKnownRegister(type, (prefixes & PrefixState.OperandSize) != 0)));
                }
                else if (type == OperandType.ImmediateByte)
                {
                    operands.SetOperand(i, new CodeOperand(CodeOperandType.Immediate, reader.ReadByte(), CodeOperandSize.Byte));
                }
                else if (type == OperandType.ImmediateByteExtend || type == OperandType.ImmediateRelativeByte)
                {
                    var operand = new CodeOperand(CodeOperandType.Immediate, (uint)(int)reader.ReadSByte(), GetOperandSize(false, prefixes));
                    if (type == OperandType.ImmediateRelativeByte)
                        operand.Type = CodeOperandType.RelativeJumpAddress;

                    operands.SetOperand(i, operand);
                }
                else if (type == OperandType.ImmediateInt16)
                {
                    operands.SetOperand(i, new CodeOperand(CodeOperandType.Immediate, reader.ReadUInt16(), CodeOperandSize.Word));
                }
                else if (type == OperandType.ImmediateInt32)
                {
                    operands.SetOperand(i, new CodeOperand(CodeOperandType.Immediate, reader.ReadUInt32(), CodeOperandSize.DoubleWord));
                }
                else if (type == OperandType.ImmediateWord)
                {
                    uint value;
                    if ((prefixes & PrefixState.OperandSize) == 0)
                        value = reader.ReadUInt16();
                    else
                        value = reader.ReadUInt32();

                    operands.SetOperand(i, new CodeOperand(CodeOperandType.Immediate, value, GetOperandSize(false, prefixes)));
                }
                else if (type == OperandType.ImmediateRelativeWord)
                {
                    uint value;
                    if ((prefixes & PrefixState.OperandSize) == 0)
                        value = (uint)(int)reader.ReadInt16();
                    else
                        value = reader.ReadUInt32();

                    operands.SetOperand(i, new CodeOperand(CodeOperandType.RelativeJumpAddress, value, GetOperandSize(false, prefixes)));
                }
                else if (type == OperandType.MemoryOffsetByte || type == OperandType.MemoryOffsetWord)
                {
                    uint value;
                    if ((prefixes & PrefixState.AddressSize) == 0)
                        value = (uint)(int)reader.ReadInt16();
                    else
                        value = reader.ReadUInt32();

                    operands.SetOperand(i, new CodeOperand(CodeMemoryBase.DisplacementOnly, value, GetOperandSize(type == OperandType.MemoryOffsetByte, prefixes)));
                }
                else if (type == OperandType.ImmediateFarPointer)
                {
                    uint value;
                    if ((prefixes & PrefixState.AddressSize) == 0)
                        value = reader.ReadUInt16();
                    else
                        value = reader.ReadUInt32();

                    ushort segment = reader.ReadUInt16();
                    operands.SetOperand(i, CodeOperand.FarPointer(segment, value));
                }
            }

            length = reader.Position;
            return operands;
        }

        private static bool IsPointerOperand(OperandType type)
        {
            return type == OperandType.RegisterOrMemoryWord || type == OperandType.EffectiveAddress
                || type == OperandType.IndirectFarPointer || type == OperandType.MemoryInt64
                || type == OperandType.RegisterOrMemory16 || type == OperandType.MemoryInt32
                || type == OperandType.MemoryFloat32 || type == OperandType.MemoryFloat64
                || type == OperandType.MemoryFloat80 || type == OperandType.RegisterOrMemoryWordNearPointer
                || type == OperandType.FullLinearAddress;
        }
        private static bool IsKnownRegister(OperandType operand)
        {
            return operand == OperandType.RegisterAL || operand == OperandType.RegisterAH || operand == OperandType.RegisterAX ||
                operand == OperandType.RegisterBL || operand == OperandType.RegisterBH || operand == OperandType.RegisterBX ||
                operand == OperandType.RegisterCL || operand == OperandType.RegisterCH || operand == OperandType.RegisterCX ||
                operand == OperandType.RegisterDL || operand == OperandType.RegisterDH || operand == OperandType.RegisterDX ||
                operand == OperandType.RegisterSI || operand == OperandType.RegisterDI || operand == OperandType.RegisterSP ||
                operand == OperandType.RegisterBP || operand == OperandType.RegisterCS || operand == OperandType.RegisterSS ||
                operand == OperandType.RegisterDS || operand == OperandType.RegisterES || operand == OperandType.RegisterFS ||
                operand == OperandType.RegisterGS;
        }
        private static CodeRegister DecodeKnownRegister(OperandType o, bool size32)
        {
            return o switch
            {
                OperandType.RegisterAL => CodeRegister.AL,
                OperandType.RegisterAH => CodeRegister.AH,
                OperandType.RegisterAX => size32 ? CodeRegister.EAX : CodeRegister.AX,
                OperandType.RegisterBL => CodeRegister.BL,
                OperandType.RegisterBH => CodeRegister.BH,
                OperandType.RegisterBX => size32 ? CodeRegister.EBX : CodeRegister.BX,
                OperandType.RegisterCL => CodeRegister.CL,
                OperandType.RegisterCH => CodeRegister.CH,
                OperandType.RegisterCX => size32 ? CodeRegister.ECX : CodeRegister.CX,
                OperandType.RegisterDL => CodeRegister.DL,
                OperandType.RegisterDH => CodeRegister.DH,
                OperandType.RegisterDX => size32 ? CodeRegister.EDX : CodeRegister.DX,
                OperandType.RegisterSI => size32 ? CodeRegister.ESI : CodeRegister.SI,
                OperandType.RegisterDI => size32 ? CodeRegister.EDI : CodeRegister.DI,
                OperandType.RegisterSP => size32 ? CodeRegister.ESP : CodeRegister.SP,
                OperandType.RegisterBP => size32 ? CodeRegister.EBP : CodeRegister.BP,
                OperandType.RegisterCS => CodeRegister.CS,
                OperandType.RegisterSS => CodeRegister.SS,
                OperandType.RegisterDS => CodeRegister.DS,
                OperandType.RegisterES => CodeRegister.ES,
                OperandType.RegisterFS => CodeRegister.FS,
                OperandType.RegisterGS => CodeRegister.GS,

                _ => throw new ArgumentException(),
            };
        }

        private static CodeOperand DecodeRw(int regCode, PrefixState prefixes)
        {
            bool isWord = (prefixes & PrefixState.OperandSize) == 0;

            return regCode switch
            {
                0 => isWord ? CodeRegister.AX : CodeRegister.EAX,
                1 => isWord ? CodeRegister.CX : CodeRegister.ECX,
                2 => isWord ? CodeRegister.DX : CodeRegister.EDX,
                3 => isWord ? CodeRegister.BX : CodeRegister.EBX,
                4 => isWord ? CodeRegister.SP : CodeRegister.ESP,
                5 => isWord ? CodeRegister.BP : CodeRegister.EBP,
                6 => isWord ? CodeRegister.SI : CodeRegister.ESI,
                7 => isWord ? CodeRegister.DI : CodeRegister.EDI,
                _ => throw new ArgumentException(),
            };
        }
        private static CodeOperand DecodeRb(int regCode)
        {
            return regCode switch
            {
                0 => CodeRegister.AL,
                1 => CodeRegister.CL,
                2 => CodeRegister.DL,
                3 => CodeRegister.BL,
                4 => CodeRegister.AH,
                5 => CodeRegister.CH,
                6 => CodeRegister.DH,
                7 => CodeRegister.BH,
                _ => throw new ArgumentException(),
            };
        }
        private static CodeOperand DecodeSreg(int regCode)
        {
            return regCode switch
            {
                0 => CodeRegister.ES,
                1 => CodeRegister.CS,
                2 => CodeRegister.SS,
                3 => CodeRegister.DS,
                4 => CodeRegister.FS,
                5 => CodeRegister.GS,
                _ => throw new ArgumentException(),
            };
        }
        private static CodeOperand DecodeRmbw(int mod, int rm, bool byteVersion, PrefixState prefixes, ref OperandReader reader)
        {
            if ((prefixes & PrefixState.AddressSize) != 0)
                return DecodeRmbw32(mod, rm, byteVersion, prefixes, ref reader);
            else
                return DecodeRmbw16(mod, rm, byteVersion, prefixes, ref reader);
        }

        private static CodeOperand DecodeRmbw16(int mod, int rm, bool byteVersion, PrefixState prefixes, ref OperandReader reader)
        {
            CodeOperandSize operandSize = byteVersion ? CodeOperandSize.Byte : CodeOperandSize.Word;

            switch (mod)
            {
                case 0:
                    if (rm == 6)
                        return new CodeOperand(CodeMemoryBase.DisplacementOnly, reader.ReadUInt16(), operandSize);
                    else
                        return new CodeOperand(DecodeEffectiveAddress16(rm), 0, operandSize);

                case 1:
                    return new CodeOperand(DecodeEffectiveAddress16(rm), (uint)(int)reader.ReadSByte(), operandSize);

                case 2:
                    return new CodeOperand(DecodeEffectiveAddress16(rm), (uint)(int)reader.ReadInt16(), operandSize);

                case 3:
                    if (byteVersion)
                        return DecodeRb(rm);
                    else
                        return DecodeRw(rm, prefixes);
            }

            throw new ArgumentException();
        }
        private static CodeOperand DecodeRmbw32(int mod, int rm, bool byteVersion, PrefixState prefixes, ref OperandReader reader)
        {
            CodeOperandSize operandSize = GetOperandSize(byteVersion, prefixes);
            uint displacement = 0;
            CodeOperand operand;

            if (mod == 3)
            {
                if (byteVersion)
                    return DecodeRb(rm);
                else
                    return DecodeRw(rm, prefixes);
            }

            // SIB byte follows ModR/M if rm is 4.
            if (rm == 4)
            {
                operand = DecodeSib(mod, reader.ReadByte(), ref reader);
                operand.OperandSize = operandSize;
            }
            else
            {
                if (rm == 5 && mod == 0)
                    operand = new CodeOperand(CodeMemoryBase.DisplacementOnly, reader.ReadUInt32(), operandSize);
                else
                    operand = new CodeOperand(DecodeEffectiveAddress32(rm), 0, operandSize);
            }

            if (mod == 1)
                displacement = (uint)(int)reader.ReadSByte();
            else if (mod == 2)
                displacement = reader.ReadUInt32();

            operand.ImmediateValue += displacement;

            return operand;
        }

        private static CodeOperandSize GetOperandSize(bool byteVersion, PrefixState prefixes)
        {
            CodeOperandSize operandSize;
            if (byteVersion)
                operandSize = CodeOperandSize.Byte;
            else if ((prefixes & PrefixState.OperandSize) != 0)
                operandSize = CodeOperandSize.DoubleWord;
            else
                operandSize = CodeOperandSize.Word;

            return operandSize;
        }

        private static CodeMemoryBase DecodeEffectiveAddress16(int rm)
        {
            switch (rm)
            {
                case 0: return CodeMemoryBase.BX_plus_SI;
                case 1: return CodeMemoryBase.BX_plus_DI;
                case 2: return CodeMemoryBase.BP_plus_SI;
                case 3: return CodeMemoryBase.BP_plus_DI;
                case 4: return CodeMemoryBase.SI;
                case 5: return CodeMemoryBase.DI;
                case 6: return CodeMemoryBase.BP;
                case 7: return CodeMemoryBase.BX;
            }

            throw new ArgumentException();
        }

        private static CodeMemoryBase DecodeEffectiveAddress32(int rm)
        {
            switch (rm)
            {
                case 0: return CodeMemoryBase.EAX;
                case 1: return CodeMemoryBase.ECX;
                case 2: return CodeMemoryBase.EDX;
                case 3: return CodeMemoryBase.EBX;
                case 5: return CodeMemoryBase.EBP;
                case 6: return CodeMemoryBase.ESI;
                case 7: return CodeMemoryBase.EDI;
            }

            throw new ArgumentException();
        }

        private static CodeOperand DecodeSib(int mod, byte sib, ref OperandReader reader)
        {
            int scale = (sib >> 6) & 0x3;
            int index = (sib >> 3) & 0x7;
            int baseIndex = sib & 0x7;

            var operand = new CodeOperand(CodeMemoryBase.SIB, 0, CodeOperandSize.Word);
            operand.Scale = (byte)(1 << scale);
            operand.Index = DecodeIndexRegister(index);
            operand.Base = DecodeBaseRegister(baseIndex, mod);

            if (baseIndex == 5)
                operand.ImmediateValue = reader.ReadUInt32();

            return operand;
        }
        private static CodeSibRegister DecodeIndexRegister(int index)
        {
            switch (index)
            {
                case 0: return CodeSibRegister.EAX;
                case 1: return CodeSibRegister.ECX;
                case 2: return CodeSibRegister.EDX;
                case 3: return CodeSibRegister.EBX;
                case 4: return CodeSibRegister.None;
                case 5: return CodeSibRegister.EBP;
                case 6: return CodeSibRegister.ESI;
                case 7: return CodeSibRegister.EDI;

                default:
                    throw new ArgumentException();
            }
        }
        private static CodeSibRegister DecodeBaseRegister(int baseIndex, int mod)
        {
            switch (baseIndex)
            {
                case 0: return CodeSibRegister.EAX;
                case 1: return CodeSibRegister.ECX;
                case 2: return CodeSibRegister.EDX;
                case 3: return CodeSibRegister.EBX;
                case 4: return CodeSibRegister.ESP;
                case 5: return mod == 0 ? CodeSibRegister.None : CodeSibRegister.EBP;
                case 6: return CodeSibRegister.ESI;
                case 7: return CodeSibRegister.EDI;

                default:
                    throw new ArgumentException();
            }
        }
    }
}
