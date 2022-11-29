using System.Text;

namespace AeonSourceGenerator.Emitters
{
    internal sealed class LoadSegmentRegister : Emitter
    {
        public LoadSegmentRegister(EmitStateInfo state)
            : base(state)
        {
        }

        public override void Initialize(StringBuilder writer)
        {
            // Reg is the middle 3 bits of the ModR/M byte.
            writer.AppendLine($"\t\tvar arg{this.ParameterIndex}Reg = GetReg(p);");
            if (this.ByRef)
            {
                writer.Append($"\t\tushort arg{this.ParameterIndex}");
                if (this.WriteOnly)
                    writer.AppendLine(";");
                else
                    writer.AppendLine($" = *p.GetSegmentRegisterPointer(arg{this.ParameterIndex}Reg);");
            }
        }
        public override void WriteParameter(StringBuilder writer)
        {
            if (this.ByRef)
            {
                if (this.WriteOnly)
                    writer.Append("out ");
                else
                    writer.Append("ref ");

                writer.Append($"arg{this.ParameterIndex}");
            }
            else
            {
                writer.Append($"*p.GetSegmentRegisterPointer(arg{this.ParameterIndex}Reg)");
            }
        }
        public override void Complete(StringBuilder writer)
        {
            if (this.ByRef)
                writer.AppendLine($"\t\tvm.WriteSegmentRegister((SegmentIndex)arg{this.ParameterIndex}Reg, arg{this.ParameterIndex});");
        }
    }
}
