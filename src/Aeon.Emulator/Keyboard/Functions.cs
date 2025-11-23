namespace Aeon.Emulator.Keyboard;

internal sealed class Functions
{
    public const byte ReadCharacter = 0x00;
    public const byte CheckForCharacter = 0x01;
    public const byte GetShiftFlags = 0x02;
    public const byte SetAutoRepeatRate = 0x03;
    public const byte StoreKeyCodeInBuffer = 0x05;
    public const byte ReadExtendedCharacter = 0x10;
    public const byte CheckForExtendedCharacter = 0x11;
    public const byte GetExtendedShiftFlags = 0x12;
}
