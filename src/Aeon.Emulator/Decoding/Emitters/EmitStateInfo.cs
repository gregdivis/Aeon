using System.Reflection.Emit;

namespace Aeon.Emulator.Decoding.Emitters
{
    internal sealed class EmitStateInfo
    {
        public EmitStateInfo(ILGenerator il, LocalBuilder processorLocal, int wordSize, EmitReturnType returnType, int addressMode)
        {
            this.IL = il;
            this.WordSize = wordSize;
            this.ProcessorLocal = processorLocal;
            this.ReturnType = returnType;
            this.AddressMode = addressMode;
        }

        public ILGenerator IL { get; }
        public LocalBuilder ProcessorLocal { get; }
        public int WordSize { get; }
        public EmitReturnType ReturnType { get; }
        public int AddressMode { get; }
    }
}
