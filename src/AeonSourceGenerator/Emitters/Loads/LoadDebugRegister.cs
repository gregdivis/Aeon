using System.Text;

namespace AeonSourceGenerator.Emitters
{
    internal sealed class LoadDebugRegister : Emitter
    {
        public LoadDebugRegister(EmitStateInfo state)
            : base(state)
        {
        }

        public override void Initialize(StringBuilder writer)
        {
            writer.AppendLine($"\t\tvar arg{this.ParameterIndex} = p.GetDebugRegisterPointer(GetReg(p));");
        }
        public override void WriteParameter(StringBuilder writer)
        {
            if (this.WriteOnly)
                writer.Append("out ");
            else if (this.ByRef)
                writer.Append("ref ");

            writer.Append('*');
            base.WriteParameter(writer);
        }
    }
}
