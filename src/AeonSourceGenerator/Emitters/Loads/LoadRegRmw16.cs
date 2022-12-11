using System.CodeDom.Compiler;

namespace Aeon.SourceGenerator.Emitters
{
    internal sealed class LoadRegRmw16 : LoadRegRmw
    {
        public LoadRegRmw16(EmitStateInfo state, int valueSize, bool memoryOnly, bool floatingPoint, bool offsetOnly, bool linearAddressOnly)
            : base(state, valueSize, memoryOnly, floatingPoint, offsetOnly, linearAddressOnly)
        {
        }

        public override void Initialize(IndentedTextWriter writer)
        {
            base.Initialize(writer);

            if (this.OffsetOnly)
            {
                writer.WriteLine($"var arg{this.ParameterIndex} = (ushort)RegRmw16Loads.LoadOffset(rm, mod, p);");
            }
            else
            {
                var memoryOnly = this.MemoryOnly ? "true" : "false";
                writer.WriteLine($"var arg{this.ParameterIndex} = GetRegRmw16<{this.GetRuntimeTypeName()}>(p, mod, rm, {memoryOnly});");
                writer.WriteLine($"{this.GetRuntimeTypeName()} arg{this.ParameterIndex}Temp = 0;");
                if (!this.WriteOnly)
                {
                    writer.WriteLine($"if (!arg{this.ParameterIndex}.IsPointer)");
                    writer.Indent++;
                    writer.WriteLine($"arg{this.ParameterIndex}Temp = vm.PhysicalMemory.Get<{this.GetRuntimeTypeName()}>(arg{this.ParameterIndex}.Address);");
                    writer.Indent--;
                }
            }
        }
    }
}
