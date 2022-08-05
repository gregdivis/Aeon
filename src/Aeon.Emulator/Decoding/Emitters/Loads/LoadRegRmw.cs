using System;
using System.Diagnostics;
using System.Reflection.Emit;
using System.Runtime.Intrinsics.X86;

#nullable disable

namespace Aeon.Emulator.Decoding.Emitters
{
    internal abstract class LoadRegRmw : Emitter
    {
        private LocalBuilder paramIsAddress;

        protected LoadRegRmw(EmitStateInfo state, int valueSize, bool memoryOnly, bool floatingPoint, bool offsetOnly, bool linearAddressOnly)
            : base(state)
        {
            if (valueSize is not 1 and not 2 and not 4 and not 6 and not 8 and not 10)
                throw new ArgumentException("Invalid size.");
            if (!memoryOnly && offsetOnly)
                throw new ArgumentException("Effective address invalid for registers.");

            this.ValueSize = valueSize;
            this.MemoryOnly = memoryOnly;
            this.FloatingPoint = floatingPoint;
            this.OffsetOnly = offsetOnly;
            this.LinearAddressOnly = linearAddressOnly;
        }

        public int ValueSize { get; }
        public bool MemoryOnly { get; }
        public bool FloatingPoint { get; }
        public bool OffsetOnly { get; }
        public bool LinearAddressOnly { get; }
        public override Type MethodArgType => this.ReturnType == EmitReturnType.Value ? this.TempType : typeof(IntPtr);
        public override bool? RequiresTemp
        {
            get
            {
                if (this.ReturnType == EmitReturnType.Value || this.OffsetOnly)
                    return false;
                else if (this.MemoryOnly)
                    return true;
                else
                    return null;
            }
        }
        public override Type TempType
        {
            get
            {
                if (!this.FloatingPoint)
                    return GetUnsignedIntType(this.ValueSize);
                else
                    return GetFloatType(this.ValueSize);
            }
        }
        public override LocalBuilder ParamIsAddressLocal => this.paramIsAddress;

        public static LoadRegRmw Create(EmitStateInfo state, int valueSize, bool memoryOnly)
        {
            if (state.AddressMode == 16)
                return new LoadRegRmw16(state, valueSize, memoryOnly, false, false, false);
            else if (state.AddressMode == 32)
                return new LoadRegRmw32(state, valueSize, memoryOnly, false, false, false);
            else
                throw new ArgumentException("Invalid addressing mode.");
        }
        public static LoadRegRmw CreateFloat(EmitStateInfo state, int valueSize)
        {
            if (state.AddressMode == 16)
                return new LoadRegRmw16(state, valueSize, true, true, false, false);
            else if (state.AddressMode == 32)
                return new LoadRegRmw32(state, valueSize, true, true, false, false);
            else
                throw new ArgumentException("Invalid addressing mode.");
        }
        public static LoadRegRmw CreateEffectiveAddress(EmitStateInfo state)
        {
            if (state.AddressMode == 16)
                return new LoadRegRmw16(state, 2, true, false, true, false);
            else if (state.AddressMode == 32)
                return new LoadRegRmw32(state, 4, true, false, true, false);
            else
                throw new ArgumentException("Invalid addressing mode.");
        }
        public static LoadRegRmw CreateLinearAddress(EmitStateInfo state)
        {
            if (state.AddressMode == 16)
                return new LoadRegRmw16(state, 4, true, false, false, true);
            else if (state.AddressMode == 32)
                return new LoadRegRmw32(state, 4, true, false, false, true);
            else
                throw new ArgumentException("Invalid addressing mode.");
        }

