using System;
using System.Reflection.Emit;

namespace Aeon.Emulator.Decoding.Emitters
{
    internal class LoadImmediate : Emitter
    {
        public LoadImmediate(EmitStateInfo state, int immediateSize, ValueExtend valueExtend)
            : base(state)
        {
            if (immediateSize != 1 && immediateSize != 2 && immediateSize != 4 && immediateSize != 6 && immediateSize != 8)
                throw new ArgumentException("Unsupported immediate size.");

            this.ImmediateSize = immediateSize;
            this.ValueExtend = valueExtend;
        }

        public int ImmediateSize { get; }
        public ValueExtend ValueExtend { get; }
        public override Type MethodArgType => this.ValueExtend == ValueExtend.Sign ? GetSignedIntType(this.ImmediateSize) : GetUnsignedIntType(this.ImmediateSize);

        public sealed override void EmitLoad()
        {
            LoadIPPointer();
            bool signExtend = this.ValueExtend == Emitters.ValueExtend.Sign;

            switch (this.ImmediateSize)
            {
                case 1:
                    il.Emit(signExtend ? OpCodes.Ldind_I1 : OpCodes.Ldind_U1);
                    break;

                case 2:
                    il.Emit(signExtend ? OpCodes.Ldind_I2 : OpCodes.Ldind_U2);
                    break;

                case 4:
                    il.Emit(signExtend ? OpCodes.Ldind_I4 : OpCodes.Ldind_U4);
                    break;

                case 6:
                case 8:
                    il.Emit(OpCodes.Ldind_I8);
                    break;

                default:
                    throw new InvalidOperationException();
            }

            IncrementIPPointer(this.ImmediateSize);
        }
    }
}
