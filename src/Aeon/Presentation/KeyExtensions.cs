using System.Collections.Generic;
using System.Windows.Input;
using Aeon.Emulator;

namespace Aeon.Emulator.Launcher.Presentation
{
    /// <summary>
    /// Contains extension methods for the Key type.
    /// </summary>
    public static class KeyExtensions
    {
        /// <summary>
        /// Converts a WPF Key into an emulator Keys.
        /// </summary>
        /// <param name="key">Key to convert.</param>
        /// <returns>Equivalent Keys value.</returns>
        public static Keys ToEmulatorKey(this Key key) => keyLookup.TryGetValue(key, out var value) ? value : Keys.Null;

        private static readonly SortedList<Key, Keys> keyLookup = new SortedList<Key, Keys>
        {
            [Key.D0] = Keys.Zero,
            [Key.D1] = Keys.One,
            [Key.D2] = Keys.Two,
            [Key.D3] = Keys.Three,
            [Key.D4] = Keys.Four,
            [Key.D5] = Keys.Five,
            [Key.D6] = Keys.Six,
            [Key.D7] = Keys.Seven,
            [Key.D8] = Keys.Eight,
            [Key.D9] = Keys.Nine,
            [Key.A] = Keys.A,
            [Key.B] = Keys.B,
            [Key.C] = Keys.C,
            [Key.D] = Keys.D,
            [Key.E] = Keys.E,
            [Key.F] = Keys.F,
            [Key.G] = Keys.G,
            [Key.H] = Keys.H,
            [Key.I] = Keys.I,
            [Key.J] = Keys.J,
            [Key.K] = Keys.K,
            [Key.L] = Keys.L,
            [Key.M] = Keys.M,
            [Key.N] = Keys.N,
            [Key.O] = Keys.O,
            [Key.P] = Keys.P,
            [Key.Q] = Keys.Q,
            [Key.R] = Keys.R,
            [Key.S] = Keys.S,
            [Key.T] = Keys.T,
            [Key.U] = Keys.U,
            [Key.V] = Keys.V,
            [Key.W] = Keys.W,
            [Key.X] = Keys.X,
            [Key.Y] = Keys.Y,
            [Key.Z] = Keys.Z,
            [Key.Tab] = Keys.Tab,
            [Key.CapsLock] = Keys.CapsLock,
            [Key.LeftShift] = Keys.LeftShift,
            [Key.LeftCtrl] = Keys.Ctrl,
            [Key.LeftAlt] = Keys.Alt,
            [Key.OemTilde] = Keys.GraveApostrophe,
            [Key.Space] = Keys.Space,
            [Key.OemOpenBrackets] = Keys.OpenBracket,
            [Key.OemCloseBrackets] = Keys.CloseBracket,
            [Key.OemSemicolon] = Keys.Semicolon,
            [Key.OemQuotes] = Keys.Apostrophe,
            [Key.OemComma] = Keys.Comma,
            [Key.OemPeriod] = Keys.Period,
            [Key.OemQuestion] = Keys.Slash,
            [Key.OemMinus] = Keys.Minus,
            [Key.Add] = Keys.Equals,
            [Key.Back] = Keys.Backspace,
            [Key.Oem5] = Keys.Backslash,
            [Key.Enter] = Keys.Enter,
            [Key.RightShift] = Keys.RightShift,
            [Key.RightCtrl] = Keys.Ctrl,
            [Key.RightAlt] = Keys.Alt,
            [Key.Insert] = Keys.Insert,
            [Key.Delete] = Keys.Delete,
            [Key.Home] = Keys.Home,
            [Key.End] = Keys.End,
            [Key.PageUp] = Keys.PageUp,
            [Key.PageDown] = Keys.PageDown,
            [Key.Up] = Keys.KeypadEight,
            [Key.Down] = Keys.KeypadTwo,
            [Key.Left] = Keys.KeypadFour,
            [Key.Right] = Keys.KeypadSix,
            [Key.Escape] = Keys.Esc,
            [Key.F1] = Keys.F1,
            [Key.F2] = Keys.F2,
            [Key.F3] = Keys.F3,
            [Key.F4] = Keys.F4,
            [Key.F5] = Keys.F5,
            [Key.F6] = Keys.F6,
            [Key.F7] = Keys.F7,
            [Key.F8] = Keys.F8,
            [Key.F9] = Keys.F9,
            [Key.F10] = Keys.F10,
            [Key.F11] = Keys.F11,
            [Key.F12] = Keys.F12
        };
    }
}
