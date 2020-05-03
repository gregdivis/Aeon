using System;
using System.Reflection.Emit;
using System.Runtime.Intrinsics.X86;

namespace Aeon.Emulator.Decoding.Emitters
{
    internal sealed class LoadRegister : Emitter
    {
        public LoadRegister(EmitStateInfo state, int registerSize)
            : base(state)
        {
            if (registerSize != 1 && registerSize != 2 && registerSize != 4)
                throw new ArgumentException("Invalid register size.");

            this.RegisterSize = registerSize;
        }

        public int RegisterSize { get; }
        public override Type MethodArgType
        {
            get
            {
                var type = GetUnsignedIntType(this.RegisterSize);

                if (this.ReturnType == EmitReturnType.Address)
                    return type.MakePointerType();
                else
                    return type;
            }
        }

        public override void EmitLoad()
        {
            //// Reg is the middle 3 bits of the ModR/M byte.
            //int reg = (*ip & 0x38) >> 3;

            LoadProcessor();

            //il.LoadLocal(ipPointer);
            LoadIPPointer();
            il.Emit(OpCodes.Ldind_U1);
            if (Bmi1.IsSupported)
            {
                il.LoadConstant(0x0303);
                il.Emit(OpCodes.Call, Infos.Intrinsics.BitFieldExtract);
            }
            else
            {
                il.LoadConstant(0x38);
                il.Emit(OpCodes.And);
                il.LoadConstant(3);
                il.Emit(OpCodes.Shr_Un);
            }

            bool byteVersion = this.RegisterSize == 1;
            bool returnValue = this.ReturnType == EmitReturnType.Value;

            if (!byteVersion)
            {
                il.Emit(OpCodes.Call, Infos.Processor.GetRegisterWordPointer);
                if (returnValue)
                    il.Emit(this.RegisterSize == 4 ? OpCodes.Ldind_U4 : OpCodes.Ldind_U2);
            }
            else
            {
                il.Emit(OpCodes.Call, Infos.Processor.GetRegisterBytePointer);
                if (returnValue)
                    il.Emit(OpCodes.Ldind_U1);
            }
        }
    }
}
