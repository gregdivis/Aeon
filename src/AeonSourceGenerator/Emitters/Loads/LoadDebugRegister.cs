using System.CodeDom.Compiler;

namespace Aeon.SourceGenerator.Emitters
{
    internal sealed class LoadDebugRegister : Emitter
    {
        public LoadDebugRegister(EmitStateInfo state)
            : base(state)
        {
        }

        public override void Initialize(IndentedTextWriter writer)
        {
            writer.WriteLine($"ref uint arg{this.ParameterIndex} = ref p.GetDebugRegisterPointer(GetReg(p));");
        }
        public override void WriteParameter(TextWriter writer)
        {
            if (this.WriteOnly)
                writer.Write("out ");
            else if (this.ByRef)
                writer.Write("ref ");

            base.WriteParameter(writer);
        }
    }
}
