using System.IO;

namespace AeonSourceGenerator.Emitters
{
    internal sealed class LoadRegRmw16 : LoadRegRmw
    {
        public LoadRegRmw16(EmitStateInfo state, int valueSize, bool memoryOnly, bool floatingPoint, bool offsetOnly, bool linearAddressOnly)
            : base(state, valueSize, memoryOnly, floatingPoint, offsetOnly, linearAddressOnly)
        {
        }

        public override void Initialize(TextWriter writer)
        {
            var offsetOnly = this.OffsetOnly ? "true" : "false";
            var memoryOnly = this.MemoryOnly ? "true" : "false";
            writer.WriteLine($"var arg{this.ParameterIndex} = GetRegRmw<{this.GetRuntimeTypeName()}>(ref ip, p, mod, rm, {offsetOnly}, {memoryOnly});");
        }
    }
}
