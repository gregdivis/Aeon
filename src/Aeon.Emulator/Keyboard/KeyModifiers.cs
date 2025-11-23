namespace Aeon.Emulator.Keyboard;

[Flags]
internal enum KeyModifiers
{
    None,
    Shift = 1,
    Ctrl = 2,
    Alt = 4,
    CapsLock = 8,
    NumLock = 16,
    ScrollLock = 32,
    Insert = 64
}
