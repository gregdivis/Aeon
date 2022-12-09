using Microsoft.CodeAnalysis;

namespace Aeon.SourceGenerator
{
    public sealed partial class DecoderGenerator
    {
        private sealed class OpcodeMethod : IEquatable<OpcodeMethod>, IOpcodeMethod
        {
            public OpcodeMethod(string opcodeText, int operandSize, int addressSize, IMethodSymbol method, string name, bool isPrefix)
            {
                this.OpcodeText = opcodeText;
                this.OperandSize = operandSize;
                this.AddressSize = addressSize;
                this.Method = method;
                this.Name = name;
                this.IsPrefix = isPrefix;
            }

            public string OpcodeText { get; }
            public int OperandSize { get; }
            public int AddressSize { get; }
            public IMethodSymbol Method { get; }
            public string Name { get; }
            public bool IsPrefix { get; }

            public bool Equals(OpcodeMethod other)
            {
                if (ReferenceEquals(this, other))
                    return true;
                if (other is null)
                    return false;

                return this.OpcodeText == other.OpcodeText
                    && this.OperandSize == other.OperandSize
                    && this.AddressSize == other.AddressSize
                    && SymbolEqualityComparer.Default.Equals(this.Method, other.Method)
                    && this.Name == other.Name
                    && this.IsPrefix == other.IsPrefix;
            }
            public override bool Equals(object obj) => this.Equals(obj as OpcodeMethod);
            public override int GetHashCode() => this.OpcodeText?.GetHashCode() ?? 0;
        }
    }
}
