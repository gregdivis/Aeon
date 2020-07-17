using System.Text;

namespace AeonSourceGenerator.Emitters
{
    internal sealed class EmitStateInfo
    {
        public EmitStateInfo(int wordSize, EmitReturnType returnType, int addressMode, int parameterIndex, EmitterTypeCode methodArgType)
        {
            this.WordSize = wordSize;
            this.ReturnType = returnType;
            this.AddressMode = addressMode;
            this.ParameterIndex = parameterIndex;
            this.MethodArgType = methodArgType;
        }

        public int WordSize { get; }
        public EmitReturnType ReturnType { get; }
        public int AddressMode { get; }
        public int ParameterIndex { get; }
        public EmitterTypeCode MethodArgType { get; }
    }
}
