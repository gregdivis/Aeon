using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Aeon.Emulator.Dos.VirtualFileSystem;

namespace Aeon.Emulator.CommandInterpreter
{
    public static class StatementParser
    {
        public static CommandStatement Parse(ReadOnlySpan<char> s)
        {
            var trimmed = s.Trim();

            if (trimmed.IsEmpty)
                return null;

            bool noEcho = false;
            if (trimmed[0] == '@')
            {
                noEcho = true;
                trimmed = trimmed.Slice(1).Trim();
            }

            // :label
            if (trimmed[0] == ':')
                return ParseLabel(trimmed.Slice(1).Trim());

            // c:
            if (trimmed.Length == 2 && char.IsLetter(trimmed[0]) && trimmed[1] == ':')
                return new SetCurrentDriveCommand(new DriveLetter(trimmed[0])) { NoEcho = noEcho };

            // cd
            if (trimmed.Equals("cd", StringComparison.OrdinalIgnoreCase))
                return new PrintCurrentDirectoryCommand { NoEcho = noEcho };

            // cd dir, cd\dir, cd..
            if (trimmed.StartsWith("cd", StringComparison.OrdinalIgnoreCase) && trimmed.Length > 2 && (char.IsWhiteSpace(trimmed[2]) || trimmed[2] == '\\' || trimmed[2] == '.'))
                return new SetCurrentDirectoryCommand(trimmed.Slice(2).Trim().ToString()) { NoEcho = noEcho };

            Split(s.TrimStart().Slice(noEcho ? 1 : 0), out var commandName, out var args);

            // call other.bat
            if (commandName.Equals("call", StringComparison.OrdinalIgnoreCase))
                return new CallCommand(args.Trim().ToString()) { NoEcho = noEcho };

            // cls
            if (commandName.Equals("cls", StringComparison.OrdinalIgnoreCase))
                return new ClsCommand { NoEcho = noEcho };

            // dir
            if (commandName.Equals("dir", StringComparison.OrdinalIgnoreCase))
                return ParseDir(args.Trim());

            // echo
            if (commandName.Equals("echo", StringComparison.OrdinalIgnoreCase))
                return new EchoCommand(args.ToString()) { NoEcho = noEcho };

            // exit
            if (commandName.Equals("exit", StringComparison.OrdinalIgnoreCase))
                return new ExitCommand { NoEcho = noEcho };

            // goto
            if (commandName.Equals("goto", StringComparison.OrdinalIgnoreCase))
                return ParseGoto(args.Trim());

            // if
            if (commandName.Equals("if", StringComparison.OrdinalIgnoreCase))
                return ParseIf(args.TrimStart());

            // rem
            if (commandName.Equals("rem", StringComparison.OrdinalIgnoreCase))
                return new RemCommand(args.ToString()) { NoEcho = noEcho };

            // set
            if (commandName.Equals("set", StringComparison.OrdinalIgnoreCase))
                return ParseSet(args.Trim());

            // type
            if (commandName.Equals("type", StringComparison.OrdinalIgnoreCase))
                return ParseType(args.Trim());

            return new LaunchCommand(commandName.ToString(), args.Trim().ToString()) { NoEcho = noEcho };
        }

        private static void Split(ReadOnlySpan<char> source, out ReadOnlySpan<char> first, out ReadOnlySpan<char> remainder)
        {
            for (int i = 0; i < source.Length; i++)
            {
                if (char.IsWhiteSpace(source[i]))
                {
                    first = source.Slice(0, i);
                    remainder = source.Slice(i + 1);
                    return;
                }
            }

            first = source;
            remainder = default;
        }

        private static CommandStatement ParseLabel(ReadOnlySpan<char> args)
        {
            if (args.IsEmpty)
                return new InvalidCommand("Missing label name.");

            return new LabelStatement(args.ToString());
        }

