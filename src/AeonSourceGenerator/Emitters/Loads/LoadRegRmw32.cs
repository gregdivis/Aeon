using System.IO;

namespace AeonSourceGenerator.Emitters
{
    internal sealed class LoadRegRmw32 : LoadRegRmw
    {
        public LoadRegRmw32(EmitStateInfo state, int valueSize, bool memoryOnly, bool floatingPoint, bool offsetOnly, bool linearAddressOnly)
            : base(state, valueSize, memoryOnly, floatingPoint, offsetOnly, linearAddressOnly)
        {
        }

        public override void Initialize(TextWriter writer)
        {
            base.Initialize(writer);
            var offsetOnly = this.OffsetOnly ? "true" : "false";
            var memoryOnly = this.MemoryOnly ? "true" : "false";
            writer.WriteLine($"\t\t\tvar arg{this.ParameterIndex} = GetRegRmw32<{this.GetRuntimeTypeName()}>(ref ip, p, mod, rm, {offsetOnly}, {memoryOnly});");
            writer.WriteLine($"\t\t\t{this.GetRuntimeTypeName()} arg{this.ParameterIndex}Temp = 0;");
            if (!this.WriteOnly)
            {
                writer.WriteLine($"\t\t\tif (!arg{this.ParameterIndex}.IsPointer)");
                writer.WriteLine($"\t\t\t\targ{this.ParameterIndex}Temp = vm.PhysicalMemory.{CallGetMemoryInt(this.ValueSize)}(arg{this.ParameterIndex}.Address);");
            }
        }
    }
}
