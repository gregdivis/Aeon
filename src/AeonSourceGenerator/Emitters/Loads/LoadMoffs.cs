using System.Text;

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

        public override void Initialize(StringBuilder writer)
        {
            bool byteVersion = this.ValueSize == 1;
            writer.AppendLine($"\t\tuint arg{this.ParameterIndex}Address = RuntimeCalls.GetMoffsAddress{this.AddressMode}(p);");
            if (!this.WriteOnly)
            {
                writer.Append($"\t\tvar arg{this.ParameterIndex} = vm.PhysicalMemory.");
                if (byteVersion)
                    writer.Append("GetByte");
                else
                    writer.Append(CallGetMemoryInt(this.ValueSize));

                writer.AppendLine($"(arg{this.ParameterIndex}Address);");
            }
        }

        public override void WriteParameter(StringBuilder writer)
        {
            if (this.ByRef)
            {
                if (this.WriteOnly)
                    writer.Append("out var ");
                else if (this.ByRef)
                    writer.Append("ref ");
            }

            writer.Append($"arg{this.ParameterIndex}");
        }

        public override void Complete(StringBuilder writer)
        {
            if (this.ByRef)
                writer.AppendLine($"\t\tvm.PhysicalMemory.Set(arg{this.ParameterIndex}Address, arg{this.ParameterIndex});");
        }
    }
}
