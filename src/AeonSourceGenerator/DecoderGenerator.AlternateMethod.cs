using Microsoft.CodeAnalysis;

namespace Aeon.SourceGenerator
{
    public sealed partial class DecoderGenerator
    {
        private sealed class AlternateMethod : IEquatable<AlternateMethod>, IOpcodeMethod
        {
            public AlternateMethod(IMethodSymbol opcodeMethod, int operandSize, int addressSize, IMethodSymbol method)
            {
                this.OpcodeMethod = opcodeMethod;
                this.OperandSize = operandSize;
                this.AddressSize = addressSize;
                this.Method = method;
            }

            public IMethodSymbol OpcodeMethod { get; }
            public int OperandSize { get; }
            public int AddressSize { get; }
            public IMethodSymbol Method { get; }

            public static bool Equals(AlternateMethod a, AlternateMethod b)
            {
                if (a is null)
                    return b is null;
                return a.Equals(b);
            }
            public bool Equals(AlternateMethod other)
            {
                if (ReferenceEquals(this, other))
                    return true;
                if (other is null)
                    return false;

                return SymbolEqualityComparer.Default.Equals(this.OpcodeMethod, other.OpcodeMethod)
                    && this.OperandSize == other.OperandSize
                    && this.AddressSize == other.AddressSize
                    && SymbolEqualityComparer.Default.Equals(this.Method, other.Method);
            }
            public override bool Equals(object obj) => this.Equals(obj as AlternateMethod);
            public override int GetHashCode() => SymbolEqualityComparer.Default.GetHashCode(this.Method);
        }
    }
}
