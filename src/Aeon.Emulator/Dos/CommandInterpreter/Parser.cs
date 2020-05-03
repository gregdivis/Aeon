using System;
using System.Collections.Generic;
using System.Linq;

namespace Aeon.Emulator.CommandInterpreter
{
    internal static class Parser
    {
        private static readonly Dictionary<string, Type> commands = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
        private static readonly char[] CommandDelimiters = new char[] { '.', '\\', '/', ' ', '\t' };

        static Parser()
        {
            var commandTypes = typeof(Command)
                .Assembly
                .GetTypes()
                .Where(t => t.IsSubclassOf(typeof(Command)));

            foreach (var t in commandTypes)
            {
                object[] attrs = t.GetCustomAttributes(typeof(CommandAttribute), false);
                if (attrs != null)
                {
                    foreach (var cmdInfo in attrs.Cast<CommandAttribute>())
                        commands[cmdInfo.Name] = t;
                }
            }
        }

        public static Command Parse(string commandLine)
        {
            if (commandLine == null)
                throw new ArgumentNullException(nameof(commandLine));

            commandLine = commandLine.Trim();
            if (commandLine == string.Empty)
                return null;

            var chdrive = ParseChangeDriveCommand(commandLine);
            if (chdrive != null)
                return chdrive;

            var command = ParseKnownCommand(commandLine);
            if (command != null)
                return command;

            return ParseLaunchCommand(commandLine);
        }

        private static Commands.Chdrive ParseChangeDriveCommand(string commandLine)
        {
            if (commandLine.Length == 2 && commandLine[1] == ':')
            {
                var chdriveCommand = new Commands.Chdrive();
                chdriveCommand.Parse(commandLine);
                return chdriveCommand;
            }

            return null;
        }
        private static Command ParseKnownCommand(string commandLine)
        {
            var commandText = commandLine;
            var commandArgs = string.Empty;
            int index = commandLine.IndexOfAny(CommandDelimiters);
            if (index > 0)
            {
                commandText = commandLine.Substring(0, index);
                commandArgs = commandLine.Substring(index);
            }

            if (commands.TryGetValue(commandText, out var commandType))
            {
                Command command = (Command)Activator.CreateInstance(commandType);
                command.Parse(commandArgs.Trim());
                return command;
            }

            return null;
        }
        private static Commands.Launch ParseLaunchCommand(string commandLine)
        {
            var launchCommand = new Commands.Launch();
            launchCommand.Parse(commandLine);
            return launchCommand;
        }
    }
}
