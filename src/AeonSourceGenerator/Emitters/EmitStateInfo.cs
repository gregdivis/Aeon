namespace AeonSourceGenerator.Emitters
{
    internal sealed class EmitStateInfo
    {
        public EmitStateInfo(int wordSize, EmitReturnType returnType, int addressMode, int parameterIndex, EmitterTypeCode methodArgType, bool writeOnly)
        {
            this.WordSize = wordSize;
            this.ReturnType = returnType;
            this.AddressMode = addressMode;
            this.ParameterIndex = parameterIndex;
            this.MethodArgType = methodArgType;
            this.WriteOnly = writeOnly;
        }

        public int WordSize { get; }
        public EmitReturnType ReturnType { get; }
        public int AddressMode { get; }
        public int ParameterIndex { get; }
        public EmitterTypeCode MethodArgType { get; }
        public bool WriteOnly { get; }
    }
}
