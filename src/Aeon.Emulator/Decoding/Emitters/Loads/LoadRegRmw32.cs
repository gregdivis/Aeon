using System.Reflection.Emit;

#nullable disable

namespace Aeon.Emulator.Decoding.Emitters
{
    internal sealed class LoadRegRmw32 : LoadRegRmw
    {
        public LoadRegRmw32(EmitStateInfo state, int valueSize, bool memoryOnly, bool floatingPoint, bool offsetOnly, bool linearAddressOnly)
            : base(state, valueSize, memoryOnly, floatingPoint, offsetOnly, linearAddressOnly)
        {
        }

        protected override void LoadPhysicalAddress(LocalBuilder rmLocal, LocalBuilder modLocal)
        {
            // Advance past RM byte.
            IncrementIPPointer(1);

            il.LoadLocal(rmLocal);
            il.LoadLocal(modLocal);
            this.LoadProcessor();
            il.Emit(OpCodes.Call, typeof(RegRmw32Loads).GetMethod(nameof(RegRmw32Loads.LoadAddress)));
        }
        protected override void LoadAddressOffset(LocalBuilder rmLocal, LocalBuilder modLocal)
        {
            // Advance past RM byte.
            IncrementIPPointer(1);

            il.LoadLocal(rmLocal);
            il.LoadLocal(modLocal);
            this.LoadProcessor();
            il.Emit(OpCodes.Call, typeof(RegRmw32Loads).GetMethod(nameof(RegRmw32Loads.LoadOffset)));
        }
    }
}
