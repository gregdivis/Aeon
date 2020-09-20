using System;
using System.IO;

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

        public override void Initialize(TextWriter writer)
        {
            // Reg is the middle 3 bits of the ModR/M byte.
            writer.WriteLine($"\t\t\tvar arg{this.ParameterIndex}Reg = (*ip & 0x38) >> 3;");
        }
        public override void WriteParameter(TextWriter writer)
        {
            if (this.WriteOnly)
                writer.Write("out ");
            else if (this.ByRef)
                writer.Write("ref ");

            writer.Write($"*({this.GetRuntimeTypeName()}*)p.GetRegister");
            writer.Write(this.RegisterSize == 1 ? "Byte" : "Word");
            writer.Write($"Pointer(arg{this.ParameterIndex}Reg)");
        }
    }
}
