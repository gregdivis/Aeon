namespace Aeon.Emulator.Sound;

/// <summary>
/// Contains initialization options for the <see cref="GeneralMidi"/> class.
/// </summary>
/// <param name="Engine">MIDI rendering engine.</param>
/// <param name="SoundFontPath">SoundFont path for MeltySynth rendering.</param>
/// <param name="Mt32RomsPath">MT32 roms path for MT-32 rendering.</param>
public sealed record class GeneralMidiOptions(MidiEngine Engine, string? SoundFontPath = null, string? Mt32RomsPath = null);
