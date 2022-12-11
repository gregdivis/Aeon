using System.CodeDom.Compiler;

namespace Aeon.SourceGenerator.Emitters
{
    internal abstract class Emitter
    {
        private readonly EmitStateInfo state;

        protected Emitter(EmitStateInfo state)
        {
            this.state = state;
        }

        protected int WordSize => this.state.WordSize;
        protected EmitReturnType ReturnType => this.state.ReturnType;
        protected int AddressMode => this.state.AddressMode;
        protected int ParameterIndex => this.state.ParameterIndex;
        protected bool ByRef => this.state.ReturnType == EmitReturnType.Address;
        protected bool WriteOnly => this.state.WriteOnly;
        protected virtual EmitterTypeCode MethodArgType => this.state.MethodArgType;

        public static void WriteCall(TextWriter writer, EmulateMethodCall method)
        {
            writer.Write(method.Name);
            writer.Write('(');
            writer.Write(method.Arg1);
            foreach (var emitter in method.ArgEmitters)
            {
                writer.Write(", ");
                emitter.WriteParameter(writer);
            }

            writer.WriteLine(");");
        }

        public virtual void Initialize(IndentedTextWriter writer)
        {
        }
        public virtual void WriteParameter(TextWriter writer)
        {
            writer.Write("arg");
            writer.Write(this.ParameterIndex);
        }
        public virtual void Complete(IndentedTextWriter writer)
        {
        }

        protected static EmitterType GetUnsignedIntType(int size)
        {
            return size switch
            {
                1 => new EmitterType(EmitterTypeCode.Byte),
                2 => new EmitterType(EmitterTypeCode.UShort),
                4 => new EmitterType(EmitterTypeCode.UInt),
                6 or 8 => new EmitterType(EmitterTypeCode.ULong),
                _ => throw new ArgumentException("Invalid size.")
            };
        }
        protected static string CallGetMemoryInt(int size)
        {
            return size switch
            {
                1 => "GetByte",
                2 => "GetUInt16",
                4 => "GetUInt32",
                8 => "GetUInt64",
                10 => "GetReal80",
                _ => throw new ArgumentException("Unsupported type."),
            };
        }
        protected string GetRuntimeTypeName() => GetRuntimeTypeName(this.MethodArgType);
        protected static string GetRuntimeTypeName(EmitterTypeCode typeCode)
        {
            return typeCode switch
            {
                EmitterTypeCode.Byte => "byte",
                EmitterTypeCode.SByte => "sbyte",
                EmitterTypeCode.Short => "short",
                EmitterTypeCode.UShort => "ushort",
                EmitterTypeCode.Int => "int",
                EmitterTypeCode.UInt => "uint",
                EmitterTypeCode.Long => "long",
                EmitterTypeCode.ULong => "ulong",
                EmitterTypeCode.Float => "float",
                EmitterTypeCode.Double => "double",
                EmitterTypeCode.Real10 => "Real10",
                _ => throw new ArgumentException()
            };
        }
    }
}
