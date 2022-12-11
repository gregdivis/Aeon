using System.CodeDom.Compiler;

namespace Aeon.SourceGenerator.Emitters
{
    internal class LoadMoffs : Emitter
    {
        public LoadMoffs(EmitStateInfo state, int valueSize)
            : base(state)
        {
            if (valueSize is not 1 and not 2 and not 4 and not 8)
                throw new ArgumentException("Invalid size.");

            this.ValueSize = valueSize;
        }

        public int ValueSize { get; }
        protected override EmitterTypeCode MethodArgType
        {
            get
            {
                if (this.ReturnType == EmitReturnType.Address)
                    return EmitterTypeCode.Int;
                else
                    return GetUnsignedIntType(this.ValueSize).TypeCode;
            }
        }
        public bool? RequiresTemp => this.ReturnType == EmitReturnType.Address;
        public EmitterType? TempType => GetUnsignedIntType(this.ValueSize);

        public override void Initialize(IndentedTextWriter writer)
        {
            bool byteVersion = this.ValueSize == 1;
            writer.WriteLine($"uint arg{this.ParameterIndex}Address = RuntimeCalls.GetMoffsAddress{this.AddressMode}(p);");
            if (!this.WriteOnly)
            {
                writer.Write($"var arg{this.ParameterIndex} = vm.PhysicalMemory.");
                if (byteVersion)
                    writer.Write("GetByte");
                else
                    writer.Write(CallGetMemoryInt(this.ValueSize));

                writer.WriteLine($"(arg{this.ParameterIndex}Address);");
            }
        }

        public override void WriteParameter(TextWriter writer)
        {
            if (this.ByRef)
            {
                if (this.WriteOnly)
                    writer.Write("out var ");
                else if (this.ByRef)
                    writer.Write("ref ");
            }

            writer.Write($"arg{this.ParameterIndex}");
        }

        public override void Complete(IndentedTextWriter writer)
        {
            if (this.ByRef)
                writer.WriteLine($"vm.PhysicalMemory.Set(arg{this.ParameterIndex}Address, arg{this.ParameterIndex});");
        }
    }
}
