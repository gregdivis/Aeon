using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AeonSourceGenerator.Emitters
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
        protected bool WriteOnly => throw new NotImplementedException();
        protected EmitterTypeCode MethodArgType => this.state.MethodArgType;

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

        public virtual void Initialize(TextWriter writer)
        {
        }
        public virtual void WriteParameter(TextWriter writer)
        {
            writer.Write("arg");
            writer.Write(this.ParameterIndex);
        }
        public virtual void Complete(TextWriter writer)
        {
        }

        //protected void IncrementIPPointer(int n)
        //{
        //    if (n != 1)
        //        this.sb.AppendLine($"p.CachedIP += {n};");
        //    else
        //        this.sb.AppendLine($"*p.CachedIP++;");
        //}
        protected static EmitterType GetUnsignedIntType(int size)
        {
            switch (size)
            {
                case 1:
                    return new EmitterType(EmitterTypeCode.Byte);

                case 2:
                    return new EmitterType(EmitterTypeCode.UShort);

                case 4:
                    return new EmitterType(EmitterTypeCode.UInt);

                case 6:
                case 8:
                    return new EmitterType(EmitterTypeCode.ULong);

                default:
                    throw new ArgumentException("Invalid size.");
            }
        }
        protected static EmitterType GetSignedIntType(int size)
        {
            switch (size)
            {
                case 1:
                    return new EmitterType(EmitterTypeCode.SByte);

                case 2:
                    return new EmitterType(EmitterTypeCode.Short);

                case 4:
                    return new EmitterType(EmitterTypeCode.Int);

                case 6:
                case 8:
                    return new EmitterType(EmitterTypeCode.Long);

                default:
                    throw new ArgumentException("Invalid size.");
            }
        }
        protected static EmitterType GetFloatType(int size)
        {
            return size switch
            {
                4 => new EmitterType(EmitterTypeCode.Float),
                8 => new EmitterType(EmitterTypeCode.Double),
                10 => new EmitterType(EmitterTypeCode.Real10),
                _ => throw new ArgumentException("Invalid size."),
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
                _ => throw new ArgumentException("Unsupported type."),
            };
        }
        protected static string CallGetMemoryReal(int size)
        {
            return size switch
            {
                4 => "GetReal32",
                8 => "GetReal64",
                10 => "GetReal80",
                _ => throw new ArgumentException("Unsupported type."),
            };
        }
        protected static string CallSetMemoryInt(int size)
        {
            return size switch
            {
                1 => "SetByte",
                2 => "SetUInt16",
                4 => "SetUInt32",
                8 => "SetUInt64",
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
