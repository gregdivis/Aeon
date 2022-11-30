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
            var methodInfo = typeof(RegRmw16Loads).GetMethod(offsetOnly ? nameof(RegRmw16Loads.LoadOffset) : nameof(RegRmw16Loads.LoadAddress));
            this.il.LoadLocal(rmLocal);
            this.il.LoadLocal(modLocal);
            this.LoadProcessor();
            this.il.Emit(OpCodes.Call, methodInfo);
        }
    }
}
