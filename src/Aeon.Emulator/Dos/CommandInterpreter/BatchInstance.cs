namespace Aeon.Emulator.CommandInterpreter;

internal sealed class BatchInstance(BatchFile batch, string arguments)
{
    private readonly string[] arguments = StatementParser.ParseArguments(arguments);

    public BatchFile Batch { get; } = batch;
    public int CurrentLine { get; set; }
    public ReadOnlySpan<string> Arguments => this.arguments;
}
