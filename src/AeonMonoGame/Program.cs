using Aeon.Emulator;
using Aeon.Emulator.Configuration;
using Aeon.Emulator.Launcher;

var emulator = EmulatorHost.CreateWithConfig(GetLaunchConfig(args));

using var game = new AeonGame(emulator, GlobalConfiguration.Load().ShowIps);
game.Run();

static AeonConfiguration GetLaunchConfig(string[] args)
{
    if (args.Length > 0)
    {
        if (args[0].EndsWith(".AeonConfig", StringComparison.OrdinalIgnoreCase))
            return AeonConfiguration.Load(args[0]);
        else if (EndsWithAny(args[0], ".exe", ".com", ".bat"))
            return AeonConfiguration.GetQuickLaunchConfiguration(Path.GetDirectoryName(Path.GetFullPath(args[0]))!, Path.GetFileName(args[0]));
        else if (Directory.Exists(args[0]))
            return AeonConfiguration.GetQuickLaunchConfiguration(args[0], "COMMAND.COM");
        else
            throw new ArgumentException("Invalid argument.");
    }

    return AeonConfiguration.GetQuickLaunchConfiguration(Environment.CurrentDirectory, "COMMAND.COM");

    static bool EndsWithAny(string s, params ReadOnlySpan<string> endings)
    {
        foreach (var e in endings)
        {
            if (s.EndsWith(e, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }
}
