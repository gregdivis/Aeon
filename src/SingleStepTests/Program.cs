using System.IO.Compression;
using MooParser;
using SingleStepTests;

var mooFile = @"C:\Code\SingleStepTests80386\v1_ex_real_mode\C0.6.MOO.gz";
//foreach (var mooFile in Directory.EnumerateFiles(@"C:\Code\SingleStepTests80386\v1_ex_real_mode", "*.MOO.gz"))
{
    using var stream = File.OpenRead(mooFile);
    using var gzip = new GZipStream(stream, CompressionMode.Decompress);

    var moo = new MooFile(gzip);
    Console.WriteLine($"{Path.GetFileName(mooFile)} {moo.Meta.Mnemonic}");

    try
    {
        foreach (var test in moo.EnumerateTests())
        {
            using var runner = new TestRunner(test);
            runner.Execute();
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
    }
}
