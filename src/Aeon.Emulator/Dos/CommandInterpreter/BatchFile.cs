using System.Text;

namespace Aeon.Emulator.CommandInterpreter;

public sealed class BatchFile
{
    private readonly CommandStatement[] statements;

    public BatchFile(IEnumerable<CommandStatement> statements)
    {
        ArgumentNullException.ThrowIfNull(statements);
        this.statements = [.. statements];
    }

    public ReadOnlySpan<CommandStatement> Statements => this.statements;

    public static BatchFile Load(Stream stream)
    {
        using var reader = new StreamReader(stream, Encoding.Latin1, leaveOpen: true);
        return Load(reader);
    }
    public static BatchFile Load(TextReader reader)
    {
        var statements = new List<CommandStatement>();

        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            if (!string.IsNullOrWhiteSpace(line))
                statements.Add(StatementParser.Parse(line)!);
        }

        return new BatchFile(statements);
    }
}