        private static CommandStatement ParseDir(ReadOnlySpan<char> args)
        {
            var parts = Regex.Split(args.ToString(), @"\s");

            var path = string.Empty;

            var options = DirectoryOptions.None;
            var filter = new List<DirectoryAttributeFilter>();
            var sortBy = new List<DirectorySort>();

            foreach (var p in parts)
            {
                if (p.Equals("/P", StringComparison.OrdinalIgnoreCase))
                    options |= DirectoryOptions.Pause;
                else if (p.Equals("/W", StringComparison.OrdinalIgnoreCase))
                    options |= DirectoryOptions.Wide;
                else if (p.Equals("/S", StringComparison.OrdinalIgnoreCase))
                    options |= DirectoryOptions.Recursive;
                else if (p.Equals("/B", StringComparison.OrdinalIgnoreCase))
                    options |= DirectoryOptions.Bare;
                else if (p.Equals("/L", StringComparison.OrdinalIgnoreCase))
                    options |= DirectoryOptions.Lowercase;
                else if (p.Equals("/V", StringComparison.OrdinalIgnoreCase))
                    options |= DirectoryOptions.Verbose;
                else if (p.StartsWith("/A:", StringComparison.OrdinalIgnoreCase) && p.Length > 3)
                {
                    bool not = false;

                    foreach (char c in p.AsSpan().Slice(3))
                    {
                        if (c == '-')
                        {
                            not = true;
                            continue;
                        }

                        FileAttributes attr;
                        if (c == 'D' || c == 'd')
                            attr = FileAttributes.Directory;
                        else if (c == 'R' || c == 'r')
                            attr = FileAttributes.ReadOnly;
                        else if (c == 'H' || c == 'h')
                            attr = FileAttributes.Hidden;
                        else if (c == 'A' || c == 'a')
                            attr = FileAttributes.Archive;
                        else if (c == 'S' || c == 's')
                            attr = FileAttributes.System;
                        else
                            return new InvalidCommand("Invalid attribute: " + c);

                        filter.Add(new DirectoryAttributeFilter(attr, !not));
                        not = false;
                    }
                }
                else if (p.StartsWith("/O:", StringComparison.OrdinalIgnoreCase) && p.Length > 3)
                {
                    bool descending = false;

                    foreach (char c in p.AsSpan().Slice(3))
                    {
                        if (c == '-')
                        {
                            descending = true;
                            continue;
                        }

                        DirectorySortKey key;
                        if (c == 'N' || c == 'n')
                            key = DirectorySortKey.Name;
                        else if (c == 'S' || c == 's')
                            key = DirectorySortKey.Size;
                        else if (c == 'E' || c == 'e')
                            key = DirectorySortKey.Extension;
                        else if (c == 'D' || c == 'd')
                            key = DirectorySortKey.Date;
                        else if (c == 'G' || c == 'g')
                            key = DirectorySortKey.GroupDirectoriesFirst;
                        else
                            return new InvalidCommand("Invalid sort key: " + c);

                        sortBy.Add(new DirectorySort(key, descending));
                        descending = false;
                    }
                }
                else if (p.StartsWith("/"))
                    return new InvalidCommand("Invalid switch: " + p);
                else
                    path = p;
            }

            return new DirectoryCommand(path, options, filter, sortBy);
        }

        private static CommandStatement ParseGoto(ReadOnlySpan<char> args)
        {
            if (args.IsEmpty)
                return new InvalidCommand("Missing label.");

            return new GotoCommand(args.ToString());
        }

        private static CommandStatement ParseIf(ReadOnlySpan<char> args)
        {
            bool not = false;

            Split(args, out var next, out var remainder);
            if (next.Equals("not", StringComparison.OrdinalIgnoreCase))
            {
                not = true;
                Split(remainder.TrimStart(), out next, out remainder);
            }

            if (next.Equals("errorlevel", StringComparison.OrdinalIgnoreCase))
            {
                Split(remainder.TrimStart(), out next, out remainder);
                if (!int.TryParse(next, out int errorLevel))
                    return new InvalidCommand("Invalid errorlevel: " + next.ToString());

                return new IfErrorLevelCommand(not, errorLevel, Parse(remainder.TrimStart()));
            }

            if (next.Equals("exist", StringComparison.OrdinalIgnoreCase))
            {
                Split(remainder.TrimStart(), out next, out remainder);
                if (next.IsEmpty)
                    return new InvalidCommand("Missing file name.");

                return new IfFileExistsCommand(not, next.ToString(), Parse(remainder.TrimStart()));
            }

            var match = Regex.Match(remainder.ToString(), @"(?<1>[^=]+)==\s*(?<2>\S+)", RegexOptions.ExplicitCapture);
            if (!match.Success)
                return new InvalidCommand("Invalid expression.");

            return new IfEqualsCommand(not, match.Groups[1].Value.Trim(), match.Groups[2].Value.Trim(), Parse(remainder.Slice(match.Length)));
        }

        private static CommandStatement ParseSet(ReadOnlySpan<char> args)
        {
            if (args.IsEmpty)
                return new PrintEnvironmentCommand();

            var parts = args.ToString().Split('=', 2);
            return new SetCommand(parts[0].Trim(), parts.Length > 1 ? parts[1].Trim() : string.Empty);
        }

        private static CommandStatement ParseType(ReadOnlySpan<char> args)
        {
            if (args.IsEmpty)
                return new InvalidCommand("Missing file name.");

            return new TypeCommand(args.ToString());
        }
    }
}
