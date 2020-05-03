using System;
using System.Reflection.Emit;

namespace Aeon.Emulator.Decoding.Emitters
{
    internal abstract class Emitter
    {
        private readonly EmitStateInfo state;

        protected readonly ILGenerator il;

        protected const int VmArgument = 0;

        protected Emitter(EmitStateInfo state)
        {
            this.il = state.IL;
            this.state = state;
        }

        public abstract Type MethodArgType { get; }
        public virtual bool? RequiresTemp => false;
        public virtual Type TempType => null;
        public virtual LocalBuilder ParamIsAddressLocal => null;

        protected int WordSize => this.state.WordSize;
        protected EmitReturnType ReturnType => this.state.ReturnType;
        protected int AddressMode => this.state.AddressMode;

        public abstract void EmitLoad();
        public virtual void EmitEpilog()
        {
        }

        protected void LoadIPPointer()
        {
            LoadProcessor();
            il.Emit(OpCodes.Ldfld, Infos.Processor.CachedIP);
        }
        protected void LoadProcessor()
        {
            il.LoadLocal(this.state.ProcessorLocal);
        }
        protected void LoadPhysicalMemory()
        {
            il.LoadArgument(VmArgument);
            il.Emit(OpCodes.Ldfld, Infos.VirtualMachine.PhysicalMemory);
        }
        protected void CallGetMemoryInt(int size)
        {
            switch (size)
            {
                case 1:
                    il.Emit(OpCodes.Call, Infos.PhysicalMemory.GetByte);
                    break;

                case 2:
                    il.Emit(OpCodes.Call, Infos.PhysicalMemory.GetUInt16);
                    break;

                case 4:
                    il.Emit(OpCodes.Call, Infos.PhysicalMemory.GetUInt32);
                    break;

                case 8:
                    il.Emit(OpCodes.Call, Infos.PhysicalMemory.GetUInt64);
                    break;

                default:
                    throw new ArgumentException("Unsupported type.", "type");
            }
        }
        protected void CallGetMemoryReal(int size)
        {
            switch (size)
            {
                case 4:
                    il.Emit(OpCodes.Call, Infos.PhysicalMemory.GetReal32);
                    break;

                case 8:
                    il.Emit(OpCodes.Call, Infos.PhysicalMemory.GetReal64);
                    break;

                case 10:
                    il.Emit(OpCodes.Call, Infos.PhysicalMemory.GetReal80);
                    break;

                default:
                    throw new ArgumentException("Unsupported type.", "type");
            }
        }
        protected void IncrementIPPointer(int n)
        {
            LoadProcessor();
            il.Emit(OpCodes.Dup);
            il.Emit(OpCodes.Ldfld, Infos.Processor.CachedIP);
            il.LoadConstant(n);
            il.Emit(OpCodes.Conv_I);
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Stfld, Infos.Processor.CachedIP);
        }
        protected static Type GetUnsignedIntType(int size)
        {
            switch (size)
            {
                case 1:
                    return typeof(byte);

                case 2:
                    return typeof(ushort);

                case 4:
                    return typeof(uint);

                case 6:
                case 8:
                    return typeof(ulong);

                default:
                    throw new ArgumentException("Invalid size.");
            }
        }
        protected static Type GetSignedIntType(int size)
        {
            switch (size)
            {
                case 1:
                    return typeof(sbyte);

                case 2:
                    return typeof(short);

                case 4:
                    return typeof(int);

                case 6:
                case 8:
                    return typeof(long);

                default:
                    throw new ArgumentException("Invalid size.");
            }
        }
        protected static Type GetFloatType(int size)
        {
            switch (size)
            {
                case 4:
                    return typeof(float);

                case 8:
                    return typeof(double);

                case 10:
                    return typeof(Real10);

                default:
                    throw new ArgumentException("Invalid size.");
            }
        }
    }
}
