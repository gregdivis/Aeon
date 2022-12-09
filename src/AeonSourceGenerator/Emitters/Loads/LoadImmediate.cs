using System.CodeDom.Compiler;

namespace Aeon.SourceGenerator.Emitters
{
    internal class LoadImmediate : Emitter
    {
        public LoadImmediate(EmitStateInfo state, int immediateSize, ValueExtend valueExtend)
            : base(state)
        {
            if (immediateSize is not 1 and not 2 and not 4 and not 6 and not 8)
                throw new ArgumentException("Unsupported immediate size.");

            this.ImmediateSize = immediateSize;
            this.ValueExtend = valueExtend;
        }

        public int ImmediateSize { get; }
        public ValueExtend ValueExtend { get; }

        public override void Initialize(IndentedTextWriter writer)
        {
            writer.WriteLine($"var arg{this.ParameterIndex} = ({this.GetRuntimeTypeName()})ReadImmediate<{this.GetTypeName()}>(p);");
        }

        private string GetTypeName()
        {
            if (this.ValueExtend == ValueExtend.Sign)
            {
                return this.ImmediateSize switch
                {
                    1 => "sbyte",
                    2 => "short",
                    4 => "int",
                    6 => "long",
                    8 => "long",
                    _ => throw new InvalidOperationException()
                };
            }
            else
            {
                return this.ImmediateSize switch
                {
                    1 => "byte",
                    2 => "ushort",
                    4 => "uint",
                    6 => "long",
                    8 => "long",
                    _ => throw new InvalidOperationException()
                };
            }
        }
    }
}
