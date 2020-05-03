using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Aeon.Emulator.Decoding.Emitters
{
    internal sealed class LoadKnownRegister : Emitter
    {
        private static readonly SortedList<OperandType, RegisterInfo> registerFields = new SortedList<OperandType, RegisterInfo>((int)OperandType.DebugRegister);

        public LoadKnownRegister(EmitStateInfo state, OperandType register)
            : base(state)
        {
            if (!IsKnownRegister(register))
                throw new ArgumentException("Regsiter is not valid.");

            this.Register = register;
        }
        static LoadKnownRegister()
        {
            registerFields[OperandType.RegisterAX] = new RegisterInfo(Infos.Processor.PAX, RegisterType.Word);
            registerFields[OperandType.RegisterBX] = new RegisterInfo(Infos.Processor.PBX, RegisterType.Word);
            registerFields[OperandType.RegisterCX] = new RegisterInfo(Infos.Processor.PCX, RegisterType.Word);
            registerFields[OperandType.RegisterDX] = new RegisterInfo(Infos.Processor.PDX, RegisterType.Word);

            registerFields[OperandType.RegisterAL] = new RegisterInfo(Infos.Processor.PAX, RegisterType.Byte);
            registerFields[OperandType.RegisterBL] = new RegisterInfo(Infos.Processor.PBX, RegisterType.Byte);
            registerFields[OperandType.RegisterCL] = new RegisterInfo(Infos.Processor.PCX, RegisterType.Byte);
            registerFields[OperandType.RegisterDL] = new RegisterInfo(Infos.Processor.PDX, RegisterType.Byte);

            registerFields[OperandType.RegisterAH] = new RegisterInfo(Infos.Processor.PAH, RegisterType.Byte);
            registerFields[OperandType.RegisterBH] = new RegisterInfo(Infos.Processor.PBH, RegisterType.Byte);
            registerFields[OperandType.RegisterCH] = new RegisterInfo(Infos.Processor.PCH, RegisterType.Byte);
            registerFields[OperandType.RegisterDH] = new RegisterInfo(Infos.Processor.PDH, RegisterType.Byte);

            registerFields[OperandType.RegisterBP] = new RegisterInfo(Infos.Processor.PBP, RegisterType.Word);
            registerFields[OperandType.RegisterSI] = new RegisterInfo(Infos.Processor.PSI, RegisterType.Word);
            registerFields[OperandType.RegisterDI] = new RegisterInfo(Infos.Processor.PDI, RegisterType.Word);
            registerFields[OperandType.RegisterSP] = new RegisterInfo(Infos.Processor.PSP, RegisterType.Word);

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
        public override Type MethodArgType
        {
            get
            {
                if (!registerFields.TryGetValue(this.Register, out var info))
                    return null;

                Type valueType;
                if (info.RegisterType == RegisterType.Byte)
                {
                    valueType = typeof(byte);
                }
                else if (info.RegisterType == RegisterType.Word)
                {
                    if (this.WordSize == 2)
                        valueType = typeof(ushort);
                    else
                        valueType = typeof(uint);
                }
                else
                {
                    valueType = typeof(double);
                }

                if (this.ReturnType == EmitReturnType.Address)
                    valueType = valueType.MakePointerType();

                return valueType;
            }
        }

        public static bool IsKnownRegister(OperandType operand) => registerFields.ContainsKey(operand);

        public override void EmitLoad()
        {
            var info = registerFields[this.Register];

            LoadProcessor();

            if (info.RegisterType != RegisterType.FPU)
            {
                il.Emit(OpCodes.Ldfld, info.Field);

                if (this.ReturnType == EmitReturnType.Value)
                {
                    if (info.RegisterType == RegisterType.Byte)
                        il.Emit(OpCodes.Ldind_U1);
                    else
                        il.Emit(this.WordSize == 4 ? OpCodes.Ldind_U4 : OpCodes.Ldind_U2);
                }
            }
            else
            {
                if (this.ReturnType == EmitReturnType.Address)
                    throw new InvalidOperationException();

                il.Emit(OpCodes.Ldfld, Infos.Processor.FPU);

                il.LoadConstant(info.FPUIndex);
                il.Emit(OpCodes.Call, Infos.FPU.GetRegisterValue);
            }
        }

        private readonly struct RegisterInfo
        {
            public RegisterInfo(FieldInfo field, RegisterType registerType)
            {
                this.Field = field;
                this.RegisterType = registerType;
                this.FPUIndex = 0;
            }
            public RegisterInfo(int fpuIndex)
            {
                this.Field = null;
                this.RegisterType = RegisterType.FPU;
                this.FPUIndex = fpuIndex;
            }

            public FieldInfo Field { get; }
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
