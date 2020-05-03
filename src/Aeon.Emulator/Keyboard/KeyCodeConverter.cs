using System.Collections.Generic;

namespace Aeon.Emulator.Keyboard
{
    /// <summary>
    /// Contains methods for converting a Keys enumerated value to an ASCII character code.
    /// </summary>
    internal static class KeyCodeConverter
    {
        /// <summary>
        /// The shift and caps lock key modifiers.
        /// </summary>
        private const KeyModifiers ShiftCaps = KeyModifiers.Shift | KeyModifiers.CapsLock;

        /// <summary>
        /// Contains normal ASCII codes.
        /// </summary>
        private static readonly SortedList<Keys, char> normalCodes = new SortedList<Keys, char>();
        /// <summary>
        /// Contains shift key ASCII codes.
        /// </summary>
        private static readonly SortedList<Keys, char> shiftCodes = new SortedList<Keys, char>();
        /// <summary>
        /// Contains caps lock ASCII codes.
        /// </summary>
        private static readonly SortedList<Keys, char> capsLockCodes = new SortedList<Keys, char>();
        /// <summary>
        /// Contains shift + caps lock ASCII codes.
        /// </summary>
        private static readonly SortedList<Keys, char> shiftCapsLockCodes = new SortedList<Keys, char>();

        static KeyCodeConverter()
        {
            AddLowercaseLetters(normalCodes);
            AddNumbersAndSymbols(normalCodes);

            AddUppercaseLetters(shiftCodes);
            AddShiftedNumbersAndSymbols(shiftCodes);

            AddUppercaseLetters(capsLockCodes);
            AddNumbersAndSymbols(capsLockCodes);

            AddLowercaseLetters(shiftCapsLockCodes);
            AddShiftedNumbersAndSymbols(shiftCapsLockCodes);
        }

        /// <summary>
        /// Returns an ASCII character code converted from the input values.
        /// </summary>
        /// <param name="key">Enumeration specifiying the key to convert.</param>
        /// <param name="modifiers">Current active key modifiers.</param>
        /// <returns>ASCII character code converted from the input values, or 0 if no conversion is possible.</returns>
        public static char ConvertToASCII(Keys key, KeyModifiers modifiers)
        {
            char c;

            if ((modifiers & ShiftCaps) == ShiftCaps)
                shiftCapsLockCodes.TryGetValue(key, out c);
            else if ((modifiers & KeyModifiers.Shift) != 0)
                shiftCodes.TryGetValue(key, out c);
            else if ((modifiers & KeyModifiers.CapsLock) != 0)
                capsLockCodes.TryGetValue(key, out c);
            else
                normalCodes.TryGetValue(key, out c);

            return c;
        }

