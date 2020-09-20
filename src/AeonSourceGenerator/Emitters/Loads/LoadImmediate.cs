using System;
using System.IO;

namespace AeonSourceGenerator.Emitters
{
    internal class LoadImmediate : Emitter
    {
        public LoadImmediate(EmitStateInfo state, int immediateSize, ValueExtend valueExtend)
            : base(state)
        {
            if (immediateSize != 1 && immediateSize != 2 && immediateSize != 4 && immediateSize != 6 && immediateSize != 8)
                throw new ArgumentException("Unsupported immediate size.");

            this.ImmediateSize = immediateSize;
            this.ValueExtend = valueExtend;
        }

        public int ImmediateSize { get; }
        public ValueExtend ValueExtend { get; }

        public override void Initialize(TextWriter writer)
        {
            writer.WriteLine($"\t\t\tvar arg{this.ParameterIndex} = ReadImmediate<{this.GetTypeName()}>(ref ip);");
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
