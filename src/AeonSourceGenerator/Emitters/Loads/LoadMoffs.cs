using System;
using System.IO;

namespace AeonSourceGenerator.Emitters
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
        public EmitterType MethodArgType
        {
            get
            {
                if (this.ReturnType == EmitReturnType.Address)
                    return new EmitterType(EmitterTypeCode.Int);
                else
                    return GetUnsignedIntType(this.ValueSize);
            }
        }
        public bool? RequiresTemp => this.ReturnType == EmitReturnType.Address;
        public EmitterType? TempType => GetUnsignedIntType(this.ValueSize);

        public override void Initialize(TextWriter writer)
        {
            bool returnValue = this.ReturnType == EmitReturnType.Value;
            bool byteVersion = this.ValueSize == 1;
            bool address32 = this.AddressMode == 32;
            // Memory offset values are always 16-bit in 16-bit addressing mode and 32-bit in 32-bit addressing mode.
            var addressSize = address32 ? "*(uint*)" : "*(ushort*)";

            writer.Write($"uint arg{this.ParameterIndex}Address = ");
            writer.WriteLine($"(uint)(p.SegmentOverride == 0 ? p.SegmentBases[{sizeof(uint) * (int)SegmentIndex.DS}] : p.BaseOverrides[(int)p.SegmentOveride]) + ({addressSize})ReadAndAdvance(ref ip, {this.AddressMode / 8});");

            writer.Write($"var arg{this.ParameterIndex} = vm.PhysicalMemory.");
            if (returnValue)
            {
                if (byteVersion)
                    writer.Write("GetByte");
                else
                    writer.Write(CallGetMemoryInt(this.ValueSize));

                writer.Write($"(arg{this.ParameterIndex}Address);");
            }
        }
    }
}
