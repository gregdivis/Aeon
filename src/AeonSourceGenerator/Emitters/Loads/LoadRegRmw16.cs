using System.Text;

namespace AeonSourceGenerator.Emitters
{
    internal sealed class LoadRegRmw16 : LoadRegRmw
    {
        public LoadRegRmw16(EmitStateInfo state, int valueSize, bool memoryOnly, bool floatingPoint, bool offsetOnly, bool linearAddressOnly)
            : base(state, valueSize, memoryOnly, floatingPoint, offsetOnly, linearAddressOnly)
        {
        }

        public override void Initialize(StringBuilder writer)
        {
            base.Initialize(writer);

            if (this.OffsetOnly)
            {
                writer.AppendLine($"\t\tvar arg{this.ParameterIndex} = (ushort)RegRmw16Loads.LoadOffset(rm, mod, p);");
            }
            else
            {
                var memoryOnly = this.MemoryOnly ? "true" : "false";
                writer.AppendLine($"\t\tvar arg{this.ParameterIndex} = GetRegRmw16<{this.GetRuntimeTypeName()}>(p, mod, rm, {memoryOnly});");
                writer.AppendLine($"\t\t{this.GetRuntimeTypeName()} arg{this.ParameterIndex}Temp = 0;");
                if (!this.WriteOnly)
                {
                    writer.AppendLine($"\t\tif (!arg{this.ParameterIndex}.IsPointer)");
                    writer.AppendLine($"\t\t\targ{this.ParameterIndex}Temp = vm.PhysicalMemory.Get<{this.GetRuntimeTypeName()}>(arg{this.ParameterIndex}.Address);");
                }
            }
        }
    }
}
