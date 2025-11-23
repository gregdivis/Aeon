namespace Aeon.Emulator.Sound;

/// <summary>
/// Specifies the engine to use for MIDI playback.
/// </summary>
public enum MidiEngine
{
    /// <summary>
    /// Use the OS MIDI mapper.
    /// </summary>
    MidiMapper,
    /// <summary>
    /// Use MeltySynth (requires soundfont).
    /// </summary>
    MeltySynth,
    /// <summary>
    /// Use munt (requires MT-32 roms).
    /// </summary>
    Mt32
}
