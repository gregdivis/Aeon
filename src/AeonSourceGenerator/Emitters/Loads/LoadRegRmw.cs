using System.CodeDom.Compiler;

namespace Aeon.SourceGenerator.Emitters
{
    internal abstract class LoadRegRmw : Emitter
    {
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

        public override void Initialize(IndentedTextWriter writer)
        {
            writer.WriteLine("GetModRm(p, out var mod, out var rm);");
        }

        public override void WriteParameter(TextWriter writer)
        {
            if (this.OffsetOnly)
            {
                base.WriteParameter(writer);
            }
            else if (!this.ByRef)
            {
                if (this.MemoryOnly)
                    writer.Write($"arg{this.ParameterIndex}Temp");
                else
                    writer.Write($"arg{this.ParameterIndex}.IsPointer ? arg{this.ParameterIndex}.RegisterValue : arg{this.ParameterIndex}Temp");
            }
            else
            {
                if (this.WriteOnly)
                    writer.Write("out ");
                else if (this.ByRef)
                    writer.Write("ref ");

                if (this.MemoryOnly)
                    writer.Write($"arg{this.ParameterIndex}Temp");
                else
                    writer.Write($"(arg{this.ParameterIndex}.IsPointer ? ref arg{this.ParameterIndex}.RegisterValue : ref arg{this.ParameterIndex}Temp)");
            }
        }

        public override void Complete(IndentedTextWriter writer)
        {
            if (!this.OffsetOnly && this.ByRef)
            {
                if (!this.MemoryOnly)
                {
                    writer.WriteLine($"if (!arg{this.ParameterIndex}.IsPointer)");
                    writer.Indent++;
                }

                writer.WriteLine($"vm.PhysicalMemory.Set(arg{this.ParameterIndex}.Address, arg{this.ParameterIndex}Temp);");

                if (!this.MemoryOnly)
                    writer.Indent--;
            }
        }
    }
}
