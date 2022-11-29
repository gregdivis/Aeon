using System.Text;

namespace AeonSourceGenerator.Emitters
{
    internal sealed class LoadKnownRegister : Emitter
    {
        private static readonly SortedList<OperandType, RegisterInfo> registerFields = new SortedList<OperandType, RegisterInfo>((int)OperandType.DebugRegister);

        public LoadKnownRegister(EmitStateInfo state, OperandType register)
            : base(state)
        {
            if (!IsKnownRegister(register))
                throw new ArgumentException("Register is not valid.");

            this.Register = register;
        }
        static LoadKnownRegister()
        {
            registerFields[OperandType.RegisterAX] = new RegisterInfo("PAX", RegisterType.Word);
            registerFields[OperandType.RegisterBX] = new RegisterInfo("PBX", RegisterType.Word);
            registerFields[OperandType.RegisterCX] = new RegisterInfo("PCX", RegisterType.Word);
            registerFields[OperandType.RegisterDX] = new RegisterInfo("PDX", RegisterType.Word);

            registerFields[OperandType.RegisterAL] = new RegisterInfo("PAX", RegisterType.Byte);
            registerFields[OperandType.RegisterBL] = new RegisterInfo("PBX", RegisterType.Byte);
            registerFields[OperandType.RegisterCL] = new RegisterInfo("PCX", RegisterType.Byte);
            registerFields[OperandType.RegisterDL] = new RegisterInfo("PDX", RegisterType.Byte);

            registerFields[OperandType.RegisterAH] = new RegisterInfo("PAH", RegisterType.Byte);
            registerFields[OperandType.RegisterBH] = new RegisterInfo("PBH", RegisterType.Byte);
            registerFields[OperandType.RegisterCH] = new RegisterInfo("PCH", RegisterType.Byte);
            registerFields[OperandType.RegisterDH] = new RegisterInfo("PDH", RegisterType.Byte);

            registerFields[OperandType.RegisterBP] = new RegisterInfo("PBP", RegisterType.Word);
            registerFields[OperandType.RegisterSI] = new RegisterInfo("PSI", RegisterType.Word);
            registerFields[OperandType.RegisterDI] = new RegisterInfo("PDI", RegisterType.Word);
            registerFields[OperandType.RegisterSP] = new RegisterInfo("PSP", RegisterType.Word);

            registerFields[OperandType.RegisterST0] = new RegisterInfo(0);
            registerFields[OperandType.RegisterST1] = new RegisterInfo(1);
            registerFields[OperandType.RegisterST2] = new RegisterInfo(2);
            registerFields[OperandType.RegisterST3] = new RegisterInfo(3);
            registerFields[OperandType.RegisterST4] = new RegisterInfo(4);
            registerFields[OperandType.RegisterST5] = new RegisterInfo(5);
            registerFields[OperandType.RegisterST6] = new RegisterInfo(6);
            registerFields[OperandType.RegisterST7] = new RegisterInfo(7);
        }

        public OperandType Register { get; }

        public static bool IsKnownRegister(OperandType operand) => registerFields.ContainsKey(operand);

        public override void WriteParameter(StringBuilder writer)
        {
            var info = registerFields[this.Register];

            if (this.WriteOnly)
                writer.Append("out ");
            else if (this.ByRef)
                writer.Append("ref ");

            if (info.RegisterType != RegisterType.FPU)
                writer.Append($"*({this.GetRuntimeTypeName()}*)p.{info.Name}");
            else
                writer.Append($"p.FPU.GetRegisterRef({info.FPUIndex})");
        }

        private readonly struct RegisterInfo
        {
            public RegisterInfo(string name, RegisterType registerType)
            {
                this.Name = name;
                this.RegisterType = registerType;
                this.FPUIndex = 0;
            }
            public RegisterInfo(int fpuIndex)
            {
                this.Name = null;
                this.RegisterType = RegisterType.FPU;
                this.FPUIndex = fpuIndex;
            }

            public string Name { get; }
            public RegisterType RegisterType { get; }
            public int FPUIndex { get; }
        }
    }

    internal enum RegisterType
    {
        Byte,
        Word,
        FPU
    }
}
