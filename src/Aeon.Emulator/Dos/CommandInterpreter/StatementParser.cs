using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Aeon.Emulator.Dos.VirtualFileSystem;

#nullable disable

namespace Aeon.Emulator.CommandInterpreter
{
    public static class StatementParser
    {
        public static CommandStatement Parse(ReadOnlySpan<char> s)
        {
            var result = ParseInternal(s, out var noEcho);
            if (result != null)
            {
                result.NoEcho = noEcho;
                result.RawStatement = s.ToString();
            }

            return result;
        }
        public static CommandStatement Parse(string s)
        {
            var result = ParseInternal(s, out var noEcho);
            if (result != null)
            {
                result.NoEcho = noEcho;
                result.RawStatement = s;
            }

            return result;
        }
        public static string[] ParseArguments(ReadOnlySpan<char> source)
        {
            var t = source.TrimStart();
            if (t.IsEmpty)
                return Array.Empty<string>();

            var args = new List<string>();

            while (!t.IsEmpty)
            {
                if (t[0] == '"')
                {
                    int endQuoteIndex = t.Slice(1).IndexOf('"');
                    if (endQuoteIndex == -1)
                        return new[] { t.Slice(1).ToString() };

                    args.Add(t[1..endQuoteIndex].ToString());
                    t = t.Slice(endQuoteIndex + 1);
                }
                else
                {
                    Split(t, out var arg, out var remainder);
                    args.Add(arg.ToString());
                    t = remainder;
                }

                t = t.TrimStart();
            }

            return args.ToArray();
        }
        public static bool IsBatchFile(ReadOnlySpan<char> source)
        {
            Split(source.TrimStart(), out var first, out _);
            return first.EndsWith(".BAT", StringComparison.OrdinalIgnoreCase);
        }

        internal static void Split(ReadOnlySpan<char> source, out ReadOnlySpan<char> first, out ReadOnlySpan<char> remainder)
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

        private static CommandStatement ParseInternal(ReadOnlySpan<char> s, out bool noEcho)
        {
            noEcho = false;

            var trimmed = s.Trim();

            if (trimmed.IsEmpty)
                return null;

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
                return new SetCurrentDriveCommand(new DriveLetter(trimmed[0]));

            // cd
            if (trimmed.Equals("cd", StringComparison.OrdinalIgnoreCase))
                return new PrintCurrentDirectoryCommand();

            // cd dir, cd\dir, cd..
            if (trimmed.StartsWith("cd", StringComparison.OrdinalIgnoreCase) && trimmed.Length > 2 && (char.IsWhiteSpace(trimmed[2]) || trimmed[2] == '\\' || trimmed[2] == '.'))
                return new SetCurrentDirectoryCommand(trimmed.Slice(2).Trim().ToString());

            Split(s.TrimStart().Slice(noEcho ? 1 : 0), out var commandName, out var args);

            // call other.bat
            if (commandName.Equals("call", StringComparison.OrdinalIgnoreCase))
            {
                Split(args.TrimStart(), out var batchName, out var batchArgs);
                return new CallCommand(batchName.Trim().ToString(), batchArgs.Trim().ToString());
            }

            // cls
            if (commandName.Equals("cls", StringComparison.OrdinalIgnoreCase))
                return new ClsCommand();

            // dir
            if (commandName.Equals("dir", StringComparison.OrdinalIgnoreCase))
                return ParseDir(args.Trim());

            // echo
            if (commandName.Equals("echo", StringComparison.OrdinalIgnoreCase))
                return new EchoCommand(args.ToString());

            // exit
            if (commandName.Equals("exit", StringComparison.OrdinalIgnoreCase))
                return new ExitCommand();

            // goto
            if (commandName.Equals("goto", StringComparison.OrdinalIgnoreCase))
                return ParseGoto(args.Trim());

            // if
            if (commandName.Equals("if", StringComparison.OrdinalIgnoreCase))
                return ParseIf(args.TrimStart());

            // rem
            if (commandName.Equals("rem", StringComparison.OrdinalIgnoreCase))
                return new RemCommand(args.ToString());

            // set
            if (commandName.Equals("set", StringComparison.OrdinalIgnoreCase))
                return ParseSet(args.Trim());

            // type
            if (commandName.Equals("type", StringComparison.OrdinalIgnoreCase))
                return ParseType(args.Trim());

            return new LaunchCommand(commandName.ToString(), args.Trim().ToString());
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
                var r = remainder.TrimStart();
                if (!r.IsEmpty && r[0] == '"')
                    return ParseIfEquals(not, r);

                Split(remainder.TrimStart(), out next, out remainder);
            }
            else if (args[0] == '"')
            {
                return ParseIfEquals(not, args);
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

            return new InvalidCommand("Invalid expression.");
        }
        private static CommandStatement ParseIfEquals(bool not, ReadOnlySpan<char> args)
        {
            if (!TryParseString(args, out var value1))
                return new InvalidCommand("Invalid expression.");

            var remainder = args.Slice(value1.Length + 2).TrimStart();
            if (!remainder.StartsWith("=="))
                return new InvalidCommand("Invalid expression.");

            remainder = remainder[2..].TrimStart();

            if (!TryParseString(remainder, out var value2))
                return new InvalidCommand("Invalid expression.");

            var command = Parse(remainder.Slice(value2.Length + 2).TrimStart());
            if (command == null)
                return new InvalidCommand("Expected command.");

            return new IfEqualsCommand(not, value1.ToString(), value2.ToString(), command);
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
        private static bool TryParseString(ReadOnlySpan<char> s, out ReadOnlySpan<char> value)
        {
            value = default;

            if (s.IsEmpty || s[0] != '"')
                return false;

            for (int i = 1; i < s.Length; i++)
            {
                if (s[i] == '"')
                {
                    value = s[1..i];
                    return true;
                }
            }

            return false;
        }
    }
}
