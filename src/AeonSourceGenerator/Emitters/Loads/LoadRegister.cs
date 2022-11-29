using System.Text;

namespace AeonSourceGenerator.Emitters
{
    internal sealed class LoadRegister : Emitter
    {
        public LoadRegister(EmitStateInfo state, int registerSize)
            : base(state)
        {
            if (registerSize != 1 && registerSize != 2 && registerSize != 4)
                throw new ArgumentException("Invalid register size.");

            this.RegisterSize = registerSize;
        }

        public int RegisterSize { get; }

        public override void Initialize(StringBuilder writer)
        {
            // Reg is the middle 3 bits of the ModR/M byte.
            writer.AppendLine($"\t\tvar arg{this.ParameterIndex}Reg = GetReg(p);");
        }
        public override void WriteParameter(StringBuilder writer)
        {
            if (this.WriteOnly)
                writer.Append("out ");
            else if (this.ByRef)
                writer.Append("ref ");

            writer.Append($"*({this.GetRuntimeTypeName()}*)p.GetRegister");
            writer.Append(this.RegisterSize == 1 ? "Byte" : "Word");
            writer.Append($"Pointer(arg{this.ParameterIndex}Reg)");
        }
    }
}