        /// <summary>
        /// Adds ASCII lowercase letters to a dictionary.
        /// </summary>
        /// <param name="codes">Dictionary to add to.</param>
        private static void AddLowercaseLetters(SortedList<Keys, char> codes)
        {
            codes.Add(Keys.A, 'a');
            codes.Add(Keys.B, 'b');
            codes.Add(Keys.C, 'c');
            codes.Add(Keys.D, 'd');
            codes.Add(Keys.E, 'e');
            codes.Add(Keys.F, 'f');
            codes.Add(Keys.G, 'g');
            codes.Add(Keys.H, 'h');
            codes.Add(Keys.I, 'i');
            codes.Add(Keys.J, 'j');
            codes.Add(Keys.K, 'k');
            codes.Add(Keys.L, 'l');
            codes.Add(Keys.M, 'm');
            codes.Add(Keys.N, 'n');
            codes.Add(Keys.O, 'o');
            codes.Add(Keys.P, 'p');
            codes.Add(Keys.Q, 'q');
            codes.Add(Keys.R, 'r');
            codes.Add(Keys.S, 's');
            codes.Add(Keys.T, 't');
            codes.Add(Keys.U, 'u');
            codes.Add(Keys.V, 'v');
            codes.Add(Keys.W, 'w');
            codes.Add(Keys.X, 'x');
            codes.Add(Keys.Y, 'y');
            codes.Add(Keys.Z, 'z');
        }
        /// <summary>
        /// Adds ASCII uppercase letters to a dictionary.
        /// </summary>
        /// <param name="codes">Dictionary to add to.</param>
        private static void AddUppercaseLetters(SortedList<Keys, char> codes)
        {
            codes.Add(Keys.A, 'A');
            codes.Add(Keys.B, 'B');
            codes.Add(Keys.C, 'C');
            codes.Add(Keys.D, 'D');
            codes.Add(Keys.E, 'E');
            codes.Add(Keys.F, 'F');
            codes.Add(Keys.G, 'G');
            codes.Add(Keys.H, 'H');
            codes.Add(Keys.I, 'I');
            codes.Add(Keys.J, 'J');
            codes.Add(Keys.K, 'K');
            codes.Add(Keys.L, 'L');
            codes.Add(Keys.M, 'M');
            codes.Add(Keys.N, 'N');
            codes.Add(Keys.O, 'O');
            codes.Add(Keys.P, 'P');
            codes.Add(Keys.Q, 'Q');
            codes.Add(Keys.R, 'R');
            codes.Add(Keys.S, 'S');
            codes.Add(Keys.T, 'T');
            codes.Add(Keys.U, 'U');
            codes.Add(Keys.V, 'V');
            codes.Add(Keys.W, 'W');
            codes.Add(Keys.X, 'X');
            codes.Add(Keys.Y, 'Y');
            codes.Add(Keys.Z, 'Z');
        }
        /// <summary>
        /// Adds non-letters to a dictionary.
        /// </summary>
        /// <param name="codes">Dictionary to add to.</param>
        private static void AddNumbersAndSymbols(SortedList<Keys, char> codes)
        {
            codes.Add(Keys.One, '1');
            codes.Add(Keys.Two, '2');
            codes.Add(Keys.Three, '3');
            codes.Add(Keys.Four, '4');
            codes.Add(Keys.Five, '5');
            codes.Add(Keys.Six, '6');
            codes.Add(Keys.Seven, '7');
            codes.Add(Keys.Eight, '8');
            codes.Add(Keys.Nine, '9');
            codes.Add(Keys.Zero, '0');

            codes.Add(Keys.Apostrophe, '\'');
            codes.Add(Keys.Backslash, '\\');
            codes.Add(Keys.Backspace, '\b');
            codes.Add(Keys.OpenBracket, '[');
            codes.Add(Keys.CloseBracket, ']');
            codes.Add(Keys.Comma, ',');
            codes.Add(Keys.Equals, '=');
            codes.Add(Keys.GraveApostrophe, '`');
            codes.Add(Keys.Minus, '-');
            codes.Add(Keys.Period, '.');
            codes.Add(Keys.Semicolon, ';');
            codes.Add(Keys.Slash, '/');
            codes.Add(Keys.Space, ' ');
        }
        /// <summary>
        /// Adds non-letters with the shift key pressed to a dictionary.
        /// </summary>
        /// <param name="codes">Dictionary to add to.</param>
        private static void AddShiftedNumbersAndSymbols(SortedList<Keys, char> codes)
        {
            codes.Add(Keys.One, '!');
            codes.Add(Keys.Two, '@');
            codes.Add(Keys.Three, '#');
            codes.Add(Keys.Four, '$');
            codes.Add(Keys.Five, '%');
            codes.Add(Keys.Six, '^');
            codes.Add(Keys.Seven, '&');
            codes.Add(Keys.Eight, '*');
            codes.Add(Keys.Nine, '(');
            codes.Add(Keys.Zero, ')');

            codes.Add(Keys.Apostrophe, '\"');
            codes.Add(Keys.Backslash, '|');
            codes.Add(Keys.Backspace, '\b');
            codes.Add(Keys.OpenBracket, '{');
            codes.Add(Keys.CloseBracket, '}');
            codes.Add(Keys.Comma, '<');
            codes.Add(Keys.Equals, '+');
            codes.Add(Keys.GraveApostrophe, '~');
            codes.Add(Keys.Minus, '_');
            codes.Add(Keys.Period, '>');
            codes.Add(Keys.Semicolon, ':');
            codes.Add(Keys.Slash, '?');
            codes.Add(Keys.Space, ' ');
        }
    }
}