        public sealed override void EmitLoad()
        {
            bool byteVersion = this.ValueSize == 1;
            bool returnValue = this.ReturnType == EmitReturnType.Value;

            var doneLabel = il.DefineLabel();

            var rmLocal = il.DeclareLocal(typeof(int));
            var modLocal = il.DeclareLocal(typeof(int));

            //// R/M is the low 3 bits of the ModR/M byte.
            //int rm = *ip & 0x07;
            //// Mod is the high 2 bits of the ModR/M byte.
            //int mod = (*ip & 0xC0) >> 6;

            //il.LoadLocal(ipPointer);
            LoadIPPointer();
            il.Emit(OpCodes.Ldind_U1);
            il.Emit(OpCodes.Dup);

            il.LoadConstant(0x07);
            il.Emit(OpCodes.And);
            il.StoreLocal(rmLocal);

            if (Bmi1.IsSupported)
            {
                il.LoadConstant(0x0206);
                il.Emit(OpCodes.Call, Infos.Intrinsics.BitFieldExtract);
            }
            else
            {
                il.LoadConstant(0xC0);
                il.Emit(OpCodes.And);
                il.LoadConstant(6);
                il.Emit(OpCodes.Shr_Un);
            }
            il.StoreLocal(modLocal);

            // This stuff is only needed when the operand could be a register.
            if (!this.MemoryOnly)
            {
                Label memoryLabel = this.il.DefineLabel();

                // This will store a value that indicates whether the operand is a pointer to a register.
                if (!returnValue)
                {
                    this.paramIsAddress = il.DeclareLocal(typeof(bool));
                    il.LoadConstant(0);
                    il.StoreLocal(this.paramIsAddress);
                }

                // If modLocal != 3, go to memoryLabel.
                il.LoadLocal(modLocal);
                il.LoadConstant(3);
                il.Emit(OpCodes.Ceq);
                il.Emit(OpCodes.Brfalse_S, memoryLabel);

                // Handle register case.
                LoadProcessor();
                il.LoadLocal(rmLocal);
                if (!byteVersion)
                {
                    il.Emit(OpCodes.Call, Infos.Processor.GetRegisterWordPointer);
                    if (returnValue)
                        il.Emit(this.ValueSize == 4 ? OpCodes.Ldind_U4 : OpCodes.Ldind_U2);
                }
                else
                {
                    il.Emit(OpCodes.Call, Infos.Processor.GetRegisterBytePointer);
                    if (returnValue)
                        il.Emit(OpCodes.Ldind_U1);
                }

                // Advance past the ModRM byte.
                IncrementIPPointer(1);
                il.Emit(OpCodes.Br, doneLabel);

                il.MarkLabel(memoryLabel);
            }
            else
                EmitMod3Test(modLocal);

            if (returnValue && !this.OffsetOnly && !this.LinearAddressOnly)
                LoadPhysicalMemory();

            if (!this.OffsetOnly)
            {
                // Handle memory case.  Address will be on the top of the stack after this call.
                LoadPhysicalAddress(rmLocal, modLocal);
            }
            else
            {
                // Handle memory case.  Offset will be on the top of the stack after this call.
                LoadAddressOffset(rmLocal, modLocal);
            }

            if (returnValue && !this.OffsetOnly && !this.LinearAddressOnly)
            {
                if (!this.FloatingPoint)
                    CallGetMemoryInt(this.ValueSize);
                else
                    CallGetMemoryReal(this.ValueSize);
            }
            else if (!this.MemoryOnly)
            {
                il.LoadConstant(1);
                il.StoreLocal(paramIsAddress);
            }

            il.MarkLabel(doneLabel);
        }

        protected abstract void LoadPhysicalAddress(LocalBuilder rmLocal, LocalBuilder modLocal);
        protected abstract void LoadAddressOffset(LocalBuilder rmLocal, LocalBuilder modLocal);

        [Conditional("DEBUG")]
        private void EmitMod3Test(LocalBuilder modLocal)
        {
            var modNot3Label = il.DefineLabel();
            il.LoadLocal(modLocal);
            il.LoadConstant(3);
            il.Emit(OpCodes.Ceq);
            il.Emit(OpCodes.Brfalse_S, modNot3Label);

            il.Emit(OpCodes.Newobj, typeof(Mod3Exception).GetConstructor(Type.EmptyTypes));
            il.Emit(OpCodes.Call, Infos.RuntimeCalls.ThrowException);

            il.Emit(OpCodes.Ret);

            il.MarkLabel(modNot3Label);
        }
    }
}
