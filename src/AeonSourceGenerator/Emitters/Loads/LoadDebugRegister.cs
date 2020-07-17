using System;
using System.IO;

namespace AeonSourceGenerator.Emitters
{
    internal sealed class LoadDebugRegister : Emitter
    {
        public LoadDebugRegister(EmitStateInfo state)
            : base(state)
        {
        }

        public override void Initialize(TextWriter writer)
        {
            writer.WriteLine($"var arg{this.ParameterIndex} = p.GetDebugRegisterPointer((*eip & 0x38) >> 3);");
        }
        public override void WriteParameter(TextWriter writer)
        {
            if (this.WriteOnly)
                writer.WriteLine("out ");
            else if (this.ByRef)
                writer.WriteLine("ref ");

            writer.Write('*');
            base.WriteParameter(writer);
        }
    }
}
