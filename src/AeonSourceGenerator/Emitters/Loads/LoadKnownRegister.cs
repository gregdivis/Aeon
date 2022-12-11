namespace Aeon.SourceGenerator.Emitters
{
    internal sealed class LoadKnownRegister : Emitter
    {
        public LoadKnownRegister(EmitStateInfo state, OperandType register) : base(state)
        {
            if (!IsKnownRegister(register))
                throw new ArgumentException("Register is not valid.");

            this.Register = register;
        }

        public OperandType Register { get; }

        public static bool IsKnownRegister(OperandType operand)
        {
            return operand is >= OperandType.RegisterAL and <= OperandType.RegisterDI
                || operand is >= OperandType.RegisterST0 and <= OperandType.RegisterST7;
        }

        public override void WriteParameter(TextWriter writer)
        {
            if (this.WriteOnly)
                writer.Write("out ");
            else if (this.ByRef)
                writer.Write("ref ");

            if (!AppendGpr(writer, this.MethodArgType, this.Register, this.WordSize == 4))
            {
                if (this.Register >= OperandType.RegisterST0 && this.Register <= OperandType.RegisterST7)
                {
                    writer.Write("p.FPU.GetRegisterRef(");
                    writer.Write(this.Register - OperandType.RegisterST0);
                    writer.Write(')');
                }
                else
                {
                    throw new InvalidOperationException($"Unexpected value for known register: {this.Register}");
                }
            }
        }

        private static bool AppendGpr(TextWriter writer, EmitterTypeCode methodArgType, OperandType operandType, bool doubleWord)
        {
            var regName = operandType switch
            {
                OperandType.RegisterAX when doubleWord => "EAX",
                OperandType.RegisterBX when doubleWord => "EBX",
                OperandType.RegisterCX when doubleWord => "ECX",
                OperandType.RegisterDX when doubleWord => "EDX",
                OperandType.RegisterBP when doubleWord => "EBP",
                OperandType.RegisterSI when doubleWord => "ESI",
                OperandType.RegisterDI when doubleWord => "EDI",
                OperandType.RegisterSP when doubleWord => "ESP",
                OperandType.RegisterAX => "AX",
                OperandType.RegisterBX => "BX",
                OperandType.RegisterCX => "CX",
                OperandType.RegisterDX => "DX",
                OperandType.RegisterBP => "BP",
                OperandType.RegisterSI => "SI",
                OperandType.RegisterDI => "DI",
                OperandType.RegisterSP => "SP",
                OperandType.RegisterAL => "AL",
                OperandType.RegisterAH => "AH",
                OperandType.RegisterBL => "BL",
                OperandType.RegisterBH => "BH",
                OperandType.RegisterCL => "CL",
                OperandType.RegisterCH => "CH",
                OperandType.RegisterDL => "DL",
                OperandType.RegisterDH => "DH",
                _ => null
            };

            if (regName == null)
                return false;

            var methodArgTypeName = methodArgType switch
            {
                EmitterTypeCode.Byte => "byte",
                EmitterTypeCode.SByte => "sbyte",
                EmitterTypeCode.Short => "short",
                EmitterTypeCode.UShort => "ushort",
                EmitterTypeCode.Int => "int",
                EmitterTypeCode.UInt => "uint",
                _ => throw new ArgumentException($"Invalid value: {methodArgType}", nameof(methodArgType))
            };

            var regTypeName = operandType switch
            {
                OperandType.RegisterAX or OperandType.RegisterBX or OperandType.RegisterCX or OperandType.RegisterDX => doubleWord ? "int" : "short",
                OperandType.RegisterBP or OperandType.RegisterSI or OperandType.RegisterDI or OperandType.RegisterSP => doubleWord ? "uint" : "ushort",
                OperandType.RegisterAH or OperandType.RegisterAL or OperandType.RegisterBL or OperandType.RegisterBH or OperandType.RegisterCL or OperandType.RegisterCH or OperandType.RegisterDL or OperandType.RegisterDH => "byte",
                _ => throw new ArgumentException($"Invalid value: {operandType}", nameof(operandType))
            };

            if (methodArgTypeName == regTypeName)
            {
                writer.Write("p.");
                writer.Write(regName);
            }
            else
            {
                writer.Write("Unsafe.As<");
                writer.Write(regTypeName);
                writer.Write(", ");
                writer.Write(methodArgTypeName);
                writer.Write(">(ref p.");
                writer.Write(regName);
                writer.Write(')');
            }

            return true;
        }
    }
}
