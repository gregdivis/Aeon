namespace Aeon.Emulator.Decoding;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
internal sealed class OperandStringAttribute : Attribute
{
    public OperandStringAttribute(string operandString) => this.OperandString = operandString;

    public string OperandString { get; }
}
