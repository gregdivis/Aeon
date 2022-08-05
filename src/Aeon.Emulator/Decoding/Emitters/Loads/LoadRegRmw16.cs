using System;
using System.Reflection.Emit;

#nullable disable

namespace Aeon.Emulator.Decoding.Emitters
{
    internal sealed class LoadRegRmw16 : LoadRegRmw
    {
        public LoadRegRmw16(EmitStateInfo state, int valueSize, bool memoryOnly, bool floatingPoint, bool offsetOnly, bool linearAddressOnly)
            : base(state, valueSize, memoryOnly, floatingPoint, offsetOnly, linearAddressOnly)
        {
        }

        public override Type MethodArgType => this.OffsetOnly ? typeof(ushort) : base.MethodArgType;

        protected override void LoadPhysicalAddress(LocalBuilder rmLocal, LocalBuilder modLocal) => GenerateAddress(rmLocal, modLocal, false);
        protected override void LoadAddressOffset(LocalBuilder rmLocal, LocalBuilder modLocal) => GenerateAddress(rmLocal, modLocal, true);

        private void GenerateAddress(LocalBuilder rmLocal, LocalBuilder modLocal, bool offsetOnly)
        {
            Label modNotZeroLabel = il.DefineLabel();
            Label rmSixLabel = il.DefineLabel();
            Label gotDisplacement = il.DefineLabel();
            Label modTwoLabel = il.DefineLabel();
            Label doneLabel = il.DefineLabel();

            // Calculate and load the displacement.
            il.LoadLocal(modLocal);
            il.Emit(OpCodes.Brtrue_S, modNotZeroLabel);
            il.LoadLocal(rmLocal);
            il.LoadConstant(6);
            il.Emit(OpCodes.Ceq);
            il.Emit(OpCodes.Brtrue_S, rmSixLabel);

            // Load a displacement of 0.
            IncrementIPPointer(1);
            il.LoadConstant(0);
            il.Emit(OpCodes.Br, gotDisplacement);

            // If rm == 6, load *(short*)(ip + 1).
            il.MarkLabel(rmSixLabel);
            //LoadProcessor();
            LoadIPPointer();
            il.LoadConstant(1);
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Ldind_U2);

            if (!offsetOnly)
            {
                LoadBaseAddress16(rmLocal, () =>
                {
                    LoadProcessor();
                    il.Emit(OpCodes.Ldfld, Infos.Processor.SegmentBases);
                    il.LoadConstant((int)SegmentIndex.DS);
                    il.Emit(OpCodes.Conv_I);
                    il.Emit(OpCodes.Sizeof, typeof(uint));
                    il.Emit(OpCodes.Mul);
                    il.Emit(OpCodes.Add);
                });
                il.Emit(OpCodes.Add);
                il.Emit(OpCodes.Conv_U4);
            }

            //il.Emit(OpCodes.Call, Infos.RuntimeCalls.GetMoffsAddress16);
            IncrementIPPointer(3);
            il.Emit(OpCodes.Br_S, doneLabel);

            // If mod != 0, check for 1, else 2.
            il.MarkLabel(modNotZeroLabel);
            il.LoadLocal(modLocal);
            il.LoadConstant(1);
            il.Emit(OpCodes.Ceq);
            il.Emit(OpCodes.Brfalse_S, modTwoLabel);

            // If mod == 1, load *(byte*)(ip + 1).
            LoadIPPointer();
            il.LoadConstant(1);
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Ldind_I1);
            IncrementIPPointer(2);
            il.Emit(OpCodes.Br_S, gotDisplacement);

            // If mod == 2, load *(short*)(ip + 1).
            il.MarkLabel(modTwoLabel);
            LoadIPPointer();
            il.LoadConstant(1);
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Ldind_I2);
            IncrementIPPointer(3);

            // Finshed getting displacement, call GetAddress.
            il.MarkLabel(gotDisplacement);
            var displacementLocal = il.DeclareLocal(typeof(ushort));
            il.StoreLocal(displacementLocal);

            GetModRMAddress16(rmLocal, displacementLocal);

            if (!offsetOnly)
            {
                LoadBaseAddress16(rmLocal, () =>
                {
                    LoadProcessor();
                    il.Emit(OpCodes.Ldfld, Infos.Processor.DefaultSegments16);
                    il.LoadLocal(rmLocal);
                    il.Emit(OpCodes.Conv_I);
                    il.Emit(OpCodes.Sizeof, typeof(uint).MakePointerType());
                    il.Emit(OpCodes.Mul);
                    il.Emit(OpCodes.Add);
                    il.Emit(OpCodes.Ldind_I);
                });

                il.Emit(OpCodes.Add);
                il.Emit(OpCodes.Conv_U4);
            }

            // Mark the end of the function.
            il.MarkLabel(doneLabel);
            if (offsetOnly)
                il.Emit(OpCodes.Conv_U2);
        }

        private void GetModRMAddress16(LocalBuilder rmLocal, LocalBuilder displacementLocal)
        {
            this.LoadProcessor();
            il.LoadLocal(rmLocal);
            il.LoadLocal(displacementLocal);
            il.Emit(OpCodes.Call, Infos.Processor.GetRM16Offset);
        }
        private void LoadBaseAddress16(LocalBuilder rmLocal, Action loadDefaultBase)
        {
            LoadProcessor();
            il.Emit(OpCodes.Ldfld, Infos.Processor.BaseOverrides);
            LoadProcessor();
            il.Emit(OpCodes.Call, Infos.Processor.SegmentOverride.GetGetMethod());
            il.Emit(OpCodes.Conv_I);
            il.Emit(OpCodes.Sizeof, typeof(uint).MakePointerType());
            il.Emit(OpCodes.Mul);
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Ldind_I);
            il.Emit(OpCodes.Dup);

            var gotBase = il.DefineLabel();
            il.Emit(OpCodes.Brtrue_S, gotBase);

            il.Emit(OpCodes.Pop);

            loadDefaultBase();

            il.MarkLabel(gotBase);
            il.Emit(OpCodes.Ldind_U4);
        }
    }
}
