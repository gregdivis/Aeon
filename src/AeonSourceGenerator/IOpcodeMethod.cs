using Microsoft.CodeAnalysis;

namespace Aeon.SourceGenerator
{
    internal interface IOpcodeMethod
    {
        public int OperandSize { get; }
        public int AddressSize { get; }
        public IMethodSymbol Method { get; }
    }
}
