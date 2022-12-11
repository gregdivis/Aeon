using System.Collections.Immutable;

namespace Aeon.SourceGenerator
{
    public sealed partial class DecoderGenerator
    {
        private sealed class OpcodeMethodGroup : IEquatable<OpcodeMethodGroup>
        {
            public OpcodeMethodGroup(OpcodeMethod opcodeMethod, ImmutableArray<IOpcodeMethod> alternateMethods)
            {
                this.OpcodeMethod = opcodeMethod;
                this.EmulateMethods = alternateMethods;
            }

            public OpcodeMethod OpcodeMethod { get; }
            public ImmutableArray<IOpcodeMethod> EmulateMethods { get; }

            public bool Equals(OpcodeMethodGroup other)
            {
                if (ReferenceEquals(this, other))
                    return true;
                if (other is null)
                    return false;

                if (!this.OpcodeMethod.Equals(other.OpcodeMethod))
                    return false;

                for (int i = 0; i < 4; i++)
                {
                    if (!Equals(this.EmulateMethods[i], other.EmulateMethods[i]))
                        return false;
                }

                return true;
            }
            public override bool Equals(object obj) => this.Equals(obj as OpcodeMethodGroup);
            public override int GetHashCode() => this.OpcodeMethod.GetHashCode();
        }
    }
}
