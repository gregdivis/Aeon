using System.CodeDom.Compiler;

namespace Aeon.SourceGenerator.Emitters
{
    internal sealed class LoadSegmentRegister(EmitStateInfo state) : Emitter(state)
    {
        public override void Initialize(IndentedTextWriter writer)
        {
            writer.WriteLine($"var arg{this.ParameterIndex}Reg = GetReg(p);");
            if (this.ByRef)
            {
                writer.Write($"ushort arg{this.ParameterIndex}");
                if (this.WriteOnly)
                    writer.WriteLine(';');
                else
                    writer.WriteLine($" = p.GetSegmentRegisterPointer(arg{this.ParameterIndex}Reg);");
            }
        }
        public override void WriteParameter(TextWriter writer)
        {
            if (this.ByRef)
            {
                if (this.WriteOnly)
                    writer.Write("out ");
                else
                    writer.Write("ref ");

                writer.Write($"arg{this.ParameterIndex}");
            }
            else
            {
                writer.Write($"p.GetSegmentRegisterPointer(arg{this.ParameterIndex}Reg)");
            }
        }
        public override void Complete(IndentedTextWriter writer)
        {
            if (this.ByRef)
                writer.WriteLine($"vm.WriteSegmentRegister((SegmentIndex)arg{this.ParameterIndex}Reg, arg{this.ParameterIndex});");
        }
    }
}
