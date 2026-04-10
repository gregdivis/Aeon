using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Aeon.Emulator.Sound.Midi;

[SupportedOSPlatform("windows")]
internal static partial class WinMM
{
    public const uint MIDI_MAPPER = 0xFFFFFFFF;

    [LibraryImport("winmm.dll", SetLastError = false)]
    public static partial uint midiOutOpen(out nint lphmo, uint uDeviceID, nint dwCallback, nint dwCallbackInstance, uint dwFlags);
    [LibraryImport("winmm.dll", SetLastError = false)]
    public static partial uint midiOutClose(nint hmo);
    [LibraryImport("winmm.dll", SetLastError = false)]
    public static partial uint midiOutShortMsg(nint hmo, uint dwMsg);
    [LibraryImport("winmm.dll", SetLastError = false)]
    public static partial uint midiOutReset(nint hmo);
}
