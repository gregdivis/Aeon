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

            var methodInfo = typeof(RuntimeCalls).GetMethod(address32 ? nameof(RuntimeCalls.GetMoffsAddress32) : nameof(RuntimeCalls.GetMoffsAddress16));
            LoadProcessor();
            il.Emit(OpCodes.Call, methodInfo);

            if (returnValue)
            {
                if (!byteVersion)
                    CallGetMemoryInt(this.ValueSize);
                else
                    il.Emit(OpCodes.Call, Infos.PhysicalMemory.GetByte);
            }
        }
    }
}
