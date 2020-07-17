using System;
using System.IO;
using System.Reflection.Emit;

namespace AeonSourceGenerator.Emitters
{
    internal sealed class LoadSegmentRegister : Emitter
    {
        public LoadSegmentRegister(EmitStateInfo state)
            : base(state)
        {
        }

        public override void Initialize(TextWriter writer)
        {
            // Reg is the middle 3 bits of the ModR/M byte.
            writer.WriteLine($"var arg{this.ParameterIndex}Reg = (*ip & 0x38) >> 3;");
            if (this.ByRef)
            {
                writer.Write($"ushort arg{this.ParameterIndex}");
                if (this.WriteOnly)
                    writer.WriteLine(';');
                else
                    writer.WriteLine($" = *p.GetSegmentRegisterPointer(arg{this.ParameterIndex}Reg);");
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
                writer.Write($"*p.GetSegmentRegisterPointer(arg{this.ParameterIndex}Reg)");
            }
        }
        public override void Complete(TextWriter writer)
        {
            if (this.ByRef)
                writer.WriteLine($"vm.WriteSegmentRegister((SegmentIndex)arg{this.ParameterIndex}Reg, arg{this.ParameterIndex});");
        }
    }
}
