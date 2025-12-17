using System.CodeDom.Compiler;

namespace Aeon.SourceGenerator.Emitters
{
    internal sealed class LoadRegister : Emitter
    {
        public LoadRegister(EmitStateInfo state, int registerSize)
            : base(state)
        {
            if (registerSize is not 1 and not 2 and not 4)
                throw new ArgumentException("Invalid register size.");

            this.RegisterSize = registerSize;
        }

        public int RegisterSize { get; }

        public override void Initialize(IndentedTextWriter writer)
        {
            writer.WriteLine($"var arg{this.ParameterIndex}Reg = GetReg(p);");
        }
        public override void WriteParameter(TextWriter writer)
        {
            if (this.WriteOnly)
                writer.Write("out ");
            else if (this.ByRef)
                writer.Write("ref ");

            if (this.RegisterSize == 1)
            {
                writer.Write($"p.GetByteRegister(arg{this.ParameterIndex}Reg)");
            }
            else
            {
                writer.Write($"p.GetWordRegister<{this.GetRuntimeTypeName()}>(arg{this.ParameterIndex}Reg)");
            }

            //    writer.Write($"*({this.GetRuntimeTypeName()}*)p.GetRegister");
            //writer.Write(this.RegisterSize == 1 ? "Byte" : "Word");
            //writer.Write($"Pointer(arg{this.ParameterIndex}Reg)");
        }
    }
}
