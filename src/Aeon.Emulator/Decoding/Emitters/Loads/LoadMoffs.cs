using System;
using System.Reflection.Emit;

#nullable disable

namespace Aeon.Emulator.Decoding.Emitters
{
    internal class LoadMoffs : Emitter
    {
        public LoadMoffs(EmitStateInfo state, int valueSize)
            : base(state)
        {
            if (valueSize != 1 && valueSize != 2 && valueSize != 4 && valueSize != 8)
                throw new ArgumentException("Invalid size.");

            this.ValueSize = valueSize;
        }

        public int ValueSize { get; }
        public override Type MethodArgType
        {
            get
            {
                if (this.ReturnType == EmitReturnType.Address)
                    return typeof(int);
                else
                    return GetUnsignedIntType(this.ValueSize);
            }
        }
        public override bool? RequiresTemp => this.ReturnType == EmitReturnType.Address;
        public override Type TempType => GetUnsignedIntType(this.ValueSize);

        public sealed override void EmitLoad()
        {
            bool returnValue = this.ReturnType == EmitReturnType.Value;
            bool byteVersion = this.ValueSize == 1;
            bool address32 = this.AddressMode == 32;

            if (returnValue)
                LoadPhysicalMemory();

            LoadIPPointer();

            // Memory offset values are always 16-bit in 16-bit addressing mode and 32-bit in 32-bit addressing mode.
            il.Emit(address32 ? OpCodes.Ldind_U4 : OpCodes.Ldind_U2);
            IncrementIPPointer(this.AddressMode / 8);

            EmitLoadMoffsAddress();

            if (returnValue)
            {
                if (!byteVersion)
                    CallGetMemoryInt(this.ValueSize);
                else
                    il.Emit(OpCodes.Call, Infos.PhysicalMemory.GetByte);
            }
        }

        private void EmitLoadMoffsAddress()
        {
            LoadProcessor();

            var doneLabel = il.DefineLabel();

            il.Emit(OpCodes.Call, Infos.Processor.SegmentOverride.GetGetMethod());
            var overrideLabel = il.DefineLabel();
            il.Emit(OpCodes.Brtrue_S, overrideLabel);

            LoadProcessor();
            il.Emit(OpCodes.Ldfld, Infos.Processor.SegmentBases);
            unsafe
            {
                il.LoadConstant(sizeof(uint) * (int)SegmentIndex.DS);
            }

            il.Emit(OpCodes.Conv_I);
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Ldind_U4);

            il.Emit(OpCodes.Br_S, doneLabel);

            il.MarkLabel(overrideLabel);
            LoadProcessor();
            il.Emit(OpCodes.Ldfld, Infos.Processor.BaseOverrides);

            LoadProcessor();
            il.Emit(OpCodes.Call, Infos.Processor.SegmentOverride.GetGetMethod());
            il.Emit(OpCodes.Conv_U4);

            unsafe
            {
                il.LoadConstant(sizeof(uint*));
            }

            il.Emit(OpCodes.Mul);
            il.Emit(OpCodes.Conv_I);
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Ldind_I);
            il.Emit(OpCodes.Ldind_U4);

            il.MarkLabel(doneLabel);

            il.Emit(OpCodes.Add);
        }
    }
}
